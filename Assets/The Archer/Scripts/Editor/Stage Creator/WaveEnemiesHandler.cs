using OctoberStudio.Enemy;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OctoberStudio.StageCreator
{
    public class WaveEnemiesHandler
    {
        public bool IsWaveLoaded { get; protected set; }
        public EnemiesDatabase EnemiesDatabase { get; protected set; }
        public ChestsDatabase ChestsDatabase { get; protected set; }

        protected List<EnemyData> Enemies { get; set; } = new List<EnemyData>();

        protected GUIStyle damageTextStyle;
        protected GUIStyle hpTextStyle;
        protected GUIStyle outlineTextStyle;

        public WaveEnemiesHandler(EnemiesDatabase enemiesDatabase, ChestsDatabase chestsDatabase)
        {
            EnemiesDatabase = enemiesDatabase;
            ChestsDatabase = chestsDatabase;
        }

        #region Load
        public virtual void Load(SerializedProperty waveProperty)
        {

            var spawnsProperty = waveProperty.FindPropertyRelative("enemySpawns");
            LoadEnemies(spawnsProperty);

            var chestsProperty = waveProperty.FindPropertyRelative("chestSpawns");
            LoadChests(chestsProperty);

            IsWaveLoaded = true;
        }

        protected virtual void LoadEnemies(SerializedProperty enemySpawnsProperty)
        {
            Enemies.Clear();

            for (int i = 0; i < enemySpawnsProperty.arraySize; i++)
            {
                var spawnProperty = enemySpawnsProperty.GetArrayElementAtIndex(i);
                var spawn = new EnemySpawnData(spawnProperty);

                var enemyPrefab = EnemiesDatabase.GetEnemy(spawn.EnemyType).Prefab;

                var obj = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);

                var dropOverride = obj.AddComponent<EnemyOverridesBehavior>();
                dropOverride.Init(spawn.OverrideData);

                while (ComponentUtility.MoveComponentUp(dropOverride)) { }

                obj.transform.Set(spawn.TransformData);
                Undo.ClearUndo(obj);

                Enemies.Add(new EnemyData { behavior = obj.GetComponent<EnemyBehavior>(), overrides = dropOverride });
            }
        }

        protected virtual void LoadChests(SerializedProperty chestSpawnsProperty)
        {
            for (int i = 0; i < chestSpawnsProperty.arraySize; i++)
            {
                var chestSpawnProperty = chestSpawnsProperty.GetArrayElementAtIndex(i);
                var chestSpawn = new ChestSpawnData(chestSpawnProperty);

                var chestPrefab = ChestsDatabase.GetChestData(chestSpawn.ChestType).ChestPrefab;

                var obj = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab);

                obj.transform.Set(chestSpawn.TransformData);
                Undo.ClearUndo(obj);
            }
        }
        #endregion

        public virtual void Save(SerializedProperty waveProperty)
        {
            var enemySpawns = new List<EnemySpawnData>();
            var chestSpawns = new List<ChestSpawnData>();

            var roomScene = SceneManager.GetActiveScene();
            var rootGameObjects = roomScene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy))
                {
                    PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(child);

                    if (status == PrefabInstanceStatus.Connected)
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
                        var enemyType = EnemiesDatabase.GetEnemyType(prefab);
                        var enemySpawn = new EnemySpawnData(enemyType, child.transform);

                        enemySpawns.Add(enemySpawn);
                    }
                    else if (status == PrefabInstanceStatus.NotAPrefab)
                    {
                        continue;
                    }
                }
                else if (child.TryGetComponent<ChestBehavior>(out var chest))
                {
                    PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(child);
                    if (status == PrefabInstanceStatus.Connected)
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
                        var chestType = ChestsDatabase.GetChestType(prefab);
                        var chestSpawn = new ChestSpawnData(chestType, child.transform);
                        chestSpawns.Add(chestSpawn);
                    }
                    else if (status == PrefabInstanceStatus.NotAPrefab)
                    {
                        continue;
                    }
                }
            }
            var enemyspawnsProperty = waveProperty.FindPropertyRelative("enemySpawns");

            enemyspawnsProperty.arraySize = enemySpawns.Count;
            for (int i = 0; i < enemySpawns.Count; i++)
            {
                enemySpawns[i].SaveToSerializedProperty(enemyspawnsProperty.GetArrayElementAtIndex(i));
            }

            var chestSpawnsProperty = waveProperty.FindPropertyRelative("chestSpawns");
            chestSpawnsProperty.arraySize = chestSpawns.Count;
            for (int i = 0; i < chestSpawns.Count; i++)
            {
                chestSpawns[i].SaveToSerializedProperty(chestSpawnsProperty.GetArrayElementAtIndex(i));
            }

            waveProperty.serializedObject.ApplyModifiedProperties();
        }

        public virtual void Delete(bool safe)
        {
            Selection.activeGameObject = null;
            Selection.objects = new Object[0];

            if (safe)
            {
                EditorApplication.delayCall += DeleteUnsafe;
            }
            else
            {
                DeleteUnsafe();
            }
        }

        public virtual void DeleteUnsafe()
        {
            var roomScene = SceneManager.GetActiveScene();
            var rootGameObjects = roomScene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                var child = rootGameObjects[i];

                if (child.TryGetComponent<EnemyBehavior>(out var enemy) || child.TryGetComponent<ChestBehavior>(out var chest))
                {
                    Undo.ClearUndo(child);
                    Object.DestroyImmediate(child);
                }
            }

            IsWaveLoaded = false;
        }

        public virtual void SpawnEnemy(GameObject prefab)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            obj.transform.rotation = Quaternion.Euler(0, 180, 0); // Set default rotation to face the player
            Selection.activeGameObject = obj;

            var dropOverride = obj.AddComponent<EnemyOverridesBehavior>();
            while (ComponentUtility.MoveComponentUp(dropOverride)) { }

            Enemies.Add(new EnemyData { behavior = obj.GetComponent<EnemyBehavior>(), overrides = dropOverride });
        }

        public virtual void SpawnChest(GameObject prefab)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0); // Set default rotation to face the player
            Selection.activeGameObject = obj;
        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            bool isPrefabStage = prefabStage != null;
            if (isPrefabStage) return;

            if (damageTextStyle == null)
            {
                damageTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                damageTextStyle.normal.textColor = Color.red;
            }

            if (outlineTextStyle == null)
            {
                outlineTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                outlineTextStyle.normal.textColor = Color.black;
            }

            if (hpTextStyle == null)
            {
                hpTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                hpTextStyle.normal.textColor = Color.green;
            }


            var hpMultiplier = StageCreatorWindow.Instance.StagePage.CalculateHPMultiplier();
            var damageMultiplier = StageCreatorWindow.Instance.StagePage.CalculateDamageMultiplier();

            for (int i = 0; i < Enemies.Count; i++)
            {
                var data = Enemies[i];

                if (data == null || data.behavior == null || data.overrides == null)
                {
                    Enemies.RemoveAt(i);
                    i--;
                    continue;
                }


                Float damage = data.overrides.OverrideDamage ? data.overrides.Damage : data.behavior.BaseDamage * damageMultiplier;
                string damageText;
                if (damage.Type == PropertyType.Constant)
                {
                    damageText = damage.Value.ToString("F1");
                }
                else
                {
                    damageText = $"{damage.Min.ToString("F1")} - {damage.Max.ToString("F1")}";
                }

                Float hp = data.overrides.OverrideHP ? data.overrides.HP : data.behavior.BaseHP * hpMultiplier;
                string hpText;
                if (hp.Type == PropertyType.Constant)
                {
                    hpText = hp.Value.ToString("F1");
                }
                else
                {
                    hpText = $"{hp.Min.ToString("F1")} - {hp.Max.ToString("F1")}";
                }

                DrawLabel(damageText, damageTextStyle, data.behavior.transform.position);
                DrawLabel(hpText, hpTextStyle, data.behavior.transform.position - Camera.main.transform.up * 0.5f);
            }
        }

        protected void DrawLabel(string text, GUIStyle style, Vector3 position)
        {
            var camera = Camera.main.transform;
            var multiplier = 50f / (camera.position - position).sqrMagnitude;

            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Handles.Label(position + camera.right * i * multiplier + camera.up * j * multiplier, text, outlineTextStyle);
                }
            }

            Handles.Label(position, text, style);
        }

        protected class EnemyData
        {
            public EnemyBehavior behavior;
            public EnemyOverridesBehavior overrides;
        }
    }
}