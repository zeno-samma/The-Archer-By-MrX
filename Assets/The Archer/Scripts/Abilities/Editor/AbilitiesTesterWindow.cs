using OctoberStudio.StageCreator;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class AbilitiesTesterWindow : EditorWindow
    {
        public static AbilitiesTesterWindow Instance { get; protected set; }

        [SerializeField] protected Texture2D dragHandleTexture;
        public Texture2D DragHandleTexture => dragHandleTexture;

        protected static readonly string TESTING_DATABASE_GUID_KEY = "AbilitiesTesterWindow: Testing Database GUID";

        protected bool isPresetsOpened = true;

        protected SerializedObject testingDatabaseObject;
        protected TestingDatabase testingDatabase;
        protected int testingDatabasesCount;

        protected GUIStyle selectedLabelStyle;
        protected GUIStyle selectedButtonStyle;

        protected TestingPresetsListContainer testingPresetsListContainer;

        [MenuItem("Tools/October/Abilities Tester #a")]
        public static void OpenWindow()
        {
            AbilitiesTesterWindow wnd = GetWindow<AbilitiesTesterWindow>();
            var content = new GUIContent("Abilities Tester");

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Common/Sprites/Editor/editor_ab_icon.png");
            if (icon != null)
            {
                content.image = icon;
            }

            wnd.titleContent = content;
            wnd.minSize = new Vector2(600, 400);
        }

        protected virtual void OnEnable()
        {
            Instance = this;

            FindTestingDatabase();

            isPresetsOpened = !Application.isPlaying;
        }

        protected virtual void OnGUI()
        {
            if (!OnTestingDatabaseGUI()) return;

            var abilityTypes = new List<AbilityType>((AbilityType[])System.Enum.GetValues(typeof(AbilityType)));

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isPresetsOpened;
            if (GUILayout.Button("Presets"))
            {
                isPresetsOpened = true;
            }
            GUI.enabled = true;

            GUI.enabled = isPresetsOpened;
            if (GUILayout.Button("Gameplay"))
            {
                isPresetsOpened = false;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (isPresetsOpened)
            {
                OnPresetsGUI();
            }
            else
            {
                OnGameplayGUI();
            }

            testingDatabaseObject.ApplyModifiedProperties();
        }

        protected virtual bool OnTestingDatabaseGUI()
        {
            if (testingDatabase == null)
            {
                FindTestingDatabase();

                EditorGUILayout.LabelField("There are no testing databases in the project.", EditorStyles.helpBox);
                return false;
            }

            GUI.enabled = testingDatabasesCount > 1;
            var newTestingDatabase = (TestingDatabase)EditorGUILayout.ObjectField("Testing Database", testingDatabase, typeof(TestingDatabase), false);

            if (newTestingDatabase != testingDatabase && newTestingDatabase != null)
            {
                testingDatabase = newTestingDatabase;
                testingDatabaseObject = new SerializedObject(testingDatabase);

                string path = AssetDatabase.GetAssetPath(testingDatabase);
                string guid = AssetDatabase.AssetPathToGUID(path);

                EditorPrefs.SetString(TESTING_DATABASE_GUID_KEY, guid);
            }

            GUI.enabled = true;

            return true;
        }

        protected virtual void FindTestingDatabase()
        {
            if (EditorPrefs.HasKey(TESTING_DATABASE_GUID_KEY))
            {
                var guid = EditorPrefs.GetString(TESTING_DATABASE_GUID_KEY);
                testingDatabase = AssetDatabase.LoadAssetAtPath<TestingDatabase>(AssetDatabase.GUIDToAssetPath(guid));
                if (testingDatabase != null)
                {
                    testingDatabaseObject = new SerializedObject(testingDatabase);
                }
            }

            string[] guiID = AssetDatabase.FindAssets("t:TestingDatabase");
            var databases = new List<TestingDatabase>();

            if (guiID != null)
            {
                for (int i = 0; i < guiID.Length; i++)
                {
                    var database = AssetDatabase.LoadAssetAtPath<TestingDatabase>(AssetDatabase.GUIDToAssetPath(guiID[i]));

                    if (database != null)
                    {
                        databases.Add(database);
                    }
                }
            }
            if (testingDatabase == null && databases.Count > 0)
            {
                testingDatabase = databases[0];
                testingDatabaseObject = new SerializedObject(testingDatabase);

                string path = AssetDatabase.GetAssetPath(testingDatabase);
                string guid = AssetDatabase.AssetPathToGUID(path);

                EditorPrefs.SetString(TESTING_DATABASE_GUID_KEY, guid);
            }

            testingDatabasesCount = databases.Count;
        }

        protected Vector2 scrollPosition;

        protected virtual void OnPresetsGUI()
        {
            var presetsProperty = testingDatabaseObject.FindProperty("presets");

            if (testingPresetsListContainer == null)
            {
                testingPresetsListContainer = new TestingPresetsListContainer(this, presetsProperty, 0);
            }

            EditorGUILayout.BeginHorizontal();

            testingPresetsListContainer.Draw();

            var selectedPreset = testingPresetsListContainer.PresetsListView.SelectedItem;

            if (selectedPreset != null)
            {
                var presetProperty = selectedPreset.TestingPresetProperty;

                var nameProperty = presetProperty.FindPropertyRelative("name");
                var enabledInEditorProperty = presetProperty.FindPropertyRelative("enabledInEditor");
                var enabledInBuildProperty = presetProperty.FindPropertyRelative("enabledInBuild");

                var abilitiesProperty = presetProperty.FindPropertyRelative("abilities");

                EditorGUILayout.BeginVertical();

                EditorGUILayout.PropertyField(nameProperty);
                EditorGUILayout.PropertyField(enabledInEditorProperty);
                EditorGUILayout.PropertyField(enabledInBuildProperty);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                scrollPosition.x = 0;
                EditorGUILayout.PropertyField(abilitiesProperty);
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected string filterText = "";
        protected int selectedAbilityId;

        protected Filter selectedTab = 0;

        protected virtual void OnGameplayGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This page only works in playmode", MessageType.Warning);
                return;
            }

            var manager = Object.FindAnyObjectByType<AbilitiesManager>();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("There are no AbilitiesManager Component in the Hierarchy", MessageType.Warning);
                return;
            }

            var abilities = manager.GetAllAbilitiesDev();

            GUILayout.Space(2);

            SerializedPropertyExtensions.DrawHorizontalSeparator(2);

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = selectedTab != Filter.All;
            if (GUILayout.Button($"All"))
            {
                selectedTab = Filter.All;
            }

            GUI.enabled = selectedTab != Filter.Acquired;
            if (GUILayout.Button($"Acquired"))
            {
                selectedTab = Filter.Acquired;
            }

            GUI.enabled = selectedTab != Filter.NotAcquired;
            if (GUILayout.Button($"Not Acquired"))
            {
                selectedTab = Filter.NotAcquired;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            filterText = EditorGUILayout.TextField("Search:", filterText);

            EditorGUILayout.BeginHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(250), GUILayout.Height(250));
            scrollPosition.x = 0;

            int counter = 0;
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];

                if (ability.Title.ToLower().Contains(filterText.ToLower()))
                {
                    if (selectedTab != Filter.All)
                    {
                        int level = manager.GetAbilityLevelDev(ability.AbilityType);

                        if (selectedTab == Filter.Acquired && level == -1) continue;
                        if (selectedTab == Filter.NotAcquired && level != -1) continue;
                    }

                    var rect = EditorGUILayout.GetControlRect();
                    if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        selectedAbilityId = i;
                        Event.current.Use();
                    }

                    if (selectedAbilityId == i)
                    {
                        EditorGUI.DrawRect(rect, Color.grey);

                    }

                    GUI.Label(rect, $"{counter + 1}. {ability.Title}", GUI.skin.label);
                    counter++;
                }
            }

            EditorGUILayout.EndScrollView();

            if (selectedAbilityId >= 0 && selectedAbilityId < abilities.Count)
            {
                var selectedAbility = abilities[selectedAbilityId];

                EditorGUILayout.BeginVertical();

                int level = manager.GetAbilityLevelDev(selectedAbility.AbilityType);

                if (level == -1)
                {
                    EditorGUILayout.LabelField($"Status: Disabled");

                    if (GUILayout.Button($"Activate {selectedAbility.Title} ability"))
                    {
                        manager.AddAbility(selectedAbility, 0);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"Status: Active, Level: {level + 1} out of {selectedAbility.LevelsCount}");

                    if (GUILayout.Button($"Disable {selectedAbility.Title} ability"))
                    {
                        manager.RemoveAbility(selectedAbility);
                    }

                    if (level > 0)
                    {
                        if (GUILayout.Button($"Decrease ability level"))
                        {
                            manager.DecreaseAbilityLevel(selectedAbility);
                        }
                    }

                    if (level < selectedAbility.LevelsCount - 1)
                    {
                        if (GUILayout.Button($"Increase ability level"))
                        {
                            manager.IncreaseAbilityLevel(selectedAbility);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual Texture2D MakeSelectedButtonBackgroundTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var backgroundTexture = new Texture2D(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }

        protected enum Filter
        {
            All,
            Acquired,
            NotAcquired
        }
    }
}