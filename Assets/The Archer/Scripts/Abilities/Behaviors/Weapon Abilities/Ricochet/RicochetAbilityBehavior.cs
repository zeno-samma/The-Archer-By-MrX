namespace OctoberStudio.Abilities
{
    public class RicochetAbilityBehavior : AbilityBehavior<RicochetAbilityData, RicochetAbilityLevel>
    {
        protected StatMultiplier arrowRicochetDamageStatMultiplier = 1;
        protected StatAdder arrowRicochetCountStatAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.ArrowRicochetDamageStat.AddMultiplier(arrowRicochetDamageStatMultiplier);
            StageController.Player.Stats.ArrowRicochetCountStat.AddAdder(arrowRicochetCountStatAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            arrowRicochetCountStatAdder.Value = AbilityLevel.ArrowRicochetCount;
            arrowRicochetDamageStatMultiplier.Value = AbilityLevel.EachRicochetDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.ArrowRicochetCountStat.RemoveAdder(arrowRicochetCountStatAdder);
            StageController.Player.Stats.ArrowRicochetDamageStat.RemoveMultiplier(arrowRicochetDamageStatMultiplier);

            base.Clear();
        }
    }
}