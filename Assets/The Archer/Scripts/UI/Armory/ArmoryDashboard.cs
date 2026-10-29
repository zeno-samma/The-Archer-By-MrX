using System;
using System.Collections.Generic;
using MrX.Armory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    /// <summary>Presentation over the existing scene panels; no second inventory or player instance.</summary>
    public sealed class ArmoryDashboard : MonoBehaviour
    {
        [SerializeField] private ArmoryDashboardDatabase art;
        [SerializeField] private RectTransform page, inventory, heroPanel, content, viewport;
        [SerializeField] private List<Cell> cells = new List<Cell>();
        private readonly List<ItemSaveData> observedItems = new List<ItemSaveData>();
        private readonly List<HeroSaveData> observedHeroes = new List<HeroSaveData>();
        [SerializeField] private List<TMP_Text> statValues = new List<TMP_Text>(), bonuses = new List<TMP_Text>();
        [SerializeField] private TMP_Text capacity, heroTitle, modalText, modalTitle;
        [SerializeField] private RectTransform modal;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Button expand, details, confirm, firstHero, filterButton;
        [SerializeField] private List<Button> heroButtons = new List<Button>();
        [SerializeField] private List<string> heroIds = new List<string>();
        [Header("Inventory presentation")]
        [SerializeField, Min(1)] private float layoutReferenceWidth = 1672f;
        [SerializeField, Min(0)] private float cellGap = 9f;
        [SerializeField] private Vector4 emptyIconBounds = new Vector4(0, 0, 1, 1);
        [SerializeField] private Vector4 emptyPlusBounds = new Vector4(.38f, .38f, .62f, .62f);
        [SerializeField] private Color emptyPlusColor = new Color(.54f, .64f, .77f);
        private UnityEngine.Events.UnityAction confirmAction;
        private void SetConfirmAction(UnityEngine.Events.UnityAction action)
        {
            if (confirmAction != null) confirm.onClick.RemoveListener(confirmAction);
            confirmAction = action; confirm.onClick.AddListener(action);
        }
        private bool wired;
        private bool listening, dirty, expansionModal;
        private int quotedCapacity, firstRow = -1;
        private float cellSize, stride;
        private int columns = 6;
        public void SetColumns(int value) { value=Mathf.Clamp(value,1,6); if(columns==value)return;columns=value;MarkDirty(); }
        private ItemType? filter;
        private readonly List<ItemSaveData> visibleItems = new List<ItemSaveData>();
        private readonly List<InventoryStack> visibleStacks = new List<InventoryStack>();
        private sealed class InventoryStack { public string key, title; public Sprite icon; public long amount; }
        private float Scale => page.rect.width / Mathf.Max(1, layoutReferenceWidth);
        private static readonly Color Ink = new Color(.12f, .23f, .56f);
        private ArmoryManager Armory => GameController.ArmoryManager;
        private readonly InspectorVisibility inspectorVisibility = new InspectorVisibility();
        private bool visibilityCaptured;

        private void Awake() => CaptureInspectorVisibility();

        private void CaptureInspectorVisibility()
        {
            if (visibilityCaptured) return;
            visibilityCaptured = true;
            foreach (var button in heroButtons)
            {
                if (button == null) continue;
                inspectorVisibility.Capture(button.gameObject);
                var locked = button.transform.Find("Locked Badge");
                var selected = button.transform.Find("Selected Badge");
                if (locked != null) inspectorVisibility.Capture(locked.gameObject);
                if (selected != null) inspectorVisibility.Capture(selected.gameObject);
            }
            foreach (var cell in cells)
            {
                if (cell == null || cell.button == null) continue;
                // Pool roots keep their own lifecycle; capture their decorations before filtering.
                inspectorVisibility.Capture(cell.button.image);
                inspectorVisibility.Capture(cell.icon);
                if (cell.label != null) inspectorVisibility.Capture(cell.label.gameObject);
                if (cell.lockIcon != null) inspectorVisibility.Capture(cell.lockIcon.gameObject);
                var inset = cell.button.transform.Find("Navy Inset");
                if (inset != null) inspectorVisibility.Capture(inset.gameObject);
            }
        }

#if UNITY_EDITOR
        public void Build(RectTransform owner, RectTransform existingHero, ArmoryDashboardDatabase configuration)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Author the Armory prefab outside Play Mode.");
            page = owner; heroPanel = existingHero; art = configuration;
            var stats = (RectTransform)page.Find("Info Panel");
            inventory = (RectTransform)page.Find("Armory Panel");
            Place(stats, .52f, .54f, .95f, .87f); Place(inventory, .52f, .15f, .95f, .52f);
            Skin(stats.GetComponent<Image>());
            var statsBackground = (RectTransform)stats.Find("Background");
            var inventoryBackground = (RectTransform)inventory.Find("Background Image");
            Place(statsBackground, 0, 0, 1, 1); Place(inventoryBackground, 0, 0, 1, 1);
            Skin(statsBackground.GetComponent<Image>()); Skin(inventoryBackground.GetComponent<Image>());
            var header = stats.Find("Text (TMP)").GetComponent<TMP_Text>();
            Style(header, 32); Place(header.rectTransform, .06f, .8f, .62f, .98f);
            var main = (RectTransform)stats.Find("Main"); Place(main, .04f, .08f, .96f, .79f);
            var oldLayout = main.GetComponent<LayoutGroup>(); if (oldLayout) oldLayout.enabled = false;
            var mainImage = main.GetComponent<Image>(); if (mainImage) mainImage.enabled = false;
            var names = new[] { "Bot", "Bot (2)", "Bot (1)", "Bot (3)" };
            var labels = new[] { "ATK", "HP", "CRIT", "SPEED" };
            for (int i = 0; i < names.Length; i++)
            {
                var card = (RectTransform)main.Find(names[i]); int x = i % 2, y = i / 2;
                Place(card, x * .51f, y == 0 ? .53f : 0, x * .51f + .49f, y == 0 ? 1 : .47f);
                Skin(card.GetComponent<Image>(), new Color(.84f, .92f, 1));
                Place((RectTransform)card.Find("Image"), .05f, .17f, .27f, .83f);
                var label = card.Find("Text (TMP) (1)").GetComponent<TMP_Text>();
                label.text = labels[i]; Style(label, 20); Place(label.rectTransform, .34f, .58f, .95f, .94f);
                var value = card.Find("Text (TMP) (2)").GetComponent<TMP_Text>();
                Style(value, 32); Place(value.rectTransform, .34f, .12f, .65f, .59f); statValues.Add(value);
                var bonus = card.Find("Text (TMP) (3)").GetComponent<TMP_Text>();
                Style(bonus, 19); bonus.color = new Color(.05f, .57f, .18f);
                Place(bonus.rectTransform, .65f, .12f, .99f, .59f); bonuses.Add(bonus);
            }
            details = Button(stats, "Details", "DETAILS  >", .7f, .83f, .95f, .97f, ShowDetails);
            var oldMask = inventory.Find("Mask"); if (oldMask) oldMask.gameObject.SetActive(false);
            var oldHeader = inventory.Find("Text (TMP) (1)"); if (oldHeader) oldHeader.gameObject.SetActive(false);
            Text(inventory, "Title", "INVENTORY", 29, .05f, .84f, .48f, .99f);
            capacity = Text(inventory, "Capacity", "", 21, .54f, .85f, .76f, .99f);
            expand = Button(inventory, "Expand Bag", "+", .78f, .85f, .87f, .98f, ShowExpansion);
            filterButton = Button(inventory, "Filter", "ALL", .88f, .85f, .98f, .98f, null);
            filterButton.onClick.AddListener(() => { filter = filter == null ? ItemType.Weapon : (int)filter.Value >= 3 ? (ItemType?)null : (ItemType)((int)filter.Value + 1); filterButton.GetComponentInChildren<TMP_Text>().text = filter?.ToString().ToUpperInvariant() ?? "ALL"; Refresh(); });
            viewport = Rect(inventory, "Inventory Viewport", .035f, .07f, .965f, .81f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 60 * Scale;
            scroll.viewport = viewport;
            content = Rect(viewport, "Content", 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1);
            scroll.content = content; scroll.onValueChanged.AddListener(OnScroll);
            // Retain the real preview and the four slot types supported by the database.
            heroPanel.anchorMin = heroPanel.anchorMax = new Vector2(.285f, .40f);
            heroPanel.anchoredPosition = Vector2.zero; heroPanel.sizeDelta = Vector2.zero;
            heroPanel.localScale = Vector3.one * Scale * .75f;
            foreach (var name in new[] { "Stats Background", "Damage Label", "HP Label" })
            { var child = heroPanel.Find(name); if (child) child.gameObject.SetActive(false); }
            var levelBar = heroPanel.Find("Info_Ui"); if (levelBar) levelBar.gameObject.SetActive(false);
            heroTitle = Text(page, "Hero Name and Level", "", 25, .17f, .83f, .41f, .9f);
            var footer = Rect(page, "Hero Selector", .13f, .15f, .47f, .28f); Skin(footer.gameObject.AddComponent<Image>());
            Text(footer, "Title", "SWITCH HERO", 20, .05f, .73f, .95f, 1);
            var heroes = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory.Heroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i]; float width = .82f / Mathf.Max(3, heroes.Count);
                var button = Button(footer, "Hero " + hero.Id, "", .09f + i * width, .07f, .09f + (i + 1) * width - .025f, .73f,
                    () => GameController.MainMenuScreenBehavior.ExpandedHeroPopup.Show(hero, Armory.GetHeroSave(hero.Id)));
                var icon = Rect(button.transform, "Portrait", .08f, .08f, .92f, .92f).gameObject.AddComponent<Image>();
                icon.sprite = hero.Icon; icon.preserveAspect = true; icon.raycastTarget = false;
                heroButtons.Add(button); heroIds.Add(hero.Id);
                if (firstHero == null) firstHero = button;
            }
            var gem = page.Find("Gem"); if (gem) gem.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();
            for (int i = 0; i < 36; i++) cells.Add(CreateCell());
            BakeModal();
        }

