using System;
using System.Collections.Generic;
using UnityEngine;

namespace MrX.Rewards
{
    [CreateAssetMenu(menuName = "MrX Studio/Loot/Boss Chest Rewards")]
    public sealed class BossChestRewards : ScriptableObject
    {
        public List<LuckyPrize> prizes = new List<LuckyPrize>();
        public bool IsValid(out double total)
        {
            total = 0; var ids = new HashSet<string>(StringComparer.Ordinal);
            if (prizes == null) return false;
            foreach (var p in prizes)
            {
                if (p == null || !p.enabled) continue;
                if (float.IsNaN(p.weight) || float.IsInfinity(p.weight) || p.weight < 0) return false;
                if (p.weight == 0) continue;
                if (!p.item || string.IsNullOrEmpty(p.id) || !ids.Add(p.id) || p.minimumAmount < 1 || p.maximumAmount < p.minimumAmount || p.maximumAmount > 1000000) return false;
                if (p.kind != LuckyRewardKind.Equipment && p.kind != LuckyRewardKind.Fragments) return false;
                if (p.kind == LuckyRewardKind.Equipment && (p.minimumAmount != 1 || p.maximumAmount != 1 || p.maxLevelDuplicateGold < 1)) return false;
                total += p.weight;
            }
            return total > 0 && !double.IsInfinity(total);
        }
        public LuckyPrize Pick(double roll)
        {
            if (double.IsNaN(roll) || roll < 0 || roll > 1 || !IsValid(out var total)) return null;
            double target = total * roll, sum = 0; LuckyPrize last = null;
            foreach (var p in prizes)
            { if (p == null || !p.enabled || p.weight <= 0) continue; last = p; sum += p.weight; if (target < sum) return p; }
            return last;
        }
    }
}
