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

        internal int SettleRun()
        {
            if (!runOpen) return 0;

            int previousTotal = TotalXp;
            double settled = Math.Min(PlayerXpCurve.MaxTotalXp,
                previousTotal + totalXpFraction + pendingRunXp + pendingRunXpFraction);
            totalXp = (int)settled;
            totalXpFraction = settled - totalXp;
            pendingRunXp = 0;
            pendingRunXpFraction = 0d;
            runOpen = false;
            return totalXp - previousTotal;
        }

        public void Flush()
        {
        }
    }
}
