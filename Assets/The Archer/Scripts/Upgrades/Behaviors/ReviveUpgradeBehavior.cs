namespace OctoberStudio.Upgrades
{
    public class ReviveUpgradeBehavior : UpgradeBehavior
    {
        protected StatAdder reviveStatAdder = 0;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.RevivesStat.AddAdder(reviveStatAdder);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            reviveStatAdder.Value = (int)Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.RevivesStat.RemoveAdder(reviveStatAdder);
            base.Clear();
        }
    }
}