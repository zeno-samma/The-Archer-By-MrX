namespace OctoberStudio.Abilities
{
    public class MinorAttackSpeedBoostAbilityBehavior : AbilityBehavior<MinorAttackSpeedBoostAbilityData, MinorAttackSpeedBoostAbilityLevel>
    {
        protected StatMultiplier attackSpeedMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);

            base.Clear();
        }
    }
}