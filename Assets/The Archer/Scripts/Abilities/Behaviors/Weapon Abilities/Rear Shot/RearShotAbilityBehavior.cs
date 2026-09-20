namespace OctoberStudio.Abilities
{
    public class RearShotAbilityBehavior : AbilityBehavior<RearShotAbilityData, RearShotAbilityLevel>
    {
        protected StatMultiplier rearArrowDamageStatMultiplier = 1;
        protected StatAdder rearArrowCountStatAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.RearArrowDamageStat.AddMultiplier(rearArrowDamageStatMultiplier);
            StageController.Player.Stats.RearArrowsCountStat.AddAdder(rearArrowCountStatAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            rearArrowCountStatAdder.Value = AbilityLevel.BackArrowsCount;
            rearArrowDamageStatMultiplier.Value = AbilityLevel.BackArrowsDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.RearArrowsCountStat.RemoveAdder(rearArrowCountStatAdder);
            StageController.Player.Stats.RearArrowDamageStat.RemoveMultiplier(rearArrowDamageStatMultiplier);

            base.Clear();
        }
    }
}