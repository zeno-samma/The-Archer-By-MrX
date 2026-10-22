using System;
using System.Collections.Generic;
using MrX.Armory;
using MrX.Currency;
using MrX.Upgrades;
using TMPro;
using UnityEngine;

namespace MrX.Achievements
{
    // Serialized identities. Append values; never reorder existing ones.
    public enum AchievementCategory { Adventure = 0, Battle = 1, Collection = 2, Upgrade = 3 }
    public enum AchievementMetric
    {
        StagesCleared = 0, UniqueStages = 1, RoomsCompleted = 2,
        EnemiesDefeated = 3, BossesDefeated = 4, ItemsCollected = 5,
        UniqueItems = 6, ChestsOpened = 7, HeroUpgrades = 8,
        ItemUpgrades = 9, PermanentUpgrades = 10, StageDifficultyClear = 11
    }

    [Serializable]
    public class AchievementDefinition
    {
        public string id;
        public AchievementCategory category;
        public AchievementMetric metric;
        public string title;
        [TextArea] public string description;
        [Min(1)] public int target = 1;
        public Price reward = new Price("gold", 100);
        public Sprite icon;
        [Tooltip("Only used for StageDifficultyClear")]
        public string stageId;
        public StageDifficulty difficulty;
    }

    [Serializable]
    public class AchievementCategoryDefinition
    {
        public AchievementCategory category;
        public string title;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "MrX Studio/Achievements/Database", fileName = "Achievements Database")]
    public class AchievementsDatabase : ScriptableObject
    {
        public const string ResourcePath = "Achievements Database";
        [Header("Presentation — placeholder art can be replaced")]
        public Sprite slotSprite;
        public Material slotMaterial;
        public Sprite trophySprite;
        public Sprite coinSprite;
        public Sprite settingsSprite;
        public Sprite circleSprite;
        public Sprite claimButtonSprite;
        public Sprite disabledButtonSprite;
        public TMP_FontAsset font;
        [Header("Existing progression used for legacy-save reconciliation")]
        public StageDatabase stages;
        public ArmoryDatabase armory;
        public UpgradesDatabase upgrades;
        public List<AchievementCategoryDefinition> categories = new List<AchievementCategoryDefinition>
        {
            new AchievementCategoryDefinition { category = AchievementCategory.Adventure, title = "Adventure" },
            new AchievementCategoryDefinition { category = AchievementCategory.Battle, title = "Battle" },
            new AchievementCategoryDefinition { category = AchievementCategory.Collection, title = "Collection" },
            new AchievementCategoryDefinition { category = AchievementCategory.Upgrade, title = "Upgrade" }
        };
        public List<AchievementDefinition> achievements = CreateDefaults();

        public bool IsValid(out string error)
        {
            var groups = new HashSet<AchievementCategory>();
            if (categories == null || achievements == null || achievements.Count == 0)
            { error = "Missing categories or achievements."; return false; }
            foreach (var group in categories)
                if (group == null || !Enum.IsDefined(typeof(AchievementCategory), group.category) || !groups.Add(group.category) || string.IsNullOrWhiteSpace(group.title))
                { error = "Invalid or duplicate category."; return false; }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in achievements)
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) || !groups.Contains(entry.category)
                    || !Enum.IsDefined(typeof(AchievementMetric), entry.metric) || string.IsNullOrWhiteSpace(entry.title) || entry.target < 1
                    || entry.reward == null || string.IsNullOrWhiteSpace(entry.reward.CurrencyId) || entry.reward.Amount < 1)
                { error = "Achievement IDs must be unique; category, metric, target and reward must be valid."; return false; }
            error = null; return true;
        }

        public static List<AchievementDefinition> CreateDefaults()
        {
            var list = new List<AchievementDefinition>(48);
            AddTiers(list, AchievementCategory.Adventure, AchievementMetric.StagesCleared, "stage_clears", "A New Journey", "Adventure Veteran", "Complete {0} stages.", new[] { 1, 3, 10, 25 });
            AddTiers(list, AchievementCategory.Adventure, AchievementMetric.UniqueStages, "unique_stages", "Explorer", "World Explorer", "Complete {0} different stages.", new[] { 1, 2, 3, 4 });
            AddTiers(list, AchievementCategory.Adventure, AchievementMetric.RoomsCompleted, "rooms", "Pathfinder", "Dungeon Pathfinder", "Complete {0} rooms.", new[] { 5, 20, 50, 100 });
            AddTiers(list, AchievementCategory.Battle, AchievementMetric.EnemiesDefeated, "enemies", "First Victory", "Monster Hunter", "Defeat {0} monsters.", new[] { 10, 50, 100, 250, 500, 1000 });
            AddTiers(list, AchievementCategory.Battle, AchievementMetric.BossesDefeated, "bosses", "Boss Challenger", "Boss Slayer", "Defeat {0} bosses.", new[] { 1, 5, 10, 25, 50, 100 });
            AddTiers(list, AchievementCategory.Collection, AchievementMetric.ItemsCollected, "item_drops", "Finder's Fortune", "Loot Collector", "Pick up {0} equipment drops.", new[] { 3, 10, 25, 50 });
            AddTiers(list, AchievementCategory.Collection, AchievementMetric.UniqueItems, "unique_items", "First Collection", "Armory Collector", "Own {0} different equipment items.", new[] { 1, 5, 10, 14 });
            AddTiers(list, AchievementCategory.Collection, AchievementMetric.ChestsOpened, "chests", "Treasure Hunter", "Treasure Seeker", "Open {0} stage treasure chests.", new[] { 1, 5, 10, 20 });
            AddTiers(list, AchievementCategory.Upgrade, AchievementMetric.HeroUpgrades, "hero_levels", "Growing Stronger", "Hero Training", "Gain {0} hero levels through upgrades.", new[] { 1, 3, 5, 10 });
            AddTiers(list, AchievementCategory.Upgrade, AchievementMetric.ItemUpgrades, "item_levels", "Better Equipment", "Equipment Master", "Gain {0} equipment levels.", new[] { 1, 5, 10, 25 });
            AddTiers(list, AchievementCategory.Upgrade, AchievementMetric.PermanentUpgrades, "permanent_levels", "Lasting Power", "Permanent Power", "Purchase {0} permanent upgrade levels.", new[] { 1, 5, 10, 25 });
            // Show different challenges together; later tiers remain further down
            // the category instead of occupying the entire first viewport.
            var ordered = new List<AchievementDefinition>(list.Count);
            foreach (AchievementCategory category in Enum.GetValues(typeof(AchievementCategory)))
            {
                var series = new Dictionary<AchievementMetric, List<AchievementDefinition>>();
                foreach (var entry in list)
                    if (entry.category == category)
                    {
                        if (!series.TryGetValue(entry.metric, out var tiers)) series.Add(entry.metric, tiers = new List<AchievementDefinition>());
                        tiers.Add(entry);
                    }
                for (var tier = 0; tier < 6; tier++)
                    foreach (var metric in new[] { AchievementMetric.StagesCleared, AchievementMetric.UniqueStages, AchievementMetric.RoomsCompleted,
                        AchievementMetric.EnemiesDefeated, AchievementMetric.BossesDefeated, AchievementMetric.ItemsCollected, AchievementMetric.UniqueItems,
                        AchievementMetric.ChestsOpened, AchievementMetric.HeroUpgrades, AchievementMetric.ItemUpgrades, AchievementMetric.PermanentUpgrades })
                        if (series.TryGetValue(metric, out var tiers) && tier < tiers.Count) ordered.Add(tiers[tier]);
            }
            return ordered;
        }

        private static void AddTiers(List<AchievementDefinition> list, AchievementCategory category, AchievementMetric metric, string prefix, string firstTitle, string tierTitle, string format, int[] targets)
        {
            for (var i = 0; i < targets.Length; i++) list.Add(new AchievementDefinition
            {
                id = prefix + "_" + targets[i], category = category, metric = metric,
                title = i == 0 ? firstTitle : tierTitle + " " + (i + 1),
                description = string.Format(format, targets[i]), target = targets[i], reward = new Price("gold", 100 + i * 75)
            });
        }
    }
}
