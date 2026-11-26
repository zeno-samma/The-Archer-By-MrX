using UnityEngine;

namespace MrX
{
    [System.Serializable]
    [UnlockCondition("Clear Stage Through Difficulty")]
    public class ClearStageThroughDifficultyCondition : StageUnlockCondition
    {
        [SerializeField] private bool usePreviousStage = true;
        [SerializeField] private StageData stage;
        [SerializeField] private StageDifficulty requiredDifficulty = StageDifficulty.Hard;

        public StageDifficulty RequiredDifficulty => requiredDifficulty;

        public StageData GetRequiredStage(StageDatabase database, int stageIndex)
        {
            if (stageIndex <= 0 || stageIndex >= database.StagesAmount) return null;
            if (usePreviousStage) return database.GetStageData(stageIndex - 1);

            int targetIndex = System.Array.IndexOf(database.Stages, stage);
            return targetIndex >= 0 && targetIndex < stageIndex ? stage : null;
        }

        public override bool IsMet(StageDatabase database, int stageIndex, StageSave stageSave)
        {
            var requiredStage = GetRequiredStage(database, stageIndex);
            return requiredStage != null && stageSave.HasClearedThrough(requiredStage, requiredDifficulty);
        }

        public string GetDescription(StageDatabase database, int stageIndex)
        {
            var requiredStage = GetRequiredStage(database, stageIndex);
            return requiredStage == null
                ? "This stage has an invalid difficulty requirement."
                : $"Clear {requiredStage.StageName} on {requiredDifficulty} to unlock.";
        }
    }
}
