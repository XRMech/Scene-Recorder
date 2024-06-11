using UnityEngine;

namespace SceneStream
{
    public abstract class BaseCamera : MonoBehaviour
    {
        public abstract void StartRecording();
        public abstract void StopRecording();
    }
}