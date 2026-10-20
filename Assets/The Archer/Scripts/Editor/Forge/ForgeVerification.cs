using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Armory;
using MrX.Currency;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class ForgeVerification
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        [MenuItem("Tools/MrX Studio/Forge/Run Level Upgrade Checks")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            var checks = 0;
            Action<bool, string> check = (ok, reason) => { if (!ok) throw new Exception("Forge check: " + reason); checks++; };
            var achievements = typeof(GameController).GetProperty("AchievementsManager");
            var originalAchievements = achievements.GetValue(null); achievements.SetValue(null, null);
            var database = ScriptableObject.CreateInstance<ArmoryDatabase>();
            var item = ScriptableObject.CreateInstance<ItemData>();
            try
            {
                JsonUtility.FromJsonOverwrite("{\"guid\":\"forge-test\",\"itemName\":\"Test Bow\",\"itemType\":0,\"itemLevels\":[{\"price\":{\"currencyId\":\"gold\",\"amount\":0},\"stats\":[{\"statType\":0,\"value\":10,\"isVisibleOnUI\":true}]},{\"price\":{\"currencyId\":\"gold\",\"amount\":100},\"stats\":[{\"statType\":0,\"value\":15,\"isVisibleOnUI\":true}]},{\"price\":{\"currencyId\":\"gold\",\"amount\":200},\"stats\":[{\"statType\":0,\"value\":20,\"isVisibleOnUI\":true}]}]}", item);
                typeof(ArmoryDatabase).GetField("items", Fields).SetValue(database, new List<ItemData> { item });
                var saves = new MemorySaves(); var wallets = new MemoryCurrencies(); var armory = saves.GetSave<ArmorySave>("Armory"); armory.Init();
                var service = new EquipmentForge(database, armory, wallets, saves);
                check(service.GetQuote(null).result == ForgeResult.InvalidData && service.GetQuote("unknown").result == ForgeResult.InvalidData, "null/unknown identities");
                check(service.TryUpgrade(item.Id, 0) == ForgeResult.Locked, "unowned equipment locked");
                var owned = armory.AddItem(item.Id); owned.IsEquipped = true;
                check(service.GetQuote(item.Id).result == ForgeResult.InsufficientFunds && service.TryUpgrade(item.Id, 0) == ForgeResult.InsufficientFunds, "insufficient funds rejected");
                check(owned.Level == 0 && saves.SaveCalls == 0, "rejection does not change data");
                wallets.Meta.Deposit(100);
                var quote = service.GetQuote(item.Id);
                check(quote.result == ForgeResult.Ready && quote.level == 0 && quote.price.Amount == 100 && quote.current.GetStatValue(StatType.Damage) == 10 && quote.next.GetStatValue(StatType.Damage) == 15, "quote matches real next-level price/stats");
                ForgeResult reentrant = ForgeResult.Success;
                bool consistent = false;
                wallets.Meta.OnAmountChanged += currency => { consistent = owned.Level == 1 && currency.Amount == 0; reentrant = service.TryUpgrade(item.Id, 0); };
                check(service.TryUpgrade(item.Id, 0) == ForgeResult.Success && reentrant == ForgeResult.Busy && consistent, "wallet observers see committed level and cannot reenter");
                check(owned.Level == 1 && owned.IsEquipped && wallets.Meta.Amount == 0 && wallets.Gameplay.Amount == 0 && saves.SaveCalls == 1, "one level, permanent payment, equipped state and immediate persistence");
                wallets.Meta.Deposit(200);
                check(service.TryUpgrade(item.Id, 0) == ForgeResult.StaleLevel && wallets.Meta.Amount == 200 && owned.Level == 1, "stale displayed level cannot buy the next tier");
                check(service.TryUpgrade(item.Id, 1) == ForgeResult.Success && owned.Level == 2 && wallets.Meta.Amount == 0, "next tier uses its own cost");
                check(service.TryUpgrade(item.Id, 2) == ForgeResult.MaxLevel && service.GetQuote(item.Id).next == null && saves.SaveCalls == 2, "maximum level safe and free of further charges");
                var restored = JsonUtility.FromJson<ArmorySave>(JsonUtility.ToJson(Flush(armory))); restored.Init();
                check(restored.GetItem(item.Id).Level == 2 && restored.GetItem(item.Id).IsEquipped, "existing Armory save roundtrip");
                JsonUtility.FromJsonOverwrite("{\"level\":-1}", owned);
                check(service.GetQuote(item.Id).result == ForgeResult.InvalidData, "negative legacy index guarded");
                JsonUtility.FromJsonOverwrite("{\"level\":9999}", owned);
                check(service.GetQuote(item.Id).result == ForgeResult.InvalidData, "out-of-range legacy index guarded");
                JsonUtility.FromJsonOverwrite("{\"level\":0}", owned);
                wallets.HasCurrency = false;
                check(service.TryUpgrade(item.Id, 0) == ForgeResult.InvalidData && owned.Level == 0, "missing wallet guarded");
                wallets.HasCurrency = true;
                typeof(ItemLevel).GetField("price", Fields).SetValue(item.ItemLevels[1], new Price("gold", -100));
                check(service.TryUpgrade(item.Id, 0) == ForgeResult.InvalidData, "negative price cannot mint gold");
                typeof(ItemLevel).GetField("price", Fields).SetValue(item.ItemLevels[1], new Price("gold", 100));
                var throwingWallets = new MemoryCurrencies(); throwingWallets.Meta.Deposit(100);
                var failureSaves = new MemorySaves();
                var failureService = new EquipmentForge(database, armory, throwingWallets, failureSaves);
                throwingWallets.Meta.OnAmountChanged += currency => { throw new InvalidOperationException("test wallet listener"); };
                try { failureService.TryUpgrade(item.Id, 0); } catch (InvalidOperationException) { }
                check(owned.Level == 1 && throwingWallets.Meta.Amount == 0 && failureSaves.SaveCalls == 1, "throwing wallet listener cannot leave a paid but unapplied upgrade");
                JsonUtility.FromJsonOverwrite("{\"level\":0}", owned);
                var itemFailureWallets = new MemoryCurrencies(); itemFailureWallets.Meta.Deposit(100);
                var itemFailureSaves = new MemorySaves(); var itemFailureService = new EquipmentForge(database, armory, itemFailureWallets, itemFailureSaves);
                owned.OnItemLevelChanged += (entry, level) => { throw new InvalidOperationException("test item listener"); };
                try { itemFailureService.TryUpgrade(item.Id, 0); } catch (InvalidOperationException) { }
                check(owned.Level == 1 && itemFailureWallets.Meta.Amount == 0 && itemFailureSaves.SaveCalls == 1, "throwing item listener preserves both sides of the purchase");
                bool negativeCommit = false;
                check(!itemFailureWallets.Meta.TryWithdrawAndCommit(-1, () => negativeCommit = true) && !negativeCommit && itemFailureWallets.Meta.Amount == 0, "transaction rejects negative withdrawals before commit");
                var pageData = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
                check(pageData != null && pageData.slotSprite != null && pageData.slotSprite.border.sqrMagnitude > 0 && pageData.slotMaterial != null && pageData.font != null, "real page references sliced pastel background and font");
                return "Stage/Armory upgrades: " + checks + " isolated logic checks passed. Player save, wallet and achievements untouched.";
            }
            finally { achievements.SetValue(null, originalAchievements); UnityEngine.Object.DestroyImmediate(item); UnityEngine.Object.DestroyImmediate(database); }
        }
        private static ArmorySave Flush(ArmorySave save) { save.Flush(); return save; }

        public static string VerifyLiveButtons()
        {
            return CraftingVerification.VerifyLiveButtons();
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
            public readonly CurrencySave Meta = new CurrencySave(); public readonly CurrencySave Gameplay = new CurrencySave(); public bool HasCurrency = true;
            public CurrencySave GetCurrency(string id, bool gameplay) => HasCurrency && id == "gold" ? (gameplay ? Gameplay : Meta) : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => GetCurrency("gold", gameplay);
            public void ApplyGameplayAmounts() { Meta.Deposit(Gameplay.Amount); Gameplay.Clear(); }
            public void EraseGameplayAmounts() { Gameplay.Clear(); }
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { gameplay ? Gameplay : Meta };
        }
    }
}
