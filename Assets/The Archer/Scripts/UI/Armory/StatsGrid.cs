using OctoberStudio.Armory;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.UI.Armory
{
    public class StatsGrid : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected GameObject statsIndicatorPrefab;
        [SerializeField] protected Vector2 spacing = new Vector2(10, 10);

        [Space]
        [SerializeField] protected List<StatIcon> statsIcons;

        protected PoolComponent<StatIndicator> indicatorsPool;

        protected List<StatIndicator> indicators = new List<StatIndicator>();

        protected float minHeight;

        public float Height => rectTransform.sizeDelta.y;

        protected virtual void Awake()
        {
            indicatorsPool = new PoolComponent<StatIndicator>(statsIndicatorPrefab, 10, transform);

            minHeight = rectTransform.sizeDelta.y;
        }

        public virtual void Init(HeroData data, bool isUnlocked, int level)
        {
            indicatorsPool.DisableAllEntities();
            indicators.Clear();

            var heroLevel = data.GetHeroLevel(level);

            if (level + 1 < data.HeroLevelCount && isUnlocked)
            {
                var nextHeroLevel = data.GetHeroLevel(level + 1);

                InitDoubleLevel(heroLevel.Stats, nextHeroLevel.Stats);
            }
            else
            {
                InitSingleLevel(heroLevel.Stats);
            }

            RecalculateSize();
        }

        public virtual void Init(ItemData data, bool isUnlocked, int level)
        {
            indicatorsPool.DisableAllEntities();
            indicators.Clear();

            var itemLevel = data.GetItemLevel(level);

            if (level + 1 < data.ItemLevelCount && isUnlocked)
            {
                var nextItemLevel = data.GetItemLevel(level + 1);

                InitDoubleLevel(itemLevel.Stats, nextItemLevel.Stats);
            }
            else
            {
                InitSingleLevel(itemLevel.Stats);
            }

            RecalculateSize();
        }

        protected virtual void RecalculateSize()
        {
            var x = 0f;
            var y = 0f;

            var width = rectTransform.sizeDelta.x;

            for (int i = 0; i < indicators.Count; i++)
            {
                var indicator = indicators[i];

                indicator.RectTransform.anchoredPosition = new Vector2(x, y);

                x += indicator.RectTransform.sizeDelta.x + spacing.x;

                if (x + indicator.RectTransform.sizeDelta.x > width)
                {
                    x = 0f;
                    y -= indicator.RectTransform.sizeDelta.y + spacing.y;
                }
            }

            var height = Mathf.Max(minHeight, -y + indicators[0].RectTransform.sizeDelta.y);

            rectTransform.SetSizeDeltaY(height);
        }

        protected virtual void InitSingleLevel(List<StatData> statList)
        {
            for (int i = 0; i < statList.Count; i++)
            {
                var stat = statList[i];

                if (!stat.IsVisibleOnUI) continue;

                var indicator = indicatorsPool.GetEntity();
                indicator.Init(GetIcon(stat.StatType), stat.Value);

                indicators.Add(indicator);
            }
        }

        protected virtual void InitDoubleLevel(List<StatData> statList, List<StatData> nextStatList)
        {
            for (int i = 0; i < statList.Count; i++)
            {
                var stat = statList[i];
                if (!stat.IsVisibleOnUI) continue;

                var nextStatValue = HasStat(nextStatList, stat.StatType) ? GetStatValue(nextStatList, stat.StatType) : stat.Value;

                var indicator = indicatorsPool.GetEntity();
                if (!Mathf.Approximately(stat.Value, nextStatValue))
                {
                    indicator.Init(GetIcon(stat.StatType), stat.Value, nextStatValue);
                }
                else
                {
                    indicator.Init(GetIcon(stat.StatType), stat.Value);
                }

                indicators.Add(indicator);
            }

            for (int i = 0; i < nextStatList.Count; i++)
            {
                var nextStat = nextStatList[i];

                if (!nextStat.IsVisibleOnUI) continue;

                if (!HasStat(statList, nextStat.StatType))
                {
                    var indicator = indicatorsPool.GetEntity();
                    indicator.Init(GetIcon(nextStat.StatType), GetStatValue(statList, nextStat.StatType), nextStat.Value);
                    indicators.Add(indicator);
                }
            }
        }

        protected virtual bool HasStat(List<StatData> statsList, StatType statType)
        {
            foreach (var stat in statsList)
            {
                if (stat.StatType == statType)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual float GetStatValue(List<StatData> statsList, StatType statType)
        {
            foreach (var stat in statsList)
            {
                if (stat.StatType == statType)
                {
                    return stat.Value;
                }
            }
            return GetDefaultStatValue(statType);
        }

        protected virtual float GetDefaultStatValue(StatType statType)
        {
            switch (statType)
            {
                case StatType.HP: return 10f;
                case StatType.Damage: return 1f;
                case StatType.AttackSpeed: return 1f;
                case StatType.MovementSpeed: return 1f; // Default speed value
                case StatType.CriticalChance: return 0f;
                case StatType.DamageReduction: return 1.5f;
                case StatType.HPRecovery: return 0f;
                default: return 0;

            }
        }

        protected virtual Sprite GetIcon(StatType statType)
        {
            for (int i = 0; i < statsIcons.Count; i++)
            {
                if (statsIcons[i].StatType == statType)
                {
                    return statsIcons[i].Icon;
                }
            }

            return null;
        }

        protected virtual void Clear()
        {
            for (int i = 0; i < indicators.Count; i++)
            {
                indicators[i].gameObject.SetActive(false);
            }

            indicators.Clear();
        }

        [System.Serializable]
        protected class StatIcon
        {
            [SerializeField] protected StatType statType;
            [SerializeField] protected Sprite icon;

            public StatType StatType => statType;
            public Sprite Icon => icon;
        }
    }
}