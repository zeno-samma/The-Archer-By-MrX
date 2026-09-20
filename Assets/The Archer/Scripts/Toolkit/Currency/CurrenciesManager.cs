using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [DefaultExecutionOrder(-5)]
    public class CurrenciesManager : MonoBehaviour, ICurrenciesManager
    {
        [SerializeField] protected CurrenciesDatabase database;

        protected Dictionary<string, CurrencySave> currencies;
        protected Dictionary<string, CurrencySave> gameplayCurrencies;
        protected Dictionary<string, CurrencyData> datas;

        protected virtual void Awake()
        {
            if (!GameController.RegisterCurrenciesManager(this))
            {
                Destroy(gameObject);
            }

            currencies = new Dictionary<string, CurrencySave>();
            gameplayCurrencies = new Dictionary<string, CurrencySave>();
        }

        protected virtual void Start()
        {
            for(int i = 0; i < database.Count; i++)
            {
                var data = database.GetCurrency(i);

                var save = GameController.SaveManager.GetSave<CurrencySave>(data.ID);
                save.Init(data);

                currencies.Add(data.ID, save);

                var gameplaySave = GameController.SaveManager.GetSave<CurrencySave>(data.ID + ":___GAMEPLAY___");
                gameplaySave.Init(data);

                gameplayCurrencies.Add(data.ID, gameplaySave);
            }
        }

        public virtual CurrencySave GetCurrency(string id, bool gameplay)
        {
            CurrencySave currency = null;
            if (gameplay)
            {
                gameplayCurrencies.TryGetValue(id, out currency);
            } else
            {
                currencies.TryGetValue(id, out currency);
            }

            return currency;
        }

        public virtual CurrencySave GetDefaultCurrency(bool gameplay)
        {
            CurrencySave currency = null;
            var id = database.GetCurrency(0).ID;

            if (gameplay)
            {
                gameplayCurrencies.TryGetValue(id, out currency);
            }
            else
            {
                currencies.TryGetValue(id, out currency);
            }

            return currency;
        }

        public virtual void ApplyGameplayAmounts()
        {
            foreach(var id in currencies.Keys)
            {
                var currency = currencies[id];
                var gameplayCurrency = gameplayCurrencies[id];

                currency.Deposit(gameplayCurrency.Amount);
                gameplayCurrency.Clear();
            }
        }

        public virtual void EraseGameplayAmounts()
        {
            foreach (var id in currencies.Keys)
            {
                var gameplayCurrency = gameplayCurrencies[id];
                gameplayCurrency.Clear();
            }
        }

        public virtual List<CurrencySave> GetAllCurrencies(bool gameplay)
        {
            return new List<CurrencySave>(gameplay ? gameplayCurrencies.Values.ToArray() : currencies.Values.ToArray());
        }
    }
}