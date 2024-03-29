#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public class EditorVideoRecorder : MonoBehaviour, IVideoRecorder
{
    private List<Texture2D> frames = new List<Texture2D>();
    private bool isRecording = false;
    private float captureInterval = 0.033f; // Approx. 30 FPS
    private string outputPath = "";

    public void StartRecording(string outputPath)
    {
        this.outputPath = outputPath;
        isRecording = true;
        StartCoroutine(CaptureFrames());
    }

    public void StopRecording()
    {
        isRecording = false;
        CompileFramesIntoVideo();
        frames.Clear();
    }

    private IEnumerator CaptureFrames()
    {
        while (isRecording)
        {
            yield return new WaitForSeconds(captureInterval);
            frames.Add(ScreenCapture.CaptureScreenshotAsTexture());
        }
    }

    private void CompileFramesIntoVideo()
    {
        // Ensure frames have been captured
        if (frames.Count == 0)
        {
            Debug.LogError("No frames to compile.");
            return;
        }
    
        // Save frames to disk
        string framesDirectory = Path.Combine(Application.temporaryCachePath, "VideoFrames");
        Directory.CreateDirectory(framesDirectory);
    
        for (int i = 0; i < frames.Count; i++)
        {
            byte[] bytes = frames[i].EncodeToPNG();
            string framePath = Path.Combine(framesDirectory, $"frame{i:0000}.png");
            File.WriteAllBytes(framePath, bytes);
        }

        // Construct the FFmpeg command
        string ffmpegCmd = $"-r 30 -i {framesDirectory}/frame%04d.png -vcodec libx264 -crf 25 -pix_fmt yuv420p {outputPath}";
        ExecuteFFmpegCommand(ffmpegCmd);

        Debug.Log($"Compiling frames into a video file at {outputPath}");
    }

    private void ExecuteFFmpegCommand(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = command,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();

            // Read the output (or errors)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
                Debug.Log(output);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);
        }
    }

}
#endif