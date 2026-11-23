using System;
using UnityEngine;

namespace MrX.Progression
{
    public static class PlayerProgression
    {
        private const string SaveKey = "ArcBorn Player Progression";
        private static PlayerProgressionSave activeRun;

        private static PlayerProgressionSave Profile =>
            GameController.SaveManager?.GetSave<PlayerProgressionSave>(SaveKey);

        public static event Action Changed;

        public static PlayerLevelProgress Current => PlayerXpCurve.Evaluate(Profile?.TotalXp ?? 0);

        public static int PendingAwardXp => activeRun?.PendingAwardXp ?? 0;

        public static void BeginRun(bool resumeRun)
        {
            activeRun = Profile;
            activeRun?.BeginRun(resumeRun);
        }

        public static void RecordEarnedXp(float earnedXp)
        {
            activeRun?.RecordEarnedXp(earnedXp);
        }

        // Call inside the existing reward batch, before it is committed.
        public static int SettleRun()
        {
            int awardedXp = activeRun?.SettleRun() ?? 0;
            activeRun = null;
            return awardedXp;
        }

        public static void NotifyRunSettled(int awardedXp)
        {
            if (awardedXp > 0)
                Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            activeRun = null;
            Changed = null;
        }
    }
}
