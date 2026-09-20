namespace OctoberStudio.Abilities
{
    public class HealAbilityBehavior : AbilityBehavior<HealAbilityData, HealAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.HealProportion(AbilityLevel.HealingProportion);
        }
    }
}