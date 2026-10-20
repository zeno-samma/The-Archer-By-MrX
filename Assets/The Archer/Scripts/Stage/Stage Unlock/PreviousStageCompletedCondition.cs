using UnityEngine;

namespace MrX
{
    [System.Serializable]
    [UnlockCondition("Previous Stage Completed")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "OctoberStudio", "Assembly-CSharp", "PreviousStageCompletedCondition")]
    public class PreviousStageCompletedCondition : StageUnlockCondition
    {
        [SerializeField, HideInInspector] protected string name = "Previous Stage Completed";

        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            if (stageIndex == 0) return true;
            var prevStage = database.GetStageData(stageIndex - 1);

            return stageSave.IsStageCompleted(prevStage);
        }
    }
}
