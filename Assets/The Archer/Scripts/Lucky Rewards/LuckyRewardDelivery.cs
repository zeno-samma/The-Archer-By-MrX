using System;
using MrX.Crafting;
using MrX.Currency;
using UnityEngine;

namespace MrX.Rewards
{
    public sealed class LuckyRewardDelivery : ILuckyRewardDelivery
    {
        private readonly ArmoryManager armory;
        private readonly ICurrenciesManager wallets;
        public LuckyRewardDelivery(ArmoryManager armory, ICurrenciesManager wallets) { this.armory = armory; this.wallets = wallets; }
        public bool Resolve(LuckyPrize prize, int amount, int gemCost, out LuckyGrant grant)
        {
            grant = null; if (prize == null || amount < 1) return false;
            var output = new LuckyGrant { kind = prize.kind, amount = amount, currencyId = prize.currencyId, material = prize.material };
            switch (prize.kind)
            {
                case LuckyRewardKind.Currency:
                    var wallet = wallets.GetCurrency(prize.currencyId, false); if (wallet == null) return false;
                    long remaining = (long)wallet.Amount - (prize.currencyId == "gem" ? gemCost : 0);
                    if (remaining + amount > int.MaxValue) return false;
                    output.title = wallet.Data != null ? wallet.Data.Name : prize.currencyId; break;
                case LuckyRewardKind.Equipment:
                case LuckyRewardKind.Fragments:
                    var item = prize.item;
                    if (item == null || armory.GetItemData(item.Id) != item || item.ItemLevelCount < 1) return false;
                    output.itemId = item.Id; output.title = item.ItemName;
                    if (prize.kind == LuckyRewardKind.Fragments)
                    {
                        if (item.Crafting == null || !item.Crafting.enabled || !item.Crafting.IsValid() || armory.Crafting == null) return false;
                        bool registered = false;
                        foreach (var recipe in armory.Crafting.Service.Recipes) if (recipe.Id == item.Id) { registered = true; break; }
                        if (!registered) return false;
                        if (armory.Crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(item.Id)) > long.MaxValue - amount) return false;
                        output.title += " fragments";
                    }
                    else
                    {
                        if (amount != 1) return false;
                        var owned = armory.GetItemSaveList().Find(entry => entry.Id == item.Id);
                        if (owned != null && owned.Level >= item.ItemLevelCount - 1)
                        {
                            var gold = wallets.GetCurrency("gold", false);
                            if (prize.maxLevelDuplicateGold < 1 || gold == null || (long)gold.Amount + prize.maxLevelDuplicateGold > int.MaxValue) return false;
                            output.kind = LuckyRewardKind.Currency; output.currencyId = "gold"; output.amount = prize.maxLevelDuplicateGold; output.title = "Gold (max-level equipment compensation)";
                        }
                        else output.queuedEquipment = owned == null && !armory.Bag.Save.HasBagSpace;
                    }
                    break;
                case LuckyRewardKind.Material:
                    if (!Enum.IsDefined(typeof(CraftingMaterial), prize.material) || armory.Crafting == null || armory.Crafting.Database.Material(prize.material) == null || armory.Crafting.Service.Inventory.Count(CraftingInventorySave.MaterialKey(prize.material)) > long.MaxValue - amount) return false;
                    output.title = prize.material.ToString(); break;
                default: return false;
            }
            grant = output; return true;
        }
        public void Grant(LuckyGrant grant)
        {
            switch (grant.kind)
            {
                case LuckyRewardKind.Currency: wallets.GetCurrency(grant.currencyId, false).Deposit(grant.amount); break;
                case LuckyRewardKind.Equipment:
                    if (armory.IsItemUnlocked(grant.itemId)) armory.IncreaseItemLevel(grant.itemId); else armory.UnlockItem(grant.itemId); break;
                case LuckyRewardKind.Fragments: armory.Crafting.Service.AddLoot(new CraftingLoot { isFragment = true, itemId = grant.itemId, amount = grant.amount }); break;
                case LuckyRewardKind.Material: armory.Crafting.Service.AddLoot(new CraftingLoot { material = grant.material, amount = grant.amount }); break;
            }
        }
        public Sprite Icon(LuckyGrant grant)
        {
            if (grant == null) return null;
            if (grant.kind == LuckyRewardKind.Currency) return wallets.GetCurrency(grant.currencyId, false)?.Data?.Icon;
            if (grant.kind == LuckyRewardKind.Equipment || grant.kind == LuckyRewardKind.Fragments) return armory.GetItemData(grant.itemId)?.Icon;
            if (armory.Crafting != null)
                foreach (var material in armory.Crafting.Database.materials) if (material.material == grant.material) return material.icon;
            return null;
        }
    }
}
