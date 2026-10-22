using System;
using System.Collections.Generic;
using MrX.Save;
using UnityEngine;

namespace MrX.Achievements
{
    [Serializable]
    public class AchievementCounter
    {
        public AchievementMetric metric;
        public long value;
    }

    [Serializable]
    public class AchievementsSave : ISave
    {
        [SerializeField] private List<AchievementCounter> counters = new List<AchievementCounter>();
        [SerializeField] private List<string> claimedIds = new List<string>();
        [SerializeField] private List<string> completedStageIds = new List<string>();
        [SerializeField] private List<string> ownedItemIds = new List<string>();
        [SerializeField] private List<string> difficultyClears = new List<string>();
        public bool RecordDifficulty(string stageId, StageDifficulty mode)
        {
            if (string.IsNullOrEmpty(stageId) || (int)mode < 0 || (int)mode > 2) return false;
            if (difficultyClears == null) difficultyClears = new List<string>();
            string key = stageId + ":" + (int)mode;
            if (difficultyClears.Contains(key)) return false;
            difficultyClears.Add(key); return true;
        }
        public long GetProgress(AchievementDefinition definition) => definition.metric == AchievementMetric.StageDifficultyClear
            ? (difficultyClears != null && difficultyClears.Contains(definition.stageId + ":" + (int)definition.difficulty) ? 1 : 0) : Get(definition.metric);
        [NonSerialized] private Dictionary<AchievementMetric, AchievementCounter> lookup;
        [NonSerialized] private HashSet<string> claimed;
        [NonSerialized] private HashSet<string> stages;
        [NonSerialized] private HashSet<string> items;

        private void EnsureInitialized()
        {
            if (lookup != null) return;
            lookup = new Dictionary<AchievementMetric, AchievementCounter>();
            if (counters == null) counters = new List<AchievementCounter>();
            foreach (var counter in counters)
            {
                if (counter == null) continue;
                counter.value = Math.Max(0, counter.value);
                if (lookup.TryGetValue(counter.metric, out var previous)) previous.value = Math.Max(previous.value, counter.value);
                else lookup.Add(counter.metric, counter);
            }
            claimed = new HashSet<string>(claimedIds ?? new List<string>(), StringComparer.Ordinal);
            stages = new HashSet<string>(completedStageIds ?? new List<string>(), StringComparer.Ordinal);
            items = new HashSet<string>(ownedItemIds ?? new List<string>(), StringComparer.Ordinal);
            // Canonicalize once on load, not on each counter update.
            counters = new List<AchievementCounter>(lookup.Values);
            claimedIds = new List<string>(claimed); completedStageIds = new List<string>(stages); ownedItemIds = new List<string>(items);
        }

        public long Get(AchievementMetric metric)
        { EnsureInitialized(); return lookup.TryGetValue(metric, out var counter) ? counter.value : 0; }

        public bool Add(AchievementMetric metric, long amount)
        {
            if (amount <= 0) return false;
            var current = Get(metric);
            return SetMinimum(metric, current > long.MaxValue - amount ? long.MaxValue : current + amount);
        }

        public bool SetMinimum(AchievementMetric metric, long value)
        {
            if (value <= Get(metric)) return false;
            if (!lookup.TryGetValue(metric, out var counter))
            { counter = new AchievementCounter { metric = metric }; lookup.Add(metric, counter); counters.Add(counter); }
            counter.value = value; return true;
        }

        public bool RecordUnique(AchievementMetric metric, string id)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(id)) return false;
            HashSet<string> set; List<string> list;
            if (metric == AchievementMetric.UniqueStages) { set = stages; list = completedStageIds; }
            else if (metric == AchievementMetric.UniqueItems) { set = items; list = ownedItemIds; }
            else return false;
            if (!set.Add(id)) return false;
            list.Add(id); SetMinimum(metric, set.Count); return true;
        }

        public bool IsClaimed(string id) { EnsureInitialized(); return claimed.Contains(id); }
        public bool IsComplete(AchievementDefinition definition) => GetProgress(definition) >= definition.target;
        public bool TryClaim(AchievementDefinition definition)
        {
            if (!IsComplete(definition) || IsClaimed(definition.id)) return false;
            claimed.Add(definition.id); claimedIds.Add(definition.id); return true;
        }
        public void Flush() { EnsureInitialized(); }
    }
}
