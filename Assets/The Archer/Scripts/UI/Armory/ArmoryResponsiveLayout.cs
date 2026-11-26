using UnityEngine;
using UnityEngine.UI;
using MrX.UI.Armory;
using Screen = UnityEngine.Device.Screen;

namespace MrX.UI
{
    [DisallowMultipleComponent]
    public sealed class ArmoryResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private Image backdrop;
        [SerializeField] private RectTransform hero;
        [SerializeField] private RectTransform stats;
        [SerializeField] private RectTransform inventory;

        // Compatibility for older prefab and scene instances.
        [SerializeField, HideInInspector] private RectTransform slots;
        [SerializeField, HideInInspector] private RectTransform inventoryViewport;
        [SerializeField, HideInInspector] private ArmoryDashboard dashboard;

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
        private bool childReferencesResolved;
        private RectTransform resolvedHero;
        private RectTransform resolvedInventory;
        private RectTransform resolvedSlots;
        private ScrollRect resolvedInventoryScroll;
        private ArmoryDashboard resolvedDashboard;

        public RectTransform SlotGroup
        {
            get { ResolveChildReferences(); return resolvedSlots; }
        }

        public ScrollRect InventoryScroll
        {
            get { ResolveChildReferences(); return resolvedInventoryScroll; }
        }

        private void ResolveChildReferences()
        {
            if (childReferencesResolved && resolvedHero == hero && resolvedInventory == inventory) return;
            childReferencesResolved = true;
            resolvedHero = hero;
            resolvedInventory = inventory;

            resolvedDashboard = GetComponent<ArmoryDashboard>();
            if (!resolvedDashboard) resolvedDashboard = dashboard;

            var heroPanel = hero ? hero.GetComponentInChildren<ArmoryHeroPanel>(true) : null;
            resolvedSlots = heroPanel ? heroPanel.SlotGroup : null;
            if (!resolvedSlots || resolvedSlots == hero || !resolvedSlots.IsChildOf(hero))
                resolvedSlots = slots;

            resolvedInventoryScroll = resolvedDashboard ? resolvedDashboard.InventoryScroll : null;
            if (inventory && (!resolvedInventoryScroll || !resolvedInventoryScroll.transform.IsChildOf(inventory)))
            {
                resolvedInventoryScroll = inventoryViewport && inventoryViewport.IsChildOf(inventory)
                    ? inventoryViewport.GetComponent<ScrollRect>() : null;
                if (!resolvedInventoryScroll)
                    resolvedInventoryScroll = inventory.GetComponentInChildren<ScrollRect>(true);
            }
        }

        private void OnValidate()
        {
            applied = false;
        }

        private void OnEnable()
        {
            page = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            rootCanvas = canvas ? canvas.rootCanvas : null;
            childReferencesResolved = false;
            applied = false;
        }

        private void LateUpdate()
        {
            var scroll = InventoryScroll;
            var viewport = scroll && scroll.viewport ? scroll.viewport : inventoryViewport;
            if (!backdrop || !hero || !SlotGroup || !stats || !inventory || !viewport || !resolvedDashboard)
                return;

            Vector2 size = page.rect.size;
            if (size.x <= 0 || size.y <= 0) return;

            if (!applied || size != previousSize)
            {
                applied = true;
                previousSize = size;
                ApplyLayout(size);
                resolvedDashboard.MarkDirty();
            }

            viewport.GetWorldCorners(corners);
            var camera = rootCanvas && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;
            float pixelWidth = Mathf.Abs(RectTransformUtility.WorldToScreenPoint(camera, corners[2]).x
                - RectTransformUtility.WorldToScreenPoint(camera, corners[0]).x);
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160;
            if (pixelWidth > 0 && (pixelWidth != previousPixelWidth || dpi != previousDpi))
            {
                previousPixelWidth = pixelWidth;
                previousDpi = dpi;
                resolvedDashboard.SetColumns(Mathf.FloorToInt(pixelWidth * 160f / (48f * dpi)));
            }
        }

        public void ApplyLayout(Vector2 size)
        {
            var slotGroup = SlotGroup;
            if (!backdrop || !hero || !slotGroup || !stats || !inventory || size.x <= 0 || size.y <= 0) return;

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
            slotGroup.anchoredPosition = slotOffset;
            slotGroup.localScale = Vector3.one * slotScale;

            ResponsiveFrame.Place(stats, new Vector2(size.x * .5f, statsTop - statsHeight * .5f),
                new Vector2(size.x - 40f, statsHeight));
            float inventoryHeight = Mathf.Max(1, statsBottom - dockHeight - 14f);
            ResponsiveFrame.Place(inventory, new Vector2(size.x * .5f, dockHeight + inventoryHeight * .5f),
                new Vector2(size.x - 24f, inventoryHeight));
        }
    }
}
