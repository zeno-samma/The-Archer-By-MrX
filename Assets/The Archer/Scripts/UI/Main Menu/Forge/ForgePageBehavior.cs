using System;
using System.Collections.Generic;
using MrX.Armory;
using MrX.Crafting;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class ForgePageBehavior : MainMenuPageBehavior
    {
        [SerializeField] private ForgePageDatabase database;
        [SerializeField] private Image showcaseIcon, detailIcon;
        [SerializeField] private TMP_Text itemName, levelLabel, enhancementLabel, selectionLabel, goldLabel, costLabel, statusLabel, resultLabel, forgeLabel, feedback;
        [SerializeField] private Button previousButton, nextButton, forgeButton, settingsButton;
        [SerializeField] private Image forgeImage;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private List<StatRow> rows = new List<StatRow>();
        [SerializeField] private List<TextStyle> typography = new List<TextStyle>();
        [SerializeField] private List<MaterialView> materialViews = new List<MaterialView>();
        private readonly List<ItemData> owned = new List<ItemData>(16);
        private readonly List<StatType> visibleStats = new List<StatType>(7);
        private ArmoryManager armory;
        private EquipmentCrafting service;
        private CraftingManager craftingManager;
        private CurrencySave gold;
        private string selectedId;
        private int selectedIndex;
        private bool dirty = true, layoutDirty = true;
        private float textScale = -1, nextForgeTime;
        private static readonly Color Ink = new Color32(37, 60, 137, 255);
        private static readonly Color Muted = new Color32(110, 137, 183, 255);
        private static readonly Color Green = new Color32(46, 165, 82, 255);
        private static readonly Color Red = new Color32(211, 86, 91, 255);
        private static readonly Color Pastel = new Color(.73f, .83f, .95f);
        [Serializable] private class TextStyle { public TMP_Text text; public float size; }
        [Serializable] private class StatRow { public RectTransform rect; public TMP_Text title, before, after, arrow; }
        [Serializable] private class MaterialView { public CraftingMaterial material; public TMP_Text count, title; public Image icon; }

        protected override void Start()
        {
            base.Start(); armory = GameController.ArmoryManager;
            gold = GameController.CurrenciesManager.GetCurrency("gold", false);
            if (gold != null) gold.OnAmountChanged += OnWalletChanged;
            previousButton.onClick.AddListener(PreviousItem); nextButton.onClick.AddListener(NextItem);
            forgeButton.onClick.AddListener(Forge); settingsButton.onClick.AddListener(OpenSettings);
            enabled = canvas.enabled;
        }
        private void OnWalletChanged(CurrencySave _) { dirty = true; }
        private void OnCraftingChanged() { dirty = true; }
        private void OnRectTransformDimensionsChange() { layoutDirty = true; }
        private void LateUpdate()
        {
            if (!canvas.enabled || !canvasGroup.blocksRaycasts || armory == null) return;
            if (service == null)
            {
                var manager = armory.Crafting; if (manager == null || manager.Service == null) return;
                if (craftingManager != manager)
                {
                    if (craftingManager != null) craftingManager.Changed -= OnCraftingChanged;
                    craftingManager = manager; craftingManager.Changed += OnCraftingChanged;
                }
                service = manager.Service; RebuildOwned();
            }
            if (layoutDirty) { layoutDirty = false; }
            if (dirty) { Refresh(); dirty = false; }
            // Only a short click debounce is timed; the idle page does no work.
            if (nextForgeTime > 0 && Time.unscaledTime >= nextForgeTime)
            { nextForgeTime = 0; dirty = true; }
        }
        public override void MoveCenter()
        {
            enabled = true; canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
            service = null; dirty = layoutDirty = true; feedback.text = "Collect fragments and materials to craft new equipment.";
            base.MoveCenter();
        }
        public override void MoveLeft() { DisableInteraction(); base.MoveLeft(); }
        public override void MoveRight() { DisableInteraction(); base.MoveRight(); }
        private void DisableInteraction() { canvasGroup.interactable = canvasGroup.blocksRaycasts = false; }
        public override void OnMoveFinished() { base.OnMoveFinished(); DisableInteraction(); enabled = false; }
        protected override void OnMoveCenterFinished()
        {
            if (GameController.InputManager.ActiveInput != Input.InputType.UIJoystick && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(forgeButton.interactable ? forgeButton.gameObject : nextButton.interactable ? nextButton.gameObject : settingsButton.gameObject);
        }

        private void RebuildOwned()
        {
            owned.Clear();
            foreach (var item in service.Recipes) owned.Add(item);
            selectedIndex = 0;
            if (selectedId == null)
            { foreach (var item in owned) if (!service.IsOwned(item.Id)) { selectedId = item.Id; break; } }
            for (var i = 0; i < owned.Count; i++) if (owned[i].Id == selectedId) { selectedIndex = i; break; }
            BindSelected();
        }
        private void BindSelected()
        {
            selectedId = owned.Count > 0 ? owned[selectedIndex].Id : null;
            dirty = true;
        }
        private void PreviousItem() { ChangeItem(-1); }
        private void NextItem() { ChangeItem(1); }
        private void ChangeItem(int direction)
        {
            if (owned.Count < 2 || !CanInteract()) return;
            selectedIndex = (selectedIndex + direction + owned.Count) % owned.Count; BindSelected();
            feedback.text = "Each equipment recipe uses its own fragments.";
            GameController.AudioManager.PlayButtonClick();
        }
        private bool CanInteract() => canvas.enabled && canvasGroup.blocksRaycasts && GameController.MainMenuScreenBehavior.IsDockEnabled;

        private void Refresh()
        {
            var quote = service.GetQuote(selectedId);
            goldLabel.text = gold != null ? gold.Amount.ToString("N0") : "0";
            var valid = quote.output != null;
            var isOwned = quote.result == CraftResult.AlreadyOwned;
            itemName.text = quote.item != null ? quote.item.ItemName : "No crafting recipes";
            showcaseIcon.sprite = detailIcon.sprite = quote.item != null ? quote.item.Icon : database.forgeIcon;
            levelLabel.text = isOwned ? "OWNED - Upgrade in Stage / Armory" : valid ? "CRAFT NEW EQUIPMENT - Lv.1" : "Recipe unavailable";
            enhancementLabel.text = isOwned ? "OWNED" : "NEW";
            selectionLabel.text = owned.Count > 0 ? (selectedIndex + 1) + " / " + owned.Count + " EQUIPMENT RECIPES" : "Configure recipes in ItemData";
            previousButton.interactable = nextButton.interactable = owned.Count > 1;
            costLabel.text = quote.item != null ? "FRAGMENTS: " + quote.fragments + " / " + quote.requiredFragments : "No recipe selected";
            costLabel.color = quote.fragments < quote.requiredFragments ? Red : Green;
            foreach (var view in materialViews)
            {
                var required = quote.item != null ? quote.item.Crafting.Cost(view.material) : 0;
                var amount = service.Inventory.Count(CraftingInventorySave.MaterialKey(view.material));
                view.count.text = amount + " / " + required; view.count.color = amount >= required ? Green : Red;
            }
            var ready = quote.result == CraftResult.Ready;
            forgeButton.interactable = ready && nextForgeTime == 0;
            forgeImage.sprite = forgeButton.interactable ? database.forgeButtonSprite : database.disabledButtonSprite;
            forgeLabel.text = isOwned ? "OWNED" : "FORGE";
            statusLabel.text = ready ? "Success Rate: 100%" : isOwned ? "Upgrade levels in Stage / Armory" : quote.result == CraftResult.InventoryFull ? "Equipment bag full - expand in Armory" : quote.result == CraftResult.MissingFragments ? "Collect this equipment's fragments" : quote.result == CraftResult.MissingMaterials ? "Not enough crafting materials" : "Recipe unavailable";
            statusLabel.color = ready || isOwned ? Green : Red;
            resultLabel.text = isOwned ? "Already in your Armory" : "Unlock equipment - Lv.1";
            RefreshStats(quote);
            var target = forgeButton.interactable ? forgeButton : settingsButton;
            previousButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = nextButton, selectOnUp = settingsButton, selectOnDown = target };
            nextButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = previousButton, selectOnRight = target, selectOnDown = target };
            forgeButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = nextButton.interactable ? nextButton : settingsButton, selectOnUp = settingsButton };
            settingsButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = forgeButton.interactable ? forgeButton : nextButton.interactable ? nextButton : settingsButton };
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == forgeButton.gameObject && !forgeButton.interactable) OnMoveCenterFinished();
        }
        private void RefreshStats(CraftQuote quote)
        {
            visibleStats.Clear();
            if (quote.output != null)
            {
                if (quote.item.ItemType == ItemType.Weapon)
                { visibleStats.Add(StatType.Damage); visibleStats.Add(StatType.CriticalChance); visibleStats.Add(StatType.AttackSpeed); }
                foreach (var stat in quote.output.Stats) if (stat != null && stat.IsVisibleOnUI && !visibleStats.Contains(stat.StatType)) visibleStats.Add(stat.StatType);
            }
            var count = Mathf.Max(3, visibleStats.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i]; row.rect.gameObject.SetActive(i < visibleStats.Count);
                if (i >= visibleStats.Count) continue;
                row.rect.anchorMin = new Vector2(.04f, 1 - (i + 1f) / count); row.rect.anchorMax = new Vector2(.96f, 1 - i / (float)count);
                var stat = visibleStats[i]; var value = quote.output.GetStatValue(stat);
                row.title.text = StatName(stat); row.before.text = ""; row.after.text = StatValue(stat, value);
                row.after.color = Ink; row.arrow.text = "";
            }
        }
        private static string StatName(StatType stat)
        {
            switch (stat) { case StatType.Damage: return "ATK"; case StatType.HP: return "HP"; case StatType.CriticalChance: return "CRIT Rate";
                case StatType.AttackSpeed: return "ATK Speed"; case StatType.MovementSpeed: return "Move Speed"; case StatType.HPRecovery: return "HP Recovery"; default: return "Damage Taken"; }
        }
        private static string StatValue(StatType stat, float value)
        { return stat == StatType.CriticalChance ? value.ToString("0.#") + "%" : stat == StatType.Damage || stat == StatType.HP || stat == StatType.HPRecovery ? value.ToString("0.##") : value.ToString("0.00") + "x"; }

        private void Forge()
        {
            if (!CanInteract() || nextForgeTime > 0 || service == null) return;
            var result = service.TryCraft(selectedId);
            if (result == CraftResult.Success)
            {
                nextForgeTime = Time.unscaledTime + .35f; forgeButton.interactable = false;
                feedback.text = "Crafted! Your new equipment is available in Armory.";
                GameController.AudioManager.PlayButtonClick();
            }
            else feedback.text = result == CraftResult.AlreadyOwned ? "Already owned. Upgrade this equipment in Stage / Armory." : "Crafting not completed. Check fragments and materials.";
            dirty = true;
        }
        private void ResizeText()
        {
            var scale = Mathf.Max(.8f, pageRect.rect.width / 1672f);
            if (Mathf.Abs(textScale - scale) < .01f) return; textScale = scale;
            foreach (var style in typography) { style.text.fontSize = style.text.fontSizeMax = style.size * scale; style.text.fontSizeMin = 10 * scale; }
        }
        private void OpenSettings()
        {
            if (!CanInteract()) return;
            var screen = GameController.MainMenuScreenBehavior; screen.DisableDock();
            screen.SettingsPopup.onPopupClosed += SettingsClosed; screen.SettingsPopup.Show(); GameController.AudioManager.PlayButtonClick();
        }
        private void SettingsClosed()
        { var screen = GameController.MainMenuScreenBehavior; screen.SettingsPopup.onPopupClosed -= SettingsClosed; screen.EnableDock(); OnMoveCenterFinished(); }
        private void OnDestroy()
        {
            if (craftingManager != null) craftingManager.Changed -= OnCraftingChanged;
            if (gold != null) gold.OnAmountChanged -= OnWalletChanged;
            moveEasingCoroutine?.Stop(); alphaEasingCoroutine?.Stop();
            var screen = GameController.MainMenuScreenBehavior; if (screen != null && screen.SettingsPopup != null) screen.SettingsPopup.onPopupClosed -= SettingsClosed;
        }

