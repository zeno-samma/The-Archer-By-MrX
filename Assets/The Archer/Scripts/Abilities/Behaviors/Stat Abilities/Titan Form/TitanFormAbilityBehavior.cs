namespace OctoberStudio.Abilities
{
    public class TitanFormAbilityBehavior : AbilityBehavior<TitanFormAbilityData, TitanFormAbilityLevel>
    {
        protected StatMultiplier playerSizeMultiplier = 1;
        protected StatMultiplier attackDamageMultiplier = 1;
        protected StatMultiplier movementSpeedMultiplier = 1;
        protected StatMultiplier maxHPMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.SizeStat.AddMultiplier(playerSizeMultiplier);
            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            playerSizeMultiplier.Value = AbilityLevel.PlayerSizeMultiplier;
            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
            movementSpeedMultiplier.Value = AbilityLevel.MovementSpeedMultiplier;
            maxHPMultiplier.Value = AbilityLevel.MaxHPMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.SizeStat.RemoveMultiplier(playerSizeMultiplier);
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.MovementSpeedStat.RemoveMultiplier(movementSpeedMultiplier);
            StageController.Player.Stats.MaxHPStat.RemoveMultiplier(maxHPMultiplier);

            base.Clear();
        }
    }
}