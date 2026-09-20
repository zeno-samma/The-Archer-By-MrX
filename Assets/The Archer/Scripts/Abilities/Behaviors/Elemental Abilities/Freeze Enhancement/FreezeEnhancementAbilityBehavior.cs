namespace OctoberStudio.Abilities
{
    public class FreezeEnhancementAbilityBehavior : AbilityBehavior<FreezeEnhancementAbilityData, FreezeEnhancementAbilityLevel>
    {
        protected StatMultiplier freezeDamageMultiplier = 1;
        protected StatMultiplier arrowSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.FreezeDamageMultiplierStat.AddMultiplier(freezeDamageMultiplier);
            StageController.Player.Stats.FreezeArrowSpeedMultiplierStat.AddMultiplier(arrowSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            freezeDamageMultiplier.Value = AbilityLevel.FreezeDamageMultiplier;
            arrowSpeedMultiplier.Value = AbilityLevel.ArrowSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.FreezeDamageMultiplierStat.RemoveMultiplier(freezeDamageMultiplier);
            StageController.Player.Stats.FreezeArrowSpeedMultiplierStat.RemoveMultiplier(arrowSpeedMultiplier);

            base.Clear();
        }
    }
}