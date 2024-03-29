using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class FFmpegHelper
{
    public static void ExecuteCommand(string command, string workingDirectory = null)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = GetFFmpegPath(),
            Arguments = command,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Application.persistentDataPath
        };

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
            {
                Debug.Log(output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    private static string GetFFmpegPath()
    {
        // Adjust the path based on the platform and where you've placed FFmpeg in your project
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return Application.dataPath + "/Plugins/FFmpeg/ffmpeg.exe";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        return Application.dataPath + "/Plugins/FFmpeg/ffmpeg";
// Add more platform directives as needed
#else
        return "ffmpeg"; // Assumes FFmpeg is installed and in the system's PATH
#endif
    }
}