#endif
        private void Wire()
        {
            if (wired) return; wired = true;
            CaptureInspectorVisibility();
            details.onClick.AddListener(ShowDetails); expand.onClick.AddListener(ShowExpansion);
            filterButton.onClick.AddListener(() => { filter = filter == null ? ItemType.Weapon : (int)filter.Value >= 3 ? (ItemType?)null : (ItemType)((int)filter.Value + 1); filterButton.GetComponentInChildren<TMP_Text>().text = filter?.ToString().ToUpperInvariant() ?? "ALL"; Refresh(); });
            scroll.onValueChanged.AddListener(OnScroll);
            foreach (var cell in cells)
            {
                cell.iconPlacement = ResponsiveMenuLayout.Placement.Capture(cell.icon.rectTransform);
                cell.iconColor = cell.icon.color; cell.frameColor = cell.button.image.color;
                var captured = cell; cell.button.onClick.AddListener(() => { if (captured.item != null) GameController.MainMenuScreenBehavior.ExpandedItemPopup.Show(Armory.GetItemData(captured.item.Id), captured.item); else if (captured.stack != null) ShowStack(captured.stack); });
            }
            for (int i = 0; i < heroButtons.Count; i++) { var id = heroIds[i]; heroButtons[i].onClick.AddListener(() => GameController.MainMenuScreenBehavior.ExpandedHeroPopup.Show(Armory.GetAllHeroDataList().Find(h => h.Id == id), Armory.GetHeroSave(id))); }
            modal.Find("Panel/Close").GetComponent<Button>().onClick.AddListener(CloseModal);
        }
        public void Show()
        {
            if (!enabled) return;
            Wire(); gameObject.SetActive(true);
            if (!listening)
            {
                listening = true; Armory.OnInventoryChanged += MarkDirty; Armory.OnItemEquipmentChanged += MarkDirty;
                Armory.OnSelectedHeroChanged += MarkDirty; Armory.Bag.Changed += MarkDirty;
                if (Armory.Crafting != null) Armory.Crafting.Changed += MarkDirty;
                foreach (var upgrade in GameController.UpgradesManager.GetAllUpgrades()) upgrade.onUpgradeLevelChanged += OnUpgrade;
            }
            BindSaves(); Refresh();
            scroll.verticalNormalizedPosition = 1;
            firstRow = -1; RenderCells();
        }
        public void Hide()
        {
            CloseModal();
            if (listening)
            {
                listening = false; Armory.OnInventoryChanged -= MarkDirty; Armory.OnItemEquipmentChanged -= MarkDirty;
                Armory.OnSelectedHeroChanged -= MarkDirty; Armory.Bag.Changed -= MarkDirty;
                if (Armory.Crafting != null) Armory.Crafting.Changed -= MarkDirty;
                foreach (var upgrade in GameController.UpgradesManager.GetAllUpgrades()) upgrade.onUpgradeLevelChanged -= OnUpgrade;
            }
            UnbindSaves();
        }
        private void BindSaves()
        {
            UnbindSaves(); observedItems.AddRange(Armory.GetItemSaveList());
            foreach (var item in observedItems) item.OnItemLevelChanged += OnItemLevel;
            foreach (var hero in Armory.GetAllHeroDataList())
            { var save = Armory.GetHeroSave(hero.Id); observedHeroes.Add(save); save.OnHeroLevelChanged += OnHeroLevel; }
        }
        private void UnbindSaves()
        {
            foreach (var item in observedItems) item.OnItemLevelChanged -= OnItemLevel;
            foreach (var hero in observedHeroes) hero.OnHeroLevelChanged -= OnHeroLevel;
            observedItems.Clear(); observedHeroes.Clear();
        }
        public void MarkDirty() { if (listening) { dirty = true; } }
        private void OnItemLevel(ItemSaveData item, int level) => MarkDirty();
        private void OnHeroLevel(HeroSaveData hero, int level) => MarkDirty();
        private void OnUpgrade(int level) => MarkDirty();
        private void LateUpdate() { if (dirty && listening) { dirty = false; BindSaves(); Refresh(); } }
        public void Focus(bool heroes = false)
        {
            var button = heroes
                ? heroButtons.Find(value => value != null && value.isActiveAndEnabled && value.IsInteractable())
                : cells.Count > 0 && cells[0].button.isActiveAndEnabled && cells[0].button.IsInteractable() ? cells[0].button : details;
            if (button == null) button = details;
            if (button && EventSystem.current) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
        private void Refresh()
        {
            for (int i = 0; i < heroButtons.Count; i++)
            { var rosterHero = Armory.GetAllHeroDataList().Find(h => h.Id == heroIds[i]); inspectorVisibility.SetVisible(heroButtons[i].gameObject, rosterHero != null && !rosterHero.HideInRoster);
                var lockBadge = heroButtons[i].transform.Find("Locked Badge");
                var selected = heroButtons[i].transform.Find("Selected Badge");
                if (lockBadge != null) inspectorVisibility.SetVisible(lockBadge.gameObject, rosterHero != null && !Armory.GetHeroSave(rosterHero.Id).IsUnlocked);
                if (selected != null) inspectorVisibility.SetVisible(selected.gameObject, rosterHero == Armory.EquippedHero);
            }
            var snapshot = ArmoryStatsSnapshot.Current(); var t = snapshot.Total; var b = snapshot.HeroOnly;
            float[] values = { t.AttackDamageStat.Value, t.MaxHPStat.Value, t.CritChanceStat.Value, t.MovementSpeedStat.Value };
            float[] basic = { b.AttackDamageStat.Value, b.MaxHPStat.Value, b.CritChanceStat.Value, b.MovementSpeedStat.Value };
            for (int i = 0; i < 4; i++)
            {
                string unit = i == 2 ? "%" : ""; statValues[i].text = values[i].ToString("0.##") + unit;
                bonuses[i].text = "(" + (values[i] >= basic[i] ? "+" : "") + (values[i] - basic[i]).ToString("0.##") + unit + ")";
            }
            var hero = Armory.EquippedHero; heroTitle.text = hero.Name + "   Lv." + (Armory.GetHeroSave(hero.Id).Level + 1);
            var bag = Armory.Bag; capacity.text = bag.Save.UsedSlots + " / " + bag.Save.BagCapacity;
            expand.interactable = bag.NextCapacity > bag.Save.BagCapacity;
            visibleItems.Clear();
            foreach (var item in Armory.GetItemSaveList())
            { var data = Armory.GetItemData(item.Id); if (data != null && (filter == null || data.ItemType == filter)) visibleItems.Add(item); }
            visibleItems.Sort((a, c) => { int equipped = c.IsEquipped.CompareTo(a.IsEquipped); return equipped != 0 ? equipped : string.CompareOrdinal(a.Id, c.Id); });
            visibleStacks.Clear();
            if (filter == null && Armory.Crafting != null)
            {
                var warehouse = Armory.Crafting.Service.Inventory;
                foreach (var item in Armory.Crafting.Service.Recipes)
                {
                    var key = Crafting.CraftingInventorySave.FragmentKey(item.Id); long amount = warehouse.Count(key);
                    if (amount > 0) visibleStacks.Add(new InventoryStack { key = key, title = item.ItemName + " fragments", icon = item.Icon, amount = amount });
                }
                foreach (var material in Armory.Crafting.Database.materials)
                {
                    var key = Crafting.CraftingInventorySave.MaterialKey(material.material); long amount = warehouse.Count(key);
                    if (amount > 0) visibleStacks.Add(new InventoryStack { key = key, title = material.title, icon = material.icon, amount = amount });
                }
            }
            Canvas.ForceUpdateCanvases();
            cellSize = viewport.rect.width / columns; stride = cellSize;
            int total = visibleItems.Count + visibleStacks.Count + bag.Save.BagCapacity - bag.Save.UsedSlots;
            content.sizeDelta = new Vector2(0, Mathf.Ceil(total / (float)columns) * stride);
            var offset = content.anchoredPosition; offset.y = Mathf.Clamp(offset.y, 0, Mathf.Max(0, content.sizeDelta.y - viewport.rect.height)); content.anchoredPosition = offset;
            int poolSize = Mathf.Min(total, (Mathf.CeilToInt(viewport.rect.height / stride) + 2) * columns);
            // Visible cells are authored in the prefab and recycled when scrolling.
            for (int i = poolSize; i < cells.Count; i++) cells[i].button.gameObject.SetActive(false);
            firstRow = -1; RenderCells();
            enabled = false;
        }
#if UNITY_EDITOR
        private Cell CreateCell()
        {
            var button = Button(content, "Equipment Slot " + cells.Count, "", 0, 0, 0, 0, null);
            var cell = new Cell { button = button };
            cell.icon = Rect(button.transform, "Item or Empty Icon", .1f, .18f, .9f, .92f).gameObject.AddComponent<Image>();
            cell.icon.preserveAspect = true; cell.icon.raycastTarget = false;
            cell.label = button.GetComponentInChildren<TMP_Text>(); Place(cell.label.rectTransform, .05f, .02f, .95f, .25f);
            cell.lockIcon = Rect(button.transform, "Equipped", .72f, .73f, .96f, .97f).gameObject.AddComponent<Image>();
            cell.lockIcon.sprite = art.lockIcon; cell.lockIcon.preserveAspect = true; cell.lockIcon.raycastTarget = false;
            button.onClick.AddListener(() => { if (cell.item != null) GameController.MainMenuScreenBehavior.ExpandedItemPopup.Show(Armory.GetItemData(cell.item.Id), cell.item); });
            return cell;
        }
#endif
        private void OnScroll(Vector2 _) => RenderCells();
        private void OnRectTransformDimensionsChange() { if (content != null) MarkDirty(); }
        private void RenderCells()
        {
            if (stride <= 0) return;
            int row = Mathf.Max(0, Mathf.FloorToInt(content.anchoredPosition.y / stride));
            if (row == firstRow) return; firstRow = row;
            int total = visibleItems.Count + visibleStacks.Count + Armory.Bag.Save.BagCapacity - Armory.Bag.Save.UsedSlots;
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i]; int index = row * columns + i;
                cell.button.gameObject.SetActive(index < total); if (index >= total) continue;
                var rect = (RectTransform)cell.button.transform; rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = Vector2.one * Mathf.Max(1, cellSize - cellGap * Scale);
                rect.anchoredPosition = new Vector2(index % columns * stride, -(index / columns) * stride);
                cell.item = index < visibleItems.Count ? visibleItems[index] : null;
                int stackIndex = index - visibleItems.Count;
                cell.stack = stackIndex >= 0 && stackIndex < visibleStacks.Count ? visibleStacks[stackIndex] : null;
                bool empty = cell.item == null && cell.stack == null;
                var inset = cell.button.transform.Find("Navy Inset");
                if (inset != null) inspectorVisibility.SetVisible(inset.gameObject, !empty);
                cell.button.interactable = !empty;
                inspectorVisibility.SetVisible(cell.button.image, !empty || art.emptyFrame != null);
                if (art.emptyFrame != null)
                {
                    cell.button.image.sprite = empty ? art.emptyFrame : cell.item != null ? art.equipmentFrame : art.stackFrame;
                    cell.button.image.color = cell.frameColor;
                    if(cell.item != null && art.rarityFrames != null)
                    {
                        var level=Armory.GetItemData(cell.item.Id).GetItemLevel(Mathf.Max(0,cell.item.Level));
                        int rarity=level != null ? (int)level.ItemRarity : 0;
                        if(rarity >= 0 && rarity < art.rarityFrames.Length && art.rarityFrames[rarity] != null)cell.button.image.sprite=art.rarityFrames[rarity];
                    }
                }
                cell.icon.sprite = empty ? art.emptySlotIcon : cell.item != null ? Armory.GetItemData(cell.item.Id).Icon : cell.stack.icon;
                cell.icon.color = cell.iconColor;
                if (empty) Place(cell.icon.rectTransform, emptyIconBounds.x, emptyIconBounds.y, emptyIconBounds.z, emptyIconBounds.w);
                else cell.iconPlacement.Apply(cell.icon.rectTransform);
                if (empty && art.emptyPlus != null)
                {
                    cell.icon.sprite = art.emptyPlus;
                    cell.icon.color = emptyPlusColor;
                    Place(cell.icon.rectTransform, emptyPlusBounds.x, emptyPlusBounds.y, emptyPlusBounds.z, emptyPlusBounds.w);
                }
                inspectorVisibility.SetVisible(cell.icon, cell.icon.sprite != null);
                inspectorVisibility.SetVisible(cell.label.gameObject, !empty);
                cell.label.text = cell.item != null ? "Lv." + (cell.item.Level + 1) : cell.stack != null ? "× " + cell.stack.amount.ToString("N0") : "";
                inspectorVisibility.SetVisible(cell.lockIcon.gameObject, cell.item != null && cell.item.IsEquipped);
            }
        }
        private void ShowStack(InventoryStack stack)
        {
            CreateModal(stack.title); expansionModal = false;
            modalText.text = "Owned: " + Armory.Crafting.Service.Inventory.Count(stack.key).ToString("N0");
        }
