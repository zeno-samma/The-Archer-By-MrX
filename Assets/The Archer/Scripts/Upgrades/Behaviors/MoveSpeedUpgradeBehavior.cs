namespace OctoberStudio.Upgrades
{
    public class MoveSpeedUpgradeBehavior : UpgradeBehavior
    {
        protected StatMultiplier moveSpeedMultiplier = 1;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(moveSpeedMultiplier);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            moveSpeedMultiplier.Value = Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.MovementSpeedStat.AddMultiplier(moveSpeedMultiplier);
            base.Clear();
        }
    }

}
