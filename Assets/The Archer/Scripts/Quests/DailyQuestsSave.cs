using System;
using System.Collections.Generic;
using MrX.Save;
using UnityEngine;

namespace MrX.Quests
{
    [Serializable]
    public class DailyQuestProgress
    {
        public string id;
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public class DailyQuestsSave : ISave
    {
        [SerializeField] private int activeDay = -1;
        [SerializeField] private List<DailyQuestProgress> entries = new List<DailyQuestProgress>();
        [SerializeField] private List<string> claimedMilestones = new List<string>();
        public int ActiveDay => activeDay;

        public bool EnsureDay(DateTime utcNow, int utcOffsetHours)
        {
            var day = (int)(utcNow.AddHours(utcOffsetHours).Date.Ticks / TimeSpan.TicksPerDay);
            // A clock rollback must not reopen already claimed rewards.
            if (day <= activeDay) return false;
            activeDay = day;
            Entries.Clear(); Milestones.Clear();
            return true;
        }

        public TimeSpan TimeUntilReset(DateTime utcNow, int utcOffsetHours)
        {
            var nextDay = new DateTime(((long)activeDay + 1) * TimeSpan.TicksPerDay);
            var remaining = nextDay - utcNow.AddHours(utcOffsetHours);
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private List<DailyQuestProgress> Entries => entries ?? (entries = new List<DailyQuestProgress>());
        private List<string> Milestones => claimedMilestones ?? (claimedMilestones = new List<string>());
        private DailyQuestProgress Find(string id) => Entries.Find(entry => entry.id == id);
        private DailyQuestProgress GetOrCreate(string id)
        {
            var entry = Find(id);
            if (entry != null) return entry;
            entry = new DailyQuestProgress { id = id }; Entries.Add(entry); return entry;
        }

        public int GetProgress(DailyQuestDefinition quest) => Math.Min(quest.target, Math.Max(0, Find(quest.id)?.progress ?? 0));
        public bool IsClaimed(string id) => Find(id)?.claimed ?? false;
        public bool IsComplete(DailyQuestDefinition quest) => GetProgress(quest) >= quest.target;
        public bool IsMilestoneClaimed(string id) => Milestones.Contains(id);
        public bool AddProgress(DailyQuestDefinition quest, int amount)
        {
            if (amount <= 0) return false;
            var entry = GetOrCreate(quest.id);
            var value = (int)Math.Min(quest.target, (long)entry.progress + amount);
            if (entry.progress == value) return false;
            entry.progress = value; return true;
        }

        public int CompletedCount(IReadOnlyList<DailyQuestDefinition> quests)
        {
            var count = 0;
            foreach (var quest in quests) if (IsComplete(quest)) count++;
            return count;
        }

        public bool TryMarkQuestClaimed(DailyQuestDefinition quest)
        {
            if (!IsComplete(quest) || IsClaimed(quest.id)) return false;
            GetOrCreate(quest.id).claimed = true; return true;
        }

        public bool TryMarkMilestoneClaimed(DailyQuestMilestone milestone, int completedCount)
        {
            if (completedCount < milestone.requiredCompletions || IsMilestoneClaimed(milestone.id)) return false;
            Milestones.Add(milestone.id); return true;
        }

        public void Flush() { }
    }
}
