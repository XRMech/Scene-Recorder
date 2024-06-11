#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

namespace SceneStream
{
    public class EditorVideoRecorder : MonoBehaviour, IVideoRecorder
    {
        public Camera Camera;
        private List<Texture2D> frames = new List<Texture2D>();
        private bool isRecording = false;
        private float captureInterval = 0.033f; // Approx. 30 FPS
        private string outputPath = "";
        string framesDirectory;
        string audioPath;

        private void Awake()
        {
            framesDirectory = Path.Combine(Application.temporaryCachePath, "VideoFrames");
            audioPath = Path.Combine(Application.temporaryCachePath, "captured_audio.wav");
        }

        public void StartRecording(string outputPath)
        {
            this.outputPath = outputPath;
            isRecording = true;
            audioFilePath = Path.Combine(Application.temporaryCachePath, "audio.wav");

            // Set up audio recording
            audioClip = Microphone.Start(null, true, 300, 44100);
            audioSource.clip = audioClip;

         
        }

        public void StopRecording()
        {
            isRecording = false;
            Microphone.End(null); // Stop the microphone

            // Save the audio data to a file
            // SaveAudioClip(audioClip, audioFilePath);

            StartCoroutine(CompileFramesIntoVideo());
        }

       

        public AudioSource audioSource;
        private AudioClip audioClip;
        private string audioFilePath;

        // public void StartRecording(string outputPath)
        // {
        //     this.outputPath = outputPath;
        //     isRecording = true;
        //     audioFilePath = Path.Combine(Application.temporaryCachePath, "audio.wav");
        //
        //     // Set up audio recording
        //     audioClip = Microphone.Start(null, true, 300, 44100);
        //     audioSource.clip = audioClip;
        //
        //     StartCoroutine(CaptureFrames());
        // }




        private IEnumerator CompileFramesIntoVideo()
        {
            // Ensure frames have been captured
            if (frames.Count == 0)
            {
                Debug.LogError("No frames to compile.");
                yield break;
            }

            // Save frames to disk

            Directory.CreateDirectory(framesDirectory);

            List<Task> saveTasks = new List<Task>();
            for (int i = 0; i < frames.Count; i++)
            {
                // Encode to PNG on the main thread
                var bytes = frames[i].EncodeToPNG();
                var framePath = Path.Combine(framesDirectory, $"frame{i:00000}.png");

                // Dispatch the file writing to a background thread
                saveTasks.Add(Task.Run(() => File.WriteAllBytes(framePath, bytes)));

                Destroy(frames[i]); // Destroy the texture after starting the save operation
            }

            frames.Clear(); // Clear the list to release references to the textures

            // Wait for all the frames to be saved
            // Wait for all the frames to be saved
            foreach (var task in saveTasks)
            {
                while (!task.IsCompleted)
                {
                    yield return null;
                }
            }

            // Execute FFmpeg command
            string ffmpegCmd =
                $"-r 30 -f image2 -i \"{framesDirectory}\\frame%05d.png\" -i \"{audioPath}\" -vf \"scale=1814:-2\" -vcodec libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -crf 23 -preset medium -shortest \"{outputPath}\"";

            yield return StartCoroutine(ExecuteFFmpegCommand(ffmpegCmd));

            Debug.Log($"Video compilation completed: {outputPath}");
        }

        private void SaveFrame(Texture2D frame, string path)
        {
            byte[] bytes = frame.EncodeToPNG();
            File.WriteAllBytes(path, bytes); // This must be done in a background thread
        }

        private IEnumerator ExecuteFFmpegCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\ffmpeg\ffmpeg-master-latest-win64-gpl\bin\ffmpeg.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                // Set the priority of the process to BelowNormal to avoid blocking Unity Editor
                process.PriorityClass = ProcessPriorityClass.BelowNormal;

                // Asynchronously wait for the process to exit
                while (!process.HasExited)
                {
                    yield return null;
                }

                // Read the output (or errors)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(output))
                    Debug.Log(output);

                if (!string.IsNullOrEmpty(error))
                    Debug.LogError(error);
                process.Close();
                process.Dispose();
            }
        }
    }


}
#endif