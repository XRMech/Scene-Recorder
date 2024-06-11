using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Runtime.InteropServices;using System.Threading.Tasks;using UnityEngine;using UnityEngine.Rendering;using UnityEngine.Networking;namespace SceneStream{    public class CameraRecorder : BaseCamera    {#if UNITY_EDITOR || UNITY_STANDALONE_WIN        [DllImport("imageencoder", CallingConvention = CallingConvention.Cdecl)]        private static extern void encodeToJPG(string filePath, byte[] imageData, int width, int height, int channels, int quality);        [DllImport("imageencoder", CallingConvention = CallingConvention.Cdecl)]        private static extern void encodeToPNG(string filePath, byte[] imageData, int width, int height, int channels, int stride);        [DllImport("imageencoder", CallingConvention = CallingConvention.Cdecl)]        private static extern void encodeToBMP(string filePath, byte[] imageData, int width, int height, int channels);#elif UNITY_ANDROID && !UNITY_EDITOR        private AndroidJavaObject imageEncoderJavaObject;#elif UNITY_WEBGL        // [DllImport("__Internal")]        // private static extern void UploadImageToS3(byte[] imageData, int length, string fileName);#endif        public Camera recordingCamera;        public string cameraName;        public string savePath = "";        private RenderTexture renderTexture;        private string outputPath = "";        private List<Texture2D> frames = new List<Texture2D>();        void Start()        {            SceneStreamManager.Instance.AddCamera(this, cameraName);            Time.captureFramerate = SceneStreamManager.Instance.frameRate;            renderTexture = new RenderTexture(SceneStreamManager.Instance.videoWidth, SceneStreamManager.Instance.videoHeight, 24);            recordingCamera.targetTexture = renderTexture;            outputPath = Path.Combine(Application.persistentDataPath, "VideoFrames", cameraName);            Directory.CreateDirectory(outputPath);  // Ensure the directory exists            Debug.Log("[CameraRecorder] Initialized and ready.");            outputPath = Path.Combine(SceneStreamManager.Instance.framesDirectory, $"{cameraName}");#if UNITY_ANDROID && !UNITY_EDITOR            AndroidJNI.AttachCurrentThread();            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))            {                var activity = jc.GetStatic<AndroidJavaObject>("currentActivity");                imageEncoderJavaObject = new AndroidJavaObject("com.xrmech.imageencoder.ImageEncoder", activity);            }#endif            Debug.Log($"[CameraRecorder] outputPath: {outputPath}");        }        private IEnumerator CaptureFrames()        {            int i = 0;            Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);            float elapsedTime = 0f;            while (SceneStreamManager.Instance.isRecording)            {                elapsedTime += Time.deltaTime;                if (elapsedTime >= SceneStreamManager.Instance.captureInterval)                {                    elapsedTime = 0f;                    AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGB24, request =>                    {                        if (request.hasError)                        {                            Debug.LogError("GPU readback error detected.");                            return;                        }                        var data = request.GetData<byte>().ToArray();                        frame.LoadRawTextureData(data);                        frame.Apply();                        byte[] jpgBytes = frame.EncodeToJPG(75);                        var framePath = Path.Combine(outputPath, $"frame{i++.ToString("D5")}.jpg");                        Debug.Log($"[CameraRecorder] Captured frame {i}, saving to {framePath}");                        GCHandle pinnedArray = GCHandle.Alloc(jpgBytes, GCHandleType.Pinned);                        Task.Run(() =>                        {                            try                            {#if UNITY_ANDROID && !UNITY_EDITOR                                imageEncoderJavaObject.Call("encodeToJPG", framePath, jpgBytes, renderTexture.width, renderTexture.height, 3, 75);#elif UNITY_WEBGL                                // UploadImageToS3(jpgBytes, jpgBytes.Length, $"frame{i++.ToString("D5")}.jpg");#else                                encodeToJPG(framePath, jpgBytes, renderTexture.width, renderTexture.height, 3, 75);#endif                                StartCoroutine(UploadImageToServer(jpgBytes, framePath));                            }                            catch (Exception ex)                            {                                Debug.LogError($"[CameraRecorder] Error saving frame to {framePath}: {ex}");                            }                            finally                            {                                pinnedArray.Free();                            }                        });                    });                }                yield return null; // Wait for the next frame            }        }        private IEnumerator UploadImageToServer(byte[] jpgBytes, string framePath)        {            using (UnityWebRequest www = new UnityWebRequest("http://your-server-address/upload-image", "POST"))            {                www.uploadHandler = new UploadHandlerRaw(jpgBytes);                www.downloadHandler = new DownloadHandlerBuffer();                www.SetRequestHeader("Content-Type", "application/octet-stream");                www.SetRequestHeader("Filename", Path.GetFileName(framePath));                www.SetRequestHeader("Event", "Sample Event"); // You can customize this as needed                Debug.Log($"[CameraRecorder] Uploading frame {Path.GetFileName(framePath)} to server...");                yield return www.SendWebRequest();                if (www.result == UnityWebRequest.Result.Success)                {                    Debug.Log($"[CameraRecorder] Successfully uploaded frame {Path.GetFileName(framePath)}");                }                else                {                    Debug.LogError($"[CameraRecorder] Error uploading frame: {www.error}");                }            }        }        public override void StartRecording()        {            DateTime startTime = DateTime.Now;            savePath = SceneStreamManager.Instance.SavePath +                       $"{cameraName}_{startTime.Month}_{startTime.Day}_{startTime.Hour}_{startTime.Minute}.mp4";            outputPath = Path.Combine(SceneStreamManager.Instance.framesDirectory, cameraName);            Debug.Log($"[CameraRecorder] StartRecording outputPath: {outputPath}");            StartCoroutine(CaptureFrames());            Debug.Log($"[CameraRecorder] Recording started: {savePath}");        }        public override void StopRecording()        {            // Implementation for stopping recording, if needed        }        void OnDestroy()        {            if (renderTexture != null)            {                recordingCamera.targetTexture = null;                Destroy(renderTexture);                Debug.Log("[CameraRecorder] Cleanup RenderTexture.");            }            SceneStreamManager.Instance.RemoveCamera(cameraName);        }    }}