namespace OctoberStudio.Abilities
{
    public class SniperAbilityBehavior : AbilityBehavior<SniperAbilityData, SniperAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            StageController.Player.DistanceDamageMultiplierCurve = AbilityLevel.DistanceDamageMultiplierCurve;
        }

        public override void Clear()
        {
            StageController.Player.DistanceDamageMultiplierCurve = null;

            base.Clear();
        }
    }
}