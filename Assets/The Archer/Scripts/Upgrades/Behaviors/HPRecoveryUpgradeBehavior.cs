namespace MrX.Upgrades
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
            if (StageController.Room.CurrentWave.EnemySpawns.Count > 0)
                StageController.Player.HealProportion(Level.Value * StageController.StageData.WaveRecoveryFactor / 100f);
        }

        public override void Clear()
        {
            StageController.Room.onWaveEnded -= OnWaveEnded;

            base.Clear();
        }
    }
}
