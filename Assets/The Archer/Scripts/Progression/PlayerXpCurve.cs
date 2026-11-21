using System;

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

        public const int MaxTotalXp = LevelsToGain * FirstLevelCost
            + CostIncreasePerLevel * LevelsToGain * (LevelsToGain - 1) / 2;

        public static PlayerLevelProgress Evaluate(int totalXp)
        {
            int remaining = Math.Min(Math.Max(0, totalXp), MaxTotalXp);
            int cost = FirstLevelCost;

            for (int level = 1; level < MaxLevel; level++)
            {
                if (remaining < cost)
                    return new PlayerLevelProgress(level, remaining, cost);

                remaining -= cost;
                cost += CostIncreasePerLevel;
            }

            return new PlayerLevelProgress(MaxLevel, 0, 0);
        }
    }
}
