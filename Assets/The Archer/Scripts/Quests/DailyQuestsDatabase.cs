using System;
using System.Collections.Generic;
using MrX.Currency;
using TMPro;
using UnityEngine;

namespace MrX.Quests
{
    // These values are serialized. Append new values; do not reorder existing ones.
    public enum DailyQuestType { ClearStages = 0, DefeatEnemies = 1, CollectItems = 2, Login = 3, UpgradeHero = 4 }

    [Serializable]
    public class DailyQuestDefinition
    {
        public string id;
        public DailyQuestType type;
        public string title;
        [TextArea] public string description;
        [Min(1)] public int target = 1;
        public Price reward = new Price("gold", 100);
        public Sprite icon;

        public DailyQuestDefinition(string id, DailyQuestType type, string title, string description, int target, int gold)
        {
            this.id = id; this.type = type; this.title = title; this.description = description;
            this.target = target; reward = new Price("gold", gold);
        }
    }

    [Serializable]
    public class DailyQuestMilestone
    {
        public string id;
        [Min(1)] public int requiredCompletions = 1;
        public Price reward = new Price("gold", 50);
        public Sprite icon;
        public DailyQuestMilestone(string id, int required, int gold)
        { this.id = id; requiredCompletions = required; reward = new Price("gold", gold); }
    }

    [CreateAssetMenu(menuName = "MrX Studio/Quests/Daily Quests Database", fileName = "Daily Quests Database")]
    public class DailyQuestsDatabase : ScriptableObject
    {
        public const string ResourcePath = "Daily Quests Database";
        [Tooltip("Fixed UTC offset for daily midnight reset. Default: Vietnam UTC+7.")]
        [Range(-12, 14)] public int resetUtcOffsetHours = 7;
        [Header("Replace placeholder art here")]
        public Sprite slotSprite;
        public Sprite coinSprite;
        public Sprite questIcon;
        public Sprite settingsIcon;
        public Sprite blueButtonSprite;
        public Sprite claimButtonSprite;
        public TMP_FontAsset font;
        public List<DailyQuestDefinition> quests = new List<DailyQuestDefinition>
        {
            new DailyQuestDefinition("clear_stages", DailyQuestType.ClearStages, "Clear any stage 3 times", "Complete stages in Adventure mode.", 3, 100),
            new DailyQuestDefinition("defeat_enemies", DailyQuestType.DefeatEnemies, "Defeat 50 monsters", "Defeat any monsters in stages.", 50, 150),
            new DailyQuestDefinition("collect_items", DailyQuestType.CollectItems, "Collect 3 item drops", "Pick up equipment dropped by monsters.", 3, 100),
            new DailyQuestDefinition("daily_login", DailyQuestType.Login, "Log in to the game", "Come back and start your adventure!", 1, 100),
            new DailyQuestDefinition("upgrade_hero", DailyQuestType.UpgradeHero, "Upgrade any hero 1 time", "Make your heroes stronger in Armory.", 1, 150)
        };
        public List<DailyQuestMilestone> milestones = new List<DailyQuestMilestone>
        {
            new DailyQuestMilestone("daily_1", 1, 50), new DailyQuestMilestone("daily_2", 2, 100),
            new DailyQuestMilestone("daily_3", 3, 150), new DailyQuestMilestone("daily_4", 4, 200),
            new DailyQuestMilestone("daily_5", 5, 300)
        };

        public bool IsValid(out string error)
        {
            var ids = new HashSet<string>();
            if (quests == null || quests.Count == 0 || milestones == null) { error = "Missing quest or milestone lists."; return false; }
            foreach (var q in quests)
                if (q == null || string.IsNullOrWhiteSpace(q.id) || !ids.Add(q.id) || q.target < 1 || !ValidReward(q.reward))
                { error = "Quest IDs must be unique; targets and rewards must be positive."; return false; }
            foreach (var m in milestones)
                if (m == null || string.IsNullOrWhiteSpace(m.id) || !ids.Add(m.id) || m.requiredCompletions < 1 || m.requiredCompletions > quests.Count || !ValidReward(m.reward))
                { error = "Invalid milestone ID, completion threshold or reward."; return false; }
            error = null; return true;
        }

        private static bool ValidReward(Price price) => price != null && !string.IsNullOrWhiteSpace(price.CurrencyId) && price.Amount > 0;
    }
}
