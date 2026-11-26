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

        public static int RequiredLevel(UI.MainMenuPageType page) =>
            PlayerXpCurve.Config != null ? PlayerXpCurve.Config.RequiredLevel(page) : 0;

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

        // A saved run can be closed from the menu before BeginRun loads it into memory.
        public static int SettleRun()
        {
            int awardedXp = (activeRun ?? Profile)?.SettleRun() ?? 0;
            activeRun = null;
            return awardedXp;
        }

        internal static int AwardResultBonusXp(int amount) => Profile?.AwardResultBonusXp(amount) ?? 0;

        public static int TotalXp => Profile?.TotalXp ?? 0;

        internal static bool ChangeDeveloperXp(int amount, bool reset)
        {
            var profile = Profile;
            if (profile == null || (!reset && amount <= 0)) return false;

            if (profile.IsRunOpen)
            {
                var continuation = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
                if (!GameController.MainMenuScreenBehavior || continuation.HasUnfinishedStageData) return false;

                // Repair saves whose checkpoint was removed before its XP was settled.
                SettleRun();
            }

            int next = reset ? 0 : (int)Math.Min(PlayerXpCurve.MaxTotalXp, (long)profile.TotalXp + amount);
            if (!profile.SetDeveloperXp(next)) return false;

            GameController.SaveManager.Save(false);
            Changed?.Invoke();
            return true;
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
