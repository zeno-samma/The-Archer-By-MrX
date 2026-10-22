using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Save;
using MrX.Upgrades;
using UnityEngine;

namespace MrX.Achievements
{
    public struct AchievementCategoryProgress
    {
        public int total;
        public int completed;
        public int claimable;
    }

    public class AchievementsManager : MonoBehaviour
    {
        public AchievementsDatabase Database { get; private set; }
        public AchievementsSave Save { get; private set; }
        public int CompletedCount { get; private set; }
        public int ClaimableCount { get; private set; }
        public event Action Changed;
        private ISaveManager saveManager;
        private ICurrenciesManager currencies;
        private readonly Dictionary<string, AchievementDefinition> byId = new Dictionary<string, AchievementDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<AchievementMetric, List<AchievementDefinition>> byMetric = new Dictionary<AchievementMetric, List<AchievementDefinition>>();
        private readonly Dictionary<AchievementCategory, List<AchievementDefinition>> byCategory = new Dictionary<AchievementCategory, List<AchievementDefinition>>();
        private readonly Dictionary<AchievementCategory, AchievementCategoryProgress> summaries = new Dictionary<AchievementCategory, AchievementCategoryProgress>();
        private bool dirty;
        private bool claiming;
        private float nextSave;

        public void Initialize(AchievementsDatabase database, ISaveManager saves = null, ICurrenciesManager wallets = null)
        {
            enabled = false;
            if (database == null || !database.IsValid(out _)) { Debug.LogError("Achievements: invalid database."); return; }
            Database = database; saveManager = saves ?? GameController.SaveManager; currencies = wallets ?? GameController.CurrenciesManager;
            Save = saveManager.GetSave<AchievementsSave>("Achievements");
            byId.Clear(); byMetric.Clear(); byCategory.Clear(); summaries.Clear();
            foreach (var category in database.categories) byCategory.Add(category.category, new List<AchievementDefinition>());
            foreach (var definition in database.achievements)
            {
                byId.Add(definition.id, definition);
                byCategory[definition.category].Add(definition);
                if (!byMetric.TryGetValue(definition.metric, out var definitions)) byMetric.Add(definition.metric, definitions = new List<AchievementDefinition>());
                definitions.Add(definition);
            }
            RebuildSummaries();
        }

        public IReadOnlyList<AchievementDefinition> GetCategory(AchievementCategory category) => byCategory[category];
        public AchievementCategoryProgress GetSummary(AchievementCategory category) => summaries[category];
        public bool CanClaim(AchievementDefinition definition) => Save.IsComplete(definition) && !Save.IsClaimed(definition.id);

        public void Record(AchievementMetric metric, long amount = 1)
        {
            if (Save == null || amount <= 0 || metric == AchievementMetric.UniqueItems || metric == AchievementMetric.UniqueStages) return;
            var before = Save.Get(metric);
            if (!Save.Add(metric, amount)) return;
            UpdateMetric(metric, before); ScheduleSave();
        }

        public void RecordStageClear(string stageId)
        { Record(AchievementMetric.StagesCleared); RecordUnique(AchievementMetric.UniqueStages, stageId); }
        public void RecordDifficultyClear(string stageId, StageDifficulty mode)
        {
            if (Save == null || !Save.RecordDifficulty(stageId, mode)) return;
            RebuildSummaries(); ScheduleSave(); Changed?.Invoke();
        }

        public void RecordItemOwned(string itemId) => RecordUnique(AchievementMetric.UniqueItems, itemId);

        private void RecordUnique(AchievementMetric metric, string id)
        {
            if (Save == null) return;
            var before = Save.Get(metric);
            if (!Save.RecordUnique(metric, id)) return;
            UpdateMetric(metric, before); ScheduleSave();
        }

