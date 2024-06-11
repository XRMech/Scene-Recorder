using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SceneStream.Scripts;

namespace SceneStream
{
    public class SceneStreamManager : MonoBehaviour
    {
        public static SceneStreamManager Instance;
        public int videoWidth = 1280 / 2;
        public int videoHeight = 720 / 2;
        public int frameRate = 10;
        public float captureInterval;
        public bool isRecording = false;
        public string SavePath;
        internal string framesDirectory;
        string audioPath;
        public string serverUrl = "http://localhost:3000/";
        public Dictionary<string, BaseCamera> cameraList = new Dictionary<string, BaseCamera>();
        [SerializeField] private Image buttonFront;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            if (string.IsNullOrEmpty(SavePath))
                SavePath = Path.Combine(Application.persistentDataPath, "SceneStream");
            
            captureInterval = 1 / frameRate;
            framesDirectory = Path.Combine(SavePath, "VideoFrames");
            audioPath = Path.Combine(SavePath, "captured_audio.wav");
        }

        public void AddCamera(BaseCamera newCamera, string name)
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
            var buttonFrontColors = buttonFront.color;
            buttonFrontColors = Color.green;
            foreach (KeyValuePair<string, BaseCamera> cameraPair in cameraList)
            {
                cameraPair.Value.StartRecording();
            }
        }

        public void StopRecording()
        {
            isRecording = false;
            var buttonFrontColors = buttonFront.color;
            buttonFrontColors = Color.gray;
            foreach (KeyValuePair<string, BaseCamera> cameraPair in cameraList)
            {
                cameraPair.Value.StopRecording();
            }
        }
    }
}
