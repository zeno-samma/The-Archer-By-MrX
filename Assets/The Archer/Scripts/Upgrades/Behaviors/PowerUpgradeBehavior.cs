namespace OctoberStudio.Upgrades
{
    public class PowerUpgradeBehavior : UpgradeBehavior
    {
        protected StatMultiplier attackDamagedMultiplier = 1;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamagedMultiplier);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            attackDamagedMultiplier.Value = Level.Value;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamagedMultiplier);
            base.Clear();
        }
    }
}