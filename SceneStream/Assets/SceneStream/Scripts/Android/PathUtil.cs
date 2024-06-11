using UnityEngine;

namespace SceneStream.Scripts
{
    public static class PathUtil
    {
        public static string GetAndroidExternalFilesDir(string dirName)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass directoryManager = new AndroidJavaClass("com.xrmech.imageencoder.DirectoryManager"))
                {
                    return directoryManager.CallStatic<string>("getExternalFilesDir", context, dirName);
                }
            }
        }

        public static bool CreateDirectory(string dirPath)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass directoryManager = new AndroidJavaClass("com.xrmech.imageencoder.DirectoryManager"))
                {
                    return directoryManager.CallStatic<bool>("createDirectory", context, dirPath);
                }
            }
        }
    }
}