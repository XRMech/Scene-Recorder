using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class CameraRecorder : MonoBehaviour
{
    // Define the camera you want to record
    public Camera recordingCamera;
    
    private IVideoRecorder videoRecorder;
    
    // Define the resolution of the recording
    public int videoWidth = 1280;
    public int videoHeight = 720;
    
    // Define the frame rate of the recording
    public int frameRate = 30;

    // Define the audio source to capture sound
    public AudioSource audioSource;

    // Internal variables used for processing
    private bool isRecording = false;
    public string savePath= "";
    private RenderTexture renderTexture;
    private GameObject tempAudioListener;

    // Use this for initialization
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        videoRecorder = new AndroidVideoRecorder();
#elif UNITY_WSA && !UNITY_EDITOR
        // videoRecorder = new HoloLensVideoRecorder(); //set up for other platforms
#endif
        
        // Set the playback framerate (real time will not be influenced)
        Time.captureFramerate = frameRate;

        // Create a RenderTexture to render the camera's view
        renderTexture = new RenderTexture(videoWidth, videoHeight, 24);
        recordingCamera.targetTexture = renderTexture;

        

        // Prepare the audio listener for audio recording
        PrepareAudioListener();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for user input to start/stop recording
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRecording)
            {
                DateTime startTime = DateTime.Now;
                // Prepare the path to save the video file
                savePath = Application.persistentDataPath + $"/SceneStream{startTime.Month}_{startTime.Day}-{startTime.Minute}_{startTime.Minute}.mp4";
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }
    }

    void PrepareAudioListener()
    {
        // If there's an AudioListener already on the camera, use it; otherwise, create a new one
        if (!recordingCamera.GetComponent<AudioListener>())
        {
            tempAudioListener = new GameObject("TempAudioListener");
            tempAudioListener.transform.SetParent(recordingCamera.transform);
            tempAudioListener.AddComponent<AudioListener>();
        }
    }

    public void StartRecording()
    {
        if (videoRecorder != null)
        {
            videoRecorder.StartRecording(savePath);
        }
    }

    public void StopRecording()
    {
        videoRecorder?.StopRecording();
    }

    void OnDestroy()
    {
        // Cleanup the RenderTexture and temporary audio listener
        if (renderTexture != null)
        {
            recordingCamera.targetTexture = null;
            Destroy(renderTexture);
        }
        if (tempAudioListener != null)
        {
            Destroy(tempAudioListener);
        }
    }
}
