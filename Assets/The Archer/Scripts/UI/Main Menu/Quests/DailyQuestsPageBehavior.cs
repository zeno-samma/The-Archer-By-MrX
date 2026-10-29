using System;
using System.Collections.Generic;
using MrX.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class DailyQuestsPageBehavior : MainMenuPageBehavior
    {
        private readonly InspectorVisibility authoredVisibility = new InspectorVisibility();
        [SerializeField, Tooltip("Opt in to gameplay state colors. Off preserves Image/TMP colors authored in Inspector.")] private bool useStateColors;
        [SerializeField] private DailyQuestsDatabase database;
        [SerializeField] private TMP_Text resetText;
        [SerializeField] private TMP_Text completionText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text walletText;
        [SerializeField] private RectTransform dailyFill;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private Button settingsButton;
        [SerializeField] private List<QuestRow> rows = new List<QuestRow>();
        [SerializeField] private List<MilestoneView> chests = new List<MilestoneView>();
        [SerializeField] private List<TextStyle> typography = new List<TextStyle>();
        private DailyQuestsManager manager;
        private CurrencySave wallet;
        private float nextRefresh;

        [Serializable] private class TextStyle { public TMP_Text text; public float size; }

        [Serializable] private class QuestRow
        {
            public string id;
            public RectTransform rect;
            public RectTransform fill;
            public Image icon;
            public Image buttonImage;
            public TMP_Text progress;
            public TMP_Text action;
            public TMP_Text reward;
            public TMP_Text title;
            public TMP_Text description;
            public Button button;
        }
        [Serializable] private class MilestoneView
        {
            public string id;
            public Image icon;
            public TMP_Text state;
            public Button button;
        }

        [Header("Reward state presentation")]
        [SerializeField] private Color Ink = new Color32(39, 65, 149, 255);
        [SerializeField] private Color Muted = new Color32(108, 134, 188, 255);
        [SerializeField] private Color Blue = new Color32(48, 146, 255, 255);
        [SerializeField] private Color Pale = new Color32(207, 230, 255, 255);
        [SerializeField] private Color claimedButtonColor = new Color32(166, 184, 206, 255);
        [SerializeField] private Color incompleteButtonColor = new Color(.9f, .7f, 1);
        [SerializeField] private Color claimedChestColor = new Color32(120, 210, 157, 255);
        [SerializeField] private Color lockedChestColor = new Color32(140, 163, 199, 255);
        [SerializeField] private Color claimedTextColor = new Color32(29, 141, 100, 255);

        protected override void Start()
        {
            base.Start();
            manager = GameController.DailyQuestsManager;
            if (manager == null || manager.Save == null) return;
            manager.Changed += Refresh;
            wallet = GameController.CurrenciesManager.GetCurrency("gold", false);
            if (wallet != null) wallet.OnAmountChanged += OnWalletChanged;
            foreach (var row in rows)
            {
                var captured = row;
                row.button.onClick.AddListener(() => OnQuestClicked(captured.id));
            }
            foreach (var chest in chests)
            {
                var captured = chest;
                chest.button.onClick.AddListener(() => OnChestClicked(captured.id));
            }
            settingsButton.onClick.AddListener(OpenSettings);
            Refresh();
        }

        private void Update()
        {
            if (canvas == null || !canvas.enabled || manager == null || manager.Save == null) return;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1;
            manager.RefreshDay();
            var remaining = manager.TimeUntilReset;
            resetText.text = $"Resets in {(int)remaining.TotalHours:00}h {remaining.Minutes:00}m";
        }

        public override void MoveCenter()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            feedbackText.text = "Complete quests to unlock the chest rewards above.";
            base.MoveCenter();
            if (manager != null) { manager.RefreshDay(); Refresh(); }
        }

        public override void MoveLeft() { DisableInteraction(); base.MoveLeft(); }
        public override void MoveRight() { DisableInteraction(); base.MoveRight(); }
        private void DisableInteraction() { canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; }

        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();
            if (GameController.InputManager.ActiveInput != Input.InputType.UIJoystick && EventSystem.current != null)
            {
                var first = rows.Find(row => row.button.interactable);
                if (first != null) EventSystem.current.SetSelectedGameObject(first.button.gameObject);
            }
        }

        private void Refresh()
        {
            if (manager == null || manager.Save == null) return;
            var completed = manager.CompletedCount;
            completionText.text = $"Daily Progress\n<size=150%>{completed} / {database.quests.Count}</size>";
            Fill(dailyFill, completed / (float)database.quests.Count);
            walletText.text = wallet == null ? "GOLD" : $"{wallet.Amount:N0}";
            foreach (var row in rows)
            {
                var quest = database.quests.Find(q => q.id == row.id);
                if (quest == null) { row.button.interactable = false; continue; }
                var progress = manager.Save.GetProgress(quest);
                var claimed = manager.Save.IsClaimed(quest.id);
                var complete = manager.Save.IsComplete(quest);
                Fill(row.fill, progress / (float)quest.target);
                row.progress.text = $"{progress} / {quest.target}";
                row.reward.text = $"x {quest.reward.Amount}";
                row.title.text = quest.title;
                row.description.text = quest.description;
                row.icon.sprite = quest.icon != null ? quest.icon : database.questIcon;
                row.action.text = claimed ? "CLAIMED" : complete ? "CLAIM" : "GO";
                row.button.interactable = !claimed;
                if (useStateColors) row.buttonImage.color = claimed ? claimedButtonColor : complete ? Color.white : incompleteButtonColor;
            }
            foreach (var chest in chests)
            {
                var data = database.milestones.Find(m => m.id == chest.id);
                if (data == null) { chest.button.interactable = false; continue; }
                var claimed = manager.Save.IsMilestoneClaimed(data.id);
                var available = completed >= data.requiredCompletions;
                chest.button.interactable = available && !claimed;
                if (useStateColors) chest.icon.color = claimed ? claimedChestColor : available ? Color.white : lockedChestColor;
                chest.state.text = claimed ? "DONE" : $"{data.requiredCompletions}\n<size=70%>+{data.reward.Amount}</size>";
                if (useStateColors) chest.state.color = claimed ? claimedTextColor : Ink;
            }
            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            var actions = new List<Button>();
            foreach (var row in rows) if (row.button.interactable) actions.Add(row.button);
            var availableChests = new List<Button>();
            foreach (var chest in chests) if (chest.button.interactable) availableChests.Add(chest.button);
            var firstAction = actions.Count > 0 ? actions[0] : settingsButton;
            var firstChest = availableChests.Count > 0 ? availableChests[0] : settingsButton;
            for (var i = 0; i < actions.Count; i++)
                actions[i].navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = i > 0 ? actions[i - 1] : firstChest, selectOnDown = i + 1 < actions.Count ? actions[i + 1] : settingsButton, selectOnLeft = firstChest };
            for (var i = 0; i < availableChests.Count; i++)
                availableChests[i].navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = settingsButton, selectOnDown = firstAction, selectOnLeft = i > 0 ? availableChests[i - 1] : settingsButton, selectOnRight = i + 1 < availableChests.Count ? availableChests[i + 1] : firstAction };
            settingsButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = availableChests.Count > 0 ? firstChest : firstAction };
            var screen = GameController.MainMenuScreenBehavior;
            if (EventSystem.current == null || screen == null || screen.CurrentPage != this || !screen.IsDockEnabled) return;
            var selected = EventSystem.current.currentSelectedGameObject;
            var selectedButton = selected != null ? selected.GetComponent<Button>() : null;
            if (selectedButton != null && !selectedButton.interactable) EventSystem.current.SetSelectedGameObject(firstAction.gameObject);
        }

        private void OnQuestClicked(string id)
        {
            if (manager == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            manager.RefreshDay();
            var quest = database.quests.Find(q => q.id == id);
            if (quest == null) return;

            if (manager.Save.IsComplete(quest))
            {
                var granted = manager.TryClaimQuest(id);
                feedbackText.text = granted ? $"Reward received: +{quest.reward.Amount} {quest.reward.CurrencyId.ToUpperInvariant()}" : "Reward unavailable. Please try again.";
                Refresh();
            }
            else
            {
                if (quest.type == DailyQuestType.UpgradeHero) GameController.MainMenuScreenBehavior.OpenHeroUpgrades();
                else GameController.MainMenuScreenBehavior.OpenPage(MainMenuPageType.Stages);
            }
        }

        private void OnChestClicked(string id)
        {
            if (!GameController.MainMenuScreenBehavior.IsDockEnabled) return;

            var data = database.milestones.Find(m => m.id == id);
            if (data == null) return;
            var granted = manager.TryClaimMilestone(id);
            feedbackText.text = granted ? $"Chest reward received: +{data.reward.Amount} {data.reward.CurrencyId.ToUpperInvariant()}" : "Complete more quests to unlock this chest.";
            Refresh();
        }

        private void OpenSettings()
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!screen.IsDockEnabled) return;

            screen.DisableDock();
            screen.SettingsPopup.onPopupClosed += OnSettingsClosed;
            screen.SettingsPopup.Show();
        }
        private void OnSettingsClosed()
        {
            var screen = GameController.MainMenuScreenBehavior;
            screen.SettingsPopup.onPopupClosed -= OnSettingsClosed;
            screen.EnableDock();
            OnMoveCenterFinished();
        }
        private void OnWalletChanged(CurrencySave value) => Refresh();

        private void OnDestroy()
        {
            if (manager != null) manager.Changed -= Refresh;
            if (wallet != null) wallet.OnAmountChanged -= OnWalletChanged;
            moveEasingCoroutine?.Stop(); alphaEasingCoroutine?.Stop();
            var screen = GameController.MainMenuScreenBehavior;
            if (screen != null && screen.SettingsPopup != null) screen.SettingsPopup.onPopupClosed -= OnSettingsClosed;
        }

        // Progress binding only; all graphics and references are authored in the prefab.
        private void Fill(RectTransform fill, float value)
        { var fraction = Mathf.Clamp01(value); fill.anchorMax = new Vector2(fraction, 1); authoredVisibility.SetVisible(fill.gameObject, fraction > 0); }
