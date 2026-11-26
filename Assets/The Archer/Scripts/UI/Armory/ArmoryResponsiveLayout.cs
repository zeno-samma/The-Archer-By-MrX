using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;

namespace MrX.UI
{
    [DisallowMultipleComponent]
    public sealed class ArmoryResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private Image backdrop;
        [SerializeField] private RectTransform hero;
        [SerializeField] private RectTransform slots;
        [SerializeField] private RectTransform stats;
        [SerializeField] private RectTransform inventory;
        [SerializeField] private RectTransform inventoryViewport;
        [SerializeField] private ArmoryDashboard dashboard;

        [SerializeField] private float headerHeight = 215;
        [SerializeField] private float dockHeight = 232;
        [SerializeField] private float statsHeight = 145;
        [SerializeField] private float floorHeight = 112;
        [SerializeField, Range(0, 1)] private float floorOnSprite = .41f;
        [SerializeField, Range(0, 1)] private float showcaseFraction = .32f;
        [SerializeField] private Vector2 showcaseHeightRange = new Vector2(565, 765);

        private readonly Vector3[] corners = new Vector3[4];
        private RectTransform page;
        private Canvas rootCanvas;
        private Vector2 previousSize;
        private float previousPixelWidth;
        private float previousDpi;
        private bool applied;

        private void OnEnable()
        {
            page = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            rootCanvas = canvas ? canvas.rootCanvas : null;
            applied = false;
        }

        private void LateUpdate()
        {
            if (!backdrop || !hero || !slots || !stats || !inventory || !inventoryViewport || !dashboard)
                return;

            Vector2 size = page.rect.size;
            if (size.x <= 0 || size.y <= 0) return;

            if (!applied || size != previousSize)
            {
                applied = true;
                previousSize = size;
                Apply(size);
            }

            inventoryViewport.GetWorldCorners(corners);
            var camera = rootCanvas && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;
            float pixelWidth = Mathf.Abs(RectTransformUtility.WorldToScreenPoint(camera, corners[2]).x
                - RectTransformUtility.WorldToScreenPoint(camera, corners[0]).x);
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160;
            if (pixelWidth > 0 && (pixelWidth != previousPixelWidth || dpi != previousDpi))
            {
                previousPixelWidth = pixelWidth;
                previousDpi = dpi;
                dashboard.SetColumns(Mathf.FloorToInt(pixelWidth * 160f / (48f * dpi)));
            }
        }

        private void Apply(Vector2 size)
        {
            float showcaseHeight = Mathf.Clamp(size.y * showcaseFraction, showcaseHeightRange.x, showcaseHeightRange.y);
            float statsTop = size.y - showcaseHeight;
            float statsBottom = statsTop - statsHeight;
            float standY = statsTop + floorHeight;

            if (backdrop.sprite)
            {
                float height = size.x * backdrop.sprite.rect.height / backdrop.sprite.rect.width;
                float centerY = Mathf.Clamp(standY + height * (.5f - floorOnSprite),
                    size.y - height * .5f, statsTop + height * .5f);
                ResponsiveFrame.Place(backdrop.rectTransform, new Vector2(size.x * .5f, centerY),
                    new Vector2(size.x, height));
            }

            ResponsiveFrame.Place(hero, new Vector2(size.x * .5f, standY), hero.sizeDelta);
            float slotScale = Mathf.Clamp01((showcaseHeight - headerHeight - 18f) / 370f);
            float slotOrigin = Mathf.Clamp(standY, statsTop + 18f - 10f * slotScale, size.y - headerHeight - 380f * slotScale);
            var slotOffset = new Vector2(0, slotOrigin - standY);
            slots.anchoredPosition = slotOffset;
            slots.localScale = Vector3.one * slotScale;

            ResponsiveFrame.Place(stats, new Vector2(size.x * .5f, statsTop - statsHeight * .5f),
                new Vector2(size.x - 40f, statsHeight));
            float inventoryHeight = Mathf.Max(1, statsBottom - dockHeight - 14f);
            ResponsiveFrame.Place(inventory, new Vector2(size.x * .5f, dockHeight + inventoryHeight * .5f),
                new Vector2(size.x - 24f, inventoryHeight));
            dashboard.MarkDirty();
        }
    }
}
