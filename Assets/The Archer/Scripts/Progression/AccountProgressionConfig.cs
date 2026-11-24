using System;
using MrX.UI;
using UnityEngine;

namespace MrX.Progression
{
    [CreateAssetMenu(menuName = "MrX Studio/Account Progression")]
    public sealed class AccountProgressionConfig : ScriptableObject
    {
        [Serializable]
        private struct Level
        {
            [Min(1)] public int xpToNextLevel;
            public MainMenuPageType[] unlocks;
        }

        [Tooltip("Element 0 is level 1. The final level ignores XP to next level.")]
        [SerializeField] private Level[] levels;

        internal int LevelCount => levels?.Length ?? 0;
        internal int Cost(int index) => Mathf.Max(1, levels[index].xpToNextLevel);

        internal int TotalXp
        {
            get
            {
                long total = 0;
                for (int i = 0; i < LevelCount - 1; i++) total += Cost(i);
                return (int)Math.Min(total, int.MaxValue);
            }
        }

        internal int RequiredLevel(MainMenuPageType page)
        {
            for (int i = 0; i < LevelCount; i++)
                if (levels[i].unlocks != null && Array.IndexOf(levels[i].unlocks, page) >= 0)
                    return i + 1;
            return 0;
        }
    }
}
