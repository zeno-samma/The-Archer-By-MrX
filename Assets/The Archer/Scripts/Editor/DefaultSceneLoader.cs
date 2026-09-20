using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OctoberStudio
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        static DefaultSceneLoader()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            RewriteStartScene();
        }

        public static void OnSceneSettingsChanged()
        {
            SetMainMenuScene();
        }

        private static void RewriteStartScene()
        {
            SetMainMenuScene();
        }

        private static void SetMainMenuScene()
        {
            if (!DefaultSceneLoaderActions.Enabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var mainMenuSceneName = GetMainMenuSceneName();

            if (mainMenuSceneName != null)
            {
                SceneAsset mainMenuScene = GetAsset<SceneAsset>(mainMenuSceneName);
                if (mainMenuScene != null)
                {
                    EditorSceneManager.playModeStartScene = mainMenuScene;
                }
            }
            else
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private static T GetAsset<T>(string name = "") where T : Object
        {
            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assets.Length > 0)
            {
                return (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(T));
            }

            return null;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            RewriteStartScene();
        }

        private static string GetMainMenuSceneName()
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(SceneSettings).Name}");
            SceneSettings projectSettings = null;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneSettings>(path);
                if (asset != null)
                {
                    projectSettings = asset;
                    break;
                }
            }

            if (projectSettings != null && SceneExistsInAssets(projectSettings.MainMenuScene.SceneName))
            {
                return projectSettings.MainMenuScene.SceneName;
            }
            else if (SceneExistsInAssets("Main Menu"))
            {
                return "Main Menu";
            }
            else
            {
                return null;
            }
        }

        public static bool SceneExistsInAssets(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            return guids.Any(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return System.IO.Path.GetFileNameWithoutExtension(path) == sceneName;
            });
        }
    }
}