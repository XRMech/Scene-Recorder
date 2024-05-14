using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // Ensure UI namespace is included for Image type
using System.Runtime.InteropServices;

namespace SceneStream
{
    public class CameraRecorder : MonoBehaviour
    {
        // Native function imports
        [DllImport("ImageEncoder")]
        private static extern void EncodeToJPG(string filePath, IntPtr imageData, int width, int height, int channels, int quality);
        
        public Camera recordingCamera;
        // private IVideoRecorder videoRecorder;
        [SerializeField] private string cameraName;
        public AudioSource audioSource; // Ensure this is assigned if needed for audio recording
        public string savePath = "";
        private RenderTexture renderTexture;
        private GameObject tempAudioListener;
        private string outputPath = "";
        
        private AudioClip audioClip;
        private string audioFilePath;
        private List<Texture2D> frames = new List<Texture2D>();
        
        

        
        private void Awake()
        {
            
        }
        void Start()
        {
// #if UNITY_ANDROID && !UNITY_EDITOR
//             videoRecorder = new AndroidVideoRecorder();
// #elif UNITY_WSA && !UNITY_EDITOR
//             // videoRecorder = new HoloLensVideoRecorder(); //set up for other platforms
// #elif UNITY_EDITOR
//             videoRecorder = gameObject.AddComponent<EditorVideoRecorder>();
// #endif
            SceneStreamManager.Instance.AddCamera(this, cameraName);
            Time.captureFramerate = SceneStreamManager.Instance.frameRate;
            renderTexture = new RenderTexture(SceneStreamManager.Instance.videoWidth,
                SceneStreamManager.Instance.videoHeight,
                24);
            recordingCamera.targetTexture = renderTexture;
            PrepareAudioListener();
            
            Debug.Log("[CameraRecorder] Initialized and ready.");
        }

        private IEnumerator CaptureFrames()
        {
            int i = 0;

            // Initialize the frame object once
            Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            int width = renderTexture.width;
            int height = renderTexture.height;
            int channels = 3; // RGB24 format has 3 channels
            while (SceneStreamManager.Instance.isRecording)
            {
                yield return new WaitForSeconds(SceneStreamManager.Instance.captureInterval);

                // Capture the frame
                RenderTexture.active = renderTexture;
                frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                frame.Apply();
                RenderTexture.active = null; // Back to default

                var framePath = Path.Combine(SceneStreamManager.Instance.framesDirectory,
                    $"{cameraName}frame{i:00000}.jpg");
                i++;

                Debug.Log($"[CameraRecorder] Captured frame {i}, saving to {framePath}");

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(framePath));

                // Get the raw texture data
                byte[] imageData = frame.GetRawTextureData();

                // Pin the byte array and get an IntPtr to it
                GCHandle pinnedArray = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                IntPtr imageDataPtr = pinnedArray.AddrOfPinnedObject();

                // Dispatch the encoding and file writing to a background thread
                Task.Run(() =>
                {
                    try
                    {
                        Debug.Log($"[CameraRecorder] Encoding and saving frame to {framePath}");
                        EncodeToJPG(framePath, imageDataPtr, width, height, channels, 75);
                        Debug.Log($"[CameraRecorder] Successfully saved frame to {framePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CameraRecorder] Error saving frame to {framePath}: {ex}");
                    }
                    finally
                    {
                        // Free the pinned handle
                        pinnedArray.Free();
                    }
                });
            }
        }

        void PrepareAudioListener()
        {
            // if (!recordingCamera.GetComponent<AudioListener>())
            // {
            //     tempAudioListener = new GameObject("TempAudioListener");
            //     tempAudioListener.transform.SetParent(recordingCamera.transform);
            //     tempAudioListener.AddComponent<AudioListener>();
            //     Debug.Log("[CameraRecorder] TempAudioListener added to the recording camera.");
            // }
        }

        public void StartRecording()
        {
            DateTime startTime = DateTime.Now;
            savePath = SceneStreamManager.Instance.SavePath +
                       $"{cameraName}_{startTime.Month}_{startTime.Day}_{startTime.Hour}_{startTime.Minute}.mp4";
                
            this.outputPath = outputPath;

            audioFilePath = Path.Combine(Application.temporaryCachePath, "audio.wav");

            // Set up audio recording
            audioClip = Microphone.Start(null, true, 300, 44100);
            audioSource.clip = audioClip;

            StartCoroutine(CaptureFrames());

            
            Debug.Log($"[CameraRecorder] Recording started: {savePath}");
            
            // videoRecorder.StartRecording(savePath);
        }

        public void StopRecording()
        {

            Microphone.End(null); // Stop the microphone
        }

        void OnDestroy()
        {
            if (renderTexture != null)
            {
                recordingCamera.targetTexture = null;
                Destroy(renderTexture);
                Debug.Log("[CameraRecorder] Cleanup RenderTexture.");
            }

            if (tempAudioListener != null)
            {
                Destroy(tempAudioListener);
                Debug.Log("[CameraRecorder] Cleanup TempAudioListener.");
            }

            SceneStreamManager.Instance.RemoveCamera(cameraName);
        }
    }
}