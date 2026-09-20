namespace OctoberStudio.Abilities
{
    public class SecondChanceAbilityBehavior : AbilityBehavior<SecondChanceAbilityData, SecondChanceAbilityLevel>
    {
        protected StatAdder revivesCountAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.RevivesStat.AddAdder(revivesCountAdder);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            revivesCountAdder.Value = AbilityLevel.RevivesCount;
        }

        public override void Clear()
        {
            StageController.Player.Stats.RevivesStat.RemoveAdder(revivesCountAdder);

            base.Clear();
        }
    }
}