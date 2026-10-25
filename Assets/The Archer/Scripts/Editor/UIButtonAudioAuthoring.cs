using System.IO;
using System.Linq;
using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class UIButtonAudioAuthoring
    {
        [MenuItem("Tools/MrX Studio/UI/Install Button Audio")]
        public static void Install()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            var buttonScript = AssetDatabase.FindAssets("Button t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .First(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p)?.GetClass() == typeof(Button));
            var buttonGuid = AssetDatabase.AssetPathToGUID(buttonScript);
            int added = 0;
            // Edit authored Button owners; nested instances inherit the component.
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/The Archer/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.ReadAllText(path).Contains(buttonGuid)) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int count = Attach(root); added += count;
                    if (count > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var original = SceneManager.GetActiveScene();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/The Archer/Scenes" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = SceneManager.GetSceneByPath(path);
                bool opened = !scene.isLoaded;
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    int count = 0;
                    foreach (var root in scene.GetRootGameObjects()) count += Attach(root);
                    added += count;
                    if (count > 0) EditorSceneManager.SaveScene(scene);
                }
                finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
            }
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            AssetDatabase.SaveAssets();
            Debug.Log("Button audio installed: " + added + " authored components added. Existing modes and custom sounds preserved.");
        }

        private static int Attach(GameObject root)
        {
            int count = 0;
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.GetComponent<UIButtonAudio>() != null) continue;
                button.gameObject.AddComponent<UIButtonAudio>();
                count++;
            }
            return count;
        }
    }
}
