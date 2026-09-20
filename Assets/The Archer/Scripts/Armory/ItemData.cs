using OctoberStudio.Abilities;
using OctoberStudio.Currency;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OctoberStudio.Armory
{
    [CreateAssetMenu(fileName = "Item Data 001", menuName = "October/Armory/Item Data")]
    public class ItemData: ScriptableObject
    {
        [SerializeField, HideInInspector] protected int dataVersion;
        public int DataVersion => dataVersion;

        [SerializeField, HideInInspector] protected string guid;
        [SerializeField] protected ItemType itemType;
        [SerializeField] protected string itemName;
        [SerializeField] protected Sprite icon;

        [Space]
        [SerializeField] protected bool isDefaultItem;
        [SerializeField] protected bool isUnlockedByDefault;

        [Space]
        [SerializeField] protected List<ItemLevel> itemLevels;

        public string Id => guid;

        public ItemType ItemType => itemType;
        public string ItemName => itemName;
        public Sprite Icon => icon;

        public bool IsDefaultItem => isDefaultItem;
        public bool IsUnlockedByDefault => isUnlockedByDefault;

        public int ItemLevelCount => itemLevels.Count;
        public List<ItemLevel> ItemLevels => itemLevels;

        public virtual ItemLevel GetItemLevel(int levelId)
        {
            if (levelId >= itemLevels.Count) return null;
            return itemLevels[levelId];
        }

        public virtual void RecalculateId()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this);
            guid = AssetDatabase.AssetPathToGUID(path);
            EditorUtility.SetDirty(this);
#endif
        }


        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(guid))
            {
                RecalculateId();
            }
        }
    }

    [System.Serializable]
    public class ItemLevel
    {
        [Obsolete("Migrating to Price")]
        [SerializeField, HideInInspector] protected int cost;
        [SerializeField] protected Price price;

        [Space]
        [SerializeField] protected ItemRarityType itemRarity;
        [SerializeField] protected List<StatData> stats;
        [SerializeField] protected List<AbilityType> attachedAbilities;

        [Obsolete("Migrating to Price")]
        public int Cost => cost;
        public Price Price => price;

        public ItemRarityType ItemRarity => itemRarity;
        public List<StatData> Stats => stats;
        public List<AbilityType> AttachedAbilities => attachedAbilities;

        public float GetStatValue(StatType statType)
        {
            foreach (var stat in stats)
            {
                if (stat.StatType == statType)
                {
                    return stat.Value;
                }
            }
            return GetDefaultStatValue(statType);
        }

        public bool HasStat(StatType statType)
        {
            foreach (var stat in stats)
            {
                if (stat.StatType == statType)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual float GetDefaultStatValue(StatType statType)
        {
            switch (statType)
            {
                case StatType.HP: return 0;
                case StatType.Damage: return 0;
                case StatType.AttackSpeed: return 1f; // multiplier
                case StatType.MovementSpeed: return 1f; // multiplier
                case StatType.CriticalChance: return 0f;
                case StatType.DamageReduction: return 1f; // multiplier
                case StatType.HPRecovery: return 0f;
                default: return 0;

            }
        }
    }
}