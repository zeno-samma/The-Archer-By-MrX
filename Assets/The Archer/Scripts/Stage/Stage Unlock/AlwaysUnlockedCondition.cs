namespace MrX
{
    [System.Serializable]
    [UnlockCondition("Always Unlocked")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "OctoberStudio", "Assembly-CSharp", "AlwaysUnlockedCondition")]
    public class AlwaysUnlockedCondition : StageUnlockCondition
    {
        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            return true;
        }
    }
}
