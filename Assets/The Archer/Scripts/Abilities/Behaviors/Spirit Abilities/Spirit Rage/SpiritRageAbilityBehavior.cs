namespace OctoberStudio.Abilities
{
    public class SpiritRageAbilityBehavior : AbilityBehavior<SpiritRageAbilityData, SpiritRageAbilityLevel>
    {
        protected StatMultiplier spiritDamageMultiplier = 1;
        protected StatMultiplier spiritAttackDelayMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.SpiritsManager.SpiritsDamageStat.AddMultiplier(spiritDamageMultiplier);
            StageController.Player.SpiritsManager.SpiritAttackDelayStat.AddMultiplier(spiritAttackDelayMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            spiritDamageMultiplier.Value = AbilityLevel.SpiritsDamageMultiplier;
            spiritAttackDelayMultiplier.Value = AbilityLevel.SpiritsAttackDelayMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.SpiritsManager.SpiritsDamageStat.RemoveMultiplier(spiritDamageMultiplier);
            StageController.Player.SpiritsManager.SpiritAttackDelayStat.RemoveMultiplier(spiritAttackDelayMultiplier);

            base.Clear();
        }
    }
}