namespace OctoberStudio.Abilities
{
    public class DiagonalShotAbilityBehavior : AbilityBehavior<DiagonalShotAbilityData, DiagonalShotAbilityLevel>
    {
        protected StatMultiplier diagonalArrowDamageStatMultiplier = 1;
        protected StatAdder diagonalArrowCountStatAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.DiagonalArrowDamageStat.AddMultiplier(diagonalArrowDamageStatMultiplier);
            StageController.Player.Stats.DiagonalArrowsCountStat.AddAdder(diagonalArrowCountStatAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            diagonalArrowCountStatAdder.Value = AbilityLevel.DiagonalArrowsCount;
            diagonalArrowDamageStatMultiplier.Value = AbilityLevel.DiagonalArrowsDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.DiagonalArrowsCountStat.RemoveAdder(diagonalArrowCountStatAdder);
            StageController.Player.Stats.DiagonalArrowDamageStat.RemoveMultiplier(diagonalArrowDamageStatMultiplier);

            base.Clear();
        }
    }
}