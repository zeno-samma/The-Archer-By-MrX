namespace OctoberStudio.Abilities
{
    public class LegendaryHealAbilityBehavior : AbilityBehavior<LegendaryHealAbilityData, LegendaryHealAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.HealProportion(AbilityLevel.HealingProportion);
        }
    }
}