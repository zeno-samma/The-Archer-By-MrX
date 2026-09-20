namespace OctoberStudio.Abilities
{
    public class GnomeFormAbilityBehavior : AbilityBehavior<GnomeFormAbilityData, GnomeFormAbilityLevel>
    {
        protected StatMultiplier attackSpeedMultiplier = 1;
        protected StatMultiplier movementSpeedMultiplier = 1;
        protected StatMultiplier playerSizeMultiplier = 1;
        protected StatAdder dodgeChancePercent = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.SizeStat.AddMultiplier(playerSizeMultiplier);
            StageController.Player.Stats.DodgeChanceStat.AddAdder(dodgeChancePercent);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
            movementSpeedMultiplier.Value = AbilityLevel.MovementSpeedMultiplier;
            playerSizeMultiplier.Value = AbilityLevel.PlayerSizeMultiplier;
            dodgeChancePercent.Value = AbilityLevel.DodgeChancePercent;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.MovementSpeedStat.RemoveMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.SizeStat.RemoveMultiplier(playerSizeMultiplier);
            StageController.Player.Stats.DodgeChanceStat.RemoveAdder(dodgeChancePercent);

            base.Clear();
        }
    }
}