using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;

namespace SceneStream
{
    public class CameraStreamer : BaseCamera
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        [DllImport("ImageEncoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern ImageBuffer EncodeToJPGBuffer(byte[] imageData, int width, int height, int channels, int quality);

        [DllImport("ImageEncoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeBuffer(ImageBuffer buffer);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageBuffer
        {
            public IntPtr data;
            public int size;
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject imageEncoderJavaObject;
#endif

       
        
        public Camera recordingCamera;
        public string cameraName;
        private RenderTexture renderTexture;
        private RTCConnectionManager connectionManager;
        private bool isStreaming;

        void Start()
        {
            StartCoroutine(WebRTC.Update());

            SceneStreamManager.Instance.AddCamera(this, cameraName);

            renderTexture = new RenderTexture(SceneStreamManager.Instance.videoWidth, SceneStreamManager.Instance.videoHeight, 0, RenderTextureFormat.BGRA32);
            renderTexture.Create();
            recordingCamera.targetTexture = renderTexture;

            connectionManager = new RTCConnectionManager(this);

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.AttachCurrentThread();
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                imageEncoderJavaObject = new AndroidJavaObject("com.xrmech.imageencoder.ImageEncoder", activity);
            }
#endif

            Debug.Log("[CameraStreamer] Initialized and ready.");
        }

        private IEnumerator CaptureFrames()
        {
            Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            float elapsedTime = 0f;

            while (isStreaming)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= SceneStreamManager.Instance.captureInterval)
                {
                    elapsedTime = 0f;

                    AsyncGPUReadback.Request(renderTexture, 0, request =>
                    {
                        if (request.hasError)
                        {
                            Debug.LogError("GPU readback error detected.");
                            return;
                        }

                        var data = request.GetData<byte>().ToArray();
                        frame.LoadRawTextureData(data);
                        frame.Apply();

                        byte[] jpgBytes;

#if UNITY_ANDROID && !UNITY_EDITOR
                        // Encode to JPG on Android using Java
                        // TODO: Implement Java encoding
#else
                        ImageBuffer buffer = EncodeToJPGBuffer(data, renderTexture.width, renderTexture.height, 3, 75);
                        jpgBytes = new byte[buffer.size];
                        Marshal.Copy(buffer.data, jpgBytes, 0, buffer.size);
                        FreeBuffer(buffer);
#endif

                        connectionManager.SendFrame(jpgBytes);
                    });
                }

                yield return null;
            }
        }

        public override void StartRecording()
        {
            isStreaming = true;
            StartCoroutine(CaptureFrames());
            StartCoroutine(connectionManager.SetupConnection());
            Debug.Log("[CameraStreamer] Streaming started.");
        }

        public override void StopRecording()
        {
            isStreaming = false;
            StopCoroutine(CaptureFrames());
            Debug.Log("[CameraStreamer] Streaming stopped.");
        }

        void OnDestroy()
        {
            if (renderTexture != null)
            {
                recordingCamera.targetTexture = null;
                Destroy(renderTexture);
                Debug.Log("[CameraStreamer] Cleanup RenderTexture.");
            }

            SceneStreamManager.Instance.RemoveCamera(cameraName);
            connectionManager?.Dispose();
        }
    }

    public class RTCConnectionManager : IDisposable
    {
        
        private RTCPeerConnection localConnection;
        private RTCDataChannel sendChannel;
        private MonoBehaviour owner;
        private bool isConnecting;

        public RTCConnectionManager(MonoBehaviour owner)
        {
            this.owner = owner;
            var config = new RTCConfiguration
            {
                iceServers = new[]
                {
                    new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
                }
            };
            localConnection = new RTCPeerConnection(ref config);
            sendChannel = localConnection.CreateDataChannel("sendChannel");
            sendChannel.OnOpen = OnSendChannelOpen;
            sendChannel.OnClose = OnSendChannelClose;
            sendChannel.OnMessage = OnMessageReceived;

            localConnection.OnIceCandidate = e =>
            {
                if (!string.IsNullOrEmpty(e.Candidate))
                {
                    Debug.Log($"[RTCConnectionManager] ICE candidate received: {e.Candidate}");
                    // Send ICE candidate to the server
                    SendIceCandidateToServer(e);
                }
            };
        }

        public IEnumerator SetupConnection()
        {
            if (isConnecting)
            {
                Debug.LogWarning("Connection setup is already in progress");
                yield break;
            }

            isConnecting = true;

            var createOfferOp = localConnection.CreateOffer();
            yield return createOfferOp;

            var offerDesc = createOfferOp.Desc;
            offerDesc.type = RTCSdpType.Offer;
            Debug.Log($"Created offer: {JsonUtility.ToJson(offerDesc)}");

            var setLocalDescOp = localConnection.SetLocalDescription(ref offerDesc);
            yield return setLocalDescOp;

            // Send offer to the server
            yield return SendOfferToServer(offerDesc);

            isConnecting = false;
        }

        private IEnumerator SendOfferToServer(RTCSessionDescription offerDesc)
        {
            
            var offerJson = JsonUtility.ToJson(new RTCSessionDescriptionWrapper
            {
                type = "offer",
                sdp = offerDesc.sdp
            });
            Debug.Log($"Sending offer to server: {offerJson}");

            using (UnityWebRequest request = new UnityWebRequest(SceneStreamManager.Instance.serverUrl + "offer", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(offerJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error sending offer: " + request.error);
                }
                else
                {
                    Debug.Log("Offer successfully sent. Response: " + request.downloadHandler.text);
                    var answerJson = request.downloadHandler.text;
                    var answerDesc = JsonUtility.FromJson<RTCSessionDescription>(answerJson);
                    var setRemoteDescOp = localConnection.SetRemoteDescription(ref answerDesc);
                    yield return setRemoteDescOp;
                }
            }
        }

        public void SendIceCandidateToServer(RTCIceCandidate candidate)
        {
            Debug.Log($"Sending ICE candidate to server: {candidate.Candidate}");
            var candidateJson = JsonUtility.ToJson(candidate);
            SendCandidateToServerAsync(candidateJson);
        }

        private async void SendCandidateToServerAsync(string candidateJson)
        {
            using (UnityWebRequest request = new UnityWebRequest(SceneStreamManager.Instance.serverUrl + "candidate", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(candidateJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error sending candidate: " + request.error);
                }
                else
                {
                    Debug.Log("Candidate successfully sent.");
                }
            }
        }

        public void AddIceCandidate(RTCIceCandidateInit candidateInit)
        {
            var candidate = new RTCIceCandidate(candidateInit);
            localConnection.AddIceCandidate(candidate);
        }


        public void SendFrame(byte[] frameData)
        {
            if (sendChannel.ReadyState == RTCDataChannelState.Open)
            {
                Debug.Log($"Sending frame data of size {frameData.Length} bytes.");
                sendChannel.Send(frameData);
            }
            else
            {
                Debug.LogWarning("DataChannel is not open. Frame data not sent.");
            }
        }

        private void OnSendChannelOpen()
        {
            Debug.Log("Send channel open.");
        }

        private void OnSendChannelClose()
        {
            Debug.Log("Send channel closed.");
        }

        private void OnMessageReceived(byte[] bytes)
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log($"Message received: {message}");
        }

        public void Dispose()
        {
            sendChannel?.Close();
            localConnection?.Close();
        }
    }

    [Serializable]
    public class RTCSessionDescriptionWrapper
    {
        public string type;
        public string sdp;
    }
}
