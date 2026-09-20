using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class RoomsListContainer
    {
        public StageMenuHeader StageMenuHeader { get; protected set; }
        public ReorderableListView<RoomListItem> RoomsListView { get; protected set; }
        public RoomListItem SelectedRoom => RoomsListView.SelectedItem;
        public SerializedObject StageSerializedObject { get; protected set; }

        protected Vector2 scrollPosition = Vector2.zero;

        public event UnityAction onBeforeDragPerformed;
        public event UnityAction onAfterDragPerformed;
        public event UnityAction onCreatedRoom;
        public event UnityAction onDuplicatedRoom;
        public event UnityAction onDeletedRoom;

        public event UnityAction<RoomListItem, RoomListItem> onRoomSelected;

        public RoomsListContainer(SerializedObject stagesSerialiezedObject, int selectedId)
        {
            StageSerializedObject = stagesSerialiezedObject;

            StageMenuHeader = new StageMenuHeader();
            StageMenuHeader.SetStageSerializedObject(stagesSerialiezedObject);
            var roomsProperty = StageSerializedObject.FindProperty("rooms");
            if (roomsProperty.arraySize < 1) roomsProperty.arraySize = 1;
            RoomsListView = new ReorderableListView<RoomListItem>(StageCreatorWindow.Instance, roomsProperty);
            RoomsListView.beforeSwap += InvokeOnBeforeDragPerformed;
            RoomsListView.afterSwap += InvokeOnAfterDragPerformed;
            RoomsListView.onItemSelected += InvokeOnRoomSelected;
            RoomsListView.CanDeselect = false;

            if (selectedId > -1 && selectedId < RoomsListView.Count)
            {
                RoomsListView.Select(selectedId);
            }
            else
            {
                RoomsListView.Select(0);
            }
        }

        protected virtual void InvokeOnBeforeDragPerformed()
        {
            onBeforeDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnAfterDragPerformed()
        {
            onAfterDragPerformed?.Invoke();
        }

        protected virtual void InvokeOnRoomSelected(RoomListItem selectedItem, RoomListItem prevSelectedItem)
        {
            onRoomSelected?.Invoke(selectedItem, prevSelectedItem);
            StageCreatorWindow.Instance.LastSelectedRoomId = selectedItem.Index;
        }

        public virtual void Clear()
        {
            RoomsListView.beforeSwap -= InvokeOnBeforeDragPerformed;
            RoomsListView.afterSwap -= InvokeOnAfterDragPerformed;
            RoomsListView.onItemSelected -= InvokeOnRoomSelected;

            RoomsListView.Clear();
        }

        public virtual bool Draw()
        {
            var roomsProperty = StageSerializedObject.FindProperty("rooms");
            if (roomsProperty.arraySize < 1) CreateRoom();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            var isBackButtonPressed = StageMenuHeader.Draw();

            var height = RoomsListView.GetListHeight();
            if (height + StageMenuHeader.GetHeight() + 50 > StageCreatorWindow.Instance.position.height) height = StageCreatorWindow.Instance.position.height - StageMenuHeader.GetHeight() - 50;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.box, GUILayout.Width(200), GUILayout.Height(height));
            scrollPosition.x = 0;

            RoomsListView.Draw();

            StageSerializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

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
                CreateRoom();
            }

            if (GUI.Button(cloneButtonRect, "Clone"))
            {
                DuplicateRoom(RoomsListView.SelectedItem.RoomProperty);
            }

            if (GUI.Button(deleteButtonRect, "Delete"))
            {
                DeleteRoom(RoomsListView.SelectedItem.RoomProperty);
            }

            EditorGUILayout.EndVertical();

            return isBackButtonPressed;
        }

        public void DuplicateRoom(SerializedProperty copyFrom)
        {
            StageCreatorWindow.Instance.StagePage.SaveScene();

            var roomsProperty = StageSerializedObject.FindProperty("rooms");
            roomsProperty.arraySize++;

            var copyTo = roomsProperty.GetArrayElementAtIndex(roomsProperty.arraySize - 1);

            var roomData = copyFrom.GetValue<RoomData>();
            roomData.SaveToSerializedProperty(copyTo);

            StageSerializedObject.ApplyModifiedProperties();
            StageSerializedObject.Update();
            RoomsListView.RefreshList();

            RoomsListView.SelectLast();

            onDuplicatedRoom?.Invoke();

            RoomsListView.Clear();
            RoomsListView.RefreshList();
            RoomsListView.SelectLast();
        }

        public void DeleteRoom(SerializedProperty roomProperty)
        {
            if (RoomsListView.Count <= 1)
            {
                Debug.LogWarning("You cannot delete the last room in the stage.");
                return;
            }

            var roomsProperty = StageSerializedObject.FindProperty("rooms");

            for (int i = 0; i < roomsProperty.arraySize; i++)
            {
                var currentRoomProperty = roomsProperty.GetArrayElementAtIndex(i);
                if (currentRoomProperty.propertyPath == roomProperty.propertyPath)
                {
                    roomsProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            StageSerializedObject.ApplyModifiedProperties();
            StageSerializedObject.Update();

            onDeletedRoom?.Invoke();

            RoomsListView.Clear();
            RoomsListView.RefreshList();
            RoomsListView.Select(0);
        }

        public void CreateRoom()
        {
            var roomsProperty = StageSerializedObject.FindProperty("rooms");
            roomsProperty.arraySize++;

            var roomProperty = roomsProperty.GetArrayElementAtIndex(roomsProperty.arraySize - 1);
            roomProperty.FindPropertyRelative("prefabs").arraySize = 0;
            roomProperty.FindPropertyRelative("waves").arraySize = 0;

            StageSerializedObject.ApplyModifiedProperties();
            StageSerializedObject.Update();
            RoomsListView.RefreshList();

            RoomsListView.SelectLast();

            onCreatedRoom?.Invoke();
        }
    }
}