namespace OctoberStudio.Abilities
{
    public class PiercingStrikeAbilityBehavior : AbilityBehavior<PiercingStrikeAbilityData, PiercingStrikeAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.IsPiercingArrowEnabled = true;
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.ArrowDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.IsPiercingArrowEnabled = false;

            base.Clear();
        }
    }
}