using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Achievements;
using MrX.Currency;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    // Disposable saves/wallets only. The player's actual save is never reset.
    public static class AchievementsVerification
    {
        [MenuItem("Tools/MrX Studio/Achievements/Run Logic Checks")]
        public static void Run() { Debug.Log(Verify()); }

        public static string Verify()
        {
            var checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception("Achievements: " + message); checks++; };
            var database = ScriptableObject.CreateInstance<AchievementsDatabase>();
            var root = new GameObject("Isolated Achievements Verification") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                check(database.IsValid(out _) && database.achievements.Count == 48, "valid 48 definitions");
                var saves = new MemorySaves(); var wallets = new MemoryCurrencies();
                var manager = root.AddComponent<AchievementsManager>(); manager.Initialize(database, saves, wallets);
                foreach (var category in database.categories) check(manager.GetCategory(category.category).Count == 12, "12 definitions per category");
                check(manager.CompletedCount == 0 && !manager.TryClaim("stage_clears_1") && !manager.TryClaim("missing") && !manager.TryClaim(null), "incomplete/unknown/null claims rejected");
                manager.RecordStageClear("stage-a"); manager.RecordStageClear("stage-a");
                check(manager.Save.Get(AchievementMetric.StagesCleared) == 2 && manager.Save.Get(AchievementMetric.UniqueStages) == 1, "repeat stage increments total but not unique count");
                manager.RecordItemOwned("item-a"); manager.RecordItemOwned("item-a");
                check(manager.Save.Get(AchievementMetric.UniqueItems) == 1, "unique equipment deduplicated");
                check(saves.SaveCalls == 0, "events batch persistence until flush");
                manager.FlushPending(); check(saves.SaveCalls == 1 && !manager.enabled, "one flush then idle manager disabled");
                var claimable = manager.ClaimableCount;
                bool reentered = true;
                wallets.Meta.OnAmountChanged += currency => reentered = manager.TryClaim("unique_stages_1");
                check(manager.TryClaim("stage_clears_1") && !reentered && wallets.Meta.Amount == 100 && wallets.Gameplay.Amount == 0, "permanent wallet only and reentrant claims blocked");
                check(manager.ClaimableCount == claimable - 1 && manager.CompletedCount == 3, "claim preserves completion and decrements notification count");
                check(!manager.TryClaim("stage_clears_1") && wallets.Meta.Amount == 100 && saves.SaveCalls == 2, "claim once and persist immediately");
                wallets.HasCurrency = false;
                check(!manager.TryClaim("unique_stages_1") && !manager.Save.IsClaimed("unique_stages_1"), "missing currency does not consume reward");
                wallets.HasCurrency = true; wallets.Meta.Deposit(int.MaxValue - wallets.Meta.Amount);
                check(!manager.TryClaim("unique_stages_1") && !manager.Save.IsClaimed("unique_stages_1"), "wallet overflow does not consume reward");
                manager.Record(AchievementMetric.EnemiesDefeated, long.MaxValue); manager.Record(AchievementMetric.EnemiesDefeated, 1);
                check(manager.Save.Get(AchievementMetric.EnemiesDefeated) == long.MaxValue, "counter saturates safely");
                manager.Record(AchievementMetric.BossesDefeated, -1);
                check(manager.Save.Get(AchievementMetric.BossesDefeated) == 0, "negative progress rejected");
                manager.Record(AchievementMetric.UniqueItems, 999);
                check(manager.Save.Get(AchievementMetric.UniqueItems) == 1, "unique counters require stable identities");
                var restored = JsonUtility.FromJson<AchievementsSave>(JsonUtility.ToJson(manager.Save));
                check(restored.IsClaimed("stage_clears_1") && !restored.TryClaim(database.achievements[0]) && restored.Get(AchievementMetric.EnemiesDefeated) == long.MaxValue, "JSON preserves long counters and permanent claim flags");
                check(!restored.RecordUnique(AchievementMetric.UniqueItems, "item-a") && restored.RecordUnique(AchievementMetric.UniqueItems, "item-b"), "unique IDs survive reload");
                foreach (AchievementMetric metric in Enum.GetValues(typeof(AchievementMetric)))
                    if (metric != AchievementMetric.UniqueItems && metric != AchievementMetric.UniqueStages) manager.Record(metric, 10000);
                int expectedCompleted = 0, expectedClaimable = 0;
                foreach (var category in database.categories)
                {
                    int completed = 0, available = 0;
                    foreach (var definition in manager.GetCategory(category.category))
                        if (manager.Save.IsComplete(definition)) { completed++; if (!manager.Save.IsClaimed(definition.id)) available++; }
                    var summary = manager.GetSummary(category.category);
                    check(summary.completed == completed && summary.claimable == available, "cached category summary matches source data");
                    expectedCompleted += completed; expectedClaimable += available;
                }
                check(manager.CompletedCount == expectedCompleted && manager.ClaimableCount == expectedClaimable, "cached global summary matches categories");
                var asset = Resources.Load<AchievementsDatabase>(AchievementsDatabase.ResourcePath);
                check(asset != null && asset.IsValid(out _) && asset.slotSprite != null && asset.slotSprite.border.sqrMagnitude > 0 && asset.font != null, "real configuration has sliced art and font");
                var legacyStages = new StageSave(); legacyStages.Init(); legacyStages.CompleteStage(asset.stages.Stages[0]);
                var legacyArmory = JsonUtility.FromJson<ArmorySave>("{\"items\":[{\"id\":\"legacy-item\",\"level\":3}],\"heroes\":[{\"id\":\"legacy-hero\",\"isUlocked\":true,\"level\":2}]}");
                legacyArmory.Init();
                var legacyUpgrades = new MrX.Upgrades.UpgradesSave(); legacyUpgrades.Init();
                legacyUpgrades.SetUpgradeLevel(asset.upgrades.GetUpgrade(0).UpgradeType, 2);
                var legacyManager = root.AddComponent<AchievementsManager>();
                legacyManager.Initialize(asset, new MemorySaves(), new MemoryCurrencies());
                legacyManager.RestoreKnownProgress(legacyStages, legacyArmory, legacyUpgrades);
                check(legacyManager.Save.Get(AchievementMetric.UniqueStages) == 1 && legacyManager.Save.Get(AchievementMetric.StagesCleared) == 1, "legacy stage facts restored as lower bounds");
                check(legacyManager.Save.Get(AchievementMetric.UniqueItems) == 1 && legacyManager.Save.Get(AchievementMetric.ItemUpgrades) == 3 && legacyManager.Save.Get(AchievementMetric.HeroUpgrades) == 2 && legacyManager.Save.Get(AchievementMetric.PermanentUpgrades) == 3, "legacy ownership and upgrade facts restored");
                check(legacyManager.Save.Get(AchievementMetric.EnemiesDefeated) == 0 && legacyManager.Save.Get(AchievementMetric.ChestsOpened) == 0 && legacyManager.Save.Get(AchievementMetric.RoomsCompleted) == 0, "unknown history is never invented");
                var legacyCompleted = legacyManager.CompletedCount;
                legacyManager.RestoreKnownProgress(legacyStages, legacyArmory, legacyUpgrades);
                check(legacyManager.CompletedCount == legacyCompleted && legacyManager.Save.Get(AchievementMetric.HeroUpgrades) == 2, "legacy reconciliation idempotent");
                legacyManager.Record(AchievementMetric.HeroUpgrades, 5);
                legacyManager.RestoreKnownProgress(legacyStages, legacyArmory, legacyUpgrades);
                check(legacyManager.Save.Get(AchievementMetric.HeroUpgrades) == 7, "reconciliation never reduces accumulated progress");
                database.achievements[1].id = database.achievements[0].id;
                check(!database.IsValid(out _), "duplicate IDs rejected");
                return "Achievements: " + checks + " isolated logic checks passed. Player save/wallet untouched.";
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(database); }
        }

        public static string VerifyLiveButtons()
        {
            var screen = GameController.MainMenuScreenBehavior;
            var page = screen != null ? screen.CurrentPage as AchievementsPageBehavior : null;
            if (!Application.isPlaying || page == null || !screen.IsDockEnabled) throw new Exception("Open achievements in Play Mode first.");
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var type = typeof(AchievementsPageBehavior);
            var managerField = type.GetField("manager", flags); var walletField = type.GetField("wallet", flags);
            var originalManager = managerField.GetValue(page); var originalWallet = walletField.GetValue(page);
            var originalCategory = type.GetField("selectedCategory", flags).GetValue(page);
            var feedback = (TMPro.TMP_Text)type.GetField("feedback", flags).GetValue(page);
            var originalFeedback = feedback.text;
            var root = new GameObject("Isolated Achievement Button Test") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var wallets = new MemoryCurrencies(); var manager = root.AddComponent<AchievementsManager>();
                manager.Initialize(Resources.Load<AchievementsDatabase>(AchievementsDatabase.ResourcePath), new MemorySaves(), wallets);
                manager.RecordStageClear("test-stage"); manager.Record(AchievementMetric.RoomsCompleted, 2);
                managerField.SetValue(page, manager); walletField.SetValue(page, wallets.Meta);
                ResetView(page, type, flags); page.SelectCategory(AchievementCategory.Adventure);
                var first = page.transform.Find("Achievements Sheet - ui_slot sliced/Body/Achievement List/Viewport/Content/Achievement Row 0/Claim").GetComponent<Button>();
                if (!first.interactable) throw new Exception("Ready reward button disabled.");
                first.onClick.Invoke(); first.onClick.Invoke();
                if (wallets.Meta.Amount != 100 || first.interactable || !manager.Save.IsClaimed("stage_clears_1")) throw new Exception("Repeated click duplicated reward or rebound the row.");
                int before = page.GetComponentsInChildren<Button>(true).Length;
                foreach (AchievementCategory category in Enum.GetValues(typeof(AchievementCategory)))
                {
                    var tab = page.transform.Find("Achievements Sheet - ui_slot sliced/Body/Categories/Category " + category).GetComponent<Button>();
                    tab.onClick.Invoke();
                    if ((AchievementCategory)type.GetField("selectedCategory", flags).GetValue(page) != category) throw new Exception("Category listener failed.");
                }
                if (page.GetComponentsInChildren<Button>(true).Length != before) throw new Exception("Category switch allocated UI objects.");
                manager.Record(AchievementMetric.StagesCleared, 100);
                manager.Record(AchievementMetric.RoomsCompleted, 100);
                manager.RecordStageClear("test-stage-b"); manager.RecordStageClear("test-stage-c"); manager.RecordStageClear("test-stage-d");
                page.SelectCategory(AchievementCategory.Adventure);
                var list = page.GetComponentInChildren<ScrollRect>();
                var offscreenClaim = list.content.Find("Achievement Row 10/Claim").GetComponent<Button>();
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(offscreenClaim.gameObject);
                if (list.content.anchoredPosition.y < 1) throw new Exception("Gamepad focus did not scroll to the offscreen reward.");
                return "Live achievement buttons passed: four category listeners, one-time CLAIM, disabled CLAIMED state, stable row binding, reused UI pool and automatic scrolling to gamepad focus. Player save/wallet untouched.";
            }
            finally
            {
                managerField.SetValue(page, originalManager); walletField.SetValue(page, originalWallet);
                ResetView(page, type, flags); page.SelectCategory((AchievementCategory)originalCategory);
                feedback.text = originalFeedback;
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ResetView(AchievementsPageBehavior page, Type type, BindingFlags flags)
        {
            foreach (var name in new[] { "lastCompleted", "lastTotal", "lastGold" }) type.GetField(name, flags).SetValue(page, -1);
            // Category view caches belong to the currently injected manager too.
            var tabs = (System.Collections.IEnumerable)type.GetField("tabs", flags).GetValue(page);
            foreach (var tab in tabs)
                foreach (var name in new[] { "lastCompleted", "lastTotal", "lastClaimable" }) tab.GetType().GetField(name).SetValue(tab, -1);
        }

        private class MemorySaves : ISaveManager
        {
            private readonly Dictionary<int, ISave> entries = new Dictionary<int, ISave>();
            public int SaveCalls;
            public T GetSave<T>(int id) where T : ISave, new() { if (!entries.ContainsKey(id)) entries[id] = new T(); return (T)entries[id]; }
            public T GetSave<T>(string id) where T : ISave, new() => GetSave<T>(id.GetHashCode());
            public void Save(bool multithreading = false) { SaveCalls++; }
        }
        private class MemoryCurrencies : ICurrenciesManager
        {
            public readonly CurrencySave Meta = new CurrencySave();
            public readonly CurrencySave Gameplay = new CurrencySave();
            public bool HasCurrency = true;
            public CurrencySave GetCurrency(string id, bool gameplay) => HasCurrency && id == "gold" ? (gameplay ? Gameplay : Meta) : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => GetCurrency("gold", gameplay);
            public void ApplyGameplayAmounts() { Meta.Deposit(Gameplay.Amount); Gameplay.Clear(); }
            public void EraseGameplayAmounts() { Gameplay.Clear(); }
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { gameplay ? Gameplay : Meta };
        }
    }
}
