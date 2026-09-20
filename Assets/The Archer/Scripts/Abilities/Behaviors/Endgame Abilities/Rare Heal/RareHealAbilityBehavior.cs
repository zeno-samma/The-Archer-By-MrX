namespace OctoberStudio.Abilities
{
    public class RareHealAbilityBehavior : AbilityBehavior<RareHealAbilityData, RareHealAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.HealProportion(AbilityLevel.HealingProportion);
        }
    }
}