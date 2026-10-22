using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Rewards;
using MrX.Save;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class DailyLuckyRewardsVerification
    {
        [MenuItem("Tools/MrX Studio/Lucky Rewards/Run Daily Free Checks")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            int checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception(message); checks++; };
            var wheel = ScriptableObject.CreateInstance<LuckyRewardTable>();
            var chest = ScriptableObject.CreateInstance<LuckyRewardTable>();
            try
            {
                wheel.id = "daily-wheel-test"; chest.id = "daily-chest-test";
                wheel.dailyFreeUse = chest.dailyFreeUse = true;
                wheel.prizes.Add(new LuckyPrize { id = "gold", minimumAmount = 10, maximumAmount = 10 });
                chest.prizes.Add(new LuckyPrize { id = "gold", minimumAmount = 20, maximumAmount = 20 });
                chest.price = new Price("gem", 30);
                var wallets = new Wallets(); var saves = new Saves(); var delivery = new Delivery();
                long now = new DateTimeOffset(2026, 10, 20, 16, 59, 59, TimeSpan.Zero).ToUnixTimeSeconds();
                var service = new LuckyRewardsService(wallets, saves, delivery, () => now);
                check(service.HasDailyFreeUse(wheel) && service.HasDailyFreeUse(chest), "Independent initial free uses");
                check(service.State(wheel) == LuckyPurchaseResult.Ready && wallets.Gems.Amount == 0, "Zero gems can use free spin");
                check(saves.Writes == 0, "Reading availability never saves");
                check(service.TryPurchase(wheel, double.NaN, 0, out _) == LuckyPurchaseResult.InvalidConfiguration && service.HasDailyFreeUse(wheel), "Invalid roll does not consume quota");
                check(service.TryPurchase(wheel, 0, double.NaN, out _) == LuckyPurchaseResult.InvalidConfiguration && service.HasDailyFreeUse(wheel), "Invalid amount roll does not consume quota");
                delivery.Available = false;
                check(service.TryPurchase(wheel, 0, 0, out _) == LuckyPurchaseResult.RewardUnavailable && service.HasDailyFreeUse(wheel), "Unavailable reward does not consume quota");
                delivery.Available = true;
                LuckyPurchaseResult nested = LuckyPurchaseResult.Ready;
                UnityEngine.Events.UnityAction<CurrencySave> observer = currency => nested = service.TryPurchase(wheel, 0, 0, out _);
                wallets.Gems.OnAmountChanged += observer;
                check(service.TryPurchase(wheel, 0, 0, out var receipt) == LuckyPurchaseResult.Success && receipt.dailyFreeUse && receipt.gemCost == 0, "First wheel grant is free");
                wallets.Gems.OnAmountChanged -= observer;
                check(wallets.Gems.Amount == 0 && delivery.Total == 10 && nested == LuckyPurchaseResult.Busy && saves.Writes == 1, "Single grant, save and reentry guard at zero cost");
                check(!service.HasDailyFreeUse(wheel) && service.HasDailyFreeUse(chest), "Wheel does not spend chest free use");
                check(service.TryPurchase(chest, 0, 0, out _) == LuckyPurchaseResult.PendingReward, "Pending receipt still requires acknowledgement");
                saves.Cell = JsonUtility.FromJson<LuckyRewardsSave>(JsonUtility.ToJson(service.Save));
                service = new LuckyRewardsService(wallets, saves, delivery, () => now);
                check(!service.HasDailyFreeUse(wheel) && service.Save.HasUnseen && delivery.Total == 10, "Reload preserves used quota and receipt without duplicate grant");
                check(service.Acknowledge(receipt.sequence) && !service.Acknowledge(receipt.sequence), "Acknowledgement cannot grant again");
                check(service.TryPurchase(wheel, 0, 0, out _) == LuckyPurchaseResult.InsufficientGems, "Second use requires normal price");
                check(service.TryPurchase(chest, 0, 0, out receipt) == LuckyPurchaseResult.Success && receipt.dailyFreeUse && delivery.Total == 30 && wallets.Gems.Amount == 0, "Independent free chest with zero gems");
                service.Acknowledge(receipt.sequence);
                wallets.Gems.Deposit(100);
                check(service.TryPurchase(wheel, 0, 0, out receipt) == LuckyPurchaseResult.Success && !receipt.dailyFreeUse && receipt.gemCost == 10 && wallets.Gems.Amount == 90, "Paid wheel price unchanged after free use");
                service.Acknowledge(receipt.sequence);
                check(service.TryPurchase(chest, 0, 0, out receipt) == LuckyPurchaseResult.Success && receipt.gemCost == 30 && wallets.Gems.Amount == 60, "Paid chest price unchanged");
                service.Acknowledge(receipt.sequence);
                now += 2;
                check(service.HasDailyFreeUse(wheel) && service.HasDailyFreeUse(chest), "Reset at midnight UTC+7");
                check(service.TryPurchase(wheel, 0, 0, out receipt) == LuckyPurchaseResult.Success && receipt.dailyFreeUse && wallets.Gems.Amount == 60, "New day grants exactly one new free spin");
                service.Acknowledge(receipt.sequence); now -= 86400;
                check(!service.HasDailyFreeUse(wheel) && !service.HasDailyFreeUse(chest), "Clock rollback cannot reopen consumed dates");
                now += 86400 * 365L;
                check(service.TryPurchase(wheel, 0, 0, out receipt) == LuckyPurchaseResult.Success && receipt.dailyFreeUse, "Long absence gives a current free use");
                service.Acknowledge(receipt.sequence);
                check(!service.HasDailyFreeUse(wheel) && service.Save.dailyFreeUses.Count == 2, "Unused days do not accumulate; only two table identities stored");
                wheel.dailyFreeUse = false;
                check(!service.HasDailyFreeUse(wheel), "Inspector can disable free use");
                wheel.dailyFreeUse = true; wheel.dailyResetUtcOffsetHours = 99;
                check(service.State(wheel) == LuckyPurchaseResult.InvalidConfiguration, "Invalid reset configuration fails closed");
                wheel.dailyResetUtcOffsetHours = 7;
                saves.Cell = JsonUtility.FromJson<LuckyRewardsSave>("{\"sequence\":0}");
                var legacy = new LuckyRewardsService(wallets, saves, delivery, () => now);
                check(legacy.HasDailyFreeUse(wheel) && legacy.HasDailyFreeUse(chest), "Old saves receive both free uses without reset");
                check(legacy.TryPurchase(wheel, 0, 0, out receipt) == LuckyPurchaseResult.Success && receipt.dailyFreeUse, "Old save initializes quota on successful use");
                return "Daily lucky rewards: " + checks + " isolated checks passed.";
            }
            finally { UnityEngine.Object.DestroyImmediate(wheel); UnityEngine.Object.DestroyImmediate(chest); }
        }
        private sealed class Saves : ISaveManager
        {
            public LuckyRewardsSave Cell = new LuckyRewardsSave(); public int Writes;
            public T GetSave<T>(int id) where T : ISave, new() => (T)(object)Cell;
            public T GetSave<T>(string id) where T : ISave, new() => (T)(object)Cell;
            public void Save(bool multithreading = false) { Writes++; }
        }
        private sealed class Wallets : ICurrenciesManager
        {
            public readonly CurrencySave Gems = new CurrencySave();
            public CurrencySave GetCurrency(string id, bool gameplay) => id == "gem" ? Gems : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => Gems;
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { Gems };
            public void ApplyGameplayAmounts() { }
            public void EraseGameplayAmounts() { }
        }
        private sealed class Delivery : ILuckyRewardDelivery
        {
            public bool Available = true; public int Total;
            public bool Resolve(LuckyPrize prize, int amount, int cost, out LuckyGrant grant)
            { grant = new LuckyGrant { amount = amount, kind = LuckyRewardKind.Currency }; return Available; }
            public void Grant(LuckyGrant grant) { Total += grant.amount; }
            public Sprite Icon(LuckyGrant grant) => null;
        }
    }
}
