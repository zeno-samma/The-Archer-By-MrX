namespace OctoberStudio.Abilities
{
    public class BouncyShotAbilityBehavior : AbilityBehavior<BouncyShotAbilityData, BouncyShotAbilityLevel>
    {
        protected StatMultiplier arrowBounceDamageStatMultiplier = 1;
        protected StatAdder arrowBounceCountStatAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.ArrowBounceDamageStat.AddMultiplier(arrowBounceDamageStatMultiplier);
            StageController.Player.Stats.ArrowBounceCountStat.AddAdder(arrowBounceCountStatAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            arrowBounceCountStatAdder.Value = AbilityLevel.BouncesCount;
            arrowBounceDamageStatMultiplier.Value = AbilityLevel.EachBounceDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.ArrowBounceCountStat.RemoveAdder(arrowBounceCountStatAdder);
            StageController.Player.Stats.ArrowBounceDamageStat.RemoveMultiplier(arrowBounceDamageStatMultiplier);

            base.Clear();
        }
    }
}