namespace OctoberStudio.Abilities
{
    public class FourLeafCloverAbilityBehavior : AbilityBehavior<FourLeafCloverAbilityData, FourLeafCloverAbilityLevel>
    {
        protected StatAdder dodgeChancePercent = 0;
        protected StatAdder chanceToIncreaceRarity = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.DodgeChanceStat.AddAdder(dodgeChancePercent);
            StageController.Player.Stats.ChanceToIncreaseRarity.AddAdder(chanceToIncreaceRarity);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            dodgeChancePercent.Value = AbilityLevel.DodgeChancePercent;
            chanceToIncreaceRarity.Value = AbilityLevel.ChanceToIncreaseRarityPercent;
        }

        public override void Clear()
        {
            StageController.Player.Stats.DodgeChanceStat.RemoveAdder(dodgeChancePercent);
            StageController.Player.Stats.ChanceToIncreaseRarity.RemoveAdder(chanceToIncreaceRarity);

            base.Clear();
        }
    }
}