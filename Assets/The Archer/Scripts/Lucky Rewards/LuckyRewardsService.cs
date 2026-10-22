using System;
using System.Collections.Generic;
using MrX.Crafting;
using MrX.Currency;
using MrX.Save;
using UnityEngine;

namespace MrX.Rewards
{
    [Serializable] public sealed class LuckyGrant
    {
        public LuckyRewardKind kind;
        public string currencyId, itemId, title;
        public CraftingMaterial material;
        public int amount;
        public bool queuedEquipment;
        public bool pendingDelivery;
    }
    [Serializable] public sealed class LuckyReceipt
    {
        public long sequence;
        public string tableId, prizeId;
        public int prizeIndex;
        public bool acknowledged;
        public bool dailyFreeUse;
        public int gemCost;
        public LuckyGrant grant;
    }
    [Serializable] public sealed class LuckyDailyUse
    {
        public string tableId;
        public long usedDay;
    }
    [Serializable] public sealed class LuckyRewardsSave : ISave
    {
        public long sequence;
        public LuckyReceipt lastReceipt;
        public List<LuckyDailyUse> dailyFreeUses = new List<LuckyDailyUse>();
        // Unity can deserialize an inline null class as a default-valued object.
        // A receipt exists only after a positive, committed purchase sequence.
        public bool HasUnseen => lastReceipt != null && lastReceipt.sequence > 0 && lastReceipt.sequence <= sequence &&
            !string.IsNullOrEmpty(lastReceipt.tableId) && !string.IsNullOrEmpty(lastReceipt.prizeId) &&
            lastReceipt.grant != null && lastReceipt.grant.amount > 0 && !lastReceipt.acknowledged;
        public void Flush() { }
    }
    public interface ILuckyRewardDelivery
    {
        bool Resolve(LuckyPrize prize, int amount, int gemCost, out LuckyGrant grant);
        void Grant(LuckyGrant grant);
        Sprite Icon(LuckyGrant grant);
    }
    public enum LuckyPurchaseResult { Ready, Success, InsufficientGems, InvalidConfiguration, PendingReward, Busy, RewardUnavailable }
    public sealed class LuckyRewardsService
    {
        public LuckyRewardsSave Save { get; }
        public ILuckyRewardDelivery Delivery { get; }
        public event Action Changed;
        private readonly ICurrenciesManager wallets;
        private readonly ISaveManager persistence;
        private readonly Func<long> utc;
        private bool purchasing;
        public LuckyRewardsService(ICurrenciesManager wallets, ISaveManager persistence, ILuckyRewardDelivery delivery, Func<long> clock = null)
        { this.wallets = wallets; this.persistence = persistence; Delivery = delivery; utc = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds()); Save = persistence.GetSave<LuckyRewardsSave>("Lucky Rewards"); }
        private long Day(LuckyRewardTable table) => (long)Math.Floor((utc() + table.dailyResetUtcOffsetHours * 3600d) / 86400d);
        public bool HasDailyFreeUse(LuckyRewardTable table) => TryGetDailyFreeDay(table, out _);
        private bool TryGetDailyFreeDay(LuckyRewardTable table, out long day)
        {
            day = 0;
            if (table == null || !table.dailyFreeUse || string.IsNullOrEmpty(table.id) || table.dailyResetUtcOffsetHours < -12 || table.dailyResetUtcOffsetHours > 14) return false;
            day = Day(table);
            return FreeOnDay(table, day);
        }
        private bool FreeOnDay(LuckyRewardTable table, long day)
        {
            if (Save.dailyFreeUses != null)
                foreach (var use in Save.dailyFreeUses)
                    if (use != null && use.tableId == table.id && use.usedDay >= day) return false;
            return true;
        }
        private void ConsumeDailyFreeUse(LuckyRewardTable table, long day)
        {
            if (Save.dailyFreeUses == null) Save.dailyFreeUses = new List<LuckyDailyUse>();
            foreach (var use in Save.dailyFreeUses)
                if (use != null && use.tableId == table.id) { use.usedDay = day; return; }
            Save.dailyFreeUses.Add(new LuckyDailyUse { tableId = table.id, usedDay = day });
        }
        public LuckyPurchaseResult State(LuckyRewardTable table)
        {
            if (table == null || !table.IsValid(out _) || Save.sequence == long.MaxValue) return LuckyPurchaseResult.InvalidConfiguration;
            if (Save.HasUnseen) return LuckyPurchaseResult.PendingReward;
            var wallet = wallets.GetCurrency("gem", false);
            if (wallet == null) return LuckyPurchaseResult.InvalidConfiguration;
            int cost = HasDailyFreeUse(table) ? 0 : table.price.Amount;
            foreach (var prize in table.prizes)
                if (prize != null && prize.enabled && prize.weight > 0 && !Delivery.Resolve(prize, prize.maximumAmount, cost, out _)) return LuckyPurchaseResult.RewardUnavailable;
            return wallet.CanAfford(cost) ? LuckyPurchaseResult.Ready : LuckyPurchaseResult.InsufficientGems;
        }
        public LuckyPurchaseResult TryPurchase(LuckyRewardTable table, double selectionRoll, double amountRoll, out LuckyReceipt receipt)
        {
            receipt = null;
            if (purchasing) return LuckyPurchaseResult.Busy;
            var state = State(table); if (state != LuckyPurchaseResult.Ready) return state;
            if (double.IsNaN(amountRoll) || double.IsInfinity(amountRoll) || amountRoll < 0 || amountRoll > 1) return LuckyPurchaseResult.InvalidConfiguration;
            int index = table.Pick(selectionRoll); if (index < 0) return LuckyPurchaseResult.InvalidConfiguration;
            var prize = table.prizes[index];
            bool free = TryGetDailyFreeDay(table, out var freeDay);
            int cost = free ? 0 : table.price.Amount;
            int amount = Math.Min(prize.maximumAmount, prize.minimumAmount + (int)(amountRoll * (prize.maximumAmount - prize.minimumAmount + 1)));
            if (!Delivery.Resolve(prize, amount, cost, out var grant)) return LuckyPurchaseResult.RewardUnavailable;
            var selected = new LuckyReceipt { sequence = Save.sequence + 1, tableId = table.id, prizeId = prize.id, prizeIndex = index, grant = grant, dailyFreeUse = free, gemCost = cost };
            purchasing = true; bool committed = false;
            try
            {
                if (!wallets.GetCurrency("gem", false).TryWithdrawAndCommit(cost, () =>
                {
                    // The receipt blocks reentry and recovery never grants it a second time.
                    Save.sequence = selected.sequence; Save.lastReceipt = selected; committed = true;
                    if (free) ConsumeDailyFreeUse(table, freeDay);
                    Delivery.Grant(grant);
                })) return LuckyPurchaseResult.InsufficientGems;
                receipt = selected; return LuckyPurchaseResult.Success;
            }
            finally
            {
                try { if (committed) { try { persistence.Save(false); } finally { Changed?.Invoke(); } } }
                finally { purchasing = false; }
            }
        }
        public bool Acknowledge(long sequence)
        {
            if (purchasing || !Save.HasUnseen || Save.lastReceipt.sequence != sequence) return false;
            Save.lastReceipt.acknowledged = true; persistence.Save(false); Changed?.Invoke(); return true;
        }
    }
}
