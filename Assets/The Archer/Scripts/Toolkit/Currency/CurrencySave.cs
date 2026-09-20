using OctoberStudio.Currency;
using OctoberStudio.Save;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class CurrencySave: ISave
    {
        [SerializeField] protected int amount;
        public int Amount => amount;

        public event UnityAction<CurrencySave> OnAmountChanged;

        public CurrencyData Data { get; protected set; }
        public string Id => Data.ID;

        public virtual void Init(CurrencyData data)
        {
            Data = data;
        }

        public virtual void Deposit(int depositedAmount)
        {
            amount += depositedAmount;

            OnAmountChanged?.Invoke(this);
        }

        public virtual void Withdraw(int withdrawnAmount)
        {
            amount -= withdrawnAmount;
            if (amount < 0) amount = 0;

            OnAmountChanged?.Invoke(this);
        }

        public virtual bool TryWithdraw(int withdrawnAmount)
        {
            var canAfford = CanAfford(withdrawnAmount);

            if(canAfford) 
            {
                amount -= withdrawnAmount;

                OnAmountChanged?.Invoke(this);
            }

            return canAfford;
        }

        public virtual bool CanAfford(int requiredAmount)
        {
            return amount >= requiredAmount;
        }

        public virtual void Clear()
        {
            amount = 0;
        }

        public virtual void Flush()
        {

        }
    }
}