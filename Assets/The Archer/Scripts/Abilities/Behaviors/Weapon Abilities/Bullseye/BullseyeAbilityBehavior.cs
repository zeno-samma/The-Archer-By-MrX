namespace OctoberStudio.Abilities
{
    public class BullseyeAbilityBehavior : AbilityBehavior<BullseyeAbilityData, BullseyeAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.MagnetismStat.ChangeInitialValue(AbilityLevel.MagnetismMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
            StageController.Player.Stats.MagnetismStat.ChangeInitialValue(AbilityLevel.MagnetismMultiplier);
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.MagnetismStat.ChangeInitialValue(0);

            base.Clear();
        }
    }
}