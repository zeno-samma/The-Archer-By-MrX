using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Quests;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace MrX.EditorTools
{
    // Uses only disposable models and wallets. Never writes or clears the player's save.
    public static class DailyQuestsVerification
    {
        [MenuItem("Tools/MrX Studio/Daily Quests/Run Logic Checks")]
        public static void Run() { Debug.Log(Verify()); }

        public static string Verify()
        {
            int checks = 0;
            Action<bool, string> check = (condition, message) =>
            { if (!condition) throw new Exception("Daily quests check failed: " + message); checks++; };
            var database = ScriptableObject.CreateInstance<DailyQuestsDatabase>();
            var gameObject = new GameObject("Daily Quests Isolated Verification") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                check(database.IsValid(out _), "default configuration");
                var save = new DailyQuestsSave();
                var midnight = new DateTime(2026, 12, 31, 17, 0, 0, DateTimeKind.Utc);
                save.EnsureDay(midnight.AddSeconds(-1), 7);
                var quest = database.quests[0];
                save.AddProgress(quest, 2);
                check(!save.TryMarkQuestClaimed(quest), "incomplete quest is locked");
                save.AddProgress(quest, int.MaxValue);
                check(save.GetProgress(quest) == 3, "progress clamps without integer overflow");
                check(save.TryMarkQuestClaimed(quest) && !save.TryMarkQuestClaimed(quest), "quest claim is one-time");
                check(!save.TryMarkMilestoneClaimed(database.milestones[1], save.CompletedCount(database.quests)), "milestone threshold enforced");
                check(save.TryMarkMilestoneClaimed(database.milestones[0], save.CompletedCount(database.quests)), "completed quests unlock milestone");
                check(!save.TryMarkMilestoneClaimed(database.milestones[0], 5), "milestone claim is one-time");
                var restored = JsonUtility.FromJson<DailyQuestsSave>(JsonUtility.ToJson(save));
                check(restored.IsClaimed(quest.id) && restored.GetProgress(quest) == 3 && restored.IsMilestoneClaimed("daily_1"), "save round-trip preserves progress and claims");
                check(!restored.EnsureDay(midnight.AddHours(-3), 7) && restored.IsClaimed(quest.id), "clock rollback does not reopen claims");
                check(restored.EnsureDay(midnight, 7), "reset at UTC+7 midnight across year boundary");
                check(restored.GetProgress(quest) == 0 && !restored.IsClaimed(quest.id) && !restored.IsMilestoneClaimed("daily_1"), "new day clears both claim types and progress");
                check(restored.TimeUntilReset(midnight, 7) == TimeSpan.FromHours(24), "reset countdown uses same boundary");

                var saves = new MemorySaves(); var wallets = new MemoryCurrencies();
                var manager = gameObject.AddComponent<DailyQuestsManager>();
                manager.Initialize(database, saves, wallets);
                check(manager.CompletedCount == 1, "login completes once on initialization");
                check(!manager.TryClaimQuest("clear_stages") && !manager.TryClaimQuest("missing"), "incomplete and unknown requests rejected");
                check(manager.TryClaimQuest("daily_login") && wallets.Meta.Amount == 100 && wallets.Gameplay.Amount == 0, "claim credits permanent wallet only");
                check(!manager.TryClaimQuest("daily_login") && wallets.Meta.Amount == 100, "repeated button activation cannot duplicate gold");
                check(manager.TryClaimMilestone("daily_1") && !manager.TryClaimMilestone("daily_1") && wallets.Meta.Amount == 150, "milestone grants exactly once");
                manager.Record(DailyQuestType.DefeatEnemies, 50);
                manager.Record(DailyQuestType.CollectItems, 3);
                manager.Record(DailyQuestType.ClearStages, 3);
                manager.Record(DailyQuestType.UpgradeHero);
                check(manager.CompletedCount == 5 && !manager.TryClaimMilestone("missing"), "all gameplay event categories tracked");
                check(manager.TryClaimQuest("upgrade_hero") && wallets.Meta.Amount == 300, "hero quest can be claimed");
                wallets.HasCurrency = false;
                check(!manager.TryClaimQuest("clear_stages") && !manager.Save.IsClaimed("clear_stages"), "missing wallet does not consume a reward");
                check(saves.SaveCalls > 0, "progress and claims request persistence");

                var asset = Resources.Load<DailyQuestsDatabase>(DailyQuestsDatabase.ResourcePath);
                check(asset != null && asset.IsValid(out _) && asset.slotSprite != null && asset.slotSprite.border.sqrMagnitude > 0 && asset.font != null, "real database has sliced background and font references");
                return "Daily quests: " + checks + " isolated logic checks passed. No player save or wallet modified.";
            }
            finally { UnityEngine.Object.DestroyImmediate(gameObject); UnityEngine.Object.DestroyImmediate(database); }
        }

        private class MemorySaves : ISaveManager
        {
            private readonly Dictionary<int, ISave> entries = new Dictionary<int, ISave>();
            public int SaveCalls;
            public T GetSave<T>(int id) where T : ISave, new()
            { if (!entries.ContainsKey(id)) entries[id] = new T(); return (T)entries[id]; }
            public T GetSave<T>(string id) where T : ISave, new() => GetSave<T>(id.GetHashCode());
            public void Save(bool multithreading = false) { SaveCalls++; }
        }

        public static string VerifyLiveClaimButtons()
        {
            if (!Application.isPlaying) throw new Exception("Open the daily quests page in Play Mode first.");
            var page = GameController.MainMenuScreenBehavior.CurrentPage as DailyQuestsPageBehavior;
            if (page == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) throw new Exception("Daily quests page must be visible and unlocked.");
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var managerField = typeof(DailyQuestsPageBehavior).GetField("manager", flags);
            var walletField = typeof(DailyQuestsPageBehavior).GetField("wallet", flags);
            var refresh = typeof(DailyQuestsPageBehavior).GetMethod("Refresh", flags);
            var originalManager = managerField.GetValue(page);
            var originalWallet = walletField.GetValue(page);
            var testObject = new GameObject("Isolated Daily Quest Button Test") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var saves = new MemorySaves(); var wallets = new MemoryCurrencies();
                var manager = testObject.AddComponent<DailyQuestsManager>();
                manager.Initialize(Resources.Load<DailyQuestsDatabase>(DailyQuestsDatabase.ResourcePath), saves, wallets);
                managerField.SetValue(page, manager); walletField.SetValue(page, wallets.Meta);
                refresh.Invoke(page, null);
                Button login = null; Button chest = null;
                foreach (var button in page.GetComponentsInChildren<Button>(true))
                {
                    if (button.transform.parent.name == "Quest daily_login") login = button;
                    if (button.name == "Chest daily_1") chest = button;
                }
                if (login == null || chest == null || !login.interactable || !chest.interactable) throw new Exception("Login/chest button wiring missing.");
                login.onClick.Invoke(); login.onClick.Invoke();
                if (wallets.Meta.Amount != 100 || login.interactable || !manager.Save.IsClaimed("daily_login")) throw new Exception("Quest claim button failed one-time grant.");
                chest.onClick.Invoke(); chest.onClick.Invoke();
                if (wallets.Meta.Amount != 150 || chest.interactable || !manager.Save.IsMilestoneClaimed("daily_1")) throw new Exception("Milestone button failed one-time grant.");
                return "Live CLAIM and chest buttons passed: real onClick listeners, one-time gold grant, CLAIMED/disabled state. Player wallet/save untouched.";
            }
            finally
            {
                managerField.SetValue(page, originalManager); walletField.SetValue(page, originalWallet);
                refresh.Invoke(page, null);
                UnityEngine.Object.DestroyImmediate(testObject);
            }
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