#if UNITY_EDITOR
        // Editor authoring only. Saved prefab instances provide all runtime UI.
        public static ForgePageBehavior Create(Transform parent, ForgePageDatabase data)
        {
            var root = Rect("Forge Page", parent, 0, 0, 1, 1); var page = root.gameObject.AddComponent<ForgePageBehavior>();
            page.database = data; page.pageType = MainMenuPageType.Forge; page.pageRect = root;
            page.canvas = root.gameObject.AddComponent<Canvas>(); root.gameObject.AddComponent<GraphicRaycaster>(); page.canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            page.Build(); page.canvasGroup.alpha = 0; page.canvas.enabled = false; page.DisableInteraction(); return page;
        }
        private void Build()
        {
            var top = Rect("Top Bar", pageRect, .055f, .89f, .945f, .985f);
            Text("Page Label", top, "EQUIPMENT FORGE", 27, Ink, 0, .1f, .6f, .9f);
            var walletPanel = Panel("Gold Wallet", top, Color.white, .72f, .14f, .9f, .86f);
            Icon("Coin", walletPanel, database.coinIcon, .04f, .15f, .28f, .85f); goldLabel = Text("Gold Amount", walletPanel, "0", 22, Ink, .32f, .1f, .96f, .9f);
            settingsButton = Button("Settings", top, database.disabledButtonSprite, "", .92f, .1f, 1, .9f, out _);
            Icon("Settings Icon", settingsButton.transform, database.settingsIcon, .2f, .2f, .8f, .8f);

            var aura = Icon("Forge Aura", pageRect, database.circleSprite, .21f, .41f, .53f, .88f); aura.color = new Color(.67f, .88f, 1, .2f);
            var showcase = Panel("Equipment Showcase", pageRect, new Color(.50f, .68f, .90f), .325f, .635f, .43f, .815f);
            showcaseIcon = Icon("Equipment Icon", showcase, database.forgeIcon, .07f, .07f, .93f, .93f);
            var badge = Panel("Enhancement Badge", showcase, new Color(.4f, .6f, .9f), .58f, -.06f, 1.08f, .23f);
            enhancementLabel = Text("Enhancement", badge, "+0", 26, Color.white, .05f, .04f, .95f, .96f, TextAlignmentOptions.Center);
            previousButton = Button("Previous Equipment", pageRect, database.disabledButtonSprite, "<", .215f, .69f, .265f, .77f, out _);
            nextButton = Button("Next Equipment", pageRect, database.disabledButtonSprite, ">", .49f, .69f, .54f, .77f, out _);
            selectionLabel = Text("Equipment Selection", pageRect, "EQUIPMENT RECIPES", 16, Ink, .20f, .60f, .54f, .635f, TextAlignmentOptions.Center);
            costLabel = Text("Fragment Progress", pageRect, "FRAGMENTS: 0 / 30", 19, Ink, .20f, .557f, .54f, .60f, TextAlignmentOptions.Center);
            var anvil = Rect("Anvil Illustration", pageRect, .215f, .385f, .535f, .565f);
            if (database.anvilIllustration != null) Icon("Anvil Art", anvil, database.anvilIllustration, 0, 0, 1, 1);
            else
            {
                var steel = new Color(.48f, .61f, .80f);
                Panel("Anvil Body", anvil, steel, .25f, .12f, .75f, .75f);
                Panel("Anvil Base", anvil, steel, .12f, 0, .88f, .23f);
                Panel("Anvil Face", anvil, new Color(.61f, .73f, .91f), .04f, .68f, .96f, .96f);
                Panel("Left Horn", anvil, steel, 0, .66f, .20f, .9f);
                Panel("Right Horn", anvil, steel, .80f, .66f, 1, .9f);
                Icon("Forge Emblem", anvil, database.forgeIcon, .39f, .25f, .61f, .68f);
            }
            var cost = Panel("Crafting Materials - ui_slot sliced", pageRect, new Color(.85f, .92f, 1), .18f, .15f, .545f, .37f);
            Border(cost); Text("Cost Heading", cost, "MATERIALS", 18, Muted, .07f, .77f, .93f, .96f, TextAlignmentOptions.Center);
            var craftingData = Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath);
            int materialCount = craftingData != null ? craftingData.materials.Count : 0;
            for (var i = 0; i < materialCount; i++)
            {
                var info = craftingData.materials[i]; var type = info.material; float step = .92f / materialCount;
                var tile = Panel("Material " + type, cost, Pastel, .04f + i * step, .12f, .04f + (i + 1) * step - .025f, .73f);
                var icon = Icon("Icon", tile, info != null && info.icon != null ? info.icon : database.forgeIcon, .13f, .30f, .87f, .96f);
                var title = Text("Material Name", tile, info != null ? info.title : type.ToString(), 12, Ink, .03f, .20f, .97f, .39f, TextAlignmentOptions.Center);
                var count = Text("Amount", tile, "0 / 0", 16, Red, .02f, .01f, .98f, .22f, TextAlignmentOptions.Center);
                materialViews.Add(new MaterialView { material = type, icon = icon, title = title, count = count });
            }

            var detail = Panel("Upgrade Details - ui_slot sliced", pageRect, new Color(.87f, .93f, 1), .555f, .15f, .905f, .87f); Border(detail);
            var card = Panel("Selected Equipment", detail, Pastel, .065f, .77f, .29f, .96f); detailIcon = Icon("Icon", card, database.forgeIcon, .06f, .06f, .94f, .94f);
            itemName = Text("Item Name", detail, "Equipment", 30, Ink, .33f, .825f, .95f, .95f);
            var level = Panel("Level Preview", detail, Pastel, .33f, .765f, .92f, .825f); levelLabel = Text("Level Text", level, "Lv.1  >  Lv.2", 22, Ink, .05f, .05f, .95f, .95f, TextAlignmentOptions.Center);
            statsRoot = Panel("Stat Comparison", detail, new Color(.75f, .84f, .96f), .055f, .425f, .945f, .73f);
            for (var i = 0; i < 7; i++)
            {
                var row = Rect("Stat Row " + i, statsRoot, .04f, 1 - (i + 1) / 3f, .96f, 1 - i / 3f);
                rows.Add(new StatRow { rect = row, title = Text("Stat Name", row, "ATK", 20, Ink, .03f, .05f, .47f, .95f),
                    before = Text("Current", row, "0", 21, Ink, .47f, .05f, .65f, .95f, TextAlignmentOptions.Center),
                    arrow = Text("Arrow", row, ">", 23, Green, .67f, .05f, .74f, .95f, TextAlignmentOptions.Center),
                    after = Text("Next", row, "0", 21, Green, .75f, .05f, .98f, .95f, TextAlignmentOptions.Center) });
                if (i > 2) row.gameObject.SetActive(false);
            }
            var result = Panel("Upgrade Result", detail, new Color(.78f, .87f, .98f), .055f, .19f, .945f, .405f);
            Text("Result Heading", result, "CRAFTING RESULT", 17, Muted, .07f, .73f, .96f, .96f);
            Icon("Upgrade Icon", result, database.forgeIcon, .07f, .25f, .22f, .72f);
            resultLabel = Text("Result Summary", result, "Equipment level +1", 23, Green, .24f, .43f, .95f, .74f);
            statusLabel = Text("Status", result, "Success Rate: 100%", 17, Green, .05f, .24f, .95f, .43f, TextAlignmentOptions.Center);
            forgeButton = Button("Forge", detail, database.forgeButtonSprite, "FORGE", .055f, .04f, .945f, .165f, out forgeLabel);
            forgeImage = forgeButton.GetComponent<Image>(); Icon("Hammer", forgeButton.transform, database.forgeIcon, .19f, .15f, .34f, .85f);
            feedback = Text("Feedback", result, "Collect fragments and materials to craft new equipment.", 12, Ink, .05f, .02f, .95f, .22f, TextAlignmentOptions.Center);
        }
        private RectTransform Panel(string name, Transform parent, Color color, float x0, float y0, float x1, float y1)
        { var rect = Rect(name, parent, x0, y0, x1, y1); var image = rect.gameObject.AddComponent<Image>(); image.sprite = database.slotSprite; image.material = database.slotMaterial;
            image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.5f; image.color = color; image.raycastTarget = false; return rect; }
        private Image Icon(string name, Transform parent, Sprite sprite, float x0, float y0, float x1, float y1)
        { var rect = Rect(name, parent, x0, y0, x1, y1); var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private TMP_Text Text(string name, Transform parent, string text, float size, Color color, float x0, float y0, float x1, float y1, TextAlignmentOptions align = TextAlignmentOptions.Left)
        { var value = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>(); value.font = database.font; value.text = text;
            value.fontSize = value.fontSizeMax = size; value.fontSizeMin = 10; value.enableAutoSizing = true; value.color = color; value.alignment = align;
            value.textWrappingMode = TextWrappingModes.Normal; value.overflowMode = TextOverflowModes.Ellipsis; value.raycastTarget = false; typography.Add(new TextStyle { text = value, size = size }); return value; }
        private Button Button(string name, Transform parent, Sprite sprite, string label, float x0, float y0, float x1, float y1, out TMP_Text text)
        { var rect = Panel(name, parent, Color.white, x0, y0, x1, y1); var image = rect.GetComponent<Image>(); image.sprite = sprite; image.material = null; image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; text = Text("Label", rect, label, 29, Color.white, .08f, .08f, .92f, .92f, TextAlignmentOptions.Center); return button; }
        private static void Border(RectTransform rect)
        { var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color32(73, 170, 255, 255); outline.effectDistance = new Vector2(2, -2); }
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5; rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; return rect; }
#endif
    }
}
