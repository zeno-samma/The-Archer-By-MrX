using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class WavesListContainer
    {
        public ReorderableListView<WaveListItem> WavesListView { get; protected set; }

        public event UnityAction onBeforeDragPerformed;
        public event UnityAction onAfterDragPerformed;
        public event UnityAction onCreatedWave;
        public event UnityAction onDuplicatedWave;
        public event UnityAction onDeletedWave;

        public event UnityAction<WaveListItem, WaveListItem> onWaveSelected;

        public SerializedProperty RoomProperty { get; protected set; }

        protected Vector2 scrollPosition;

        public WavesListContainer(SerializedProperty roomProperty, int selectedId)
        {
            RoomProperty = roomProperty;
            var wavesProperty = roomProperty.FindPropertyRelative("waves");
            if (wavesProperty.arraySize < 1) wavesProperty.arraySize = 1;
            WavesListView = new ReorderableListView<WaveListItem>(StageCreatorWindow.Instance, wavesProperty);
            WavesListView.beforeSwap += InvokeOnBeforeDragPerformed;
            WavesListView.afterSwap += InvokeOnAfterDragPerformed;
            WavesListView.onItemSelected += InvokeOnWaveSelected;
            WavesListView.CanDeselect = false;

            if (selectedId > -1 && selectedId < WavesListView.Count)
            {
                WavesListView.Select(selectedId);
            }
            else
            {
                WavesListView.Select(0);
            }
        }

        public virtual void Clear()
        {
            WavesListView.beforeSwap -= InvokeOnBeforeDragPerformed;
            WavesListView.afterSwap -= InvokeOnAfterDragPerformed;
            WavesListView.onItemSelected -= InvokeOnWaveSelected;

            WavesListView.Clear();
        }

        protected virtual void InvokeOnBeforeDragPerformed()
        {
            onBeforeDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnAfterDragPerformed()
        {
            onAfterDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnWaveSelected(WaveListItem selectedWave, WaveListItem prevSelectedWave)
        {
            onWaveSelected?.Invoke(selectedWave, prevSelectedWave);
            StageCreatorWindow.Instance.LastSelectedWaveId = selectedWave.Index;
        }

        public virtual void Draw()
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");
            if (wavesProperty.arraySize < 1) CreateWave();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            var height = WavesListView.GetListHeight();
            if (height + 235 > StageCreatorWindow.Instance.position.height) height = StageCreatorWindow.Instance.position.height - 235;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.box, GUILayout.Width(200), GUILayout.Height(height));
            scrollPosition.x = 0;

            if (WavesListView != null) WavesListView.Draw();

            EditorGUILayout.EndScrollView();

            RoomProperty.serializedObject.ApplyModifiedProperties();

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
                CreateWave();
            }

            GUI.enabled = WavesListView.SelectedItem != null;
            if (GUI.Button(cloneButtonRect, "Clone"))
            {
                DuplicateWave(WavesListView.SelectedItem.WaveProperty);
            }

            if (GUI.Button(deleteButtonRect, "Delete"))
            {
                DeleteWave(WavesListView.SelectedItem.WaveProperty);
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        public void DuplicateWave(SerializedProperty copyFrom)
        {
            StageCreatorWindow.Instance.StagePage.SaveScene();

            var wavesProperty = RoomProperty.FindPropertyRelative("waves");
            wavesProperty.arraySize++;

            var copyTo = wavesProperty.GetArrayElementAtIndex(wavesProperty.arraySize - 1);

            var waveData = copyFrom.GetValue<WaveData>();
            waveData.SaveToSerializedProperty(copyTo);

            RoomProperty.serializedObject.ApplyModifiedProperties();
            RoomProperty.serializedObject.Update();

            WavesListView.RefreshList();

            onDuplicatedWave?.Invoke();

            WavesListView.RefreshList();
            WavesListView.SelectLast();
        }

        public void DeleteWave(SerializedProperty waveProperty)
        {
            if (WavesListView.Count <= 1)
            {
                Debug.LogWarning("You cannot delete the last wave in the room.");
                return;
            }

            var wavesProperty = RoomProperty.FindPropertyRelative("waves");

            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                var currentWaveProperty = wavesProperty.GetArrayElementAtIndex(i);
                if (currentWaveProperty.propertyPath == waveProperty.propertyPath)
                {
                    wavesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            RoomProperty.serializedObject.ApplyModifiedProperties();
            RoomProperty.serializedObject.Update();

            onDeletedWave?.Invoke();

            WavesListView.RefreshList();
            WavesListView.Select(0);
        }

        public void CreateWave(bool refreshList = true)
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");
            wavesProperty.arraySize++;

            var waveProperty = wavesProperty.GetArrayElementAtIndex(wavesProperty.arraySize - 1);
            new WaveData().SaveToSerializedProperty(waveProperty);

            RoomProperty.serializedObject.ApplyModifiedProperties();
            RoomProperty.serializedObject.Update();

            if (refreshList)
            {
                WavesListView.RefreshList();

                WavesListView.SelectLast();
            }

            onCreatedWave?.Invoke();
        }
    }
}