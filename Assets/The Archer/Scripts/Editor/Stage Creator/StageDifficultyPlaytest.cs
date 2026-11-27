using MrX.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrX.StageCreator
{
    [InitializeOnLoad]
    public static class StageDifficultyPlaytest
    {
        private const string MainMenuPath = "Assets/The Archer/Scenes/Main Menu.unity";
        private const string OriginalStartSceneKey = "MrX.StageDifficultyTrial.OriginalStartScene";

        public static bool CanStart => !EditorApplication.isPlayingOrWillChangePlaymode &&
            SceneManager.GetSceneByPath(MainMenuPath).isLoaded;

        static StageDifficultyPlaytest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void Start(StageData stage, StageDifficulty difficulty, StageDatabase balanceDatabase = null,
            bool copyCurrentProfile = false)
        {
            if (!CanStart || stage == null || !stage.UsesDifficulties || !StageDifficultyTrial.IsValid(difficulty))
                return;

            var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
            if (menu == null) return;

            Debug.Log(VerificationSession.Begin(copyCurrentProfile));
            SessionState.SetString(OriginalStartSceneKey, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetString(StageDifficultyTrial.StageKey, stage.StageId);
            SessionState.SetInt(StageDifficultyTrial.EntryDifficultyKey, (int)difficulty);
            if (balanceDatabase != null)
                SessionState.SetString(StageDifficultyTrial.BalanceDatabaseKey, AssetDatabase.GetAssetPath(balanceDatabase));
            SessionState.EraseBool(StageDifficultyTrial.SeededKey);
            EditorSceneManager.playModeStartScene = menu;
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode ||
                string.IsNullOrEmpty(SessionState.GetString(StageDifficultyTrial.StageKey, ""))) return;

            string previousScene = SessionState.GetString(OriginalStartSceneKey, "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previousScene)
                ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScene);
            SessionState.EraseString(OriginalStartSceneKey);
            SessionState.EraseString(StageDifficultyTrial.StageKey);
            SessionState.EraseInt(StageDifficultyTrial.EntryDifficultyKey);
            SessionState.EraseBool(StageDifficultyTrial.SeededKey);
            SessionState.EraseString(StageDifficultyTrial.BalanceDatabaseKey);
            Debug.Log(VerificationSession.End());
        }
    }
}
