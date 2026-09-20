using OctoberStudio.Currency;
using OctoberStudio.Enemy;
using OctoberStudio.Save;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class WavePanel
    {
        protected EditorGridView<GameObject, RoomPropGridItem> enemiesGridView = new EditorGridView<GameObject, RoomPropGridItem>(StageCreatorWindow.Instance.DefaultEnemyTexture);
        protected EditorGridView<Sprite, IconGridItem> iconsGridView = new EditorGridView<Sprite, IconGridItem>(null);
        protected EditorGridView<GameObject, RoomPropGridItem> chestsGridView = new EditorGridView<GameObject, RoomPropGridItem>(StageCreatorWindow.Instance.DefaultChestTexture);

        protected Vector2 scrollPosition = Vector2.zero;

        protected GUIStyle titleTextStyle;
        protected GUIStyle gridTextStyle;

        #region Properties

        protected EnemiesSpawner enemiesSpawner;
        public EnemiesSpawner EnemiesSpawner
        {
            get
            {
                if (enemiesSpawner == null)
                {
#if UNITY_6000_4_OR_NEWER
                    enemiesSpawner = Object.FindAnyObjectByType<EnemiesSpawner>();
#elif UNITY_6000_0_OR_NEWER
                    enemiesSpawner = Object.FindFirstObjectByType<EnemiesSpawner>();
#else
                    enemiesSpawner = Object.FindObjectOfType<EnemiesSpawner>();
#endif
                }

                return enemiesSpawner;
            }
        }

        protected EnemiesDatabase enemiesDatabase;
        public EnemiesDatabase EnemiesDatabase
        {
            get
            {
                if (enemiesDatabase == null)
                {
                    enemiesDatabase = EnemiesSpawner.EnemiesDatabase;
                }
                return enemiesDatabase;
            }
        }

        protected ChestsDatabase chestsDatabase;
        public ChestsDatabase ChestsDatabase
        {
            get
            {
                if (chestsDatabase == null)
                {
                    chestsDatabase = EnemiesSpawner.ChestsDatabase;
                }
                return chestsDatabase;
            }
        }

        #endregion

        public WavePanel()
        {
            if (iconsGridView.ItemsCount == 0)
            {
                var icons = StageCreatorWindow.Instance.StageDatabase.GetWaveIcons();
                iconsGridView.SetItems(icons);
            }

            InitPrefabGrids();
        }

        protected bool isGridsInited = false;
        protected virtual void InitPrefabGrids()
        {
            if (EnemiesSpawner != null && EnemiesDatabase != null)
            {
                if (enemiesGridView.ItemsCount == 0)
                {
                    var prefabs = EnemiesDatabase.GetAllEnemiesPrefabs();
                    enemiesGridView.SetItems(prefabs);
                }

                if (chestsGridView.ItemsCount == 0)
                {
                    var prefabs = ChestsDatabase.GetAllChestsPrefabs();
                    chestsGridView.SetItems(prefabs);
                }
            }

            isGridsInited = true;
        }

        #region GUI

        public virtual void Draw(SerializedProperty waveProperty, UnityAction<GameObject> spawnEnemy, UnityAction<GameObject> spawnChest)
        {
            if (titleTextStyle == null) titleTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };
            if (gridTextStyle == null) gridTextStyle = new GUIStyle(EditorStyles.label) { fontSize = 14 };

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Wave Settings", StageCreatorWindow.HeaderTextStyle, GUILayout.ExpandWidth(false), GUILayout.Width(140));
            DrawGroupDropDown(waveProperty);
            DrawRoomMusicProperty(waveProperty);
            DrawCustomEndWaveDelayProperty(waveProperty);
            DrawTestWaveButton();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            SerializedPropertyExtensions.DrawVerticalSeparator(1);
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Progression", StageCreatorWindow.HeaderTextStyle, GUILayout.ExpandWidth(false), GUILayout.Width(150));

            DrawRewards(waveProperty);

            //DrawXPProperty(waveProperty);
            //DrawGoldProperty(waveProperty);
            //DrawGoldAndXPInfo(waveProperty);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            var icon = (Sprite)waveProperty.FindPropertyRelative("icon").objectReferenceValue;

            var iconItems = iconsGridView.GetItems();

            for (int i = 0; i < iconItems.Count; i++)
            {
                if (icon == null)
                {
                    icon = iconItems[0].Source;
                    waveProperty.FindPropertyRelative("icon").objectReferenceValue = icon;
                    iconItems[0].IsSelected = true;
                    continue;
                }
                iconItems[i].IsSelected = iconItems[i].Source == icon;
            }

            GUILayout.Label(new GUIContent("Icons"), gridTextStyle);

            iconsGridView.Draw(60, new Rect(0, 0, 45, 45), 5, 5, (icon) =>
            {
                waveProperty.FindPropertyRelative("icon").objectReferenceValue = icon;
                for (int i = 0; i < iconItems.Count; i++)
                {
                    iconItems[i].IsSelected = iconItems[i].Source == icon;
                }
            });

            if (!isGridsInited)
            {
                if (EnemiesSpawner == null)
                {
                    EditorGUILayout.LabelField("Enemies Spawner not found in the scene.");
                }
                else
                {
                    if (EnemiesDatabase == null)
                    {
                        EditorGUILayout.LabelField("Enemies Database not assigned in Enemies Spawner.");
                    }
                    else
                    {
                        InitPrefabGrids();
                    }
                }
            }

            if (isGridsInited)
            {
                GUILayout.Space(5f);
                GUILayout.Label(new GUIContent("Enemies"), gridTextStyle);
                enemiesGridView.Draw(220, new Rect(0, 0, 60, 60), 3, 3, spawnEnemy);
                GUILayout.Space(5f);
                GUILayout.Label(new GUIContent("Chests"), gridTextStyle);
                chestsGridView.Draw(65, new Rect(0, 0, 55, 55), 3, 3, spawnChest);
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }

        Vector2 rewardsScrollPosition;

        protected virtual void DrawRewards(SerializedProperty waveProperty)
        {
            rewardsScrollPosition = EditorGUILayout.BeginScrollView(rewardsScrollPosition, GUI.skin.box, GUILayout.Width(210), GUILayout.Height(100));
            rewardsScrollPosition.x = 0;

            var waveDropExperienceProperty = waveProperty.FindPropertyRelative("waveDropExpericence");
            waveDropExperienceProperty.floatValue = DrawReward("XP", "Level: {0}", waveDropExperienceProperty.floatValue, StageCreatorWindow.Instance.StagePage.CalculateExperienceLevel(waveProperty));

            var currencies = new Dictionary<string, string>();
            if (CurrencyCache.Database != null)
            {
                for (int i = 0; i < CurrencyCache.Database.Count; i++)
                {
                    var data = CurrencyCache.Database.GetCurrency(i);
                    currencies.Add(data.ID, data.Name);
                }
            } else
            {
                currencies.Add("gold", "Gold");
            }

            foreach(var id in currencies.Keys)
            {
                var hasReward = GetReward(waveProperty, id, out var amountProperty);
                var value = amountProperty != null ? amountProperty.intValue : 0;
                value = DrawReward(currencies[id], "Total: {0}", value, StageCreatorWindow.Instance.StagePage.CalculateCurrency(waveProperty, id));
                if (value < 0) value = 0;

                if(amountProperty != null)
                {
                    amountProperty.intValue = value;
                } else if(value > 0)
                {
                    AddReward(waveProperty, id, value);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual bool GetReward(SerializedProperty waveProperty, string id, out SerializedProperty amountProperty)
        {
            amountProperty = null;

            var rewardsProperty = waveProperty.FindPropertyRelative("rewards");
            for (int i = 0; i < rewardsProperty.arraySize; i++)
            {
                var rewardProperty = rewardsProperty.GetArrayElementAtIndex(i);

                var currencyIdProperty = rewardProperty.FindPropertyRelative("currencyId");
                if(currencyIdProperty.stringValue == id)
                {
                    amountProperty = rewardProperty.FindPropertyRelative("amount");
                    return true;
                }
            }

            return false;
        }

        protected virtual void AddReward(SerializedProperty waveProperty, string id, int amount)
        {
            var rewardsProperty = waveProperty.FindPropertyRelative("rewards");
            rewardsProperty.arraySize++;

            var newReward = rewardsProperty.GetLastArrayElement();
            var currencyIdProperty = newReward.FindPropertyRelative("currencyId");
            var amountProperty = newReward.FindPropertyRelative("amount");

            currencyIdProperty.stringValue = id;
            amountProperty.intValue = amount;
        }

        protected virtual float DrawReward(string name, string totalFormat, float value, float total)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent(name);
            EditorGUILayout.LabelField(label, GUILayout.Width(60));
            var result = EditorGUILayout.FloatField(value, GUILayout.Width(40));

            var totalText = string.Format(totalFormat, total);
            var totalLabel = new GUIContent(totalText, totalText);

            EditorGUILayout.LabelField(totalLabel, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();

            return result;
        }

        protected virtual int DrawReward(string name, string totalFormat, int value, int total)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent(name);
            EditorGUILayout.LabelField(label, GUILayout.Width(60));
            var result = EditorGUILayout.IntField(value, GUILayout.Width(40));

            var totalText = string.Format(totalFormat, total);
            var totalLabel = new GUIContent(totalText, totalText);

            EditorGUILayout.LabelField(totalLabel, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();

            return result;
        }

        protected virtual void DrawTestWaveButton()
        {
            GUI.enabled = DefaultSceneLoaderActions.Enabled;
            var tooltip = GUI.enabled ?
                "Start playmode with and automatically enter selected stage" :
                "Default Scene Loader is disabled. Starting play mode will not load necessary systems and will break the game. \n" +
                "To enable Default Scene Loader, use 'Tools -> October -> Default Scene Loader Enabled'";

            if (GUILayout.Button(new GUIContent("Test Wave", tooltip), GUILayout.Width(143)))
            {
                var saveDatabase = SaveManager.GetSaveDatabaseEditor();
                saveDatabase.Init();

                var save = saveDatabase.GetSave<StageSave>("Stage");
                save.Init();

                save.SetTestingData(StageCreatorWindow.Instance.StagePage.StageIndex, StageCreatorWindow.Instance.StagePage.RoomIndex, StageCreatorWindow.Instance.StagePage.WaveIndex);
                saveDatabase.Flush();

                SaveManager.SaveSaveDatabaseEditor(saveDatabase);

                StageCreatorWindow.Instance.StagePage.SaveScene();
                StageCreatorWindow.Instance.StagePage.ClearScene(true);

                StageCreatorWindow.Instance.IsTesting = true;

                EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
            }

            GUI.enabled = true;
        }
        
        protected virtual void DrawXPProperty(SerializedProperty waveProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("XP Received", "All experience dropped by enemies in this wave");
            EditorGUILayout.LabelField(label, GUILayout.Width(90));
            var waveDropExperienceProperty = waveProperty.FindPropertyRelative("waveDropExpericence");
            EditorGUILayout.PropertyField(waveDropExperienceProperty, new GUIContent(""), GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawGoldProperty(SerializedProperty waveProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Gold Received", "All gold dropped by enemies in this wave");
            EditorGUILayout.LabelField(label, GUILayout.Width(90));
            var waveDropGoldProperty = waveProperty.FindPropertyRelative("waveDropGold");
            EditorGUILayout.PropertyField(waveDropGoldProperty, new GUIContent(""), GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawCustomEndWaveDelayProperty(SerializedProperty waveProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("End Delay", "Overrides room's 'Wave Delay' for this wave");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));

            var customWaveEndDelayProperty = waveProperty.FindPropertyRelative("customWaveEndDelay");
            bool enabled = customWaveEndDelayProperty.floatValue >= 0f;
            var newEnabled = EditorGUILayout.Toggle(enabled);

            if (newEnabled != enabled)
            {
                if (newEnabled)
                {
                    var roomProperty = StageCreatorWindow.Instance.StagePage.RoomsListContainer.SelectedRoom.RoomProperty;
                    customWaveEndDelayProperty.floatValue = roomProperty.FindPropertyRelative("delayBetweenWaves").floatValue;
                }
                else
                {
                    customWaveEndDelayProperty.floatValue = -1f;
                }
            }

            if (newEnabled)
            {
                EditorGUILayout.PropertyField(customWaveEndDelayProperty, GUIContent.none, GUILayout.Width(50));
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawGroupDropDown(SerializedProperty waveProperty)
        {
            var groupIdProperty = waveProperty.FindPropertyRelative("groupId");

            var groups = new List<string> { "None", "Group 1", "Group 2", "Group 3", "Group 4", "Group 5", "Group 6", "Group 7", "Group 8", "Group 9", "Group 10" };

            if (groupIdProperty.intValue < 0) groupIdProperty.intValue = 0;

            int selectedIndex = groupIdProperty.intValue;

            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Group Id", "Waves with the same group id will be randomly shuffeled when playing");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));
            selectedIndex = EditorGUILayout.Popup(selectedIndex, groups.ToArray(), GUILayout.Width(70));
            groupIdProperty.intValue = selectedIndex;
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawRoomMusicProperty(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Wave Music", "If assigned, this music will start playing at the start of this wave");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));
            var roomMusicProperty = roomProperty.FindPropertyRelative("waveMusic");
            EditorGUILayout.PropertyField(roomMusicProperty, new GUIContent(""), GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}