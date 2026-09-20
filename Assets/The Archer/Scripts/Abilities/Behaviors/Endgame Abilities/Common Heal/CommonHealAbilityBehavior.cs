namespace OctoberStudio.Abilities
{
    public class CommonHealAbilityBehavior : AbilityBehavior<CommonHealAbilityData, CommonHealAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.HealProportion(AbilityLevel.HealingProportion);
        }

        protected virtual void LateUpdate()
        {
            transform.position = StageController.Player.Position;
        }
    }
}