#if UNITY_EDITOR
        private void BakeModal()
        {
            modal = Rect(page, "Armory Modal", 0, 0, 1, 1); var shade = modal.gameObject.AddComponent<Image>(); shade.color = new Color(.02f, .06f, .18f, .72f);
            var panel = Rect(modal, "Panel", .25f, .18f, .75f, .86f); Skin(panel.gameObject.AddComponent<Image>());
            modalTitle = Text(panel, "Title", "PLAYER STATS", 30, .05f, .85f, .85f, .99f);
            Button(panel, "Close", "X", .88f, .87f, .98f, .98f, null);
            var view = Rect(panel, "Scroll View", .06f, .2f, .94f, .83f); view.gameObject.AddComponent<RectMask2D>();
            var scroller = view.gameObject.AddComponent<ScrollRect>(); scroller.horizontal = false; scroller.viewport = view; scroller.scrollSensitivity = 60 * Scale;
            var body = Rect(view, "Body", 0, 1, 1, 1); body.pivot = new Vector2(.5f, 1); scroller.content = body;
            modalText = Text(body, "Description", "All player stats", 21, 0, 0, 1, 1); modalText.alignment = TextAlignmentOptions.TopLeft;
            modalText.enableAutoSizing = false; modalText.textWrappingMode = TextWrappingModes.Normal;
            var fitter = body.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = body.gameObject.AddComponent<VerticalLayoutGroup>(); layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            confirm = Button(panel, "Confirm", "OK", .25f, .04f, .75f, .17f, null);
            modal.gameObject.SetActive(false);
        }
