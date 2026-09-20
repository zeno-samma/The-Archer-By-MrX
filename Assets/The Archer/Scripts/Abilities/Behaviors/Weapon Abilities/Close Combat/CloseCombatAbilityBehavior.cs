namespace OctoberStudio.Abilities
{
    public class CloseCombatAbilityBehavior : AbilityBehavior<CloseCombatAbilityData, CloseCombatAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;
        protected StatMultiplier attackSpeedMultiplier = 1;
        protected StatMultiplier arrowRangeMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.ArrowRangeStat.AddMultiplier(arrowRangeMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
            arrowRangeMultiplier.Value = AbilityLevel.ArrowRangeMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.ArrowRangeStat.RemoveMultiplier(arrowRangeMultiplier);

            base.Clear();
        }
    }
}