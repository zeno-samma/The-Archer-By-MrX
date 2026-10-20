using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Save;

namespace MrX.Armory
{
    public enum ForgeResult { Ready, Success, Locked, MaxLevel, InvalidData, InsufficientFunds, StaleLevel, Busy }

    public struct ForgeQuote
    {
        public ItemData item;
        public ItemSaveData save;
        public ItemLevel current;
        public ItemLevel next;
        public CurrencySave wallet;
        public Price price;
        public ForgeResult result;
        public int level;
    }

    // Paid level upgrades in Stage/Armory popups. Equipment creation is handled
    // separately by Crafting.EquipmentCrafting; ItemLevels remain authoritative.
    public sealed class EquipmentForge
    {
        private readonly Dictionary<string, ItemData> items = new Dictionary<string, ItemData>(StringComparer.Ordinal);
        private readonly ArmorySave armory;
        private readonly ICurrenciesManager currencies;
        private readonly ISaveManager saves;
        private bool upgrading;

        public EquipmentForge(ArmoryDatabase database, ArmorySave save, ICurrenciesManager wallets, ISaveManager persistence)
        {
            armory = save; currencies = wallets; saves = persistence;
            foreach (var item in database.Items)
                if (item != null && !string.IsNullOrEmpty(item.Id) && !items.ContainsKey(item.Id)) items.Add(item.Id, item);
        }

        public ForgeQuote GetQuote(string id)
        {
            var quote = new ForgeQuote { result = ForgeResult.InvalidData };
            if (string.IsNullOrEmpty(id) || !items.TryGetValue(id, out quote.item) || armory == null || !armory.IsInitialized) return quote;
            quote.save = armory.GetItem(id);
            if (quote.save == null) { quote.result = ForgeResult.Locked; return quote; }
            quote.level = quote.save.Level;
            if (quote.level < 0 || quote.item.ItemLevels == null || quote.level >= quote.item.ItemLevelCount) return quote;
            quote.current = quote.item.GetItemLevel(quote.level);
            if (quote.current == null || quote.current.Stats == null) return quote;
            if (quote.level == quote.item.ItemLevelCount - 1) { quote.result = ForgeResult.MaxLevel; return quote; }
            quote.next = quote.item.GetItemLevel(quote.level + 1);
            if (quote.next == null || quote.next.Stats == null) return quote;
            quote.price = quote.next.Price;
            if (quote.price == null || quote.price.Amount < 0 || string.IsNullOrWhiteSpace(quote.price.CurrencyId)) return quote;
            quote.wallet = currencies.GetCurrency(quote.price.CurrencyId, false);
            if (quote.wallet == null) return quote;
            quote.result = quote.wallet.CanAfford(quote.price.Amount) ? ForgeResult.Ready : ForgeResult.InsufficientFunds;
            return quote;
        }

        public ForgeResult TryUpgrade(string id, int expectedLevel)
        {
            if (upgrading) return ForgeResult.Busy;
            var quote = GetQuote(id);
            if (quote.save != null && quote.level != expectedLevel) return ForgeResult.StaleLevel;
            if (quote.result != ForgeResult.Ready) return quote.result;
            upgrading = true;
            var charged = false;
            try
            {
                // Balance and level are committed before wallet observers run.
                // Even a throwing UI listener cannot leave a partial purchase.
                var paid = quote.wallet.TryWithdrawAndCommit(quote.price.Amount, () =>
                {
                    charged = true;
                    quote.save.Level = quote.level + 1;
                });
                return paid ? ForgeResult.Success : ForgeResult.InsufficientFunds;
            }
            finally
            {
                try { if (charged) saves.Save(false); }
                finally { upgrading = false; }
            }
        }
    }
}
