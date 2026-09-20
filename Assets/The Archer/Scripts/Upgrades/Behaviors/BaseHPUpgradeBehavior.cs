namespace OctoberStudio.Upgrades
{
    public class BaseHPUpgradeBehavior : UpgradeBehavior
    {
        protected StatMultiplier maxHPMultiplier = 1;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            maxHPMultiplier.Value = Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
            base.Clear();
        }
    }
}