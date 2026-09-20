namespace OctoberStudio.Abilities
{
    public class MeteorRainAbilityBehavior : AbilityBehavior<MeteorRainAbilityData, MeteorRainAbilityLevel>
    {
        protected StatMultiplier meteorSpawnDelayStatMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MeteorSpawnDelayStat.AddMultiplier(meteorSpawnDelayStatMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            meteorSpawnDelayStatMultiplier.Value = AbilityLevel.MeteorSpawnDelayMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MeteorSpawnDelayStat.RemoveMultiplier(meteorSpawnDelayStatMultiplier);

            base.Clear();
        }
    }
}