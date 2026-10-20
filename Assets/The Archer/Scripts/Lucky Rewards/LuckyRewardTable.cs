using System;
using System.Collections.Generic;
using MrX.Armory;
using MrX.Crafting;
using MrX.Currency;
using UnityEngine;

namespace MrX.Rewards
{
    public enum LuckyRewardKind { Currency = 0, Equipment = 1, Fragments = 2, Material = 3 }
    [Serializable]
    public sealed class LuckyPrize
    {
        public string id;
        public bool enabled = true;
        public string title;
        public Sprite icon;
        [Min(0)] public float weight = 1;
        public LuckyRewardKind kind;
        public string currencyId = "gold";
        public ItemData item;
        public CraftingMaterial material;
        [Min(1)] public int minimumAmount = 1, maximumAmount = 1;
        [Min(1), Tooltip("Gold compensation if duplicate equipment is already at maximum level.")]
        public int maxLevelDuplicateGold = 100;
    }
    [CreateAssetMenu(menuName = "MrX Studio/Lucky Rewards/Reward Table", fileName = "Lucky Reward Table")]
    public sealed class LuckyRewardTable : ScriptableObject
    {
        [Tooltip("Stable save identity. Do not change to rename the popup.")]
        public string id;
        public string title = "LUCKY WHEEL";
        public Price price = new Price("gem", 10);
        public List<LuckyPrize> prizes = new List<LuckyPrize>();
        [Min(.1f)] public float animationSeconds = 3.5f;
        public bool IsValid(out double total)
        {
            total = 0;
            if (string.IsNullOrEmpty(id) || price == null || price.CurrencyId != "gem" || price.Amount < 1 || prizes == null) return false;
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var prize in prizes)
            {
                if (prize == null || !prize.enabled) continue;
                if (float.IsNaN(prize.weight) || float.IsInfinity(prize.weight) || prize.weight < 0) return false;
                if (prize.weight == 0) continue;
                if (string.IsNullOrEmpty(prize.id) || !identities.Add(prize.id) || prize.minimumAmount < 1 || prize.maximumAmount < prize.minimumAmount || prize.maximumAmount > 1000000) return false;
                if (prize.kind == LuckyRewardKind.Equipment && (prize.minimumAmount != 1 || prize.maximumAmount != 1 || prize.maxLevelDuplicateGold < 1)) return false;
                total += prize.weight;
            }
            return total > 0 && !double.IsInfinity(total);
        }
        public int Pick(double roll)
        {
            if (!IsValid(out var total) || double.IsNaN(roll) || double.IsInfinity(roll) || roll < 0 || roll > 1) return -1;
            double cumulative = 0, target = roll * total; int last = -1;
            for (int i = 0; i < prizes.Count; i++)
            { var prize = prizes[i]; if (prize == null || !prize.enabled || prize.weight <= 0) continue; last = i; cumulative += prize.weight; if (target < cumulative) return i; }
            return last; // Unity Random.value may equal 1.
        }
    }
}
