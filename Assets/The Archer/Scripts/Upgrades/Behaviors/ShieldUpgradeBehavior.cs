namespace OctoberStudio.Upgrades
{
    public class ShieldUpgradeBehavior : UpgradeBehavior
    {
        protected StatMultiplier damageReductionMultiplier = 1;

        public override void Init(UpgradeData data)
        {
            base.Init(data);

            StageController.Player.Stats.DamageReduction.AddMultiplier(damageReductionMultiplier);
        }

        public override void SetUpgradeLevel(int level)
        {
            base.SetUpgradeLevel(level);

            damageReductionMultiplier.Value = 1 - (Level.Value / 100f);
        }

        public override void Clear()
        {
            StageController.Player.Stats.DamageReduction.AddMultiplier(damageReductionMultiplier);
            base.Clear();
        }
    }
}