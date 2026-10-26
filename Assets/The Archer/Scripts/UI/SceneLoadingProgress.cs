using System;
using UnityEngine;

namespace MrX.UI
{
    /// <summary>Progress of the current scene transition; values come from actual loading operations.</summary>
    public static class SceneLoadingProgress
    {
        public static float Value { get; private set; }
        public static bool IsLoading { get; private set; }
        public static event Action<float> Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Value=0; IsLoading=false; Changed=null; }
        public static void Begin() { Value=0; IsLoading=true; Changed?.Invoke(Value); }
        public static void Report(float value)
        {
            if(!IsLoading)return;
            value=Mathf.Clamp01(value);
            if(value<=Value)return;
            Value=value;Changed?.Invoke(Value);
        }
        public static void End() { IsLoading=false; }
    }
}
