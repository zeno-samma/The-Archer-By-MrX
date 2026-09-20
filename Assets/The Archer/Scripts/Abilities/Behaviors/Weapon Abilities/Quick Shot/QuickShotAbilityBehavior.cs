namespace OctoberStudio.Abilities
{
    public class QuickShotAbilityBehavior : AbilityBehavior<QuickShotAbilityData, QuickShotAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;
        protected StatMultiplier attackSpeedMultiplier = 1;
        protected StatMultiplier arrowSizeMultiplier = 1;
        protected StatAdder arrowSpreadAdder = 0;
        
        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.ArrowSizeStat.AddMultiplier(arrowSizeMultiplier);
            StageController.Player.Stats.ArrowSpreadStat.AddAdder(arrowSpreadAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
            arrowSizeMultiplier.Value = AbilityLevel.ArrowSizeMultiplier;
            arrowSpreadAdder.Value = AbilityLevel.ArrowSpreadAngle;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.ArrowSizeStat.RemoveMultiplier(arrowSizeMultiplier);
            StageController.Player.Stats.ArrowSpreadStat.RemoveAdder(arrowSpreadAdder);

            base.Clear();
        }
    }
}