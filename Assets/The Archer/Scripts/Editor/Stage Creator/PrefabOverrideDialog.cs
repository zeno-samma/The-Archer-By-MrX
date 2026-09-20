using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class PrefabOverrideDialog : EditorWindow
    {
        protected RoomPrefabsHandler RoomPrefabsHandler { get; set; }
        protected List<GameObject> PrefabOverrides { get; set; }

        protected Vector2 scroll;

        protected UnityAction<bool> onClose;

        protected bool resolved = false;

        public static void Show(RoomPrefabsHandler roomPrefabsHandler, UnityAction<bool> onClose)
        {
            var window = CreateInstance<PrefabOverrideDialog>();
            window.resolved = false;
            window.onClose = onClose;

            window.RoomPrefabsHandler = roomPrefabsHandler;
            window.PrefabOverrides = window.RoomPrefabsHandler.GetAllOverrides();

            window.titleContent = new GUIContent("Unsaved Prefabs");

            var main = EditorGUIUtility.GetMainWindowPosition();

            var width = 400f;
            var height = 300f;

            var x = main.x + (main.width - width) / 2f;
            var y = main.y + (main.height - height) / 2f;

            window.position = new Rect(x, y, width, height);

            window.ShowModalUtility();
        }

        protected virtual void OnGUI()
        {
            EditorGUILayout.LabelField("Overridden Prefabs", EditorStyles.boldLabel);

            GUILayout.Space(4);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            scroll.x = 0;

            for (int i = 0; i < PrefabOverrides.Count; i++)
            {
                var prefabOverride = PrefabOverrides[i];

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(prefabOverride, typeof(GameObject), false);

                if (GUILayout.Button("Save"))
                {
                    SaveObject(i);
                }

                if (GUILayout.Button("Clone and Replace"))
                {
                    ClonePrefabWithOverrides(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(6);
        }

        protected virtual void SaveObject(int index)
        {
            var prefabOverride = PrefabOverrides[index];

            PrefabUtility.ApplyPrefabInstance(prefabOverride, InteractionMode.UserAction);
            PrefabOverrides.RemoveAt(index);
            resolved = true;

            if (PrefabOverrides.Count == 0) Close();
        }

        protected virtual void ClonePrefabWithOverrides(int index)
        {
            var prefab = PrefabOverrides[index];

            var newPath = GetUniqueClonePath(prefab);
            Undo.RegisterFullObjectHierarchyUndo(prefab, "Clone Prefab With Overrides");

            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            var newPrefabInstance = PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, newPath, InteractionMode.UserAction);

            Debug.Log($"Created prefab clone: {newPath}");

            PrefabOverrides.RemoveAt(index);

            resolved = true;
            if (PrefabOverrides.Count == 0) Close();
        }

        protected virtual string GetUniqueClonePath(GameObject prefab)
        {
            var originalPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);

            var directory = Path.GetDirectoryName(originalPath);
            var baseName = Path.GetFileNameWithoutExtension(originalPath);

            string candidate;
            var index = 0;

            do
            {
                string suffix = index == 0 ? " Clone" : $" Clone {index}";
                candidate = Path.Combine(directory, $"{baseName}{suffix}.prefab");
                index++;
            }
            while (File.Exists(candidate));

            return candidate.Replace("\\", "/");
        }

        protected virtual void OnDestroy()
        {
            onClose?.Invoke(false);
        }
    }
}