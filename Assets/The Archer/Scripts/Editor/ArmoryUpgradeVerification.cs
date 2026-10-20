using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Armory;
using MrX.Currency;
using MrX.Save;
using MrX.UI;
using MrX.Upgrades;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class ArmoryUpgradeVerification
    {
        [MenuItem("Tools/MrX Studio/UI/Verify Bag Stats and Upgrades")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            int checks = 0; Action<bool, string> check = (ok, reason) => { if (!ok) throw new Exception(reason); checks++; };
            var config = ScriptableObject.CreateInstance<ArmoryDashboardDatabase>();
            var upgrades = new MemoryUpgrades();
            try
            {
                var save = JsonUtility.FromJson<ArmorySave>("{\"items\":[],\"heroes\":[]}"); save.Init();
                check(save.BagCapacity == 40 && save.UsedSlots == 0, "Old save defaults to forty slots");
                for (int i = 0; i < 40; i++) save.AddItem("item" + i);
                check(!save.HasBagSpace, "Forty unique equipment entries fill bag");
                save.AddItem("item0"); check(save.UsedSlots == 40, "Existing equipment never consumes another slot");
                var wallets = new MemoryCurrencies(); var persistence = new MemorySaves(); int pendingClaims = 0;
                var bag = new InventoryBagService(save, config, wallets, persistence, () => pendingClaims++);
                check(bag.TryExpand(40) == BagExpansionResult.InsufficientFunds && save.BagCapacity == 40, "Insufficient gold changes nothing");
                wallets.Meta.Deposit(100); BagExpansionResult nested = BagExpansionResult.Success; bool coherent = false;
                wallets.Meta.OnAmountChanged += w => { coherent = save.BagCapacity == 50 && pendingClaims == 1; nested = bag.TryExpand(50); };
                check(bag.TryExpand(40) == BagExpansionResult.Success && coherent && nested == BagExpansionResult.Busy, "Payment capacity and pending delivery commit before observers; no reentry");
                check(wallets.Meta.Amount == 0 && save.BagCapacity == 50 && persistence.Calls == 1 && wallets.Gameplay.Amount == 0, "Exactly ten slots purchased once using permanent wallet");
                check(bag.TryExpand(40) == BagExpansionResult.Stale, "Old confirmation cannot buy another tier");
                save.QueueEquipment("overflow"); save.QueueEquipment("overflow"); save.Flush();
                var restored = JsonUtility.FromJson<ArmorySave>(JsonUtility.ToJson(save)); restored.Init();
                check(restored.BagCapacity == 50 && restored.UsedSlots == 40 && restored.PendingEquipment[0].amount == 2, "Bag and pending rewards survive save roundtrip");
                config.maximumCapacity = 50; check(bag.GetState() == BagExpansionResult.Maximum, "Maximum capacity is enforced without payment");
                var old = new ArmorySave(); old.Init(); for (int i = 0; i < 47; i++) old.AddItem("old" + i); old.Flush();
                var migrated = JsonUtility.FromJson<ArmorySave>(JsonUtility.ToJson(old)); migrated.Init();
                check(migrated.BagCapacity == 47 && migrated.UsedSlots == 47, "Legacy saves over forty keep every item");
                var throwingWallets = new MemoryCurrencies(); throwingWallets.Meta.Deposit(100); config.maximumCapacity = 200;
                var failureSave = new ArmorySave(); failureSave.Init(); var failureSaves = new MemorySaves();
                var failure = new InventoryBagService(failureSave, config, throwingWallets, failureSaves);
                throwingWallets.Meta.OnAmountChanged += w => { throw new InvalidOperationException("observer"); };
                try { failure.TryExpand(40); } catch (InvalidOperationException) { }
                check(failureSave.BagCapacity == 50 && throwingWallets.Meta.Amount == 0 && failureSaves.Calls == 1, "Throwing observer cannot leave gold charged without capacity");

                var hero = JsonUtility.FromJson<HeroLevel>("{\"stats\":[{\"statType\":0,\"value\":40},{\"statType\":1,\"value\":100},{\"statType\":2,\"value\":1},{\"statType\":3,\"value\":5},{\"statType\":5,\"value\":10},{\"statType\":6,\"value\":1}]}");
                var item = JsonUtility.FromJson<ItemLevel>("{\"stats\":[{\"statType\":0,\"value\":5},{\"statType\":1,\"value\":20},{\"statType\":2,\"value\":1.5},{\"statType\":3,\"value\":1.2},{\"statType\":5,\"value\":2}]}");
                var gear = new[] { item }; var basic = ArmoryStatsSnapshot.Calculate(hero, gear, null);
                check(basic.Total.AttackDamageStat.Value == 45 && basic.Total.MaxHPStat.Value == 120 && basic.Total.CritChanceStat.Value == 12 && Mathf.Approximately(basic.Total.MovementSpeedStat.Value, 6), "Shared player formula includes equipped gear in correct units");
                upgrades.Add(UpgradeType.MorePower, 1.2f); upgrades.Add(UpgradeType.BaseHp, 1.5f); upgrades.Add(UpgradeType.MoveSpeed, 1.1f); upgrades.Add(UpgradeType.AttackSpeed, 1.2f);
                upgrades.Add(UpgradeType.Shield, 25); upgrades.Add(UpgradeType.CritChance, 5); upgrades.Add(UpgradeType.DodgeChance, 7); upgrades.Add(UpgradeType.Revive, 2); upgrades.Add(UpgradeType.HPRecovery, 10);
                var snapshot = ArmoryStatsSnapshot.Calculate(hero, gear, upgrades);
                check(Mathf.Approximately(snapshot.Total.AttackDamageStat.Value, 54) && Mathf.Approximately(snapshot.Total.MaxHPStat.Value, 180), "Permanent damage and HP multipliers match gameplay");
                check(Mathf.Approximately(snapshot.Total.MovementSpeedStat.Value, 6.6f) && Mathf.Approximately(snapshot.Total.AttackSpeedStat.Value, 1.8f), "Movement and attack speed preview use multiplicative effects");
                check(snapshot.Total.CritChanceStat.Value == 17 && snapshot.Total.DodgeChanceStat.Value == 7 && snapshot.Total.RevivesStat.Value == 2, "Crit dodge and revives use additive effects");
                check(Mathf.Approximately(snapshot.Total.DamageReduction.Value, .75f) && snapshot.WaveEndHealing == 10 && snapshot.Total.HPRecoveryStat.Value == 0, "Shield and wave-end recovery keep gameplay units");
                check(Mathf.Approximately(snapshot.Total.FrontArrowDamageStat.Value, 54), "Linked projectile damage reflects permanent upgrade");
                check(ArmoryStatsSnapshot.Calculate(hero, gear, upgrades).Total.CritChanceStat.Value == 17 && basic.Total.CritChanceStat.Value == 12, "Repeated previews never modify or accumulate player stats");
                check(snapshot.Details().Contains("Dodge Chance") && snapshot.Details().Contains("Wave end healing"), "Details includes additional stats");

                var money = new MemoryCurrencies(); var saves = new MemorySaves(); var type = UpgradeType.MorePower; upgrades.levels[type] = -1;
                var purchase = new PermanentUpgradeService(upgrades, money, saves);
                check(purchase.TryBuy(type, -1) == UpgradePurchaseResult.InsufficientFunds && upgrades.levels[type] == -1, "Upgrade rejects insufficient funds");
                money.Meta.Deposit(100); UpgradePurchaseResult reentry = UpgradePurchaseResult.Success;
                UnityEngine.Events.UnityAction<CurrencySave> reentryListener = w => reentry = purchase.TryBuy(type, 0);
                money.Meta.OnAmountChanged += reentryListener;
                check(purchase.TryBuy(type, -1) == UpgradePurchaseResult.Success && reentry == UpgradePurchaseResult.Busy && upgrades.levels[type] == 0 && money.Meta.Amount == 0 && saves.Calls == 1, "Real next-tier price is paid and saved exactly once");
                money.Meta.OnAmountChanged -= reentryListener;
                check(purchase.TryBuy(type, -1) == UpgradePurchaseResult.Stale, "Stale upgrade display cannot purchase a second tier");
                money.Meta.Deposit(200); check(purchase.TryBuy(type, 0) == UpgradePurchaseResult.Success && upgrades.levels[type] == 1 && money.Meta.Amount == 0, "Next Inspector price applied");
                check(purchase.TryBuy(type, 1) == UpgradePurchaseResult.Maximum && saves.Calls == 2, "Maximum upgrade cannot consume further gold");
                return "Armory / Upgrades: " + checks + " isolated checks passed. Player data untouched.";
            }
            finally { UnityEngine.Object.DestroyImmediate(config); upgrades.Dispose(); }
        }
        public static string VerifyLiveBagButton()
        {
            var page = GameController.MainMenuScreenBehavior.CurrentPage as MainMenuArmoryPageBehavior;
            if (page == null) throw new Exception("Open Armory first");
            var manager = GameController.ArmoryManager; var prop = typeof(ArmoryManager).GetProperty("Bag"); var original = prop.GetValue(manager);
            var save = new ArmorySave(); save.Init(); var money = new MemoryCurrencies(); money.Meta.Deposit(100); var saves = new MemorySaves();
            var bag = new InventoryBagService(save, Resources.Load<ArmoryDashboardDatabase>(ArmoryDashboardDatabase.ResourcePath), money, saves);
            var dashboard = page.GetComponent<ArmoryDashboard>();
            try
            {
                prop.SetValue(manager, bag);
                page.transform.Find("Armory Panel/Expand Bag").GetComponent<Button>().onClick.Invoke();
                var modal = page.transform.Find("Armory Modal");
                if (!modal.gameObject.activeSelf || GameController.MainMenuScreenBehavior.IsDockEnabled) throw new Exception("Expansion modal must lock dock");
                modal.Find("Panel/Confirm").GetComponent<Button>().onClick.Invoke();
                if (save.BagCapacity != 50 || money.Meta.Amount != 0 || saves.Calls != 1 || !GameController.MainMenuScreenBehavior.IsDockEnabled) throw new Exception("Live purchase did not commit once");
                return "Live authored bag button: modal, dock lock, +10 slots, one payment and persistence passed using isolated data.";
            }
            finally { dashboard.CloseModal(); prop.SetValue(manager, original); dashboard.MarkDirty(); }
        }
        public static string VerifyLiveUpgradeButton()
        {
            var page = GameController.MainMenuScreenBehavior.CurrentPage as UpgradeDashboardPage;
            if (page == null) throw new Exception("Open Upgrades first");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var rows = (List<UpgradeDashboardPage.Row>)typeof(UpgradeDashboardPage).GetField("rows", flags).GetValue(page);
            var row = rows.Find(r => r.data.UpgradeType == UpgradeType.CritChance);
            var actualLevel = GameController.UpgradesManager.GetUpgradeLevel(row.data.UpgradeType);
            var actualGold = GameController.CurrenciesManager.GetCurrency("gold", false).Amount;
            var upgrades = new MemoryUpgrades(); upgrades.AddExisting(row.data, actualLevel);
            var wallets = new MemoryCurrencies(); wallets.Meta.Deposit(row.data.GetLevel(actualLevel + 1).Price.Amount);
            var saves = new MemorySaves(); var purchases = new PermanentUpgradeService(upgrades, wallets, saves);
            var field = typeof(UpgradeDashboardPage).GetField("purchases", flags); var original = field.GetValue(page);
            try
            {
                field.SetValue(page, purchases); row.select.onClick.Invoke(); row.buy.onClick.Invoke();
                if (upgrades.GetUpgradeLevel(row.data.UpgradeType) != actualLevel + 1 || wallets.Meta.Amount != 0 || saves.Calls != 1) throw new Exception("Live upgrade button failed");
                if (GameController.UpgradesManager.GetUpgradeLevel(row.data.UpgradeType) != actualLevel || GameController.CurrenciesManager.GetCurrency("gold", false).Amount != actualGold) throw new Exception("Player data changed during isolated UI verification");
                return "Live authored upgrade button: selection, purchase, single payment and save passed; real level and gold unchanged.";
            }
            finally { field.SetValue(page, original); typeof(UpgradeDashboardPage).GetMethod("Refresh", flags).Invoke(page, null); upgrades.Dispose(); }
        }
        private sealed class MemoryUpgrades : IUpgradesManager, IDisposable
        {
            public readonly Dictionary<UpgradeType, int> levels = new Dictionary<UpgradeType, int>();
            private readonly Dictionary<UpgradeType, UpgradeData> data = new Dictionary<UpgradeType, UpgradeData>();
            public void AddExisting(UpgradeData entry, int level) { data[entry.UpgradeType] = UnityEngine.Object.Instantiate(entry); levels[entry.UpgradeType] = level; }
            public void Add(UpgradeType type, float value)
            {
                var entry = ScriptableObject.CreateInstance<UpgradeData>();
                JsonUtility.FromJsonOverwrite("{\"upgradeType\":" + (int)type + ",\"levels\":[{\"value\":" + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\"price\":{\"currencyId\":\"gold\",\"amount\":100}},{\"value\":2,\"price\":{\"currencyId\":\"gold\",\"amount\":200}}]}", entry);
                data[type] = entry; levels[type] = 0;
            }
            public int GetUpgradeLevel(UpgradeType type) => levels.TryGetValue(type, out var level) ? level : -1;
            public bool IsUpgradeAquired(UpgradeType type) => GetUpgradeLevel(type) >= 0;
            public UpgradeData GetUpgradeData(UpgradeType type) => data.TryGetValue(type, out var entry) ? entry : null;
            public List<UpgradeData> GetAllUpgrades() => new List<UpgradeData>(data.Values);
            public void IncrementUpgradeLevel(UpgradeType type) { levels[type]++; data[type].SetLevel(levels[type]); }
            public void OnStageLoaded() { } public void OnStageUnloaded() { }
            public void Dispose() { foreach (var entry in data.Values) UnityEngine.Object.DestroyImmediate(entry); }
        }
        private sealed class MemorySaves : ISaveManager
        {
            public int Calls;
            public T GetSave<T>(int id) where T : ISave, new() => new T();
            public T GetSave<T>(string id) where T : ISave, new() => new T();
            public void Save(bool multithreading = false) { Calls++; }
        }
        private sealed class MemoryCurrencies : ICurrenciesManager
        {
            public readonly CurrencySave Meta = new CurrencySave(), Gameplay = new CurrencySave();
            public CurrencySave GetCurrency(string id, bool gameplay) => id == "gold" ? gameplay ? Gameplay : Meta : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => gameplay ? Gameplay : Meta;
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { GetDefaultCurrency(gameplay) };
            public void ApplyGameplayAmounts() { Meta.Deposit(Gameplay.Amount); Gameplay.Clear(); }
            public void EraseGameplayAmounts() { Gameplay.Clear(); }
        }
    }
}