        private void UpdateMetric(AchievementMetric metric, long before)
        {
            if (!byMetric.TryGetValue(metric, out var definitions)) return;
            var after = Save.Get(metric); bool visibleProgressChanged = false;
            foreach (var definition in definitions)
            {
                if (before < definition.target) visibleProgressChanged = true;
                if (before >= definition.target || after < definition.target) continue;
                var summary = summaries[definition.category]; summary.completed++; CompletedCount++;
                if (!Save.IsClaimed(definition.id)) { summary.claimable++; ClaimableCount++; }
                summaries[definition.category] = summary;
            }
            if (visibleProgressChanged) Changed?.Invoke();
        }

        private void RebuildSummaries()
        {
            CompletedCount = 0; ClaimableCount = 0;
            foreach (var pair in byCategory)
            {
                var summary = new AchievementCategoryProgress { total = pair.Value.Count };
                foreach (var definition in pair.Value)
                    if (Save.IsComplete(definition)) { summary.completed++; if (!Save.IsClaimed(definition.id)) summary.claimable++; }
                summaries[pair.Key] = summary; CompletedCount += summary.completed; ClaimableCount += summary.claimable;
            }
        }

        public bool TryClaim(string id)
        {
            if (Save == null || claiming || string.IsNullOrWhiteSpace(id) || !byId.TryGetValue(id, out var definition) || !CanClaim(definition)) return false;
            var wallet = currencies?.GetCurrency(definition.reward.CurrencyId, false);
            if (wallet == null || wallet.Amount > int.MaxValue - definition.reward.Amount) return false;
            claiming = true; bool marked = false;
            try
            {
                if (!Save.TryClaim(definition)) return false;
                marked = true; dirty = true;
                var summary = summaries[definition.category]; summary.claimable--; summaries[definition.category] = summary; ClaimableCount--;
                wallet.Deposit(definition.reward.Amount);
                return true;
            }
            finally
            {
                try { if (marked) { FlushPending(); Changed?.Invoke(); } }
                finally { claiming = false; }
            }
        }

        private void ScheduleSave() { dirty = true; enabled = true; }
        private void LateUpdate() { if (dirty && Time.unscaledTime >= nextSave) FlushPending(); }
        public void FlushPending()
        {
            if (!dirty) return;
            dirty = false; enabled = false; nextSave = Time.unscaledTime + 2;
            saveManager.Save(false);
        }

        // Reconcile only facts present in the old save. Never invent historical kill,
        // chest, room or loot-drop totals; those start when tracking is introduced.
        public void RestoreKnownProgress(StageSave stageSave, ArmorySave armorySave, UpgradesSave upgradesSave)
        {
            if (Save == null) return;
            bool changed = false;
            if (stageSave != null && Database.stages != null)
                foreach (var stage in Database.stages.Stages)
                    if (stage != null && stageSave.IsStageCompleted(stage)) changed |= Save.RecordUnique(AchievementMetric.UniqueStages, stage.StageId);
            changed |= Save.SetMinimum(AchievementMetric.StagesCleared, Save.Get(AchievementMetric.UniqueStages));
            if (armorySave != null && armorySave.IsInitialized)
            {
                long itemLevels = 0, heroLevels = 0;
                foreach (var item in armorySave.GetItems())
                { changed |= Save.RecordUnique(AchievementMetric.UniqueItems, item.Id); itemLevels += Math.Max(0, item.Level); }
                foreach (var hero in armorySave.GetHeroes()) if (hero.IsUnlocked) heroLevels += Math.Max(0, hero.Level);
                changed |= Save.SetMinimum(AchievementMetric.ItemUpgrades, itemLevels);
                changed |= Save.SetMinimum(AchievementMetric.HeroUpgrades, heroLevels);
            }
            if (upgradesSave != null && Database.upgrades != null)
            {
                long levels = 0;
                for (var i = 0; i < Database.upgrades.UpgradesCount; i++) levels += Math.Max(0L, (long)upgradesSave.GetUpgradeLevel(Database.upgrades.GetUpgrade(i).UpgradeType) + 1);
                changed |= Save.SetMinimum(AchievementMetric.PermanentUpgrades, levels);
            }
            if (changed) { RebuildSummaries(); ScheduleSave(); Changed?.Invoke(); }
        }
    }
}
