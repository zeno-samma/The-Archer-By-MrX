using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    /// <summary>Only binds data to authored prefab views. No runtime UI construction.</summary>
    public sealed class UpgradeDashboardPage : MainMenuPageBehavior
    {
        [SerializeField, Tooltip("Opt in to gameplay state colors. Off preserves Image/TMP colors authored in Inspector.")] private bool useStateColors;
        [Serializable] public sealed class Row
        {
            public UpgradeData data;
            public Image icon, background, buyImage;
            public TMP_Text title, description, level, values, price;
            public Button select, buy;
            public Image[] segments;
            [NonSerialized] public int quotedLevel;
        }
        [SerializeField] private List<Row> rows = new List<Row>();
        [SerializeField] private Image selectedIcon;
        [SerializeField] private TMP_Text selectedTitle, selectedDescription, selectedLevel, selectedEffect, selectedCost, walletLabel, feedback;
        [SerializeField] private TMP_Text[] currentStats, nextStats;
        [SerializeField] private Button upgradeButton;
        [SerializeField, HideInInspector] private Sprite blueButton, yellowButton, grayButton;
        [Header("Selection and level presentation")]
        [SerializeField] private Color selectedRowColor = new Color(1, .97f, .85f);
        [SerializeField] private Color idleRowColor = new Color(.9f, .95f, 1);
        [SerializeField] private Color filledSegmentColor = new Color(.22f, .57f, 1);
        [SerializeField] private Color emptySegmentColor = new Color(.72f, .83f, .95f);
        private readonly List<CurrencySave> observedWallets = new List<CurrencySave>();
        private PermanentUpgradeService purchases;
        private Row selected;
        private bool dirty = true, visible;
        protected override void Start()
        {
            base.Start();
            purchases = new PermanentUpgradeService(GameController.UpgradesManager, GameController.CurrenciesManager, GameController.SaveManager);
            foreach (var row in rows)
            {
                var captured = row;
                row.select.onClick.AddListener(() => Select(captured)); row.buy.onClick.AddListener(() => Buy(captured));
                row.data.onUpgradeLevelChanged += OnUpgrade;
                foreach (var level in GetPrices(row.data))
                {
                    var wallet = GameController.CurrenciesManager.GetCurrency(level.CurrencyId, false);
                    if (wallet != null && !observedWallets.Contains(wallet)) { observedWallets.Add(wallet); wallet.OnAmountChanged += OnWallet; }
                }
            }
            upgradeButton.onClick.AddListener(() => { if (selected != null) Buy(selected); });
            GameController.ArmoryManager.OnItemEquipmentChanged += MarkDirty;
            GameController.ArmoryManager.OnSelectedHeroChanged += MarkDirty;
            selected = rows.Count > 0 ? rows[0] : null; Refresh();
        }
        private static IEnumerable<Price> GetPrices(UpgradeData data)
        { for (int i = 0; i < data.LevelsCount; i++) if (data.GetLevel(i).Price != null) yield return data.GetLevel(i).Price; }
        private void MarkDirty() { dirty = true; }
        private void OnUpgrade(int level) => MarkDirty();
        private void OnWallet(CurrencySave wallet) => MarkDirty();
        private void LateUpdate() { if (visible && dirty && purchases != null) Refresh(); }
        public override void MoveCenter()
        { visible = true; dirty = true; canvasGroup.interactable = canvasGroup.blocksRaycasts = true; base.MoveCenter(); }
        public override void MoveLeft() { Leave(); base.MoveLeft(); }
        public override void MoveRight() { Leave(); base.MoveRight(); }
        private void Leave() { visible = false; canvasGroup.interactable = canvasGroup.blocksRaycasts = false; }
        protected override void OnMoveCenterFinished()
        { if (selected != null && EventSystem.current) EventSystem.current.SetSelectedGameObject(selected.select.gameObject); }
        private void Select(Row row) { selected = row; feedback.text = "Preview includes equipped gear and permanent upgrades."; Refresh(); }
        private void Buy(Row row)
        {
            if (!visible || !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            selected = row;
            var result = purchases.TryBuy(row.data.UpgradeType, row.quotedLevel);
            feedback.text = result == UpgradePurchaseResult.Success ? "Upgrade complete!" : result == UpgradePurchaseResult.InsufficientFunds ? "Not enough gold." : "Upgrade unavailable. Please check the current level.";

            Refresh();
        }
        private void Refresh()
        {
            dirty = false;
            foreach (var row in rows)
            {
                var state = purchases.Quote(row.data.UpgradeType, out var level, out var price); row.quotedLevel = level;
                var max = state == UpgradePurchaseResult.Maximum;
                row.icon.sprite = row.data.Icon; row.title.text = row.data.Title; row.description.text = row.data.Description;
                row.level.text = "Lv. " + (Mathf.Clamp(level, -1, row.data.LevelsCount - 1) + 1) + (max ? "  MAX" : "");
                row.values.text = Effect(row.data, level) + (max ? "" : "  >  " + Effect(row.data, level + 1));
                row.price.text = max ? "MAX" : price == null ? "--" : price.Amount.ToString("N0");
                row.buy.interactable = state == UpgradePurchaseResult.Ready;
                if (useStateColors) row.background.color = row == selected ? selectedRowColor : idleRowColor;
                for (int i = 0; i < row.segments.Length; i++) row.segments[i].color = (i + 1f) / row.segments.Length <= (level + 1f) / Mathf.Max(1, row.data.LevelsCount) ? filledSegmentColor : emptySegmentColor;
            }
            walletLabel.text = GameController.CurrenciesManager.GetCurrency("gold", false).Amount.ToString("N0");
            if (selected == null) return;
            selectedIcon.sprite = selected.data.Icon; selectedTitle.text = selected.data.Title; selectedDescription.text = selected.data.Description;
            var quote = purchases.Quote(selected.data.UpgradeType, out var current, out var cost);
            bool hasNext = current >= -1 && current + 1 < selected.data.LevelsCount;
            selectedLevel.text = hasNext ? "Lv. " + (current + 1) + "  >  Lv. " + (current + 2) : "MAX LEVEL";
            selectedEffect.text = Effect(selected.data, current) + (hasNext ? "  >  " + Effect(selected.data, current + 1) : "");
            selectedCost.text = cost == null ? "MAX LEVEL" : cost.Amount.ToString("N0") + " " + cost.CurrencyId.ToUpperInvariant() + "\nUPGRADE";
            upgradeButton.interactable = quote == UpgradePurchaseResult.Ready;
            var now = ArmoryStatsSnapshot.Current();
            var preview = hasNext ? ArmoryStatsSnapshot.Current(new PreviewUpgrades(GameController.UpgradesManager, selected.data.UpgradeType, current + 1)) : now;
            float[] before = { now.Total.AttackDamageStat.Value, now.Total.MaxHPStat.Value, now.Total.CritChanceStat.Value, now.Total.MovementSpeedStat.Value, now.Total.AttackSpeedStat.Value };
            float[] after = { preview.Total.AttackDamageStat.Value, preview.Total.MaxHPStat.Value, preview.Total.CritChanceStat.Value, preview.Total.MovementSpeedStat.Value, preview.Total.AttackSpeedStat.Value };
            for (int i = 0; i < currentStats.Length; i++)
            { var unit = i == 2 ? "%" : ""; currentStats[i].text = before[i].ToString("0.##") + unit; nextStats[i].text = "> " + after[i].ToString("0.##") + unit + "  (" + (after[i] >= before[i] ? "+" : "") + (after[i] - before[i]).ToString("0.##") + unit + ")"; }
        }
        public static string Effect(UpgradeData data, int level)
        {
            if (data == null || data.LevelsCount == 0) return "--";
            float value = level < 0 ? data.InitialValue : data.GetLevel(Mathf.Min(level, data.LevelsCount - 1)).Value;
            switch (data.UpgradeType)
            {
                case UpgradeType.MorePower: case UpgradeType.BaseHp: case UpgradeType.AttackSpeed: case UpgradeType.MoveSpeed:
                    if (level < 0) value = 1;
                    return "+" + ((value - 1) * 100).ToString("0.##") + "%";
                case UpgradeType.Revive: return "+" + value.ToString("0");
                default: return "+" + value.ToString("0.##") + "%";
            }
        }
        private void OnDestroy()
        {
            foreach (var row in rows) if (row.data) row.data.onUpgradeLevelChanged -= OnUpgrade;
            foreach (var wallet in observedWallets) wallet.OnAmountChanged -= OnWallet;
            if (GameController.ArmoryManager != null)
            { GameController.ArmoryManager.OnItemEquipmentChanged -= MarkDirty; GameController.ArmoryManager.OnSelectedHeroChanged -= MarkDirty; }
            moveEasingCoroutine?.Stop(); alphaEasingCoroutine?.Stop();
        }
        private sealed class PreviewUpgrades : IUpgradesManager
        {
            private readonly IUpgradesManager source; private readonly UpgradeType type; private readonly int level;
            public PreviewUpgrades(IUpgradesManager source, UpgradeType type, int level) { this.source = source; this.type = type; this.level = level; }
            public int GetUpgradeLevel(UpgradeType value) => value == type ? level : source.GetUpgradeLevel(value);
            public bool IsUpgradeAquired(UpgradeType value) => GetUpgradeLevel(value) >= 0;
            public UpgradeData GetUpgradeData(UpgradeType value) => source.GetUpgradeData(value);
            public List<UpgradeData> GetAllUpgrades() => source.GetAllUpgrades();
            public void IncrementUpgradeLevel(UpgradeType value) => throw new InvalidOperationException("Read-only preview");
            public void OnStageLoaded() { } public void OnStageUnloaded() { }
        }
    }
}
