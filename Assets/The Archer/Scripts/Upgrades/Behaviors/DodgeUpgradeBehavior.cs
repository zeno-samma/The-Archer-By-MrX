namespace OctoberStudio.Upgrades
{
    public class DodgeUpgradeBehavior : UpgradeBehavior
    {
        protected StatAdder dodgeChanceStatAdder = 0;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.DodgeChanceStat.AddAdder(dodgeChanceStatAdder);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            dodgeChanceStatAdder.Value = (int)Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.DodgeChanceStat.RemoveAdder(dodgeChanceStatAdder);
            base.Clear();
        }
    }
}