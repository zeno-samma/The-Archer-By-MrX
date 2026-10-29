using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MrX
{
    /// <summary>
    /// Keeps authored animation endpoints for the lifetime of a view. Capture before
    /// hiding the view; later show/hide cycles must never capture an animated value.
    /// </summary>
    internal sealed class UIAnimationDefaults
    {
        private readonly Dictionary<Transform, Vector3> scales = new Dictionary<Transform, Vector3>();
        private readonly Dictionary<Object, float> alphas = new Dictionary<Object, float>();

        public void Capture(params Transform[] targets)
        {
            foreach (var target in targets)
                if (target != null && !scales.ContainsKey(target)) scales.Add(target, target.localScale);
        }

        public void CaptureAlpha(params Graphic[] targets)
        {
            foreach (var target in targets)
                if (target != null && !alphas.ContainsKey(target)) alphas.Add(target, target.color.a);
        }

        public void CaptureAlpha(params CanvasGroup[] targets)
        {
            foreach (var target in targets)
                if (target != null && !alphas.ContainsKey(target)) alphas.Add(target, target.alpha);
        }

        public Vector3 Scale(Transform target)
        {
            if (target == null) return Vector3.one;
            if (!scales.TryGetValue(target, out var value))
            {
                value = target.localScale;
                scales.Add(target, value);
            }
            return value;
        }

        public Vector3 Scale(Transform target, Vector3 multiplier)
        {
            return Vector3.Scale(Scale(target), multiplier);
        }

        public float Alpha(Graphic target)
        {
            if (target == null) return 0;
            if (!alphas.TryGetValue(target, out var value))
            {
                value = target.color.a;
                alphas.Add(target, value);
            }
            return value;
        }

        public float Alpha(CanvasGroup target)
        {
            if (target == null) return 0;
            if (!alphas.TryGetValue(target, out var value))
            {
                value = target.alpha;
                alphas.Add(target, value);
            }
            return value;
        }
    }
}
