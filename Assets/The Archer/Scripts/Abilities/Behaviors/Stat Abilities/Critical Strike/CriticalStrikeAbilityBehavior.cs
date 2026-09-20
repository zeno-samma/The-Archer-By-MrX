namespace OctoberStudio.Abilities
{
    public class CriticalStrikeAbilityBehavior : AbilityBehavior<CriticalStrikeAbilityData, CriticalStrikeAbilityLevel>
    {
        protected StatMultiplier critDamageMultiplier = 1;
        protected StatAdder critChanceIncreasePercent = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.CritDamageMultiplierStat.AddMultiplier(critDamageMultiplier);
            StageController.Player.Stats.CritChanceStat.AddAdder(critChanceIncreasePercent);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            critDamageMultiplier.Value = AbilityLevel.CritDamageMultiplier;
            critChanceIncreasePercent.Value = AbilityLevel.CritChanceIncreasePercent;
        }

        public override void Clear()
        {
            StageController.Player.Stats.CritDamageMultiplierStat.RemoveMultiplier(critDamageMultiplier);
            StageController.Player.Stats.CritChanceStat.RemoveAdder(critChanceIncreasePercent);
            base.Clear();
        }
    }
}