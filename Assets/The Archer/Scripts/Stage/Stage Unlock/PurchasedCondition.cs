using OctoberStudio.Currency;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    [UnlockCondition("Purchased")]
    public class PurchasedCondition : StageUnlockCondition
    {
        [SerializeField] protected CurrencyId currency = "gold";
        [SerializeField] protected int cost;

        public CurrencySave Currency => GameController.CurrenciesManager.GetCurrency(currency, false);
        public int Cost => cost;

        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            return false;
        }
    }
}