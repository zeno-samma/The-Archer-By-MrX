namespace OctoberStudio.Abilities
{
    public class PoisonEnhancementAbilityBehavior : AbilityBehavior<PoisonEnhancementAbilityData, PoisonEnhancementAbilityLevel>
    {
        protected StatMultiplier poisonDamageMultiplier = 1;
        protected StatMultiplier arrowSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.PoisonDamageMultiplierStat.AddMultiplier(poisonDamageMultiplier);
            StageController.Player.Stats.PoisonArrowSpeedMultiplierStat.AddMultiplier(arrowSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            poisonDamageMultiplier.Value = AbilityLevel.PoisonDamageMultiplier;
            arrowSpeedMultiplier.Value = AbilityLevel.ArrowSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.PoisonDamageMultiplierStat.RemoveMultiplier(poisonDamageMultiplier);
            StageController.Player.Stats.PoisonArrowSpeedMultiplierStat.RemoveMultiplier(arrowSpeedMultiplier);

            base.Clear();
        }
    }
}