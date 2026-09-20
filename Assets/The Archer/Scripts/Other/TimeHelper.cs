using UnityEngine;

namespace OctoberStudio
{
    public static class TimeHelper
    {
        public static void SetTimeScale(float timeScale)
        {
            Time.timeScale = Mathf.Clamp01(timeScale);
        }
    }
}