#endif
        private void CreateModal(string title)
        {
            CloseModal(); GameController.MainMenuScreenBehavior.DisableDock();
            modal.gameObject.SetActive(true); modal.SetAsLastSibling(); modalTitle.text = title;
            GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancel;
            SetConfirmAction(CloseModal);
            confirm.interactable = true; confirm.GetComponentInChildren<TMP_Text>().text = "OK";
            var close = modal.Find("Panel/Close").GetComponent<Button>();
            close.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = confirm };
            confirm.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = close };
            modal.Find("Panel/Scroll View").GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(confirm.gameObject);
        }
        private void ShowDetails() { CreateModal("PLAYER STATS"); modalText.text = ArmoryStatsSnapshot.Current().Details(); }
        private void ShowExpansion()
        {
            CreateModal("EXPAND EQUIPMENT BAG"); expansionModal = true; quotedCapacity = Armory.Bag.Save.BagCapacity;
            UpdateExpansion();
            SetConfirmAction(() =>
            {
                var result = Armory.Bag.TryExpand(quotedCapacity);
                if (result == BagExpansionResult.Success) { CloseModal(); MarkDirty(); }
                else { quotedCapacity = Armory.Bag.Save.BagCapacity; UpdateExpansion(); }
            });
            GameController.CurrenciesManager.GetCurrency(art.expansionPrice.CurrencyId, false).OnAmountChanged += OnMoney;
        }
        private void OnMoney(CurrencySave wallet) { if (expansionModal) UpdateExpansion(); }
        private void UpdateExpansion()
        {
            var bag = Armory.Bag;
            modalText.text = "Capacity: " + bag.Save.BagCapacity + "  >  " + bag.NextCapacity + "\n\nCost: " + art.expansionPrice.Amount + " " + art.expansionPrice.CurrencyId + "\n\nEquipped items use bag slots. Materials and fragments have separate storage.\n\n" + (bag.GetState() == BagExpansionResult.InsufficientFunds ? "Not enough gold." : "Stored overflow rewards are delivered after expansion.");
            confirm.GetComponentInChildren<TMP_Text>().text = "EXPAND"; confirm.interactable = bag.GetState() == BagExpansionResult.Ready;
        }
        public void CloseModal()
        {
            if (!modal || !modal.gameObject.activeSelf) return;
            GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancel;
            if (expansionModal) GameController.CurrenciesManager.GetCurrency(art.expansionPrice.CurrencyId, false).OnAmountChanged -= OnMoney;
            expansionModal = false; modal.gameObject.SetActive(false);
            GameController.MainMenuScreenBehavior.EnableDock(); Focus();
        }
        private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context) => CloseModal();
        private void OnDestroy() { if (listening) Hide(); }
        [Serializable] private sealed class Cell { public Button button; public Image icon, lockIcon; public TMP_Text label; [NonSerialized] public ItemSaveData item; [NonSerialized] public InventoryStack stack; [NonSerialized] public ResponsiveMenuLayout.Placement iconPlacement; [NonSerialized] public Color iconColor, frameColor; }
