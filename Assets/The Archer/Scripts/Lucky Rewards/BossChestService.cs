using System;
using System.Collections.Generic;
using MrX.Save;
using UnityEngine;

namespace MrX.Rewards
{
    [Serializable] public sealed class BossChestReceipt
    {
        public string key, runToken;
        public LuckyGrant grant;
        // claimed is the old persisted grant flag. Never re-grant legacy receipts.
        public bool claimed, collected;
        public bool settlementRequested;
        public int maxLevelDuplicateGold = 100;
    }
    [Serializable] public sealed class BossChestSave : ISave
    {
        public string runToken;
        public List<BossChestReceipt> receipts = new List<BossChestReceipt>();
        public void Flush() { }
    }
    public sealed class BossChestService
    {
        private readonly ILuckyRewardDelivery delivery;
        private readonly ArmoryManager armory;
        private readonly BossChestSave save;
        private readonly ISaveManager saves;
        private bool busy;
        public BossChestService(ArmoryManager armory, ILuckyRewardDelivery delivery, ISaveManager saves)
        {
            this.armory = armory; this.delivery = delivery; this.saves = saves; save = saves.GetSave<BossChestSave>("Boss Chests");
            if (save.receipts == null) save.receipts = new List<BossChestReceipt>();
            foreach (var receipt in save.receipts) if (receipt != null && string.IsNullOrEmpty(receipt.runToken)) receipt.runToken = save.runToken;
        }
        public BossChestReceipt Prepare(string runToken, string key, BossChestRewards table, double roll, double quantity)
        { return PreparePrize(runToken, key, table != null ? table.Pick(roll) : null, quantity); }

        // Uses the live Stage Creator drop pool, including repeated item entries.
        // Multiple entries are weights, just as the original boss GetOverride path.
        public BossChestReceipt PrepareFromDrops(string token, string key, List<EnemyDropData> drops, float hp, double roll, double quantity)
        {
            if (drops == null || double.IsNaN(roll) || roll < 0 || roll > 1) return null;
            double total = 0; int count = 0; EnemyDropData last = null;
            foreach (var d in drops) if (d != null && d.DropType == Drop.DropType.Item && d.ItemData != null)
            { count++; float chance = d.GetChance(hp); if (chance > 0 && !float.IsInfinity(chance) && !float.IsNaN(chance)) { total += chance; last = d; } }
            if (last == null || total <= 0) return null;
            if (count == 1 && total < 100 && roll * 100 >= total) return null;
            double target = roll * total, sum = 0; var selected = last;
            foreach (var d in drops) if (d != null && d.DropType == Drop.DropType.Item && d.ItemData != null)
            { float chance = d.GetChance(hp); if (chance <= 0 || float.IsNaN(chance) || float.IsInfinity(chance)) continue; sum += chance; if (target < sum) { selected = d; break; } }
            return PreparePrize(token, key, new LuckyPrize { kind = LuckyRewardKind.Equipment, item = selected.ItemData }, quantity);
        }
        private BossChestReceipt PreparePrize(string token, string key, LuckyPrize prize, double quantity)
        {
            if (busy || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(key) || double.IsNaN(quantity) || quantity < 0 || quantity > 1) return null;
            var found = save.receipts.Find(r => r != null && r.key == key); if (found != null) return found;
            if (prize == null || prize.item == null || prize.minimumAmount < 1 || prize.maximumAmount < prize.minimumAmount || prize.maximumAmount > 1000000) return null;
            if (prize.kind != LuckyRewardKind.Equipment && prize.kind != LuckyRewardKind.Fragments) return null;
            if (armory != null && armory.GetItemData(prize.item.Id) != prize.item) return null;
            if (prize.kind == LuckyRewardKind.Equipment && (prize.minimumAmount != 1 || prize.maximumAmount != 1)) return null;
            int amount = prize.minimumAmount + Math.Min(prize.maximumAmount - prize.minimumAmount, (int)(quantity * ((long)prize.maximumAmount - prize.minimumAmount + 1)));
            // Selection is run-local. Ownership/full-bag/MAX compensation is resolved
            // at settlement, not when the chest drops or is picked up.
            var grant = new LuckyGrant { kind = prize.kind, itemId = prize.item.Id, amount = amount, title = prize.item.ItemName + (prize.kind == LuckyRewardKind.Fragments ? " fragments" : "") };
            if (save.runToken != token) { save.receipts.RemoveAll(r => r == null || r.runToken != token && (!r.collected || r.claimed)); save.runToken = token; }
            var receipt = new BossChestReceipt { key = key, runToken = token, grant = grant, maxLevelDuplicateGold = prize.maxLevelDuplicateGold };
            save.receipts.Add(receipt); return receipt;
        }
        public bool Claim(BossChestReceipt receipt)
        {
            if (busy || receipt == null || receipt.claimed || receipt.collected || receipt.grant == null || !save.receipts.Contains(receipt)) return false;
            receipt.collected = true; return true;
        }
        public List<LuckyGrant> GetCollected(string token)
        {
            var result = new List<LuckyGrant>();
            foreach (var receipt in save.receipts) if (receipt != null && receipt.runToken == token && receipt.collected && receipt.grant != null) result.Add(receipt.grant);
            return result;
        }
        public void CommitRun(string token)
        {
            if (busy) return; busy = true;
            try
            {
                foreach (var receipt in save.receipts)
                {
                    if (receipt == null || receipt.runToken != token || !receipt.collected || receipt.claimed || receipt.grant == null) continue;
                    receipt.settlementRequested = true;
                    var prior = receipt.grant;
                    var p = new LuckyPrize { kind = prior.kind, item = !string.IsNullOrEmpty(prior.itemId) ? armory?.GetItemData(prior.itemId) : null, maxLevelDuplicateGold = receipt.maxLevelDuplicateGold };
                    if (!delivery.Resolve(p, prior.amount, 0, out var actual)) { prior.pendingDelivery = true; continue; }
                    receipt.claimed = true; receipt.grant = actual; delivery.Grant(actual);
                }
            }
            finally { busy = false; }
        }
        // Only retry rewards from an already finalized run (e.g. a capped wallet).
        // Ground loot and unfinished runs are never awarded on loading the menu.
        public void RecoverPending()
        {
            var tokens = new HashSet<string>(); int before = 0;
            foreach (var r in save.receipts) if (r != null)
            { if (r.claimed) before++; else if (r.collected && r.settlementRequested) tokens.Add(r.runToken); }
            foreach (var token in tokens) CommitRun(token);
            int after = 0; foreach (var r in save.receipts) if (r != null && r.claimed) after++;
            if (after != before) saves.Save(false);
        }
    }
}
