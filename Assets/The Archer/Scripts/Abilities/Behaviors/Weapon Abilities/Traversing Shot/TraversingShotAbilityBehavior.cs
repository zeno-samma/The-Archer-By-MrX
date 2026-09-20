namespace OctoberStudio.Abilities
{
    public class TraversingShotAbilityBehavior : AbilityBehavior<TraversingShotAbilityData, TraversingShotAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.ProjectilesIgnoreObstacles = true;
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.ProjectilesIgnoreObstacles = false;
            base.Clear();
        }
    }
}