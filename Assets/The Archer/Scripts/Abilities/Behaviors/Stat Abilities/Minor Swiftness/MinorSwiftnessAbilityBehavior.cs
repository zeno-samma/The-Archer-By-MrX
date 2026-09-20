namespace OctoberStudio.Abilities
{
    public class MinorSwiftnessAbilityBehavior : AbilityBehavior<MinorSwiftnessAbilityData, MinorSwiftnessAbilityLevel>
    {
        protected StatMultiplier movementSpeedMultiplier = 1;
        protected StatMultiplier attackSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            movementSpeedMultiplier.Value = AbilityLevel.MovementSpeedMultiplier;
            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MovementSpeedStat.RemoveMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);

            base.Clear();
        }
    }
}