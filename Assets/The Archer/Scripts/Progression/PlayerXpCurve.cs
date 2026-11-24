using System;
using UnityEngine;

namespace MrX.Progression
{
    public readonly struct PlayerLevelProgress
    {
        public int Level { get; }
        public int XpIntoLevel { get; }
        public int XpToNextLevel { get; }
        public bool IsMaxLevel => XpToNextLevel == 0;

        internal PlayerLevelProgress(int level, int xpIntoLevel, int xpToNextLevel)
        {
            Level = level;
            XpIntoLevel = xpIntoLevel;
            XpToNextLevel = xpToNextLevel;
        }
    }

    public static class PlayerXpCurve
    {
        public const int MaxLevel = 100;
        private const int FirstLevelCost = 300;
        private const int CostIncreasePerLevel = 50;
        private const int LevelsToGain = MaxLevel - 1;

        private const int DefaultMaxTotalXp = LevelsToGain * FirstLevelCost
            + CostIncreasePerLevel * LevelsToGain * (LevelsToGain - 1) / 2;

        private static AccountProgressionConfig config;
        internal static AccountProgressionConfig Config =>
            config != null ? config : config = Resources.Load<AccountProgressionConfig>("Account Progression");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetConfig() => config = null;

        public static int MaxTotalXp => Config != null && Config.LevelCount > 0
            ? Config.TotalXp : DefaultMaxTotalXp;

        public static PlayerLevelProgress Evaluate(int totalXp)
        {
            int remaining = Math.Min(Math.Max(0, totalXp), MaxTotalXp);
            var table = Config;
            int maxLevel = table != null && table.LevelCount > 0 ? table.LevelCount : MaxLevel;

            for (int level = 1; level < maxLevel; level++)
            {
                int cost = table != null && table.LevelCount > 0 ? table.Cost(level - 1) : FirstLevelCost + (level - 1) * CostIncreasePerLevel;
                if (remaining < cost)
                    return new PlayerLevelProgress(level, remaining, cost);

                remaining -= cost;
            }

            return new PlayerLevelProgress(maxLevel, 0, 0);
        }
    }
}
