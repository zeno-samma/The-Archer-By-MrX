using System;
using MrX.Currency;
using MrX.Save;
using MrX.UI;

namespace MrX.Armory
{
    public enum BagExpansionResult { Ready, Success, InsufficientFunds, Maximum, Stale, Invalid, Busy }
    public sealed class InventoryBagService
    {
        public event Action Changed;
        public ArmorySave Save { get; }
        public ArmoryDashboardDatabase Configuration { get; }
        private readonly ICurrenciesManager currencies;
        private readonly ISaveManager persistence;
        private readonly Action claimPending;
        private bool expanding;
        public InventoryBagService(ArmorySave save, ArmoryDashboardDatabase config, ICurrenciesManager wallets, ISaveManager saves, Action pending = null)
        { Save = save; Configuration = config; currencies = wallets; persistence = saves; claimPending = pending; }
        public int NextCapacity => Configuration == null ? Save.BagCapacity : (int)Math.Min((long)Configuration.maximumCapacity, (long)Save.BagCapacity + Configuration.expansionSlots);
        public BagExpansionResult GetState()
        {
            var config = Configuration;
            if (config == null || config.expansionSlots < 1 || config.expansionPrice == null || config.expansionPrice.Amount < 0 || string.IsNullOrEmpty(config.expansionPrice.CurrencyId)) return BagExpansionResult.Invalid;
            if (NextCapacity <= Save.BagCapacity) return BagExpansionResult.Maximum;
            var wallet = currencies.GetCurrency(config.expansionPrice.CurrencyId, false);
            return wallet == null ? BagExpansionResult.Invalid : wallet.CanAfford(config.expansionPrice.Amount) ? BagExpansionResult.Ready : BagExpansionResult.InsufficientFunds;
        }
        public BagExpansionResult TryExpand(int expectedCapacity)
        {
            if (expanding) return BagExpansionResult.Busy;
            if (expectedCapacity != Save.BagCapacity) return BagExpansionResult.Stale;
            var state = GetState(); if (state != BagExpansionResult.Ready) return state;
            expanding = true; bool committed = false; var next = NextCapacity;
            try
            {
                var wallet = currencies.GetCurrency(Configuration.expansionPrice.CurrencyId, false);
                if (!wallet.TryWithdrawAndCommit(Configuration.expansionPrice.Amount, () => { Save.SetBagCapacity(next); committed = true; claimPending?.Invoke(); })) return BagExpansionResult.InsufficientFunds;
                return BagExpansionResult.Success;
            }
            finally
            {
                try { if (committed) { try { persistence.Save(false); } finally { Changed?.Invoke(); } } }
                finally { expanding = false; }
            }
        }
    }
}
