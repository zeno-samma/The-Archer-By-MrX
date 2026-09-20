namespace OctoberStudio.Abilities
{
    public class HealEnhancementAbilityBehavior : AbilityBehavior<HealEnhancementAbilityData, HealEnhancementAbilityLevel>
    {
        protected StatMultiplier maxHPMultiplier = 1;
        protected StatMultiplier healingMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
            StageController.Player.Stats.HealingStat.AddMultiplier(healingMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            maxHPMultiplier.Value = AbilityLevel.MaxHPMultiplier;
            healingMultiplier.Value = AbilityLevel.HealingMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MaxHPStat.RemoveMultiplier(maxHPMultiplier);
            StageController.Player.Stats.HealingStat.RemoveMultiplier(healingMultiplier);

            base.Clear();
        }
    }
}
