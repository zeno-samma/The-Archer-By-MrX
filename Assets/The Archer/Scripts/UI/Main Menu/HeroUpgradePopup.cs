using System.Collections.Generic;
using MrX.Armory;
using MrX.Currency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    internal sealed class HeroUpgradePopup
    {
        private static readonly Color Insufficient = new Color32(255, 142, 142, 255);
        private readonly RectTransform root, statsGroup;
        private readonly Image portrait, currencyIcon;
        private readonly TMP_Text heroName, currentLevel, nextLevel, currencyCost, shardCost;
        private readonly Button confirm, cancel, close;
        private readonly List<StatRow> rows = new();

        public HeroUpgradePopup(RectTransform root, UnityAction onConfirm, UnityAction onCancel)
        {
            this.root = root;
            var panel = root.Find("Popup");
            var header = panel.Find("Group_Top");
            heroName = header.Find("Text_Tiitle_BoxingGlove").GetComponent<TMP_Text>();
            portrait = header.Find("Icon_Frame/Image_Glove").GetComponent<Image>();
            currentLevel = header.Find("Lvup_Panel/Text_Current").GetComponent<TMP_Text>();
            nextLevel = header.Find("Lvup_Panel/Text_Up").GetComponent<TMP_Text>();
            statsGroup = (RectTransform)panel.Find("Group_Stats");
            foreach (RectTransform row in statsGroup)
                rows.Add(new StatRow(row));
            var costs = (RectTransform)panel.Find("Bot_Panel");
            currencyIcon = costs.Find("Image").GetComponent<Image>();
            currencyCost = costs.Find("Text (TMP)").GetComponent<TMP_Text>();
            shardCost = costs.Find("Text (TMP) (1)").GetComponent<TMP_Text>();
            confirm = panel.Find("Button_Upgrade").GetComponent<Button>();
            cancel = panel.Find("Button_Equip").GetComponent<Button>();
            close = panel.Find("Button_Close").GetComponent<Button>();
            confirm.onClick.AddListener(onConfirm);
            cancel.onClick.AddListener(onCancel);
            close.onClick.AddListener(onCancel);
        }

        public void Show(HeroData hero, int index, HeroSaveData save, Price price, CurrencySave currency)
        {
            heroName.text = hero.Name;
            portrait.sprite = hero.Icon;
            portrait.enabled = hero.Icon != null;
            currentLevel.text = (index + 1).ToString();
            nextLevel.text = (index + 2).ToString();

            var current = hero.GetHeroLevel(index);
            var nextStats = hero.GetHeroLevel(index + 1).Stats;
            int count = 0;
            var seen = new HashSet<StatType>();
            if (nextStats != null)
            {
                foreach (var stat in nextStats)
                {
                    if (stat == null || !seen.Add(stat.StatType) || !stat.IsVisibleOnUI) continue;
                    if (count == rows.Count)
                        rows.Add(new StatRow(Object.Instantiate(rows[0].Rect, statsGroup, false), rows[0].IncreaseColor));
                    rows[count++].Bind(stat, current.GetStatValue(stat.StatType));
                }
            }
            if (count == 0) rows[count++].Bind(null, 0f);
            for (int i = count; i < rows.Count; i++) rows[i].Rect.gameObject.SetActive(false);

            BindCosts(price, currency, save, hero.HeroLevelCount);
            root.SetAsLastSibling();
            root.gameObject.SetActive(true);
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(cancel.gameObject);
        }

        public void BindCosts(Price price, CurrencySave currency, HeroSaveData save, int levelCount)
        {
            bool enoughCurrency = currency != null && currency.CanAfford(price.Amount);
            bool enoughShards = save != null && save.CanUpgradeWithShards(levelCount);
            if (currency?.Data.Icon) currencyIcon.sprite = currency.Data.Icon;
            currencyCost.text = currency == null ? "Unavailable" : $"{currency.Data.Name}\n{currency.Amount:N0} / {price.Amount:N0}";
            shardCost.text = save == null ? "Unavailable" : $"Hero Shards\n{save.Shards:N0} / {save.ShardsToUpgrade:N0}";
            currencyCost.color = enoughCurrency ? Color.white : Insufficient;
            shardCost.color = enoughShards ? Color.white : Insufficient;
            confirm.interactable = enoughCurrency && enoughShards;
            confirm.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = cancel, selectOnUp = close };
            cancel.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = confirm.interactable ? confirm : null, selectOnUp = close };
            close.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = cancel };
        }

        public void Hide()
        {
            if (root) root.gameObject.SetActive(false);
        }

        private sealed class StatRow
        {
            public RectTransform Rect { get; }
            public Color IncreaseColor { get; }
            private readonly TMP_Text label, current, delta;

            public StatRow(RectTransform rect, Color? increaseColor = null)
            {
                Rect = rect;
                label = rect.Find("Text_AttackDamage").GetComponent<TMP_Text>();
                current = rect.Find("Text_current").GetComponent<TMP_Text>();
                delta = rect.Find("Text_UP").GetComponent<TMP_Text>();
                IncreaseColor = increaseColor ?? delta.color;
            }

            public void Bind(StatData stat, float value)
            {
                Rect.gameObject.SetActive(true);
                label.text = stat == null ? "No stat changes shown" : StatName(stat.StatType);
                var unit = stat?.StatType == StatType.CriticalChance ? "%" : "";
                current.text = stat == null ? "" : value.ToString("0.##") + unit;
                float difference = stat == null ? 0f : Mathf.Round((stat.Value - value) * 100f) / 100f;
                delta.text = stat == null ? "" : difference.ToString("+0.##;-0.##;0") + unit;
                delta.color = difference < 0f ? Insufficient : difference > 0f ? IncreaseColor : Color.white;
            }

            private static string StatName(StatType stat) => stat switch
            {
                StatType.Damage => "Attack Damage",
                StatType.HP => "HP",
                StatType.AttackSpeed => "Attack Speed",
                StatType.MovementSpeed => "Move Speed",
                StatType.HPRecovery => "HP Recovery",
                StatType.CriticalChance => "Critical Chance",
                StatType.DamageReduction => "Damage Reduction",
                _ => stat.ToString()
            };
        }
    }
}
