namespace MrX
{
    public enum StageDifficulty
    {
        Basic = 0,
        Intermediate = 1,
        Hard = 2
    }

    public static class StageDifficultyTrial
    {
#if UNITY_EDITOR
        public const string StageKey = "MrX.StageDifficultyTrial.Stage";
        public const string EntryDifficultyKey = "MrX.StageDifficultyTrial.EntryDifficulty";
        public const string SeededKey = "MrX.StageDifficultyTrial.Seeded";
#endif

        public static bool IsActive
        {
            get
            {
#if UNITY_EDITOR
                return UnityEngine.Application.isPlaying &&
                    !string.IsNullOrEmpty(UnityEditor.SessionState.GetString(StageKey, "")) &&
                    !string.IsNullOrEmpty(UnityEditor.SessionState.GetString(Save.SerializationHelper.VerificationDirectoryKey, ""));
#else
                return false;
#endif
            }
        }

        public static bool IsValid(StageDifficulty difficulty) =>
            difficulty >= StageDifficulty.Basic && difficulty <= StageDifficulty.Hard;

        public static int GetInitialStageIndex(StageDatabase database, StageSave save)
        {
#if UNITY_EDITOR
            if (!IsActive) return 0;
            if (UnityEditor.SessionState.GetBool(SeededKey, false))
                return UnityEngine.Mathf.Clamp(save.SelectedStageId, 0, database.StagesAmount - 1);

            UnityEditor.SessionState.SetBool(SeededKey, true);
            string stageId = UnityEditor.SessionState.GetString(StageKey, "");
            int index = System.Array.FindIndex(database.Stages, stage => stage != null && stage.StageId == stageId);
            var difficulty = (StageDifficulty)UnityEditor.SessionState.GetInt(EntryDifficultyKey, 0);
            if (index >= 0 && CanSeed(database, index, difficulty))
            {
                Seed(database, save, index, difficulty);
                return index;
            }
#endif
            return 0;
        }

#if UNITY_EDITOR
        private static bool CanSeed(StageDatabase database, int index, StageDifficulty difficulty)
        {
            var stage = database.GetStageData(index);
            if (!stage.UsesDifficulties || !IsValid(difficulty)) return false;
            var gate = stage.GetDifficultyUnlock(index);
            if (gate == null) return true;

            var requiredStage = gate.GetRequiredStage(database, index);
            return requiredStage != null && CanSeed(database, System.Array.IndexOf(database.Stages, requiredStage), gate.RequiredDifficulty);
        }

        private static void Seed(StageDatabase database, StageSave save, int index, StageDifficulty entryDifficulty)
        {
            var stage = database.GetStageData(index);
            var gate = stage.GetDifficultyUnlock(index);
            if (gate != null)
            {
                var requiredStage = gate.GetRequiredStage(database, index);
                Seed(database, save, System.Array.IndexOf(database.Stages, requiredStage), gate.RequiredDifficulty);
                save.CompleteDifficulty(requiredStage, gate.RequiredDifficulty);
            }

            save.UnlockStage(stage);
            for (int level = 0; level < (int)entryDifficulty; level++)
                save.CompleteDifficulty(stage, (StageDifficulty)level);
        }
#endif
    }
}
