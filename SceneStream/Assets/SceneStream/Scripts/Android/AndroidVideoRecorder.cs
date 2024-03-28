#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using System.Runtime.InteropServices;

// Implementing the interface
public class AndroidVideoRecorder : MonoBehaviour, IVideoRecorder
{
    [DllImport("NameOfYourAndroidLibrary")]
    private static extern void startRecording(string outputPath);

    [DllImport("NameOfYourAndroidLibrary")]
    private static extern void stopRecording();

    public void StartRecording(string outputPath)
    {
        startRecording(outputPath);
    }

    public void StopRecording()
    {
        stopRecording();
    }
}

#endif