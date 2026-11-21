using MrX.Armory;
using UnityEngine;

namespace MrX.Progression
{
    public static class HeroProgression
    {
        public static int GetEffectiveLevelIndex(HeroData hero, HeroSaveData heroSave)
        {
            if (!heroSave.IsUnlocked)
                return 0;

            int profileLevelIndex = PlayerProgression.Current.Level - 1;
            int effectiveLevelIndex = Mathf.Max(heroSave.Level, profileLevelIndex);
            return Mathf.Clamp(effectiveLevelIndex, 0, hero.HeroLevelCount - 1);
        }
    }
}
