namespace OctoberStudio.Upgrades
{
    public class CritChanceUpgradeBehavior : UpgradeBehavior
    {
        protected StatAdder critChanceStatAdder = 0;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.CritChanceStat.AddAdder(critChanceStatAdder);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            critChanceStatAdder.Value = (int)Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.CritChanceStat.RemoveAdder(critChanceStatAdder);
            base.Clear();
        }
    }
}