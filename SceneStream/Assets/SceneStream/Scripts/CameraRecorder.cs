using System;
using UnityEngine;
using UnityEngine.UI; // Ensure UI namespace is included for Image type

public class CameraRecorder : MonoBehaviour
{
    public Camera recordingCamera;
    private IVideoRecorder videoRecorder;
    public int videoWidth = 1280;
    public int videoHeight = 720;
    public int frameRate = 30;
    public AudioSource audioSource; // Ensure this is assigned if needed for audio recording
    private bool isRecording = false;
    public string savePath = "";
    private RenderTexture renderTexture;
    private GameObject tempAudioListener;
    [SerializeField] private Image buttonFront; // Make sure this is assigned in the inspector

    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            videoRecorder = new AndroidVideoRecorder();
        #elif UNITY_WSA && !UNITY_EDITOR
            // videoRecorder = new HoloLensVideoRecorder(); //set up for other platforms
        #elif UNITY_EDITOR
            videoRecorder = gameObject.AddComponent<EditorVideoRecorder>();
        #endif

        Time.captureFramerate = frameRate;
        renderTexture = new RenderTexture(videoWidth, videoHeight, 24);
        recordingCamera.targetTexture = renderTexture;
        PrepareAudioListener();

        Debug.Log("[CameraRecorder] Initialized and ready.");
    }

    public void ToggleRecording() 
    {
        if (!isRecording)
        {
            DateTime startTime = DateTime.Now;
            savePath = Application.persistentDataPath + $"/SceneStream_{startTime.Month}_{startTime.Day}_{startTime.Hour}_{startTime.Minute}.mp4";
            StartRecording();
            isRecording = true;
            buttonFront.color = Color.green;
            Debug.Log($"[CameraRecorder] Recording started: {savePath}");
        }
        else
        {
            StopRecording();
            isRecording = false;
            buttonFront.color = Color.white;
            Debug.Log("[CameraRecorder] Recording stopped.");
        }
    }

    void PrepareAudioListener()
    {
        if (!recordingCamera.GetComponent<AudioListener>())
        {
            tempAudioListener = new GameObject("TempAudioListener");
            tempAudioListener.transform.SetParent(recordingCamera.transform);
            tempAudioListener.AddComponent<AudioListener>();
            Debug.Log("[CameraRecorder] TempAudioListener added to the recording camera.");
        }
    }

    public void StartRecording()
    {
        if (videoRecorder == null)
        {
            Debug.LogError("[CameraRecorder] VideoRecorder not initialized.");
            return;
        }

        videoRecorder.StartRecording(savePath);
    }

    public void StopRecording()
    {
        if (videoRecorder == null)
        {
            Debug.LogError("[CameraRecorder] VideoRecorder not initialized or recording not started.");
            return;
        }

        videoRecorder.StopRecording();
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
    }
}
