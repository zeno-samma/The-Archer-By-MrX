namespace OctoberStudio.Abilities
{
    public class MediumPowerAbilityBehavior : AbilityBehavior<MediumPowerAbilityData, MediumPowerAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);

            base.Clear();
        }
    }
}