using UnityEngine;

namespace Utils
{
    public class FpsSetter : MonoBehaviour
    {
        public int fpsRate = 30;
        void Awake () {
            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            Application.targetFrameRate = fpsRate;
        }
    }
}