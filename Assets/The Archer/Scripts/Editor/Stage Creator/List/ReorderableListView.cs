using OctoberStudio.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class ReorderableListView<T> : IEnumerable<T> where T : ListItem, new()
    {
        public SerializedProperty ArrayProperty { get; protected set; }
        public EditorWindow Window { get; protected set; }

        public T this[int i]
        {
            get => items[i];
        }

        public int Count => items.Count;

        protected List<T> items;

        protected int dragIndex = -1;
        protected int dropIndex = -1;

        public T SelectedItem { get; set; }
        public int SelectedIndex { get; set; }
        protected bool wasMouseReleased = true;

        public bool CanDeselect { get; set; } = true;

        public event UnityAction beforeSwap;
        public event UnityAction afterSwap;

        public event UnityAction<T> onItemDoubleClicked;
        public event UnityAction<T, T> onItemSelected;

        public ReorderableListView(EditorWindow window, SerializedProperty arrayProperty)
        {
            Window = window;

            ArrayProperty = arrayProperty;

            items = new List<T>();

            for (int i = 0; i < ArrayProperty.arraySize; i++)
            {
                var item = new T();
                item.Init(window, ArrayProperty.GetArrayElementAtIndex(i), i);

                items.Add(item);
            }
        }

        public void Select(int id)
        {
            if (id < 0 || id >= items.Count) return;

            var item = items[id];

            if (SelectedItem != null && SelectedItem != item)
            {
                SelectedItem.Deselect();
            }
            var prevSelectedItem = SelectedItem;

            item.WasClickedOn = true;
            SelectedItem = item;
            SelectedIndex = id;
            item.Select();

            onItemSelected?.Invoke(item, prevSelectedItem);

            GUI.FocusControl(null);
        }

        public void SelectLast()
        {
            if (items.Count == 0) return;

            var item = items[items.Count - 1];
            if (SelectedItem != null && SelectedItem != item)
            {
                SelectedItem.Deselect();
                SelectedItem.WasClickedOn = false;
            }
            var prevSelectedItem = SelectedItem;

            item.WasClickedOn = true;
            SelectedItem = item;
            SelectedIndex = items.Count - 1;
            item.Select();

            onItemSelected?.Invoke(item, prevSelectedItem);

            GUI.FocusControl(null);
        }

        public void Deselect()
        {
            if (SelectedItem != null)
            {
                SelectedItem.Deselect();
                SelectedItem.WasClickedOn = false;
                SelectedItem = null;
                SelectedIndex = -1;
            }
        }

        public void Clear()
        {
            items.Clear();
            SelectedItem = null;
            SelectedIndex = -1;
            dragIndex = -1;
            dropIndex = -1;
            wasMouseReleased = true;
        }

        public virtual void Draw()
        {
            var evt = Event.current;

            dropIndex = -1;

            T pickedUpStage = null;
            if(items.Count != ArrayProperty.arraySize)
            {
                RefreshList();
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                var itemRect = item.ReserveRect();

                if (item.IsPickedUp)
                {
                    pickedUpStage = item;

                    Window.Repaint();
                }
                else
                {
                    item.Draw();
                }

                if (item.IsMouseDownInsideHeader())
                {
                    HandleMouseDownInsideHeader(item, i);
                }
                else
                {
                    CheckForDrag(item, i);
                }

                if (dragIndex != -1 && itemRect.Contains(evt.mousePosition))
                {
                    dropIndex = i;
                }

                // Visual drop indicator
                if (dragIndex != -1 && dropIndex == i && i != dragIndex)
                {
                    beforeSwap?.Invoke();

                    ArrayProperty.MoveArrayElement(dragIndex, dropIndex);
                    ArrayProperty.serializedObject.ApplyModifiedProperties();

                    var draggedItem = items[dragIndex];
                    items.RemoveAt(dragIndex);

                    var up = dropIndex <= dragIndex;

                    items.Insert(dropIndex, draggedItem);
                    items.Random();
                    items[dropIndex].Move(up);
                    dragIndex = dropIndex;

                    for (int j = 0; j < items.Count; j++)
                    {
                        items[j].ChangeIndex(j);
                        items[j].Init(Window, ArrayProperty.GetArrayElementAtIndex(j), j);
                    }

                    afterSwap?.Invoke();

                    break;
                }
            }

            if (pickedUpStage != null)
            {
                pickedUpStage.Draw();
            }

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.Use();
            }

            CheckForDragPerformed();

            if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                dragIndex = -1;
                dropIndex = -1;

                wasMouseReleased = true;
                var selected = false;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Drop();
                    if (items[i].WasClickedOn)
                    {
                        SelectedItem.Select();
                        SelectedItem.WasClickedOn = false;
                        selected = true;
                    }

                    items[i].ChangeIndex(i);
                }

                Event.current.Use();

                if (!selected && SelectedItem != null && (!SelectedItem.FullRect.Contains(Event.current.mousePosition) || SelectedItem.HeaderRect.Contains(Event.current.mousePosition)) && CanDeselect)
                {
                    SelectedItem.Deselect();
                    SelectedItem = null;
                }

                Window.Repaint();
            }
        }

        public bool IsDragActive()
        {
            return DragAndDrop.GetGenericData(typeof(T).Name) != null;
        }

        public virtual void RefreshList()
        {
            if (items == null || !wasMouseReleased) return;

            var newStagesList = new List<T>();

            for (int i = 0; i < ArrayProperty.arraySize; i++)
            {
                var itemProperty = ArrayProperty.GetArrayElementAtIndex(i);

                var item = items.Find(s => s.EqualsTo(itemProperty));

                if (item != null)
                {
                    newStagesList.Add(item);
                }
                else
                {
                    var newItem = new T();
                    newItem.Init(Window, itemProperty, i);

                    newStagesList.Add(newItem);
                }
            }

            items = newStagesList;

            if(SelectedIndex > items.Count - 1)
            {
                SelectedIndex = items.Count - 1;
                SelectedItem = items[SelectedIndex];
                SelectedItem.Select();

                onItemSelected?.Invoke(SelectedItem, null);
            }
        }

        protected void HandleMouseDownInsideHeader(T item, int id)
        {
            if (Event.current.clickCount == 2)
            {
                onItemDoubleClicked?.Invoke(item);
            }
            
            var wasSelectedBefore = false;
            if (SelectedItem != null)
            {
                if (SelectedItem != item)
                {
                    SelectedItem.Deselect();
                    if (!CanDeselect)
                    {
                        var prevSelectedItem = SelectedItem;
                        SelectedItem = item;
                        SelectedIndex = id;
                        item.Select();

                        onItemSelected?.Invoke(item, prevSelectedItem);
                    }
                    GUI.FocusControl(null);
                }

                if (SelectedItem == item && CanDeselect)
                {
                    wasSelectedBefore = true;
                }
            }

            if (wasSelectedBefore)
            {
                item.WasClickedOn = false;
                SelectedItem = item;
                SelectedIndex = id;

                wasMouseReleased = false;
            }
            else
            {
                item.WasClickedOn = true;
                SelectedItem = item;
                SelectedIndex = id;

                wasMouseReleased = false;

                Window.Repaint();
            }
        }

        protected void CheckForDrag(T item, int index)
        {
            if (Event.current.type == EventType.MouseDrag && item == SelectedItem && !wasMouseReleased && !item.IsPickedUp)
            {
                if (Mathf.Abs(item.SelectMousePosition - Event.current.mousePosition.y) > 5)
                {
                    dragIndex = index;

                    if (CanDeselect) item.Deselect();
                    item.PickUp();
                    item.WasClickedOn = false;
                }
            }
        }

        public virtual float GetListHeight()
        {
            var height = 0f;

            for (int i = 0; i < items.Count; i++)
            {
                height += items[i].GetHeight();
            }

            var additionalHeight = items.Count < 8 ? 8 : items.Count;
            height += additionalHeight;

            var maxHeight = Window.position.height - 50f;
            if (height > maxHeight)
            {
                height = maxHeight;
            }
            return height;
        }

        protected virtual void CheckForDragPerformed()
        {
            if (Event.current.type == EventType.DragPerform)
            {
                dragIndex = -1;
                dropIndex = -1;

                wasMouseReleased = true;
                var selected = false;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Drop();
                    if (items[i].WasClickedOn)
                    {
                        SelectedItem.Select();
                        SelectedItem.WasClickedOn = false;
                        selected = true;
                    }

                    items[i].ChangeIndex(i);
                }

                if (!selected && SelectedItem != null && CanDeselect)
                {
                    SelectedItem = null;
                    SelectedIndex = -1;
                }

                Window.Repaint();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
    }
}