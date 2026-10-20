using System;
using MrX.Armory;
using MrX.Rewards;
using MrX.Save;

namespace MrX.Development
{
    // Uses the existing reward delivery path: wallets, bag capacity, duplicates and pending equipment.
    public sealed class DeveloperGrantService
    {
        private readonly DeveloperToolsSettings settings;
        private readonly ILuckyRewardDelivery delivery;
        private readonly ISaveManager saves;
        private readonly bool editor, developmentBuild;
        private bool busy;
        public DeveloperGrantService(DeveloperToolsSettings settings, ILuckyRewardDelivery delivery,
            ISaveManager saves, bool editor, bool developmentBuild)
        { this.settings = settings; this.delivery = delivery; this.saves = saves; this.editor = editor; this.developmentBuild = developmentBuild; }
        public bool GrantCurrency(string id, int amount, out string message)
        {
            if (id != "gold" && id != "gem") { message = "Unsupported currency."; return false; }
            return Grant(new LuckyPrize { kind = LuckyRewardKind.Currency, currencyId = id }, amount, out message);
        }
        public bool GrantItem(ItemData item, out string message)
        {
            if (!settings || !settings.armory || !item || settings.armory.GetItem(item.Id) != item)
            { message = "Select registered equipment."; return false; }
            return Grant(new LuckyPrize { kind = LuckyRewardKind.Equipment, item = item }, 1, out message);
        }
        private bool Grant(LuckyPrize prize, int amount, out string message)
        {
            message = "Developer tools disabled.";
            if (!settings || !settings.Allows(editor, developmentBuild) || busy) return false;
            if (amount <= 0 || !delivery.Resolve(prize, amount, 0, out var grant))
            { message = "Cannot grant: invalid amount, full balance or unavailable reward."; return false; }
            busy = true;
            try
            {
                try { delivery.Grant(grant); }
                finally { saves.Save(); }
                message = "+" + grant.amount.ToString("N0") + " " + grant.title +
                    (grant.queuedEquipment ? " (queued: bag full)" : " (saved)");
                return true;
            }
            finally { busy = false; }
        }
    }
}
