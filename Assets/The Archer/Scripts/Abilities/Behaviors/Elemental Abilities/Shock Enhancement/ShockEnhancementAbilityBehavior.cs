namespace OctoberStudio.Abilities
{
    public class ShockEnhancementAbilityBehavior : AbilityBehavior<ShockEnhancementAbilityData, ShockEnhancementAbilityLevel>
    {
        protected StatMultiplier shockDamageMultiplier = 1;
        protected StatMultiplier arrowSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.ShockDamageMultiplierStat.AddMultiplier(shockDamageMultiplier);
            StageController.Player.Stats.ShockArrowSpeedMultiplierStat.AddMultiplier(arrowSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            shockDamageMultiplier.Value = AbilityLevel.ShockDamageMultiplier;
            arrowSpeedMultiplier.Value = AbilityLevel.ArrowSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.ShockDamageMultiplierStat.RemoveMultiplier(shockDamageMultiplier);
            StageController.Player.Stats.ShockArrowSpeedMultiplierStat.RemoveMultiplier(arrowSpeedMultiplier);

            base.Clear();
        }
    }
}