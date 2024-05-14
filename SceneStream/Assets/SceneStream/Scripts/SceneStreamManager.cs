using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SceneStream
{
    public class SceneStreamManager : MonoBehaviour
    {
        public static SceneStreamManager Instance;
        public int videoWidth = 1280/2;
        public int videoHeight = 720/2;
        public int frameRate = 10;
        public float captureInterval; // Approx. 30 FPS
        public bool isRecording = false;
        public string SavePath;
        internal string framesDirectory;
        string audioPath;
        public Dictionary<string, CameraRecorder> cameraList = new Dictionary<string, CameraRecorder>();
        [SerializeField] private Button buttonFront; // Make sure this is assigned in the inspector

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            if(SavePath==null)
                SavePath= Application.persistentDataPath + "/SceneStream/";
            captureInterval = 1 / frameRate;
            framesDirectory = Path.Combine(SceneStreamManager.Instance.SavePath, "VideoFrames");
            audioPath = Path.Combine(SceneStreamManager.Instance.SavePath, "captured_audio.wav");
        }

        public void AddCamera(CameraRecorder newCamera, string name)
        {
            cameraList.TryAdd(name, newCamera);
        }

        public void RemoveCamera(string name)
        {
            cameraList.Remove(name);
        }
        public void ToggleRecording()
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }
        public void StartRecording()
        {
            isRecording = true;
            var buttonFrontColors = buttonFront.colors;
            buttonFrontColors.normalColor = Color.green;
            foreach (KeyValuePair<string, CameraRecorder> cameraPair in cameraList)
            {
                cameraPair.Value.StartRecording();
            }
        }

        public void StopRecording()
        {
            isRecording = false;
            var buttonFrontColors = buttonFront.colors;
            buttonFrontColors.normalColor = Color.gray;
            foreach (KeyValuePair<string, CameraRecorder> cameraPair in cameraList)
            {
                cameraPair.Value.StopRecording();
            }
        }
    }
}