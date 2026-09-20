using OctoberStudio.Save;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class StagePage
    {
        public SerializedObject StageSerializedObject { get; protected set; }

        public RoomsListContainer RoomsListContainer { get; protected set; }
        public RoomPanel RoomPanel { get; protected set; }
        public static RoomPrefabsHandler RoomPrefabsHandler { get; protected set; }
        public WavesListContainer WavesListContainer { get; protected set; }
        public WavePanel WavePanel { get; protected set; }
        public static WaveEnemiesHandler WaveEnemiesHandler { get; protected set; }

        public int StageIndex { get; protected set; }
        public int RoomIndex => RoomsListContainer.RoomsListView.SelectedIndex;
        public int WaveIndex => WavesListContainer.WavesListView.SelectedIndex;

        public bool IsOpened { get; protected set; } = false;

        [UnityEditor.Callbacks.DidReloadScripts]
        protected static void Reload()
        {
            RoomPrefabsHandler.OnReload();
        }

        public virtual void ApplyTestingData()
        {
            var saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(SaveManager.SAVE_FILE_NAME, useLogs: false);
            saveDatabase.Init();
            var save = saveDatabase.GetSave<StageSave>("Stage");
            save.Init();

            save.SetTestingData(StageCreatorWindow.Instance.StagePage.StageIndex, StageCreatorWindow.Instance.StagePage.RoomIndex, StageCreatorWindow.Instance.StagePage.WaveIndex);
            saveDatabase.Flush();

            SerializationHelper.SerializePersistent(saveDatabase, SaveManager.SAVE_FILE_NAME);
        }

        public virtual void OnDisable()
        {
            IsOpened = false;

            if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
            {
                if (WavesListContainer != null && WavesListContainer.WavesListView.SelectedItem != null)
                {
                    WaveEnemiesHandler.Save(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
                }

                WaveEnemiesHandler.Delete(true);
            }

            if (RoomPrefabsHandler != null)
            {
                if (RoomPrefabsHandler.IsRoomLoaded)
                {
                    if (RoomPrefabsHandler.HasOverridenPrefabs)
                    {
                        PrefabOverrideDialog.Show(RoomPrefabsHandler, null);
                    }

                    var prefabsProperty = RoomsListContainer.SelectedRoom.RoomProperty.FindPropertyRelative("prefabs");
                    RoomPrefabsHandler.SavePrefabs(prefabsProperty);
                    RoomPrefabsHandler.DeletePrefabs(true);
                }

                RoomPrefabsHandler.OnPrefabsChanged -= InitPrefabsGrid;
            }

            if (RoomsListContainer != null)
            {
                RoomsListContainer.onBeforeDragPerformed -= OnBeforeRoomDragPerformed;
                RoomsListContainer.onAfterDragPerformed -= OnAfterRoomDragPerformed;
                RoomsListContainer.onRoomSelected -= OnRoomSelected;
                RoomsListContainer.onDeletedRoom -= OnRoomDeleted;
            }

            if (WavesListContainer != null)
            {
                WavesListContainer.onBeforeDragPerformed -= OnBeforeWaveDragPerformed;
                WavesListContainer.onAfterDragPerformed -= OnAfterWaveDragPerformed;
                WavesListContainer.onWaveSelected -= OnWaveSelected;
                WavesListContainer.onDeletedWave -= OnWaveDeleted;
            }

            if (RoomPanel != null) RoomPanel.Clear();
        }

        public void Open(SerializedObject stageSerializedObject, int stageIndex)
        {
            StageSerializedObject = stageSerializedObject;
            StageIndex = stageIndex;

            if (RoomsListContainer != null) RoomsListContainer.Clear();
            if (RoomPanel != null) RoomPanel.Clear();
            if (WavesListContainer != null) WavesListContainer.Clear();

            RoomsListContainer = new RoomsListContainer(StageSerializedObject, StageCreatorWindow.Instance.LastSelectedRoomId);
            RoomPrefabsHandler = new RoomPrefabsHandler();
            RoomPanel = new RoomPanel(this, RoomPrefabsHandler);
            WavesListContainer = new WavesListContainer(RoomsListContainer.SelectedRoom.RoomProperty, StageCreatorWindow.Instance.LastSelectedWaveId);
            WavePanel = new WavePanel();
            WaveEnemiesHandler = new WaveEnemiesHandler(WavePanel.EnemiesDatabase, WavePanel.ChestsDatabase);

            var prefabsProperty = RoomsListContainer.SelectedRoom.RoomProperty.FindPropertyRelative("prefabs");
            RoomPrefabsHandler.LoadPrefabs(prefabsProperty);
            WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            InitPrefabsGrid();

            RoomPrefabsHandler.OnPrefabsChanged += InitPrefabsGrid;

            RoomsListContainer.onBeforeDragPerformed += OnBeforeRoomDragPerformed;
            RoomsListContainer.onAfterDragPerformed += OnAfterRoomDragPerformed;
            RoomsListContainer.onRoomSelected += OnRoomSelected;
            RoomsListContainer.onDeletedRoom += OnRoomDeleted;

            WavesListContainer.onBeforeDragPerformed += OnBeforeWaveDragPerformed;
            WavesListContainer.onAfterDragPerformed += OnAfterWaveDragPerformed;
            WavesListContainer.onWaveSelected += OnWaveSelected;
            WavesListContainer.onDeletedWave += OnWaveDeleted;

            IsOpened = true;
        }

        protected virtual void OnBeforeWaveDragPerformed()
        {
            if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
            {
                if (WavesListContainer != null && WavesListContainer.WavesListView.SelectedItem != null)
                {
                    WaveEnemiesHandler.Save(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
                }
                WaveEnemiesHandler.Delete(false);
            }
        }

        protected virtual void OnAfterWaveDragPerformed()
        {
            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            }
        }

        protected virtual void OnWaveSelected(WaveListItem selectedWave, WaveListItem prevSelectedWave)
        {
            if (selectedWave != prevSelectedWave)
            {
                if (WaveEnemiesHandler != null)
                {
                    try
                    {
                        // Just checking if prevSelectedWave was deleted
                        var so = prevSelectedWave.WaveProperty.serializedObject;
                        WaveEnemiesHandler.Save(prevSelectedWave.WaveProperty);
                    }
                    catch
                    {

                    }
                    WaveEnemiesHandler.Delete(false);
                    WaveEnemiesHandler.Load(selectedWave.WaveProperty);
                }
            }
        }

        protected virtual void OnWaveDeleted()
        {

        }

        protected virtual void OnBeforeRoomDragPerformed()
        {
            if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
            {
                if (WavesListContainer != null && WavesListContainer.WavesListView.SelectedItem != null)
                {
                    WaveEnemiesHandler.Save(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
                }
                WaveEnemiesHandler.Delete(false);
            }

            WavesListContainer.Clear();

            WavesListContainer.onBeforeDragPerformed -= OnBeforeWaveDragPerformed;
            WavesListContainer.onAfterDragPerformed -= OnAfterWaveDragPerformed;
            WavesListContainer.onWaveSelected -= OnWaveSelected;
            WavesListContainer.onDeletedWave -= OnWaveDeleted;

            WavesListContainer = null;
        }

        protected virtual void OnAfterRoomDragPerformed()
        {
            WavesListContainer = new WavesListContainer(RoomsListContainer.SelectedRoom.RoomProperty, 0);

            WavesListContainer.onBeforeDragPerformed += OnBeforeWaveDragPerformed;
            WavesListContainer.onAfterDragPerformed += OnAfterWaveDragPerformed;
            WavesListContainer.onWaveSelected += OnWaveSelected;
            WavesListContainer.onDeletedWave += OnWaveDeleted;

            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            }
        }

        protected virtual void LoadPrefabs(RoomListItem item) 
        {
            var prefabsProperty = item.RoomProperty.FindPropertyRelative("prefabs");
            RoomPrefabsHandler.LoadPrefabs(prefabsProperty);
        }

        protected virtual void SavePrefabs(RoomListItem item)
        {
            var prevPrefabsProperty = item.RoomProperty.FindPropertyRelative("prefabs");
            RoomPrefabsHandler.SavePrefabs(prevPrefabsProperty);
        }

        protected UnityAction onDrawEnded;

        protected virtual void SwapRooms(RoomListItem selectedRoom, RoomListItem prevRoom)
        {
            if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
            {
                if (WavesListContainer != null && WavesListContainer.WavesListView.SelectedItem != null)
                {
                    var wavePropertyValid = false;
                    try
                    {
                        var testName = WavesListContainer.WavesListView.SelectedItem.WaveProperty.name;
                        wavePropertyValid = true;
                    }
                    catch { }

                    if (wavePropertyValid)
                    {
                        WaveEnemiesHandler.Save(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
                    }            
                }
                WaveEnemiesHandler.Delete(false);
            }

            if (RoomPrefabsHandler != null)
            {
                if (RoomPrefabsHandler.IsRoomLoaded)
                {
                    if (prevRoom != null) SavePrefabs(prevRoom);
                    RoomPrefabsHandler.DeletePrefabs(false);
                }

                LoadPrefabs(selectedRoom);
            }

            if (WavesListContainer != null)
            {
                WavesListContainer.Clear();

                WavesListContainer.onBeforeDragPerformed -= OnBeforeWaveDragPerformed;
                WavesListContainer.onAfterDragPerformed -= OnAfterWaveDragPerformed;
                WavesListContainer.onWaveSelected -= OnWaveSelected;
                WavesListContainer.onDeletedWave -= OnWaveDeleted;

                WavesListContainer = new WavesListContainer(selectedRoom.RoomProperty, 0);

                WavesListContainer.onBeforeDragPerformed += OnBeforeWaveDragPerformed;
                WavesListContainer.onAfterDragPerformed += OnAfterWaveDragPerformed;
                WavesListContainer.onWaveSelected += OnWaveSelected;
                WavesListContainer.onDeletedWave += OnWaveDeleted;

                if (WaveEnemiesHandler != null)
                {
                    WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
                }
            }
        }

        protected virtual void OnRoomSelected(RoomListItem selectedRoom, RoomListItem prevRoom)
        {
            if (prevRoom == selectedRoom) return;

            if(RoomPrefabsHandler != null && RoomPrefabsHandler.IsRoomLoaded && prevRoom != null && RoomPrefabsHandler.HasOverridenPrefabs)
            {
                onDrawEnded = () =>
                {
                    if (Event.current.type == EventType.Used)
                    {
                        PrefabOverrideDialog.Show(RoomPrefabsHandler, (_) =>
                        {
                            SwapRooms(selectedRoom, prevRoom);
                        });
                    }
                };
            } else
            {
                SwapRooms(selectedRoom, prevRoom);
            }           
        }

        protected virtual void OnRoomDeleted()
        {
            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Delete(false);
            }

            if (RoomPrefabsHandler != null && RoomPrefabsHandler.IsRoomLoaded)
            {
                RoomPrefabsHandler.DeletePrefabs(false);
            }
        }

        public void Draw()
        {
            if (!IsOpened) return;

            StageSerializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.BeginHorizontal();

            var isBackButtonPressed = RoomsListContainer.Draw();

            SerializedPropertyExtensions.DrawVerticalSeparator(2);

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            RoomPanel.Draw(RoomsListContainer.SelectedRoom.RoomProperty, RoomPrefabsHandler.SpawnObject);

            GUILayout.Space(10);

            SerializedPropertyExtensions.DrawHorizontalSeparator(2);

            EditorGUILayout.BeginHorizontal();

            WavesListContainer.Draw();
            WavePanel.Draw(WavesListContainer.WavesListView.SelectedItem.WaveProperty, WaveEnemiesHandler.SpawnEnemy, WaveEnemiesHandler.SpawnChest);

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            StageSerializedObject.ApplyModifiedProperties();

            onDrawEnded?.Invoke();
            onDrawEnded = null;

            if (isBackButtonPressed)
            {
                StageCreatorWindow.Instance.OpenPage(StageCreatorWindow.PageType.StageDatabase);
            }
        }

        public virtual float CalculateExperienceLevel(SerializedProperty currentWave)
        {
            var expienceData = StageCreatorWindow.Instance.ExperienceData;

            if (expienceData == null) return -1f;

            var xp = 0f;
            for (int i = 0; i < RoomsListContainer.RoomsListView.SelectedItem.Index; i++)
            {
                var roomItem = RoomsListContainer.RoomsListView[i];
                xp += roomItem.GetAllExperience();
            }

            xp += RoomsListContainer.RoomsListView.SelectedItem.GetAllExperienceUpTo(currentWave);

            var level = 0f;
            var levelId = 0;
            while (xp > 0)
            {
                var xpLevel = expienceData.GetXP(levelId);

                if (xp >= xpLevel)
                {
                    level += 1f;
                    xp -= xpLevel;
                }
                else
                {
                    level += xp / xpLevel;

                    break;
                }

                levelId++;
            }

            return level;
        }

        public virtual int CalculateCurrency(SerializedProperty currentWave, string currencyId)
        {
            var amount = 0;

            for (int i = 0; i < RoomsListContainer.RoomsListView.SelectedItem.Index; i++)
            {
                var roomItem = RoomsListContainer.RoomsListView[i];
                amount += roomItem.GetAllCurrency(currencyId);
            }

            amount += RoomsListContainer.RoomsListView.SelectedItem.GetAllCurrencyUpTo(currentWave, currencyId);

            return amount;
        }

        public virtual float CalculateHPMultiplier()
        {
            var enemyHPMulitplier = StageSerializedObject.FindProperty("enemyHPMulitplier").floatValue;
            var enemyHPMultiplierRoomStep = StageSerializedObject.FindProperty("enemyHPMultiplierRoomStep").floatValue;
            var enemyHPMultiplierWaveStep = StageSerializedObject.FindProperty("enemyHPMultiplierWaveStep").floatValue;

            return enemyHPMulitplier +
                enemyHPMultiplierRoomStep * RoomsListContainer.SelectedRoom.Index +
                enemyHPMultiplierWaveStep * WavesListContainer.WavesListView.SelectedItem.Index;
        }

        public virtual float CalculateDamageMultiplier()
        {
            var enemyDamageMulitplier = StageSerializedObject.FindProperty("enemyDamageMulitplier").floatValue;
            var enemyDamageMultiplierRoomStep = StageSerializedObject.FindProperty("enemyDamageMultiplierRoomStep").floatValue;
            var enemyDamageMultiplierWaveStep = StageSerializedObject.FindProperty("enemyDamageMultiplierWaveStep").floatValue;

            return enemyDamageMulitplier +
                enemyDamageMultiplierRoomStep * RoomsListContainer.SelectedRoom.Index +
                enemyDamageMultiplierWaveStep * WavesListContainer.WavesListView.SelectedItem.Index;
        }

        public virtual void InitPrefabsGrid()
        {
            var prefabs = new List<GameObject>();

            if (RoomsListContainer != null)
            {
                for (int i = 0; i < RoomsListContainer.RoomsListView.Count; i++)
                {
                    var roomProperty = RoomsListContainer.RoomsListView[i].RoomProperty;
                    var prefabsProperty = roomProperty.FindPropertyRelative("prefabs");

                    for (int j = 0; j < prefabsProperty.arraySize; j++)
                    {
                        var prefabDataProperty = prefabsProperty.GetArrayElementAtIndex(j);
                        var prefab = prefabDataProperty.FindPropertyRelative("prefab").objectReferenceValue as GameObject;

                        if (!prefabs.Contains(prefab)) prefabs.Add(prefab);
                    }
                }
            }


            if (RoomPrefabsHandler != null && RoomPrefabsHandler.IsRoomLoaded)
            {
                for (int i = 0; i < RoomPrefabsHandler.Prefabs.Count; i++)
                {
                    var prefab = RoomPrefabsHandler.Prefabs[i];
                    if (prefab == null) continue;

                    if (!prefabs.Contains(prefab)) prefabs.Add(prefab);
                }
            }

            RoomPanel.SetGridPrefabs(prefabs);
        }

        public virtual void SaveScene()
        {
            if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
            {
                WaveEnemiesHandler.Save(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            }

            if (RoomPrefabsHandler != null && RoomPrefabsHandler.IsSceneLoaded)
            {
                RoomPrefabsHandler.SavePrefabs(RoomsListContainer.SelectedRoom.RoomProperty.FindPropertyRelative("prefabs"));
            }
        }

        public virtual void ClearScene(bool deleteScene = false)
        {
            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Delete(false);
            }

            if (RoomPrefabsHandler != null)
            {
                RoomPrefabsHandler.DeletePrefabs(deleteScene);
            }
        }

        public virtual void ClearSceneEnemies()
        {
            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Delete(false);
            }
        }

        public virtual void LoadScene()
        {
            if (RoomPrefabsHandler != null)
            {
                RoomPrefabsHandler.LoadPrefabs(RoomsListContainer.SelectedRoom.RoomProperty.FindPropertyRelative("prefabs"));
            }

            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            }
        }

        public virtual void LoadSceneEnemies()
        {
            if (WaveEnemiesHandler != null)
            {
                WaveEnemiesHandler.Load(WavesListContainer.WavesListView.SelectedItem.WaveProperty);
            }
        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {
            if (!IsOpened) return;
            if (RoomPanel == null) return;

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            bool isPrefabStage = prefabStage != null;
            if (isPrefabStage) return;

            if (RoomPanel.OnSceneGUI(RoomsListContainer.SelectedRoom.RoomProperty))
            {
                if (WaveEnemiesHandler != null && WaveEnemiesHandler.IsWaveLoaded)
                {
                    WaveEnemiesHandler.OnSceneGUI(sceneView);
                }
            }

            if(RoomPrefabsHandler != null && RoomPrefabsHandler.IsSceneLoaded)
            {
                RoomPrefabsHandler.OnSceneGUI(sceneView);
            }
        }
    }
}