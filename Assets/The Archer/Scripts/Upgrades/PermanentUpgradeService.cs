using System;
using MrX.Currency;
using MrX.Save;

namespace MrX.Upgrades
{
    public enum UpgradePurchaseResult { Ready, Success, InsufficientFunds, Maximum, Invalid, Stale, Busy }
    public sealed class PermanentUpgradeService
    {
        private readonly IUpgradesManager upgrades;
        private readonly ICurrenciesManager wallets;
        private readonly ISaveManager saves;
        private bool buying;
        public PermanentUpgradeService(IUpgradesManager upgrades, ICurrenciesManager wallets, ISaveManager saves)
        { this.upgrades = upgrades; this.wallets = wallets; this.saves = saves; }
        public UpgradePurchaseResult Quote(UpgradeType type, out int level, out Price price)
        {
            level = upgrades.GetUpgradeLevel(type); price = null;
            var data = upgrades.GetUpgradeData(type);
            if (data == null || data.LevelsCount < 1 || level < -1 || level >= data.LevelsCount) return UpgradePurchaseResult.Invalid;
            if (level == data.LevelsCount - 1) return UpgradePurchaseResult.Maximum;
            price = data.GetLevel(level + 1).Price;
            if (price == null || price.Amount < 0 || string.IsNullOrEmpty(price.CurrencyId)) return UpgradePurchaseResult.Invalid;
            var wallet = wallets.GetCurrency(price.CurrencyId, false);
            if (wallet == null) return UpgradePurchaseResult.Invalid;
            return wallet.CanAfford(price.Amount) ? UpgradePurchaseResult.Ready : UpgradePurchaseResult.InsufficientFunds;
        }
        public UpgradePurchaseResult TryBuy(UpgradeType type, int expectedLevel)
        {
            if (buying) return UpgradePurchaseResult.Busy;
            var state = Quote(type, out var level, out var price);
            if (level != expectedLevel) return UpgradePurchaseResult.Stale;
            if (state != UpgradePurchaseResult.Ready) return state;
            bool committed = false; buying = true;
            try
            {
                return wallets.GetCurrency(price.CurrencyId, false).TryWithdrawAndCommit(price.Amount, () =>
                { committed = true; upgrades.IncrementUpgradeLevel(type); }) ? UpgradePurchaseResult.Success : UpgradePurchaseResult.InsufficientFunds;
            }
            finally { try { if (committed) saves.Save(false); } finally { buying = false; } }
        }
    }
}
