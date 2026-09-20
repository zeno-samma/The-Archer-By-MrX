namespace OctoberStudio.Abilities
{
    public class FrontProjectileAbilityBehavior : AbilityBehavior<FrontProjectileAbilityData, FrontProjectileAbilityLevel>
    {
        protected StatMultiplier frontArrowDamageStatMultiplier = 1;
        protected StatAdder frontArrowCountStatAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.FrontArrowDamageStat.AddMultiplier(frontArrowDamageStatMultiplier);
            StageController.Player.Stats.FrontArrowsCountStat.AddAdder(frontArrowCountStatAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            frontArrowCountStatAdder.Value = AbilityLevel.AdditionalFrontArrowsCount;
            frontArrowDamageStatMultiplier.Value = AbilityLevel.FrontArrowsDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.FrontArrowsCountStat.RemoveAdder(frontArrowCountStatAdder);
            StageController.Player.Stats.FrontArrowDamageStat.RemoveMultiplier(frontArrowDamageStatMultiplier);

            base.Clear();
        }
    }
}