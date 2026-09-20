namespace OctoberStudio.Abilities
{
    public class CureAbilityBehavior : AbilityBehavior<CureAbilityData, CureAbilityLevel>
    {
        protected StatMultiplier maxHPMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            maxHPMultiplier.Value = AbilityLevel.MaxHPMultiplier;

            StageController.Player.HealProportion(AbilityLevel.MaxHealthProportionHeal);
        }

        public override void Clear()
        {
            StageController.Player.Stats.MaxHPStat.RemoveMultiplier(maxHPMultiplier);

            base.Clear();
        }
    }
}