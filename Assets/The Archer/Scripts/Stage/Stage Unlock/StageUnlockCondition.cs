namespace OctoberStudio
{
    [System.Serializable]
    public abstract class StageUnlockCondition
    {
        public abstract bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave);
    }
}