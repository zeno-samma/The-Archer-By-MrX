namespace OctoberStudio.Abilities
{
    public class IgniteEnhancementAbilityBehavior : AbilityBehavior<IgniteEnhancementAbilityData, IgniteEnhancementAbilityLevel>
    {
        protected StatMultiplier burnDamageMultiplier = 1;
        protected StatMultiplier arrowSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.BurnDamageMultiplierStat.AddMultiplier(burnDamageMultiplier);
            StageController.Player.Stats.BurnArrowSpeedMultiplierStat.AddMultiplier(arrowSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            burnDamageMultiplier.Value = AbilityLevel.IgniteDamageMultiplier;
            arrowSpeedMultiplier.Value = AbilityLevel.ArrowSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.BurnDamageMultiplierStat.RemoveMultiplier(burnDamageMultiplier);
            StageController.Player.Stats.BurnArrowSpeedMultiplierStat.RemoveMultiplier(arrowSpeedMultiplier);

            base.Clear();
        }
    }
}