#if UNITY_EDITOR
        private void Skin(Image image, Color? tint = null)
        { image.sprite = art.slotSprite; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.5f; image.material = art.slotMaterial; image.color = tint ?? new Color(.9f, .96f, 1); image.raycastTarget = false; }
        private TMP_Text Text(Transform parent, string name, string text, float size, float x, float y, float right, float top)
        { var label = Rect(parent, name, x, y, right, top).gameObject.AddComponent<TextMeshProUGUI>(); Style(label, size); label.text = text; return label; }
        private void Style(TMP_Text label, float size)
        { label.font = art.font; label.fontSize = size * Scale; label.enableAutoSizing = true; label.fontSizeMax = size * Scale; label.fontSizeMin = size * Scale * .65f; label.color = Ink; label.alignment = TextAlignmentOptions.MidlineLeft; label.raycastTarget = false; }
        private Button Button(Transform parent, string name, string text, float x, float y, float right, float top, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(parent, name, x, y, right, top); var image = rect.gameObject.AddComponent<Image>(); Skin(image); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Text(rect, "Label", text, 22, .05f, .05f, .95f, .95f); label.alignment = TextAlignmentOptions.Center;
            if (action != null) button.onClick.AddListener(action); return button;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float right, float top)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); Place(rect, x, y, right, top); return rect; }
#endif
        private static void Place(RectTransform rect, float x, float y, float right, float top)
        { rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(right, top); rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one; }
    }
}
