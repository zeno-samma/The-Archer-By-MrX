using OctoberStudio.Abilities;
using OctoberStudio.Currency;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Armory
{
    [CreateAssetMenu(fileName = "Hero Data 001", menuName = "October/Armory/Hero Data")]
    public class HeroData: ScriptableObject
    {
        [SerializeField, HideInInspector] protected int dataVersion;
        public int DataVersion => dataVersion;

        [SerializeField, HideInInspector] protected string guid;

        [SerializeField] protected string heroName;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected GameObject prefab;

        [Space]
        [SerializeField] protected bool isDefaultHero;
        [SerializeField] protected bool isUnlockedByDefault;

        [Space]
        [SerializeField] protected List<HeroLevel> heroLevels;

        public string Id => guid;

        public Sprite Icon => icon;
        public string Name => heroName;
        public GameObject Prefab => prefab;

        public bool IsDefaultHero => isDefaultHero;
        public bool IsUnlockedByDefault => isUnlockedByDefault;

        public int HeroLevelCount => heroLevels.Count;
        public List<HeroLevel> HeroLevels => heroLevels;

        public virtual HeroLevel GetHeroLevel(int levelId)
        {
            return heroLevels[levelId];
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
    public class HeroLevel
    {
        [Obsolete("Migrating to Price")]
        [SerializeField, HideInInspector] protected int cost;
        [SerializeField] protected Price price;

        [Space]
        [FormerlySerializedAs("itemRarity")]
        [SerializeField] protected ItemRarityType heroRarity;
        [SerializeField] protected List<StatData> stats;
        [SerializeField] protected List<AbilityType> attachedAbilities;

        [Obsolete("Migrating to Price")]
        public int Cost => cost;
        public Price Price => price;

        public ItemRarityType HeroRarity => heroRarity;
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
                case StatType.AttackSpeed: return 0f;
                case StatType.MovementSpeed: return 0f; // Default speed value
                case StatType.CriticalChance: return 0f;
                case StatType.DamageReduction: return 0f;
                case StatType.HPRecovery: return 0f;
                default: return 0;

            }
        }
    }
}