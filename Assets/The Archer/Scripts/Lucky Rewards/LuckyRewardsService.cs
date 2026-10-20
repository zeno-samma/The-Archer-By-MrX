using System;
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
        public LuckyGrant grant;
    }
    [Serializable] public sealed class LuckyRewardsSave : ISave
    {
        public long sequence;
        public LuckyReceipt lastReceipt;
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
        private bool purchasing;
        public LuckyRewardsService(ICurrenciesManager wallets, ISaveManager persistence, ILuckyRewardDelivery delivery)
        { this.wallets = wallets; this.persistence = persistence; Delivery = delivery; Save = persistence.GetSave<LuckyRewardsSave>("Lucky Rewards"); }
        public LuckyPurchaseResult State(LuckyRewardTable table)
        {
            if (table == null || !table.IsValid(out _) || Save.sequence == long.MaxValue) return LuckyPurchaseResult.InvalidConfiguration;
            if (Save.HasUnseen) return LuckyPurchaseResult.PendingReward;
            var wallet = wallets.GetCurrency("gem", false);
            if (wallet == null) return LuckyPurchaseResult.InvalidConfiguration;
            foreach (var prize in table.prizes)
                if (prize != null && prize.enabled && prize.weight > 0 && !Delivery.Resolve(prize, prize.maximumAmount, table.price.Amount, out _)) return LuckyPurchaseResult.RewardUnavailable;
            return wallet.CanAfford(table.price.Amount) ? LuckyPurchaseResult.Ready : LuckyPurchaseResult.InsufficientGems;
        }
        public LuckyPurchaseResult TryPurchase(LuckyRewardTable table, double selectionRoll, double amountRoll, out LuckyReceipt receipt)
        {
            receipt = null;
            if (purchasing) return LuckyPurchaseResult.Busy;
            var state = State(table); if (state != LuckyPurchaseResult.Ready) return state;
            if (double.IsNaN(amountRoll) || double.IsInfinity(amountRoll) || amountRoll < 0 || amountRoll > 1) return LuckyPurchaseResult.InvalidConfiguration;
            int index = table.Pick(selectionRoll); if (index < 0) return LuckyPurchaseResult.InvalidConfiguration;
            var prize = table.prizes[index];
            int amount = Math.Min(prize.maximumAmount, prize.minimumAmount + (int)(amountRoll * (prize.maximumAmount - prize.minimumAmount + 1)));
            if (!Delivery.Resolve(prize, amount, table.price.Amount, out var grant)) return LuckyPurchaseResult.RewardUnavailable;
            var selected = new LuckyReceipt { sequence = Save.sequence + 1, tableId = table.id, prizeId = prize.id, prizeIndex = index, grant = grant };
            purchasing = true; bool committed = false;
            try
            {
                if (!wallets.GetCurrency("gem", false).TryWithdrawAndCommit(table.price.Amount, () =>
                {
                    // The receipt blocks reentry and recovery never grants it a second time.
                    Save.sequence = selected.sequence; Save.lastReceipt = selected; committed = true;
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
