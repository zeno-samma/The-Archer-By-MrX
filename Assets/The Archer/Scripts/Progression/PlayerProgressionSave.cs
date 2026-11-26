using System;
using MrX.Save;
using UnityEngine;

namespace MrX.Progression
{
    [Serializable]
    public sealed class PlayerProgressionSave : ISave
    {
        [SerializeField] private int totalXp;
        [SerializeField] private int pendingRunXp;
        [SerializeField] private bool runOpen;
        [SerializeField] private double totalXpFraction;
        [SerializeField] private double pendingRunXpFraction;

        public int TotalXp => Math.Min(Math.Max(0, totalXp), PlayerXpCurve.MaxTotalXp);
        internal bool IsRunOpen => runOpen;

        internal void BeginRun(bool resumeRun)
        {
            if (!resumeRun || !runOpen)
            {
                pendingRunXp = 0;
                pendingRunXpFraction = 0d;
            }

            runOpen = true;
        }

        internal void RecordEarnedXp(float earnedXp)
        {
            if (!runOpen || !(earnedXp > 0f) || float.IsInfinity(earnedXp)) return;

            double capacity = Math.Max(0d, PlayerXpCurve.MaxTotalXp - TotalXp - totalXpFraction);
            double pending = Math.Min(capacity, pendingRunXp + pendingRunXpFraction + earnedXp);
            pendingRunXp = (int)pending;
            pendingRunXpFraction = pending - pendingRunXp;
        }

        private double SettledTotal => Math.Min(PlayerXpCurve.MaxTotalXp,
            TotalXp + totalXpFraction + pendingRunXp + pendingRunXpFraction);

        internal int PendingAwardXp => runOpen ? (int)SettledTotal - TotalXp : 0;

        internal int SettleRun()
        {
            if (!runOpen) return 0;

            int previousTotal = TotalXp;
            double settled = SettledTotal;
            totalXp = (int)settled;
            totalXpFraction = settled - totalXp;
            pendingRunXp = 0;
            pendingRunXpFraction = 0d;
            runOpen = false;
            return totalXp - previousTotal;
        }

        internal int AwardResultBonusXp(int amount)
        {
            if (runOpen || amount <= 0) return 0;

            int previous = TotalXp;
            totalXp = (int)Math.Min(PlayerXpCurve.MaxTotalXp, (long)previous + amount);
            if (totalXp == PlayerXpCurve.MaxTotalXp) totalXpFraction = 0d;
            return totalXp - previous;
        }

        internal bool SetDeveloperXp(int amount)
        {
            if (runOpen) return false;
            totalXp = Math.Min(Math.Max(0, amount), PlayerXpCurve.MaxTotalXp);
            totalXpFraction = 0d;
            return true;
        }

        public void Flush()
        {
        }
    }
}
