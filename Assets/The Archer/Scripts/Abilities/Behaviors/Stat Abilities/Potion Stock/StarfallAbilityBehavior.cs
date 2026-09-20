namespace OctoberStudio.Abilities
{
    public class StarfallAbilityBehavior : AbilityBehavior<StarfallAbilityData, StarfallAbilityLevel>
    {
        protected StatMultiplier starsSpawnIntervalMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.StarSpawnIntervalMultiplierStat.AddMultiplier(starsSpawnIntervalMultiplier);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            starsSpawnIntervalMultiplier.Value = AbilityLevel.StarsSpawnIntervalMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.StarSpawnIntervalMultiplierStat.RemoveMultiplier(starsSpawnIntervalMultiplier);

            base.Clear();
        }
    }
}