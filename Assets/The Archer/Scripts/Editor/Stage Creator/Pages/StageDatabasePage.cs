using System.IO;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class StageDatabasePage
    {
        public SerializedObject StageDatabaseSerializedObject { get; protected set; }

        protected SerializedProperty stagesProperty;
        protected SerializedProperty StagesProperty
        {
            get
            {
                if (stagesProperty == null)
                {
                    if (StageDatabaseSerializedObject == null) return null;
                    stagesProperty = StageDatabaseSerializedObject.FindProperty("stages");
                    ValidateStages();
                }
                return stagesProperty;
            }
        }

        protected ReorderableListView<StageListItem> stagesListView;
        protected ReorderableListView<StageListItem> StagesListView
        {
            get
            {
                if (stagesListView == null)
                {
                    stagesListView = new ReorderableListView<StageListItem>(StageCreatorWindow.Instance, StagesProperty);
                    stagesListView.onItemDoubleClicked += OnStageDoubleClicked;

                    StagesListView.Select(0);
                }
                return stagesListView;
            }
        }

        protected StageUnlockConditionsList UnlockConditionsList { get; set; }

        public SerializedObject GetStageObject(int index)
        {
            return StagesListView[index].StageObject;
        }

        protected Vector2 scrollPosition = Vector2.zero;

        public SerializedObject DoubleClickedStageObject { get; protected set; }
        public int DoubleClickedStageIndex { get; protected set; }

        protected GUIStyle stageTextStyle;
        protected GUIStyle openButtonStyle;

        public StageDatabasePage(SerializedObject stageDatabaseSerializedObject)
        {
            StageDatabaseSerializedObject = stageDatabaseSerializedObject;
        }

        protected virtual void OnStageDoubleClicked(StageListItem stageObject)
        {
            DoubleClickedStageObject = stageObject.StageObject;
            DoubleClickedStageIndex = stageObject.Index;
            StageCreatorWindow.Instance.OpenPage(StageCreatorWindow.PageType.Stage);
        }

        protected virtual void ValidateStages()
        {
            var changed = false;
            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                var stageProperty = stagesProperty.GetArrayElementAtIndex(i);

                if (stageProperty.objectReferenceValue == null)
                {
                    stagesProperty.DeleteArrayElementAtIndex(i);
                    i--;
                    changed = true;
                }
            }

            if (changed)
            {
                stagesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        public virtual void Open()
        {
            StagesListView.Select(0);
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();

            DrawStagesHierarchyGUI();

            if (StagesListView.SelectedItem != null)
            {
                EditorGUILayout.BeginVertical();

                if (StagesListView.SelectedItem != null)
                {
                    DrawStageData(StagesListView.SelectedItem);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawStageData(StageListItem selectedStage)
        {
            if (stageTextStyle == null) stageTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 20 };
            if (openButtonStyle == null) openButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 50, fixedWidth = 100 };

            var stageSerializedObject = selectedStage.StageObject;

            var stageNameProperty = stageSerializedObject.FindProperty("stageName");
            var stageImageProperty = stageSerializedObject.FindProperty("stageImage");
            var showStageObjectiveProperty = stageSerializedObject.FindProperty("showStageObjective");
            var stageObjectiveProperty = stageSerializedObject.FindProperty("stageObjective");
            var musicProperty = stageSerializedObject.FindProperty("stageMusic");
            var showAbilitySelectorProperty = stageSerializedObject.FindProperty("showAbilitySelector");
            var abilityRerollPriceProperty = stageSerializedObject.FindProperty("abilityRerollPrice");
            var nextAbilityRerollMultiplierProperty = stageSerializedObject.FindProperty("nextAbilityRerollMultiplier");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Edit", openButtonStyle))
            {
                OnStageDoubleClicked(StagesListView.SelectedItem);
            }

            GUILayout.Space(10f);

            EditorGUILayout.LabelField($"Stage {StagesListView.SelectedItem.Index + 1} - {stageNameProperty.stringValue}", stageTextStyle, GUILayout.Height(50));

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            var nameLabel = new GUIContent("Stage Name");
            GUILayout.Label(nameLabel, GUILayout.Width(100));
            EditorGUILayout.PropertyField(stageNameProperty, new GUIContent(), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var useObjectiveLabel = new GUIContent("Show Objective");
            GUILayout.Label(useObjectiveLabel, GUILayout.Width(100));
            EditorGUILayout.PropertyField(showStageObjectiveProperty, new GUIContent(), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var objectiveLabel = new GUIContent("Stage Objective");
            GUILayout.Label(objectiveLabel, GUILayout.Width(100));
            stageObjectiveProperty.stringValue = GUILayout.TextArea(stageObjectiveProperty.stringValue, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var imageLabel = new GUIContent("Stage Image");
            GUILayout.Label(imageLabel, GUILayout.Width(100));
            EditorGUILayout.PropertyField(stageImageProperty, new GUIContent(), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var musicLabel = new GUIContent("Stage Music");
            GUILayout.Label(musicLabel, GUILayout.Width(100));
            EditorGUILayout.PropertyField(musicProperty, new GUIContent(), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var abilitiesLabel = new GUIContent("Use Start Ability");
            GUILayout.Label(abilitiesLabel, GUILayout.Width(100));
            EditorGUILayout.PropertyField(showAbilitySelectorProperty, new GUIContent(), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            DrawMultipliers(stageSerializedObject);

            GUILayout.Space(10f);

            DrawAbilitiesRerollSettings(stageSerializedObject);

            GUILayout.Space(10f);

            if (UnlockConditionsList == null || UnlockConditionsList.SerializedObject.targetObject != stageSerializedObject.targetObject)
            {
                UnlockConditionsList = new StageUnlockConditionsList(stageSerializedObject);
            }

            UnlockConditionsList.Draw();

            stageSerializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawAbilitiesRerollSettings(SerializedObject serializedObject)
        {
            var abilityRerollPriceProperty = serializedObject.FindProperty("abilityRerollPrice");
            var nextAbilityRerollMultiplierProperty = serializedObject.FindProperty("nextAbilityRerollMultiplier");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Abilities Reroll Price", GUILayout.Width(120));
            EditorGUILayout.PropertyField(abilityRerollPriceProperty, GUIContent.none, GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Next Reroll Multiplier", GUILayout.Width(120));
            EditorGUILayout.PropertyField(nextAbilityRerollMultiplierProperty, GUIContent.none, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawMultipliers(SerializedObject stageSerializedObject)
        {
            var enemyDamageMultiplier = stageSerializedObject.FindProperty("enemyDamageMulitplier");
            var enemyDamageMultiplierRoomStep = stageSerializedObject.FindProperty("enemyDamageMultiplierRoomStep");
            var enemyDamageMultiplierWaveStep = stageSerializedObject.FindProperty("enemyDamageMultiplierWaveStep");

            if (enemyDamageMultiplier.floatValue < 0f) enemyDamageMultiplier.floatValue = 0f;
            if (enemyDamageMultiplierRoomStep.floatValue < 0f) enemyDamageMultiplierRoomStep.floatValue = 0f;
            if (enemyDamageMultiplierWaveStep.floatValue < 0f) enemyDamageMultiplierWaveStep.floatValue = 0f;

            var enemyHPMultiplier = stageSerializedObject.FindProperty("enemyHPMulitplier");
            var enemyHPMultiplierRoomStep = stageSerializedObject.FindProperty("enemyHPMultiplierRoomStep");
            var enemyHPMultiplierWaveStep = stageSerializedObject.FindProperty("enemyHPMultiplierWaveStep");

            if (enemyHPMultiplier.floatValue < 0f) enemyHPMultiplier.floatValue = 0f;
            if (enemyHPMultiplierRoomStep.floatValue < 0f) enemyHPMultiplierRoomStep.floatValue = 0f;
            if (enemyHPMultiplierWaveStep.floatValue < 0f) enemyHPMultiplierWaveStep.floatValue = 0f;

            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Enemy Damage Multiplier", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(enemyDamageMultiplier, new GUIContent("Stage Multiplier"));
            EditorGUILayout.PropertyField(enemyDamageMultiplierRoomStep, new GUIContent("Multiplier Room Step"));
            EditorGUILayout.PropertyField(enemyDamageMultiplierWaveStep, new GUIContent("Multiplier Wave Step"));

            EditorGUILayout.EndVertical();

            GUILayout.Space(10f);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Enemy HP Multiplier", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(enemyHPMultiplier, new GUIContent("Stage Multiplier"));
            EditorGUILayout.PropertyField(enemyHPMultiplierRoomStep, new GUIContent("Multiplier Room Step"));
            EditorGUILayout.PropertyField(enemyHPMultiplierWaveStep, new GUIContent("Multiplier Wave Step"));

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        protected void DrawStagesHierarchyGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.box, GUILayout.Width(300), GUILayout.Height(StagesListView.GetListHeight()));
            scrollPosition.x = 0;

            StagesListView.Draw();

            EditorGUILayout.EndScrollView();

            StageDatabaseSerializedObject.ApplyModifiedProperties();

            var buttonsRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

            var buttonWidth = (300 - 20) / 3f;

            var createButtonRect = new Rect(buttonsRect);
            createButtonRect.x += 5f;
            createButtonRect.width = buttonWidth;

            var cloneButtonRect = new Rect(createButtonRect);
            cloneButtonRect.x += buttonWidth + 5f;

            var deleteButtonRect = new Rect(cloneButtonRect);
            deleteButtonRect.x += buttonWidth + 5f;

            if (GUI.Button(createButtonRect, "Create"))
            {
                CreateStage();
            }

            GUI.enabled = StagesListView.SelectedItem != null;
            if (GUI.Button(cloneButtonRect, "Duplicate"))
            {
                DuplicateStageAsset(StagesListView.SelectedItem.StageObject);
            }

            if (GUI.Button(deleteButtonRect, "Delete"))
            {
                DeleteStageAsset(StagesListView.SelectedItem.StageObject);
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        public void DuplicateStageAsset(SerializedObject copyFrom)
        {
            var newStageData = CreateStageAsset(); ;
            StagesProperty.arraySize++;
            StagesProperty.GetArrayElementAtIndex(StagesProperty.arraySize - 1).objectReferenceValue = newStageData;

            StageDatabaseSerializedObject.ApplyModifiedProperties();
            StageDatabaseSerializedObject.Update();

            var copyTo = new SerializedObject(newStageData);

            copyTo.CopyFromSerializedObject(copyFrom);

            copyTo.ApplyModifiedProperties();

            StagesListView.RefreshList();
            StagesListView.SelectLast();
        }

        public void DeleteStageAsset(SerializedObject stageObject)
        {
            if (StagesListView.Count <= 1)
            {
                Debug.LogWarning("Cannot delete the last stage asset.");
                return;
            }

            var stageAsset = stageObject.targetObject;

            var path = AssetDatabase.GetAssetPath(stageAsset);

            for (int i = 0; i < StagesProperty.arraySize; i++)
            {
                if (StagesProperty.GetArrayElementAtIndex(i).objectReferenceValue == stageAsset)
                {
                    StagesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();

            StagesListView.Deselect();

            StageDatabaseSerializedObject.ApplyModifiedProperties();

            StagesListView.RefreshList();
        }

        public void CreateStage()
        {
            StagesProperty.arraySize++;
            StagesProperty.GetArrayElementAtIndex(StagesProperty.arraySize - 1).objectReferenceValue = CreateStageAsset();

            StageDatabaseSerializedObject.ApplyModifiedProperties();
            StageDatabaseSerializedObject.Update();

            StagesListView.RefreshList();
            StagesListView.SelectLast();
        }

        protected virtual StageData CreateStageAsset()
        {
            var baseFileName = "Stage Data";
            var folderPath = "Assets/The Archer/Scriptables/Stages";
            var extension = ".asset";

            var stageFileName = GetAvailableAssetName(folderPath, baseFileName, extension);

            // Create instance
            var asset = ScriptableObject.CreateInstance<StageData>();

            // Save to disk
            var fullPath = Path.Combine(folderPath, stageFileName + extension);
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }

        public string GetAvailableAssetName(string folderPath, string baseName, string extension)
        {
            var index = 1;
            string formattedName;
            string fullPath;
            do
            {
                formattedName = $"{baseName} {index:000}"; // e.g. Name 001
                fullPath = Path.Combine(folderPath, formattedName + extension);
                index++;
            } while (File.Exists(fullPath));

            return formattedName;
        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {

        }
    }
}