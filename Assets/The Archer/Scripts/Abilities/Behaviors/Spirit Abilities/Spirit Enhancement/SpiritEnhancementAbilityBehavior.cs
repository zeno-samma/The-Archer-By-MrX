namespace OctoberStudio.Abilities
{
    public class SpiritEnhancementAbilityBehavior : AbilityBehavior<SpiritEnhancementAbilityData, SpiritEnhancementAbilityLevel>
    {
        protected StatMultiplier spiritDamageMultiplier;

        public override void Init(AbilityData data, int levelId)
        {
            spiritDamageMultiplier = 1;

            base.Init(data, levelId);

            StageController.Player.SpiritsManager.SpiritsDamageStat.AddMultiplier(spiritDamageMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            spiritDamageMultiplier.Value = AbilityLevel.SpiritsDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.SpiritsManager.SpiritsDamageStat.RemoveMultiplier(spiritDamageMultiplier);

            base.Clear();
        }
    }
}