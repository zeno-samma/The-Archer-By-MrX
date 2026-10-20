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

        private void ApplySafeArea()
        {
            var rect = Screen.safeArea;

            previousSafeArea = rect;
            previousSize = new Vector2(Screen.width,Screen.height);

            if (Screen.width > 0 && Screen.height > 0)
            {
                var anchorMin = rect.position;
                var anchorMax = rect.position + rect.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    for (int i = 0; i < screens.Count; i++)
                    {
                        if (screens[i] != null)
                        {
                            screens[i].anchorMin = anchorMin;
                            screens[i].anchorMax = anchorMax;
                        }
                        else
                        {
                            screens.RemoveAt(i);

                            i--;
                        }
                    }
                }
            }
        }
    }
}
