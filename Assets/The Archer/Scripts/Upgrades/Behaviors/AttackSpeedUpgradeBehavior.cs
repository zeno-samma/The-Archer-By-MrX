namespace OctoberStudio.Upgrades
{
    public class AttackSpeedUpgradeBehavior : UpgradeBehavior
    {
        protected StatMultiplier attackSpeedMultiplier = 1;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            attackSpeedMultiplier.Value = Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            base.Clear();
        }
    }
}