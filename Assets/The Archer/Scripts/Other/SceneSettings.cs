using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "SceneSettings", menuName = "October/Scene Settings", order = 1)]
    public class SceneSettings : ScriptableObject
    {
        [SerializeField] protected SceneWrapper mainMenuScene;
        public SceneWrapper MainMenuScene => mainMenuScene;

        [SerializeField] protected SceneWrapper gameScene;
        public SceneWrapper GameScene => gameScene;

        [SerializeField] protected SceneWrapper loadingScene;
        public SceneWrapper LoadingScene => loadingScene;

        protected void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this); // Mark asset dirty for saving

            // Dynamically call editor-only method
            var editorType = System.Type.GetType("OctoberStudio.DefaultSceneLoader, Assembly-CSharp-Editor");
            if (editorType != null)
            {
                var method = editorType.GetMethod("OnSceneSettingsChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(null, new object[] { });
            }
#endif
        }
    }
}