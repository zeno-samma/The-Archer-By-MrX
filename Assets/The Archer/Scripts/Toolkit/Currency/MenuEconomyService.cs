using System;
using MrX.Save;

namespace MrX.Currency
{
    [Serializable]
    public sealed class EnergyRecoverySave : ISave
    {
        public bool initialized;
        public long lastRecoveryUtc;
        public long purchaseDay = -1;
        public int energyPurchases;
        public void Flush() { }
    }

    public enum ExchangeResult { Success, NotEnoughGems, EnergyFull, InvalidConfiguration, Busy, DailyLimit }

    // Uses the existing currency wallet. Only the recovery clock has its own save cell.
    public sealed class MenuEconomyService
    {
        private readonly ISaveManager saves;
        private readonly Func<long> utc;
        private readonly EnergyRecoverySave recovery;
        private bool busy;
        public MenuEconomySettings Settings { get; private set; }
        public CurrencySave Energy { get; private set; }
        public CurrencySave Gold { get; private set; }
        public CurrencySave Gems { get; private set; }
        public event Action Changed;
        public bool Ready => Settings != null && Settings.maximumEnergy > 0 && Settings.energyPerHour > 0 && Energy != null && Gold != null && Gems != null;
        public int RemainingEnergyPurchases
        {
            get { EnsurePurchaseDay(); return Math.Max(0, Settings.dailyEnergyPurchaseLimit - recovery.energyPurchases); }
        }
        private void EnsurePurchaseDay()
        {
            if (Settings == null) return;
            long day = (long)Math.Floor((utc() + Settings.purchaseResetUtcOffsetHours * 3600d) / 86400d);
            if (day <= recovery.purchaseDay) return;
            recovery.purchaseDay = day; recovery.energyPurchases = 0;
        }

        public MenuEconomyService(MenuEconomySettings settings, ICurrenciesManager wallets, ISaveManager persistence, Func<long> clock = null)
        {
            Settings = settings; saves = persistence;
            utc = clock ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Energy = wallets.GetCurrency("energy", false); Gold = wallets.GetCurrency("gold", false); Gems = wallets.GetCurrency("gem", false);
            recovery = saves.GetSave<EnergyRecoverySave>("Energy Recovery");
            if (!Ready) return;
            if (!recovery.initialized)
            {
                recovery.initialized = true; recovery.lastRecoveryUtc = utc();
                Energy.Deposit(Math.Max(0, settings.maximumEnergy - Energy.Amount));
                saves.Save(false);
            }
            Refresh();
        }

        public int SecondsUntilRecovery
        {
            get
            {
                if (!Ready || Energy.Amount >= Settings.maximumEnergy) return 0;
                long elapsed = Math.Max(0, utc() - recovery.lastRecoveryUtc);
                return (int)Math.Max(0, 3600 - Math.Min(elapsed, 3600));
            }
        }

        public bool Refresh()
        {
            if (!Development.GameFeatureSettings.IsEnabled(Development.GameFeature.Energy))
            { recovery.lastRecoveryUtc = Math.Max(recovery.lastRecoveryUtc, utc()); return false; }
            if (!Ready || busy) return false;
            long now = utc();
            // No banked recovery while full. A clock rollback never grants energy.
            if (Energy.Amount >= Settings.maximumEnergy)
            { recovery.lastRecoveryUtc = Math.Max(recovery.lastRecoveryUtc, now); return false; }
            long hours = Math.Max(0, now - recovery.lastRecoveryUtc) / 3600;
            if (hours == 0) return false;
            int missing = Settings.maximumEnergy - Energy.Amount;
            int grant = (int)Math.Min(missing, Math.Min(hours, missing) * (long)Settings.energyPerHour);
            busy = true;
            try
            {
                recovery.lastRecoveryUtc = grant >= missing ? now : recovery.lastRecoveryUtc + hours * 3600;
                Energy.Deposit(grant); saves.Save(false); Changed?.Invoke();
                return true;
            }
            finally { busy = false; }
        }

        // Caller commits attempts/run selection and then saves before scene transition.
        public bool TrySpendForNewRun(int amount)
        {
            if (!Development.GameFeatureSettings.IsEnabled(Development.GameFeature.Energy)) return amount >= 0;
            if (!Ready || busy || amount < 0) return false;
            Refresh();
            if (!Energy.CanAfford(amount)) return false;
            busy = true;
            try
            {
                bool wasFull = Energy.Amount >= Settings.maximumEnergy;
                return Energy.TryWithdrawAndCommit(amount, () =>
                {
                    if (wasFull && amount > 0) recovery.lastRecoveryUtc = Math.Max(recovery.lastRecoveryUtc, utc());
                    Changed?.Invoke();
                });
            }
            finally { busy = false; }
        }

        public ExchangeResult Buy(string currencyId)
        {
            if (currencyId == "energy" && !Development.GameFeatureSettings.IsEnabled(Development.GameFeature.Energy)) return ExchangeResult.InvalidConfiguration;
            if (busy) return ExchangeResult.Busy;
            if (!Ready || Gems == null || Gold == null || (currencyId != "gold" && currencyId != "energy")) return ExchangeResult.InvalidConfiguration;
            Refresh();
            int amount = currencyId == "energy" ? Settings.energyPurchaseAmount : Settings.goldPurchaseAmount;
            int price = currencyId == "energy" ? Settings.energyGemPrice : Settings.goldGemPrice;
            var target = currencyId == "energy" ? Energy : Gold;
            if (amount <= 0 || price <= 0 || target.Amount < 0 || target.Amount > int.MaxValue - amount) return ExchangeResult.InvalidConfiguration;
            if (currencyId == "energy")
            {
                if (RemainingEnergyPurchases == 0) return ExchangeResult.DailyLimit;
                // A purchase always gives the displayed quantity. No partial, full-price purchase.
                if ((long)target.Amount + amount > Settings.maximumEnergy) return ExchangeResult.EnergyFull;
            }
            if (!Gems.CanAfford(price)) return ExchangeResult.NotEnoughGems;
            busy = true;
            try
            {
                bool paid = Gems.TryWithdrawAndCommit(price, () =>
                {
                    if (currencyId == "energy" && target.Amount + amount >= Settings.maximumEnergy) recovery.lastRecoveryUtc = Math.Max(recovery.lastRecoveryUtc, utc());
                    if (currencyId == "energy") recovery.energyPurchases++;
                    target.Deposit(amount);
                });
                if (!paid) return ExchangeResult.NotEnoughGems;
                saves.Save(false); Changed?.Invoke(); return ExchangeResult.Success;
            }
            finally { busy = false; }
        }
    }
}
