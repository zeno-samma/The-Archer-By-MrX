namespace OctoberStudio.Abilities
{
    public class RareCoinsAbilityBehavior : AbilityBehavior<RareCoinsAbilityData, RareCoinsAbilityLevel>
    {
        protected CurrencySave goldCurrency;

        public override void Init(AbilityData data, int levelId)
        {
            goldCurrency = GameController.CurrenciesManager.GetCurrency("gold", true);

            base.Init(data, levelId);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            if (goldCurrency != null)
            {
                goldCurrency.Deposit(AbilityLevel.AmountOfCoins);
            }
        }
    }
}