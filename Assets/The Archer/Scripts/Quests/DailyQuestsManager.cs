using System;
using MrX.Currency;
using MrX.Save;
using UnityEngine;

namespace MrX.Quests
{
    public class DailyQuestsManager : MonoBehaviour
    {
        public DailyQuestsDatabase Database { get; private set; }
        public DailyQuestsSave Save { get; private set; }
        public event Action Changed;
        private float nextCheck;
        private bool claiming;
        private ISaveManager saveManager;
        private ICurrenciesManager currenciesManager;
        private bool pendingSave;

        public void Initialize(DailyQuestsDatabase database, ISaveManager saves = null, ICurrenciesManager currencies = null)
        {
            if (database == null || !database.IsValid(out var error))
            { Debug.LogError("Daily quests: invalid or missing database."); enabled = false; return; }
            Database = database;
            saveManager = saves ?? GameController.SaveManager;
            currenciesManager = currencies ?? GameController.CurrenciesManager;
            Save = saveManager.GetSave<DailyQuestsSave>("Daily Quests");
            RefreshDay();
        }

        private void Update()
        {
            if (Save == null || Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 1;
            RefreshDay();
        }

        private void LateUpdate()
        {
            FlushPending();
        }
        public void FlushPending()
        {
            if (!pendingSave) return;
            pendingSave = false;
            saveManager.Save(false);
        }

        public void RefreshDay()
        {
            if (Save == null) return;
            var changed = Save.EnsureDay(DateTime.UtcNow, Database.resetUtcOffsetHours);
            foreach (var quest in Database.quests)
                if (quest.type == DailyQuestType.Login && Save.GetProgress(quest) == 0) changed |= Save.AddProgress(quest, 1);
            if (changed) PersistAndNotify();
        }

        public void Record(DailyQuestType type, int amount = 1)
        {
            if (Save == null || amount <= 0) return;
            RefreshDay();
            var changed = false;
            foreach (var quest in Database.quests)
                if (quest.type == type) changed |= Save.AddProgress(quest, amount);
            if (changed) PersistAndNotify();
        }

        public TimeSpan TimeUntilReset => Save.TimeUntilReset(DateTime.UtcNow, Database.resetUtcOffsetHours);
        public int CompletedCount => Save.CompletedCount(Database.quests);

        public bool TryClaimQuest(string id)
        {
            if (Save == null || claiming) return false;
            RefreshDay();
            var quest = Database.quests.Find(q => q.id == id);
            if (quest == null) return false;
            return Grant(quest.reward, () => Save.TryMarkQuestClaimed(quest));
        }

        public bool TryClaimMilestone(string id)
        {
            if (Save == null || claiming) return false;
            RefreshDay();
            var milestone = Database.milestones.Find(m => m.id == id);
            if (milestone == null) return false;
            return Grant(milestone.reward, () => Save.TryMarkMilestoneClaimed(milestone, CompletedCount));
        }

        private bool Grant(Price reward, Func<bool> markClaimed)
        {
            // Resolve the real permanent wallet before consuming the claim flag.
            var currency = currenciesManager?.GetCurrency(reward.CurrencyId, false);
            if (currency == null) return false;
            claiming = true;
            try
            {
                if (!markClaimed()) return false;
                currency.Deposit(reward.Amount);
                PersistAndNotify(true);
                return true;
            }
            finally { claiming = false; }
        }

        private void PersistAndNotify(bool immediate = false)
        {
            // Gameplay operations such as a hero purchase update several saves in
            // one frame. Persist counters after those operations finish; claims
            // persist wallet and claim flags immediately in the same save snapshot.
            if (immediate) { pendingSave = false; saveManager.Save(false); }
            else pendingSave = true;
            Changed?.Invoke();
        }
    }
}
