namespace OctoberStudio.Abilities
{
    public class MysticHealAbilityBehavior : AbilityBehavior<MysticHealAbilityData, MysticHealAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.HealProportion(AbilityLevel.HealingProportion);
        }
    }
}