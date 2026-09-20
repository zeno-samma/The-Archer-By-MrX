namespace OctoberStudio.Upgrades
{
    public class HPRecoveryUpgradeBehavior : UpgradeBehavior
    {
        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Room.onWaveEnded += OnWaveEnded;
        }

        protected virtual void OnWaveEnded()
        {
            StageController.Player.HealProportion(Level.Value / 100f);
        }

        public override void Clear()
        {
            StageController.Room.onWaveEnded -= OnWaveEnded;

            base.Clear();
        }
    }
}