#if UNITY_EDITOR
        // Editor authoring only. Saved prefab instances provide all runtime UI.
        public static DailyQuestsPageBehavior Create(Transform parent, DailyQuestsDatabase data)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Author the Quests prefab outside Play Mode.");
            var root = Rect("Daily Quests Page", parent, 0, 0, 1, 1);
            var page = root.gameObject.AddComponent<DailyQuestsPageBehavior>();
            page.database = data; page.pageType = MainMenuPageType.DailyQuests; page.pageRect = root;
            page.canvas = root.gameObject.AddComponent<Canvas>();
            root.gameObject.AddComponent<GraphicRaycaster>();
            page.canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            page.Build();
            page.canvasGroup.alpha = 0; page.DisableInteraction(); page.canvas.enabled = false;
            return page;
        }

        private void Build()
        {
            var top = Rect("Top Bar", pageRect, .055f, .89f, .945f, .985f);
            Text("Adventure Label", top, "DAILY ADVENTURE", 27, Ink, 0, .1f, .60f, .9f);
            var walletPanel = Panel("Gold Wallet", top, Color.white, .72f, .14f, .9f, .86f);
            Icon("Gold", walletPanel, database.coinSprite, .05f, .15f, .28f, .85f);
            walletText = Text("Gold Amount", walletPanel, "0", 22, Ink, .32f, .1f, .96f, .9f);
            settingsButton = MakeButton("Settings", top, database.blueButtonSprite, .92f, .1f, 1, .9f, database.settingsIcon != null ? "" : "...", out _, out _);
            if (database.settingsIcon != null) Icon("Settings Icon", settingsButton.transform, database.settingsIcon, .2f, .2f, .8f, .8f);

            var sheet = Panel("Quest Sheet - ui_slot sliced", pageRect, new Color(.95f, 1.01f, 1.1f), .135f, .13f, .865f, .88f);
            var outline = sheet.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(74, 172, 255, 255); outline.effectDistance = new Vector2(2, -2);
            var header = Rect("Header", sheet, .025f, .84f, .975f, .98f);
            var titleIcon = Panel("Title Icon", header, Pale, 0, .12f, .075f, .95f);
            Icon("Quest Icon", titleIcon, database.questIcon, .1f, .1f, .9f, .9f);
            Text("Title", header, "DAILY QUESTS", 35, Ink, .095f, .46f, .65f, .99f);
            Text("Subtitle", header, "Complete daily missions to earn great rewards!", 17, Muted, .095f, 0, .64f, .48f);
            var reset = Panel("Reset Timer", header, Pale, .67f, .31f, 1, .88f);
            resetText = Text("Countdown", reset, "Resets in --h --m", 19, Ink, .04f, .08f, .96f, .92f, TextAlignmentOptions.Center);

            var progressPanel = Panel("Daily Milestones", sheet, Pale, .025f, .635f, .975f, .835f);
            var total = Panel("Completion Counter", progressPanel, new Color32(186, 216, 253, 255), .02f, .29f, .2f, .91f);
            completionText = Text("Completed Quests", total, "Daily Progress\n0 / 5", 18, Ink, .03f, .08f, .97f, .92f, TextAlignmentOptions.Center);
            dailyFill = ProgressBar("Milestone Progress", progressPanel, .025f, .08f, .975f, .22f);
            for (var i = 0; i < database.milestones.Count; i++)
            {
                var milestone = database.milestones[i];
                var center = .27f + .69f * (i + 1) / database.milestones.Count;
                var button = MakeButton("Chest " + milestone.id, progressPanel, null, center - .05f, .22f, center + .05f, .96f, "", out var state, out var bg);
                bg.color = Color.clear;
                var icon = Icon("Chest Art", button.transform, milestone.icon, .12f, .35f, .88f, 1);
                Place(state.rectTransform, .02f, 0, .98f, .40f); state.color = Ink; state.fontSize = 17;
                chests.Add(new MilestoneView { id = milestone.id, icon = icon, state = state, button = button });
            }

            var scrollRoot = Rect("Quest List", sheet, .025f, .075f, .975f, .62f);
            scrollRoot.gameObject.AddComponent<Image>().color = Color.clear;
            var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            viewport = Rect("Viewport", scrollRoot, 0, 0, 1, 1);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10; layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 25;
            foreach (var quest in database.quests) BuildRow(content, quest);
            feedbackText = Text("Feedback", sheet, "Complete quests to unlock the chest rewards above.", 15, Muted, .04f, .015f, .96f, .065f, TextAlignmentOptions.Center);
        }

        private void BuildRow(RectTransform parent, DailyQuestDefinition quest)
        {
            var row = Panel("Quest " + quest.id, parent, new Color(.97f, 1.02f, 1.12f), 0, 0, 1, 1);
            row.sizeDelta = new Vector2(0, 100);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 100;
            var iconPanel = Panel("Icon Tile", row, new Color32(94, 151, 246, 255), .012f, .08f, .085f, .92f);
            var icon = Icon("Replaceable Icon", iconPanel, quest.icon, .15f, .15f, .85f, .85f);
            var title = Text("Title", row, quest.title, 22, Ink, .105f, .48f, .408f, .89f);
            var description = Text("Description", row, quest.description, 16, Muted, .105f, .11f, .408f, .49f);
            var fill = ProgressBar("Progress", row, .425f, .39f, .615f, .62f);
            var progress = Text("Progress Label", row, "0 / " + quest.target, 19, Ink, .625f, .22f, .709f, .78f, TextAlignmentOptions.Center);
            var reward = Panel("Reward", row, Pale, .72f, .16f, .835f, .84f);
            Icon("Reward Icon", reward, database.coinSprite, .05f, .15f, .35f, .85f);
            var rewardText = Text("Reward Amount", reward, "x " + quest.reward.Amount, 20, Ink, .36f, .1f, .98f, .9f, TextAlignmentOptions.Center);
            var button = MakeButton("Quest Action", row, database.blueButtonSprite, .855f, .15f, .985f, .85f, "GO", out var action, out var image);
            rows.Add(new QuestRow { id = quest.id, rect = row, fill = fill, icon = icon, progress = progress, reward = rewardText, title = title, description = description, button = button, action = action, buttonImage = image });
        }

        private RectTransform Panel(string name, Transform parent, Color color, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = database.slotSprite;
            image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.5f; image.color = color; image.raycastTarget = false;
            return rect;
        }
        private Image Icon(string name, Transform parent, Sprite sprite, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false;
            return image;
        }
        private TMP_Text Text(string name, Transform parent, string value, float size, Color color, float x0, float y0, float x1, float y1, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = database.font;
            text.text = value; text.fontSize = size; text.fontSizeMax = size; text.fontSizeMin = Mathf.Min(size, 11);
            text.enableAutoSizing = true; text.color = color; text.alignment = alignment;
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Ellipsis;
            typography.Add(new TextStyle { text = text, size = size });
            return text;
        }
        private Button MakeButton(string name, Transform parent, Sprite sprite, float x0, float y0, float x1, float y1, string label, out TMP_Text text, out Image image)
        {
            var rect = Panel(name, parent, Color.white, x0, y0, x1, y1);
            image = rect.GetComponent<Image>(); image.sprite = sprite != null ? sprite : database.slotSprite; image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            text = Text("Label", rect, label, 24, Color.white, .04f, .08f, .96f, .92f, TextAlignmentOptions.Center);
            return button;
        }
        private RectTransform ProgressBar(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var track = Panel(name, parent, new Color32(170, 200, 231, 255), x0, y0, x1, y1);
            return Panel("Fill", track, Blue, 0, 0, 0, 1);
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = 5; rect.SetParent(parent, false); Place(rect, x0, y0, x1, y1); return rect;
        }
        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        { rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
#endif
    }
}
