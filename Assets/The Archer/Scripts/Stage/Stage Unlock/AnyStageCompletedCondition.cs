using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    [System.Serializable]
    [UnlockCondition("One Of Stages Completed")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "OctoberStudio", "Assembly-CSharp", "AnyStageCompletedCondition")]
    public class AnyStageCompletedCondition : StageUnlockCondition
    {
        [SerializeField, HideInInspector] protected string name = "One Of Stages Completed";

        [SerializeField] protected List<StageData> stages;
        public List<StageData> Stages => stages;

        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                var stageData = stages[i];

                if (stageData == null) continue;
                if (stageSave.IsStageCompleted(stageData)) return true;
            }

            return false;
        }
    }
}
