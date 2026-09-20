namespace OctoberStudio.Abilities
{
    public class OrbEnhancementAbilityBehavior : AbilityBehavior<OrbEnhancementAbilityData, OrbEnhancementAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.OrbsManager.SetOrbsDamageMultiplier(AbilityLevel.OrbsDamageMultiplier);
        }
    }
}