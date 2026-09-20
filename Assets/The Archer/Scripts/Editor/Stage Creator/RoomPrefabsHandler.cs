using OctoberStudio.Enemy;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace OctoberStudio.StageCreator
{
    public class RoomPrefabsHandler
    {
        protected readonly string prefabsFolderPath = "Assets/The Archer/Prefabs";
        protected readonly string generatedPrefabsFolderName = "Generated Stage Prefabs";
        protected readonly string generatedPrefabsFolderPath = "Assets/The Archer/Prefabs/Generated Stage Prefabs";
        public static readonly string roomCreatorScenePath = "Assets/The Archer/Scenes/Room Creator.unity";

        public bool IsRoomLoaded { get; protected set; } = false;
        public bool IsSceneLoaded { get; protected set; } = false;

        public bool HasOverridenPrefabs { get; protected set; } = false;

        protected Dictionary<GameObject, PrefabData> prefabDataDictionary = new Dictionary<GameObject, PrefabData>();
        protected Scene roomScene;
        protected Scene RoomScene
        {
            get
            {
                if (roomScene.IsValid()) return roomScene;
                roomScene = SceneManager.GetSceneByPath(roomCreatorScenePath);
                return roomScene;
            }

            set => roomScene = value;
        }

        public List<GameObject> Prefabs { get; protected set; }

        public event UnityAction OnPrefabsChanged;

        public virtual void LoadPrefabs(SerializedProperty prefabsProperty)
        {
            if (!IsSceneLoaded)
            {
                RoomScene = SceneUtility.CreateAndAddScene(roomCreatorScenePath, true);
                IsSceneLoaded = true;
            }

            SceneManager.SetActiveScene(RoomScene);

            Prefabs = new List<GameObject>();

            for (int i = 0; i < prefabsProperty.arraySize; i++)
            {
                var prefabData = new PrefabData(prefabsProperty.GetArrayElementAtIndex(i));

                if (prefabData.Prefab == null) continue;

                var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabData.Prefab);
                var transformData = prefabData.TransformData;
                obj.transform.position = transformData.Position;
                obj.transform.rotation = transformData.Rotation;
                obj.transform.localScale = transformData.LocalScale;

                if (!Prefabs.Contains(prefabData.Prefab)) Prefabs.Add(prefabData.Prefab);

                prefabDataDictionary.Add(obj, prefabData);
            }

            IsRoomLoaded = true;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        public virtual void SavePrefabs(SerializedProperty prefabsProperty)
        {
            var prefabDataList = new List<PrefabData>();

            var rootGameObjects = RoomScene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy) || child.TryGetComponent<ChestBehavior>(out var chest)) continue;

                PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(child);

                if (status == PrefabInstanceStatus.Connected)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
                    var prefabData = new PrefabData(prefab, child.transform);

                    prefabDataList.Add(prefabData);
                }
                else if (status == PrefabInstanceStatus.NotAPrefab)
                {
                    if (TryCreatePrefab(child, out var prefab))
                    {
                        prefab.transform.position = Vector3.zero;
                        prefab.transform.rotation = Quaternion.identity;
                        prefab.transform.localScale = Vector3.one;

                        EditorUtility.SetDirty(prefab);

                        var prefabData = new PrefabData(prefab, child.transform);
                        prefabDataList.Add(prefabData);
                    }
                }
            }

            prefabsProperty.arraySize = prefabDataList.Count;
            for (int i = 0; i < prefabDataList.Count; i++)
            {
                prefabDataList[i].SaveToSerializedProperty(prefabsProperty.GetArrayElementAtIndex(i));
            }

            prefabsProperty.serializedObject.ApplyModifiedProperties();
        }

        protected virtual bool TryCreatePrefab(GameObject obj, out GameObject prefab)
        {
            if (!AssetDatabase.IsValidFolder(generatedPrefabsFolderPath))
            {
                AssetDatabase.CreateFolder(prefabsFolderPath, generatedPrefabsFolderName);
            }

            int number = 1;
            var prefabPath = BuildPrefabName(generatedPrefabsFolderPath, obj.name, number);

            while (System.IO.File.Exists(generatedPrefabsFolderPath + "/" + prefabPath + ".prefab"))
            {
                number++;
                prefabPath = BuildPrefabName(generatedPrefabsFolderPath, obj.name, number);
            }

            prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.AutomatedAction, out var success);

            return success;
        }

        public virtual void SpawnObject(GameObject prefab)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var prefabData = new PrefabData(prefab, obj.transform);

            prefabDataDictionary.Add(obj, prefabData);

            Selection.activeGameObject = obj;
        }

        public virtual void DeletePrefabs(bool deleteScene)
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            if (deleteScene)
            {
                Selection.activeGameObject = null;
                Selection.objects = new Object[0];

                EditorApplication.delayCall += CloseAndDeleteScene;

                IsSceneLoaded = false;
            }
            else
            {
                var rootGameObjects = RoomScene.GetRootGameObjects();
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    var child = rootGameObjects[i];

                    if (!(child.TryGetComponent<EnemyBehavior>(out var enemy) && child.TryGetComponent<ChestBehavior>(out var chest)))
                    {
                        Object.DestroyImmediate(child);
                    }
                }
            }
            IsRoomLoaded = false;
        }

        public virtual void CloseAndDeleteScene()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            SceneUtility.CloseAndDeleteScene(roomCreatorScenePath);
        }

        public static void OnReload()
        {
            SceneUtility.CloseAndDeleteScene(roomCreatorScenePath);
        }

        protected virtual void OnHierarchyChanged()
        {
            Prefabs.Clear();

            var rootGameObjects = RoomScene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy) || child.TryGetComponent<ChestBehavior>(out var chest)) continue;

                if (!prefabDataDictionary.TryGetValue(child, out var prefabData))
                {
                    PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(child);

                    if (status == PrefabInstanceStatus.Connected)
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
                        var newPrefabData = new PrefabData(prefab, child.transform);

                        prefabDataDictionary.Add(child, new PrefabData(prefab, child.transform));

                        if (!Prefabs.Contains(prefab)) Prefabs.Add(prefab);
                    }
                    else if (status == PrefabInstanceStatus.NotAPrefab)
                    {
                        if (TryCreatePrefab(child, out var prefab))
                        {
                            prefab.transform.position = Vector3.zero;
                            prefab.transform.rotation = Quaternion.identity;
                            prefab.transform.localScale = Vector3.one;

                            EditorUtility.SetDirty(prefab);

                            prefabDataDictionary.Add(child, new PrefabData(prefab, child.transform));

                            var newPrefabData = new PrefabData(prefab, child.transform);
                            if (!Prefabs.Contains(prefab)) Prefabs.Add(prefab);
                        }
                    }
                }
                else
                {
                    if (!Prefabs.Contains(prefabData.Prefab)) Prefabs.Add(prefabData.Prefab);
                }
            }
            OnPrefabsChanged?.Invoke();
            StageCreatorWindow.Instance?.Repaint();

            CheckForOverridenPrefabs();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string BuildPrefabName(string folder, string name, int number)
        {
            return $"{folder}/{name}_{number}.prefab";
        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {
            var e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                CheckForOverridenPrefabs();
            }
        }

        protected virtual void CheckForOverridenPrefabs()
        {
            var rootGameObjects = RoomScene.GetRootGameObjects();

            HasOverridenPrefabs = false;
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy) || child.TryGetComponent<ChestBehavior>(out var chest)) continue;

                if (PrefabUtility.HasPrefabInstanceAnyOverrides(child, false) && !PrefabUtility.IsPartOfModelPrefab(child))
                {
                    HasOverridenPrefabs = true;
                    break;
                }
            }
        }

        public virtual List<GameObject> GetAllOverrides()
        {
            var result = new List<GameObject>();

            var rootGameObjects = RoomScene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy) || child.TryGetComponent<ChestBehavior>(out var chest)) continue;

                if (PrefabUtility.HasPrefabInstanceAnyOverrides(child, false) && !PrefabUtility.IsPartOfModelPrefab(child))
                {
                    result.Add(child);
                }
            }

            return result;
        }
    }
}