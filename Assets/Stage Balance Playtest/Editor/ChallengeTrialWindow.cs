using MrX.StageCreator;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    internal sealed class ChallengeTrialWindow : EditorWindow
    {
        [SerializeField] private int stageIndex;
        [SerializeField] private StageDifficulty difficulty = StageDifficulty.Basic;
        [SerializeField] private bool copyCurrentProfile;

        [MenuItem("Tools/MrX Studio/Challenge Playtest")]
        private static void Open()
        {
            GetWindow<ChallengeTrialWindow>("Challenge Playtest");
        }

        private void OnGUI()
        {
            var database = AssetDatabase.LoadAssetAtPath<StageDatabase>(StageDifficultyTrial.ChallengeDatabasePath);
            if (database == null)
            {
                EditorGUILayout.HelpBox("Challenge Trial Database is missing.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Bản chơi thử độ khó", EditorStyles.boldLabel);
            var names = System.Array.ConvertAll(database.Stages, stage => stage.StageName);
            stageIndex = EditorGUILayout.Popup("Ải", stageIndex, names);
            difficulty = (StageDifficulty)EditorGUILayout.EnumPopup("Độ khó", difficulty);
            copyCurrentProfile = EditorGUILayout.ToggleLeft("Sao chép trang bị và tiến trình hiện tại", copyCurrentProfile);
            EditorGUILayout.HelpBox("Bản thử lưu riêng. Có thể tiếp tục nâng cấp và chơi nhiều lượt trong cùng phiên Play.",
                MessageType.Info);

            if (!StageDifficultyPlaytest.CanStart)
                EditorGUILayout.HelpBox("Mở scene Main Menu và thoát Play Mode để bắt đầu.", MessageType.Info);

            using (new EditorGUI.DisabledScope(!StageDifficultyPlaytest.CanStart))
            {
                if (GUILayout.Button("Mở bản chơi thử"))
                    StageDifficultyPlaytest.Start(database.GetStageData(stageIndex), difficulty, database, copyCurrentProfile);
            }
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Mở dữ liệu trong Stage Creator"))
                    StageCreatorWindow.OpenChallengeDatabase(database);
            }
        }
    }
}
