namespace OctoberStudio.Abilities
{
    public class SplitShotAbilityBehavior : AbilityBehavior<SplitShotAbilityData, SplitShotAbilityLevel>
    {
        protected StatMultiplier splitArrowDamageMultiplier = 1;
        protected StatAdder splitArrowCountAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.SplitArrowCountStat.AddAdder(splitArrowCountAdder);
            StageController.Player.Stats.SpliArrowDamageMultiplierStat.AddMultiplier(splitArrowDamageMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            splitArrowDamageMultiplier.Value = AbilityLevel.SplitArrowDamageMultiplier;
            splitArrowCountAdder.Value = AbilityLevel.SplitArrowsCount;
        }

        public override void Clear()
        {
            StageController.Player.Stats.SplitArrowCountStat.RemoveAdder(splitArrowCountAdder);
            StageController.Player.Stats.SpliArrowDamageMultiplierStat.RemoveMultiplier(splitArrowDamageMultiplier);

            base.Clear();
        }
    }
}