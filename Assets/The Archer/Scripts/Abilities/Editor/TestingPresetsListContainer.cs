using OctoberStudio.Abilities;
using OctoberStudio.StageCreator;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class TestingPresetsListContainer
    {
        public ReorderableListView<TestingPresetListItem> PresetsListView { get; protected set; }

        public event UnityAction onBeforeDragPerformed;
        public event UnityAction onAfterDragPerformed;
        public event UnityAction onCreatedPreset;
        public event UnityAction onDuplicatedPreset;
        public event UnityAction onDeletedPreset;

        public event UnityAction<TestingPresetListItem, TestingPresetListItem> onPresetSelected;

        public SerializedProperty PresetsProperty { get; protected set; }

        protected Vector2 scrollPosition;

        public TestingPresetsListContainer(AbilitiesTesterWindow window, SerializedProperty presetsProperty, int selectedId)
        {
            PresetsProperty = presetsProperty;
            if (PresetsProperty.arraySize < 1) PresetsProperty.arraySize = 1;
            PresetsListView = new ReorderableListView<TestingPresetListItem>(window, PresetsProperty);

            PresetsListView.beforeSwap += InvokeOnBeforeDragPerformed;
            PresetsListView.afterSwap += InvokeOnAfterDragPerformed;
            PresetsListView.onItemSelected += InvokeOnPresetSelected;
            PresetsListView.CanDeselect = false;

            if (selectedId > -1 && selectedId < PresetsListView.Count)
            {
                PresetsListView.Select(selectedId);
            }
            else
            {
                PresetsListView.Select(0);
            }
        }

        public virtual void Clear()
        {
            PresetsListView.beforeSwap -= InvokeOnBeforeDragPerformed;
            PresetsListView.afterSwap -= InvokeOnAfterDragPerformed;
            PresetsListView.onItemSelected -= InvokeOnPresetSelected;

            PresetsListView.Clear();
        }

        protected virtual void InvokeOnBeforeDragPerformed()
        {
            onBeforeDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnAfterDragPerformed()
        {
            onAfterDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnPresetSelected(TestingPresetListItem selectedPreset, TestingPresetListItem prevSelectedPreset)
        {
            onPresetSelected?.Invoke(selectedPreset, prevSelectedPreset);
        }

        public virtual void Draw()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            var height = PresetsListView.GetListHeight();

            if (height + 130 > AbilitiesTesterWindow.Instance.position.height) height = AbilitiesTesterWindow.Instance.position.height - 130;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.box, GUILayout.Width(200), GUILayout.Height(height));
            scrollPosition.x = 0;

            if (PresetsListView != null) PresetsListView.Draw();

            EditorGUILayout.EndScrollView();

            PresetsProperty.serializedObject.ApplyModifiedProperties();

            var buttonsRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

            var buttonWidth = (200 - 20) / 3f;

            var createButtonRect = new Rect(buttonsRect);
            createButtonRect.x += 5f;
            createButtonRect.width = buttonWidth;

            var cloneButtonRect = new Rect(createButtonRect);
            cloneButtonRect.x += buttonWidth + 5f;

            var deleteButtonRect = new Rect(cloneButtonRect);
            deleteButtonRect.x += buttonWidth + 5f;

            if (GUI.Button(createButtonRect, "Create"))
            {
                CreatePreset();
            }

            GUI.enabled = PresetsListView.SelectedItem != null;
            if (GUI.Button(cloneButtonRect, "Clone"))
            {
                DuplicatePreset(PresetsListView.SelectedItem.TestingPresetProperty);
            }

            if (GUI.Button(deleteButtonRect, "Delete"))
            {
                DeletePreset(PresetsListView.SelectedItem.TestingPresetProperty);
            }
            GUI.enabled = true;

            var clearButtonsRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

            var clearButtonWidth = (clearButtonsRect.width - 24) / 2f;

            var clearEditorButtonRect = new Rect(clearButtonsRect);
            clearEditorButtonRect.x += 5f;
            clearEditorButtonRect.width = clearButtonWidth;

            var clearBuildButtonRect = new Rect(clearEditorButtonRect);
            clearBuildButtonRect.x += clearButtonWidth + 5f;
            clearBuildButtonRect.width = clearButtonWidth;

            if (GUI.Button(clearEditorButtonRect, "Clear Editor"))
            {
                ClearEditor();
            }

            if (GUI.Button(clearBuildButtonRect, "Clear Build"))
            {
                ClearBuild();
            }

            EditorGUILayout.EndVertical();
        }

        public virtual void ClearEditor()
        {
            for (int i = 0; i < PresetsProperty.arraySize; i++)
            {
                var presetProperty = PresetsProperty.GetArrayElementAtIndex(i);

                presetProperty.FindPropertyRelative("enabledInEditor").boolValue = false;
            }
        }

        public virtual void ClearBuild()
        {
            for (int i = 0; i < PresetsProperty.arraySize; i++)
            {
                var presetProperty = PresetsProperty.GetArrayElementAtIndex(i);

                presetProperty.FindPropertyRelative("enabledInBuild").boolValue = false;
            }
        }

        public void DuplicatePreset(SerializedProperty copyFrom)
        {
            PresetsProperty.arraySize++;

            var copyTo = PresetsProperty.GetArrayElementAtIndex(PresetsProperty.arraySize - 1);

            var presetData = copyFrom.GetValue<TestingPreset>();
            presetData.SaveToSerializedProperty(copyTo);

            PresetsProperty.serializedObject.ApplyModifiedProperties();
            PresetsProperty.serializedObject.Update();

            PresetsListView.RefreshList();

            onDuplicatedPreset?.Invoke();

            PresetsListView.RefreshList();
            PresetsListView.SelectLast();
        }

        public void DeletePreset(SerializedProperty presetProperty)
        {
            if (PresetsListView.Count <= 1)
            {
                Debug.LogWarning("You cannot delete the last testing preset.");
                return;
            }

            for (int i = 0; i < PresetsProperty.arraySize; i++)
            {
                var currentPresetProperty = PresetsProperty.GetArrayElementAtIndex(i);
                if (currentPresetProperty.propertyPath == presetProperty.propertyPath)
                {
                    PresetsProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            PresetsProperty.serializedObject.ApplyModifiedProperties();
            PresetsProperty.serializedObject.Update();

            onDeletedPreset?.Invoke();

            PresetsListView.RefreshList();
            PresetsListView.Select(0);
        }

        public void CreatePreset(bool refreshList = true)
        {
            PresetsProperty.arraySize++;

            var presetProperty = PresetsProperty.GetArrayElementAtIndex(PresetsProperty.arraySize - 1);
            new TestingPreset().SaveToSerializedProperty(presetProperty);

            PresetsProperty.serializedObject.ApplyModifiedProperties();
            PresetsProperty.serializedObject.Update();

            if (refreshList)
            {
                PresetsListView.RefreshList();

                PresetsListView.SelectLast();
            }

            onCreatedPreset?.Invoke();
        }

    }
}