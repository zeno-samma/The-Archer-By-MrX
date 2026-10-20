using System;
using System.Collections.Generic;
using MrX.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class AchievementsPageBehavior : MainMenuPageBehavior
    {
        [SerializeField] private AchievementsDatabase database;
        [SerializeField] private TMP_Text overallCount;
        [SerializeField] private TMP_Text overallPercent;
        [SerializeField] private RectTransform overallFill;
        [SerializeField] private TMP_Text walletText;
        [SerializeField] private TMP_Text feedback;
        [SerializeField] private Button settingsButton;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private List<CategoryView> tabs = new List<CategoryView>();
        [SerializeField] private List<RowView> rowPool = new List<RowView>();
        [SerializeField] private List<TextStyle> typography = new List<TextStyle>();
        private AchievementsManager manager;
        private CurrencySave wallet;
        private AchievementCategory selectedCategory = AchievementCategory.Adventure;
        private readonly List<Button> claimNavigation = new List<Button>(16);
        private bool dirty = true;
        private bool rebind = true;
        private bool layoutDirty = true;
        private int lastCompleted = -1;
        private int lastTotal = -1;
        private int lastGold = -1;
        private float lastTextScale = -1;
        private static readonly Color Ink = new Color32(38, 62, 140, 255);
        private static readonly Color Muted = new Color32(108, 133, 183, 255);
        private static readonly Color Blue = new Color32(50, 146, 255, 255);
        private static readonly Color Pale = new Color32(208, 230, 255, 255);

        [Serializable] private class TextStyle { public TMP_Text text; public float size; }
        [Serializable] private class CategoryView
        {
            public AchievementCategory category;
            public Button button;
            public Image background;
            public Outline outline;
            public TMP_Text title;
            public TMP_Text progress;
            public GameObject badge;
            [NonSerialized] public int lastCompleted = -1;
            [NonSerialized] public int lastTotal = -1;
            [NonSerialized] public int lastClaimable = -1;
        }
        [Serializable] private class RowView
        {
            public RectTransform rect;
            public LayoutElement layout;
            public Outline outline;
            public Image background;
            public Image icon;
            public TMP_Text title;
            public TMP_Text description;
            public RectTransform fill;
            public TMP_Text progress;
            public TMP_Text reward;
            public Button button;
            public Image buttonImage;
            public TMP_Text buttonLabel;
            public GameObject badge;
            [NonSerialized] public AchievementDefinition definition;
            [NonSerialized] public long lastProgress = -1;
            [NonSerialized] public int lastState = -1;
        }

        protected override void Start()
        {
            base.Start();
            manager = GameController.AchievementsManager;
            if (manager == null || manager.Save == null) { enabled = false; return; }
            manager.Changed += OnChanged;
            wallet = GameController.CurrenciesManager.GetCurrency("gold", false);
            if (wallet != null) wallet.OnAmountChanged += OnWalletChanged;
            foreach (var tab in tabs)
            {
                var captured = tab;
                tab.button.onClick.AddListener(() => SelectCategory(captured.category));
                tab.lastCompleted = tab.lastTotal = tab.lastClaimable = -1;
            }
            foreach (var row in rowPool)
            {
                var captured = row;
                row.button.onClick.AddListener(() => Claim(captured));
            }
            settingsButton.onClick.AddListener(OpenSettings);
            enabled = canvas.enabled;
        }

        private void OnChanged() { dirty = true; }
        private void OnWalletChanged(CurrencySave currency) { dirty = true; }
        private void OnRectTransformDimensionsChange() { layoutDirty = true; }

        private void LateUpdate()
        {
            if (manager == null || !canvas.enabled || !canvasGroup.blocksRaycasts) return;
            if (layoutDirty) { /* Layout is authored in the prefab. */ layoutDirty = false; }
            if (!dirty) return;
            RefreshVisible(); dirty = false;
        }

        public override void MoveCenter()
        {
            enabled = true;
            canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
            dirty = rebind = layoutDirty = true;
            feedback.text = "Complete challenges and claim your rewards.";
            base.MoveCenter();
        }
        public override void MoveLeft() { DisableInteraction(); base.MoveLeft(); }
        public override void MoveRight() { DisableInteraction(); base.MoveRight(); }
        private void DisableInteraction() { canvasGroup.interactable = canvasGroup.blocksRaycasts = false; }
        public override void OnMoveFinished() { base.OnMoveFinished(); DisableInteraction(); enabled = false; }
        protected override void OnMoveCenterFinished()
        {
            if (GameController.InputManager.ActiveInput != Input.InputType.UIJoystick && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(SelectedTab().button.gameObject);
        }

        public void SelectCategory(AchievementCategory category)
        {
            if (manager == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            selectedCategory = category; rebind = dirty = true;
            GameController.AudioManager.PlayButtonClick();
            RefreshVisible(); dirty = false;
        }

        private CategoryView SelectedTab()
        { foreach (var tab in tabs) if (tab.category == selectedCategory) return tab; return tabs[0]; }

        private void RefreshVisible()
        {
            var completed = manager.CompletedCount;
            var total = database.achievements.Count;
            if (lastCompleted != completed || lastTotal != total)
            {
                lastCompleted = completed; lastTotal = total;
                overallCount.SetText("{0} / {1}", (float)completed, (float)total);
                overallPercent.SetText("{0}%", total == 0 ? 0 : Mathf.FloorToInt(100f * completed / total));
                Fill(overallFill, total == 0 ? 0 : completed / (float)total);
            }
            if (wallet != null && wallet.Amount != lastGold)
            { lastGold = wallet.Amount; walletText.text = lastGold.ToString("N0"); }
            foreach (var tab in tabs)
            {
                var summary = manager.GetSummary(tab.category);
                if (tab.lastCompleted != summary.completed || tab.lastTotal != summary.total)
                { tab.progress.SetText("{0} / {1}", (float)summary.completed, (float)summary.total); tab.lastCompleted = summary.completed; tab.lastTotal = summary.total; }
                if (tab.lastClaimable != summary.claimable) { tab.badge.SetActive(summary.claimable > 0); tab.lastClaimable = summary.claimable; }
                var selected = tab.category == selectedCategory;
                tab.background.color = selected ? new Color(.94f, .99f, 1) : new Color(.86f, .94f, 1);
                tab.outline.enabled = selected;
                tab.title.color = selected ? Blue : Ink;
            }
            if (rebind) { BindCategory(); rebind = false; }
            foreach (var row in rowPool) if (row.definition != null) RefreshRow(row);
            RebuildNavigation();
        }

        private void BindCategory()
        {
            var definitions = manager.GetCategory(selectedCategory);
            var index = 0;
            // Stable three-way partition, no LINQ/sort allocations. The order is
            // frozen while a category is open, so claiming cannot move another
            // reward underneath the user's double-click or held submit button.
            for (var rank = 0; rank < 3; rank++)
                for (var i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    var state = manager.Save.IsClaimed(definition.id) ? 2 : manager.Save.IsComplete(definition) ? 0 : 1;
                    if (state != rank) continue;
                    var row = rowPool[index++]; row.definition = definition;
                    row.lastProgress = -1; row.lastState = -1; row.rect.gameObject.SetActive(true);
                    row.title.text = definition.title; row.description.text = definition.description;
                    row.reward.text = definition.reward.Amount.ToString("N0");
                    row.icon.sprite = definition.icon != null ? definition.icon : database.trophySprite;
                }
            for (; index < rowPool.Count; index++) { rowPool[index].definition = null; rowPool[index].rect.gameObject.SetActive(false); }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 1;
        }

        private void RefreshRow(RowView row)
        {
            var definition = row.definition;
            var progress = Math.Min((long)definition.target, manager.Save.Get(definition.metric));
            if (row.lastProgress != progress)
            {
                row.lastProgress = progress; row.progress.text = progress + " / " + definition.target;
                Fill(row.fill, progress / (float)definition.target);
            }
            var state = manager.Save.IsClaimed(definition.id) ? 2 : manager.Save.IsComplete(definition) ? 1 : 0;
            if (row.lastState == state) return;
            row.lastState = state;
            row.button.interactable = state == 1;
            row.buttonLabel.text = state == 2 ? "CLAIMED" : "CLAIM";
            row.buttonImage.sprite = state == 1 ? database.claimButtonSprite : database.disabledButtonSprite;
            row.buttonImage.color = state == 2 ? new Color32(134, 183, 163, 255) : Color.white;
            row.background.color = state == 1 ? new Color(1, 1, .98f) : new Color(.94f, .99f, 1);
            row.outline.enabled = state == 1; row.badge.SetActive(state == 1);
        }

        private void Claim(RowView row)
        {
            if (row.definition == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            var definition = row.definition;
            var granted = manager.TryClaim(definition.id);
            if (granted)
            {
                GameController.AudioManager.PlayButtonClick();
                feedback.text = "Reward received: +" + definition.reward.Amount + " " + definition.reward.CurrencyId.ToUpperInvariant();
            }
            else feedback.text = manager.Save.IsClaimed(definition.id) ? "This reward has already been claimed." : "Reward unavailable. Complete the challenge and check the reward currency.";
            dirty = true; RefreshVisible(); dirty = false;
        }

        private void RebuildNavigation()
        {
            claimNavigation.Clear();
            foreach (var row in rowPool) if (row.definition != null && row.button.interactable) claimNavigation.Add(row.button);
            var selectedTab = SelectedTab().button;
            var firstClaim = claimNavigation.Count > 0 ? claimNavigation[0] : settingsButton;
            for (var i = 0; i < tabs.Count; i++) tabs[i].button.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit, selectOnUp = i > 0 ? tabs[i - 1].button : settingsButton,
                selectOnDown = i + 1 < tabs.Count ? tabs[i + 1].button : tabs[0].button, selectOnRight = firstClaim
            };
            for (var i = 0; i < claimNavigation.Count; i++) claimNavigation[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit, selectOnLeft = selectedTab, selectOnUp = i > 0 ? claimNavigation[i - 1] : settingsButton,
                selectOnDown = i + 1 < claimNavigation.Count ? claimNavigation[i + 1] : selectedTab
            };
            settingsButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = selectedTab, selectOnLeft = selectedTab };
            if (EventSystem.current == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            var selected = EventSystem.current.currentSelectedGameObject;
            var button = selected != null ? selected.GetComponent<Button>() : null;
            if (button != null && button.transform.IsChildOf(transform) && !button.interactable)
                EventSystem.current.SetSelectedGameObject(selectedTab.gameObject);
        }

        private void ResizeLayout()
        {
            var rowHeight = Mathf.Max(90, viewport.rect.height / 4.15f - 10);
            foreach (var row in rowPool) row.layout.preferredHeight = rowHeight;
            var scale = Mathf.Max(.85f, pageRect.rect.width / 1672f);
            if (Mathf.Abs(scale - lastTextScale) < .01f) return;
            lastTextScale = scale;
            foreach (var style in typography)
            { style.text.fontSizeMax = style.size * scale; style.text.fontSizeMin = Mathf.Min(style.size, 11) * scale; style.text.fontSize = style.size * scale; }
        }

        private void OpenSettings()
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!screen.IsDockEnabled) return;
            GameController.AudioManager.PlayButtonClick(); screen.DisableDock();
            screen.SettingsPopup.onPopupClosed += OnSettingsClosed; screen.SettingsPopup.Show();
        }
        private void OnSettingsClosed()
        { var screen = GameController.MainMenuScreenBehavior; screen.SettingsPopup.onPopupClosed -= OnSettingsClosed; screen.EnableDock(); OnMoveCenterFinished(); }
        private void OnDestroy()
        {
            if (manager != null) manager.Changed -= OnChanged;
            if (wallet != null) wallet.OnAmountChanged -= OnWalletChanged;
            moveEasingCoroutine?.Stop(); alphaEasingCoroutine?.Stop();
            var screen = GameController.MainMenuScreenBehavior;
            if (screen != null && screen.SettingsPopup != null) screen.SettingsPopup.onPopupClosed -= OnSettingsClosed;
        }

        private static void Fill(RectTransform rect, float value)
        { var fraction = Mathf.Clamp01(value); rect.anchorMax = new Vector2(fraction, 1); rect.gameObject.SetActive(fraction > 0); }
#if UNITY_EDITOR
        // Editor authoring only. Saved prefab instances provide all runtime UI.
        public static AchievementsPageBehavior Create(Transform parent, AchievementsDatabase data)
        {
            var root = Rect("Achievements Page", parent, 0, 0, 1, 1);
            var page = root.gameObject.AddComponent<AchievementsPageBehavior>();
            page.database = data; page.pageType = MainMenuPageType.Achievements; page.pageRect = root;
            page.canvas = root.gameObject.AddComponent<Canvas>(); root.gameObject.AddComponent<GraphicRaycaster>();
            page.canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            page.Build(); page.canvasGroup.alpha = 0; page.DisableInteraction(); page.canvas.enabled = false;
            return page;
        }

        private void Build()
        {
            var top = Rect("Top Bar", pageRect, .055f, .89f, .945f, .985f);
            Text("Journey Label", top, "YOUR JOURNEY", 27, Ink, 0, .1f, .60f, .9f);
            var walletPanel = Panel("Gold Wallet", top, Color.white, .72f, .14f, .9f, .86f);
            Icon("Gold", walletPanel, database.coinSprite, .05f, .15f, .28f, .85f);
            walletText = Text("Gold Amount", walletPanel, "0", 22, Ink, .32f, .1f, .96f, .9f);
            settingsButton = Button("Settings", top, database.disabledButtonSprite, .92f, .1f, 1, .9f, "", out _, out _);
            Icon("Settings Icon", settingsButton.transform, database.settingsSprite, .2f, .2f, .8f, .8f);

            var sheet = Panel("Achievements Sheet - ui_slot sliced", pageRect, new Color(.91f, .97f, 1), .105f, .135f, .895f, .88f);
            Outline(sheet, new Color32(75, 173, 255, 255), 2);
            var header = Rect("Header", sheet, .02f, .71f, .98f, .98f);
            Icon("Trophy Illustration", header, database.trophySprite, .015f, .02f, .25f, .99f);
            Text("Title", header, "ACHIEVEMENTS", 39, Ink, .27f, .43f, .635f, .86f);
            Text("Subtitle", header, "Complete challenges to earn rewards\nand show your progress!", 18, Muted, .27f, .12f, .63f, .44f);
            var summary = Panel("Overall Completion", header, new Color(.97f, 1, 1), .66f, .08f, .99f, .94f);
            Text("Label", summary, "OVERALL COMPLETION", 17, Ink, .055f, .7f, .95f, .94f);
            overallFill = ProgressBar("Overall Progress", summary, .06f, .4f, .72f, .58f);
            overallPercent = Text("Percentage", summary, "0%", 34, Ink, .75f, .27f, .98f, .69f, TextAlignmentOptions.Center);
            Icon("Small Trophy", summary, database.trophySprite, .06f, .1f, .16f, .30f);
            overallCount = Text("Completed Count", summary, "0 / " + database.achievements.Count, 22, Ink, .19f, .07f, .94f, .31f);

            var body = Panel("Body", sheet, new Color(.97f, 1, 1), .012f, .015f, .988f, .705f);
            var categoryColumn = Rect("Categories", body, .012f, .045f, .255f, .98f);
            for (var i = 0; i < database.categories.Count; i++)
            {
                var group = database.categories[i];
                var y1 = 1 - i / (float)database.categories.Count;
                var y0 = y1 - 1f / database.categories.Count + .02f;
                var button = Button("Category " + group.category, categoryColumn, database.slotSprite, 0, y0, 1, y1, "", out _, out var image);
                var tile = Panel("Icon Tile", button.transform, Pale, .025f, .10f, .30f, .90f);
                Icon("Category Icon", tile, group.icon, .12f, .12f, .88f, .88f);
                var title = Text("Category Title", button.transform, group.title, 22, Ink, .35f, .48f, .98f, .88f);
                var count = 0; foreach (var definition in database.achievements) if (definition.category == group.category) count++;
                var progress = Text("Category Progress", button.transform, "0 / " + count, 19, Muted, .35f, .13f, .96f, .48f);
                var outline = Outline(button.transform as RectTransform, Blue, 1.5f); outline.enabled = false;
                var badge = Badge("Claimable Badge", button.transform, .9f, .76f, 1.02f, 1.04f); badge.SetActive(false);
                tabs.Add(new CategoryView { category = group.category, button = button, background = image, title = title, progress = progress, badge = badge, outline = outline });
            }

            var listRoot = Rect("Achievement List", body, .275f, .055f, .99f, .98f);
            listRoot.gameObject.AddComponent<Image>().color = Color.clear;
            scroll = listRoot.gameObject.AddComponent<ScrollRect>();
            viewport = Rect("Viewport", listRoot, 0, 0, 1, 1); viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", viewport, 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10; layout.padding = new RectOffset(3, 3, 3, 3); layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.scrollSensitivity = 35; scroll.movementType = ScrollRect.MovementType.Clamped;
            var capacity = 0;
            foreach (var group in database.categories)
            { var count = 0; foreach (var definition in database.achievements) if (definition.category == group.category) count++; capacity = Mathf.Max(capacity, count); }
            for (var i = 0; i < capacity; i++) BuildRow(i);
            feedback = Text("Feedback", body, "Complete challenges and claim your rewards.", 13, Muted, .28f, .005f, .985f, .045f, TextAlignmentOptions.Center);
        }

        private void BuildRow(int index)
        {
            var rect = Panel("Achievement Row " + index, content, new Color(.94f, .99f, 1), 0, 0, 1, 1);
            var layout = rect.gameObject.AddComponent<LayoutElement>(); layout.preferredHeight = 140;
            var outline = Outline(rect, new Color32(255, 204, 61, 255), 1.5f); outline.enabled = false;
            var tile = Panel("Icon Tile", rect, Pale, .015f, .12f, .105f, .88f);
            var icon = Icon("Achievement Icon", tile, database.trophySprite, .08f, .08f, .92f, .92f);
            var title = Text("Title", rect, "Achievement", 23, Ink, .13f, .60f, .66f, .91f);
            var description = Text("Description", rect, "Complete this challenge.", 16, Muted, .13f, .36f, .66f, .61f);
            var fill = ProgressBar("Progress", rect, .13f, .14f, .52f, .28f);
            var progress = Text("Progress Label", rect, "0 / 1", 18, Muted, .535f, .04f, .665f, .35f, TextAlignmentOptions.Center);
            var rewardPanel = Panel("Reward", rect, Pale, .68f, .19f, .805f, .81f);
            Icon("Reward Icon", rewardPanel, database.coinSprite, .04f, .16f, .39f, .84f);
            var reward = Text("Reward Amount", rewardPanel, "100", 22, Ink, .40f, .08f, .98f, .92f, TextAlignmentOptions.Center);
            var button = Button("Claim", rect, database.disabledButtonSprite, .825f, .17f, .98f, .83f, "CLAIM", out var label, out var image);
            button.gameObject.AddComponent<AchievementRowFocus>().Initialize(scroll, rect);
            button.interactable = false;
            var badge = Badge("Claimable Badge", button.transform, .87f, .76f, 1.04f, 1.09f); badge.SetActive(false);
            rowPool.Add(new RowView { rect = rect, layout = layout, outline = outline, background = rect.GetComponent<Image>(), icon = icon,
                title = title, description = description, fill = fill, progress = progress, reward = reward, button = button, buttonImage = image, buttonLabel = label, badge = badge });
        }

        private RectTransform Panel(string name, Transform parent, Color color, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = database.slotSprite; image.type = Image.Type.Sliced;
            image.material = database.slotMaterial;
            image.pixelsPerUnitMultiplier = 1.5f; image.color = color; image.raycastTarget = false; return rect;
        }
        private Image Icon(string name, Transform parent, Sprite sprite, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        private TMP_Text Text(string name, Transform parent, string text, float size, Color color, float x0, float y0, float x1, float y1, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var value = rect.gameObject.AddComponent<TextMeshProUGUI>(); value.font = database.font; value.text = text;
            value.fontSize = value.fontSizeMax = size; value.fontSizeMin = Mathf.Min(size, 11); value.enableAutoSizing = true;
            value.color = color; value.alignment = align; value.raycastTarget = false; value.textWrappingMode = TextWrappingModes.Normal; value.overflowMode = TextOverflowModes.Ellipsis;
            typography.Add(new TextStyle { text = value, size = size }); return value;
        }
        private Button Button(string name, Transform parent, Sprite sprite, float x0, float y0, float x1, float y1, string text, out TMP_Text label, out Image image)
        {
            var rect = Panel(name, parent, Color.white, x0, y0, x1, y1);
            image = rect.GetComponent<Image>(); image.sprite = sprite != null ? sprite : database.slotSprite; image.raycastTarget = true;
            if (image.sprite != database.slotSprite) image.material = null;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            label = Text("Label", rect, text, 24, Color.white, .04f, .08f, .96f, .92f, TextAlignmentOptions.Center); return button;
        }
        private Outline Outline(RectTransform rect, Color color, float distance)
        { var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = color; outline.effectDistance = new Vector2(distance, -distance); return outline; }
        private GameObject Badge(string name, Transform parent, float x0, float y0, float x1, float y1)
        { var image = Icon(name, parent, database.circleSprite, x0, y0, x1, y1); image.color = new Color32(255, 75, 80, 255); Outline(image.rectTransform, Color.white, 2); return image.gameObject; }
        private RectTransform ProgressBar(string name, Transform parent, float x0, float y0, float x1, float y1)
        { var track = Panel(name, parent, new Color32(174, 201, 233, 255), x0, y0, x1, y1); return Panel("Fill", track, Blue, 0, 0, 0, 1); }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5;
            rect.SetParent(parent, false); rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
#endif
    }
}
