using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    [System.Serializable]
    [UnlockCondition("All Stages Completed")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "OctoberStudio", "Assembly-CSharp", "AllStagesCompletedCondition")]
    public class AllStagesCompletedCondition : StageUnlockCondition
    {
        [SerializeField, HideInInspector] protected string name = "All Stages Completed";

        [SerializeField] protected List<StageData> stages;
        public List<StageData> Stages => stages;

        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                var stageData = stages[i];
                if(stageData == null)
                {
                    return false;
                }

                if (!stageSave.IsStageCompleted(stageData)) return false;
            }

            return true;
        }
    }
}
