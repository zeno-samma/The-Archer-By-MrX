using System.Collections.Generic;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MrX.UI
{
    public class SafeAreaManager : MonoBehaviour
    {
        [SerializeField] List<RectTransform> screens = new List<RectTransform>();
        private Rect previousSafeArea;
        private Vector2 previousSize;

        private void Update()
        {
            if (previousSafeArea != Screen.safeArea || previousSize != new Vector2(Screen.width,Screen.height)) ApplySafeArea();
        }

        public void Awake()
        {
            ApplySafeArea();
        }

        internal static Rect Normalize(Rect area, Vector2Int pixels)
        {
            var normalized = Rect.MinMaxRect(
                Mathf.Clamp01(area.xMin / pixels.x), Mathf.Clamp01(area.yMin / pixels.y),
                Mathf.Clamp01(area.xMax / pixels.x), Mathf.Clamp01(area.yMax / pixels.y));
            return normalized.width > 0 && normalized.height > 0 ? normalized : new Rect(0, 0, 1, 1);
        }

        private void ApplySafeArea()
        {
            var rect = Screen.safeArea;

            previousSafeArea = rect;
            previousSize = new Vector2(Screen.width,Screen.height);

            if (Screen.width > 0 && Screen.height > 0)
            {
                var normalized = Normalize(rect, new Vector2Int(Screen.width, Screen.height));
                foreach (var screen in screens)
                {
                    if (!screen) continue;
                    screen.anchorMin = normalized.min;
                    screen.anchorMax = normalized.max;
                }
            }
        }
    }
}
