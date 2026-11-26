using System;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MrX.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [DefaultExecutionOrder(-90)]
    public sealed class ResponsiveFrame : MonoBehaviour
    {
        [Serializable]
        private struct Region
        {
            public RectTransform frame;
            [Min(0)] public float designWidth;
            [Min(0)] public float minimumHeight;
            public RectTransform fullScreenBackdrop;
        }

        [Serializable]
        private struct VerticalPin
        {
            public RectTransform target;
            public RectTransform frame;
            [Range(0, 1)] public float heightFraction;
            public float minimum;
            public float maximum;
            public float offset;
        }

        [SerializeField] private Region[] regions = Array.Empty<Region>();
        [SerializeField] private VerticalPin[] pins = Array.Empty<VerticalPin>();

        private RectTransform canvasRect;
        private Rect previousSafeArea;
        private Vector2Int previousPixels;
        private Vector2 previousCanvasSize;
        private bool applied;

        private void OnEnable()
        {
            canvasRect = (RectTransform)transform;
            applied = false;
            Refresh();
        }

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            var pixels = new Vector2Int(Screen.width, Screen.height);
            if (pixels.x <= 0 || pixels.y <= 0) return;

            var size = canvasRect.rect.size;
            if (size.x <= 0 || size.y <= 0) return;

            var safeArea = Screen.safeArea;
            if (applied && pixels == previousPixels && safeArea == previousSafeArea && size == previousCanvasSize) return;

            applied = true;
            previousPixels = pixels;
            previousSafeArea = safeArea;
            previousCanvasSize = size;

            var normalized = SafeAreaManager.Normalize(safeArea, pixels);
            var safe = new Rect(Vector2.Scale(normalized.position, size), Vector2.Scale(normalized.size, size));
            foreach (var region in regions)
            {
                if (!region.frame) continue;
                float scale = region.designWidth > 0 && region.minimumHeight > 0
                    ? Mathf.Min(safe.width / region.designWidth, safe.height / region.minimumHeight) : 1;
                Vector2 visibleSize = region.designWidth > 0 && region.minimumHeight > 0
                    ? new Vector2(region.designWidth * scale, safe.height) : safe.size;
                Place(region.frame, safe.center, visibleSize / scale);
                region.frame.localScale = Vector3.one * scale;

                if (region.fullScreenBackdrop)
                {
                    Vector2 regionOrigin = safe.center - visibleSize * .5f;
                    Place(region.fullScreenBackdrop, (size * .5f - regionOrigin) / scale, size / scale);
                }
            }

            foreach (var pin in pins)
            {
                if (!pin.frame || !pin.target) continue;
                var frameRect = pin.frame.rect;
                float height = Mathf.Clamp(frameRect.height * pin.heightFraction, pin.minimum, pin.maximum) + pin.offset;
                Vector3 world = pin.frame.TransformPoint(new Vector3(frameRect.center.x, frameRect.yMin + height, 0));
                var position = pin.target.localPosition;
                position.y = pin.target.parent.InverseTransformPoint(world).y;
                pin.target.localPosition = position;
            }
        }

        // Coordinates are measured from the parent's lower-left corner.
        internal static void Place(RectTransform target, Vector2 center, Vector2 size)
        {
            target.anchorMin = target.anchorMax = Vector2.zero;
            target.pivot = Vector2.one * .5f;
            target.sizeDelta = size;
            target.anchoredPosition = center;
        }
    }
}
