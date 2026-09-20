namespace OctoberStudio
{
    [System.Serializable]
    [UnlockCondition("Always Unlocked")]
    public class AlwaysUnlockedCondition : StageUnlockCondition
    {
        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            return true;
        }
    }
}