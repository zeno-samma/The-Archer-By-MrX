using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace OctoberStudio
{
    public static class SceneUtility
    {
        /// <summary>
        /// Creates a new scene, saves it to the specified path, and loads it additively.
        /// </summary>
        /// <param name="scenePath">Path inside the Assets folder (e.g. "Assets/Scenes/MyScene.unity")</param>
        public static Scene CreateAndAddScene(string scenePath, bool rewriteIfExists)
        {
            // Ensure folder exists
            string directory = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            } else if(rewriteIfExists)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }

                AssetDatabase.DeleteAsset(scenePath);
                AssetDatabase.Refresh();

                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // Create new scene additively
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // Save to disk
            EditorSceneManager.SaveScene(newScene, scenePath);

            return newScene;
        }

        /// <summary>
        /// Closes and deletes the scene at the specified path.
        /// </summary>
        /// <param name="scenePath">Path inside the Assets folder (e.g. "Assets/Scenes/MyScene.unity")</param>
        public static void CloseAndDeleteScene(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);

            // Close if it's currently open
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.DeleteAsset(scenePath);
        }
    }
}