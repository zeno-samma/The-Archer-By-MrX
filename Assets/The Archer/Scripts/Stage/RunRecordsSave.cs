using System;
using System.Collections.Generic;
using MrX.Save;
using UnityEngine;

namespace MrX
{
    [Serializable]
    public sealed class RunPersonalBest
    {
        public string stageId;
        public StageDifficulty difficulty;
        public double fastestClearSeconds, highestDps;
    }

    [Serializable]
    public sealed class RunRecordsSave : ISave
    {
        [SerializeField] private List<RunPersonalBest> records = new List<RunPersonalBest>();
        public void Init() { if (records == null) records = new List<RunPersonalBest>(); }
        public void Flush() { }
        public RunPersonalBest Find(string stageId, StageDifficulty difficulty)
        { Init(); return records.Find(r => r != null && r.stageId == stageId && r.difficulty == difficulty); }
        public void Record(string stageId, StageDifficulty difficulty, RunCombatStatistics stats, bool victory)
        {
            if (stats == null || string.IsNullOrEmpty(stageId)) return;
            var previous = Find(stageId, difficulty);
            stats.previousBestTime = previous != null ? previous.fastestClearSeconds : 0;
            stats.previousBestDps = previous != null ? previous.highestDps : 0;
            stats.newTimeRecord = stats.newDpsRecord = false;
            if (!(stats.seconds > 0) || float.IsInfinity(stats.seconds) || float.IsNaN(stats.seconds)) return;
            double dps = stats.Dps;
            stats.newTimeRecord = victory && (stats.previousBestTime <= 0 || stats.seconds < stats.previousBestTime - .001);
            stats.newDpsRecord = dps > 0 && dps > stats.previousBestDps + .000001;
            if (!stats.newTimeRecord && !stats.newDpsRecord) return;
            if (previous == null) { previous = new RunPersonalBest { stageId = stageId, difficulty = difficulty }; records.Add(previous); }
            if (stats.newTimeRecord) previous.fastestClearSeconds = stats.seconds;
            if (stats.newDpsRecord) previous.highestDps = dps;
        }
    }
}
