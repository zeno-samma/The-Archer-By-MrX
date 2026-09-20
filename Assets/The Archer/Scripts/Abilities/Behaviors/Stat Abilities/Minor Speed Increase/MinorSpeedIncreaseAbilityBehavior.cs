namespace OctoberStudio.Abilities
{
    public class MinorSpeedIncreaseAbilityBehavior : AbilityBehavior<MinorSpeedIncreaseAbilityData, MinorSpeedIncreaseAbilityLevel>
    {
        protected StatMultiplier movementSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(movementSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            movementSpeedMultiplier.Value = AbilityLevel.MovementSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MovementSpeedStat.RemoveMultiplier(movementSpeedMultiplier);

            base.Clear();
        }
    }
}