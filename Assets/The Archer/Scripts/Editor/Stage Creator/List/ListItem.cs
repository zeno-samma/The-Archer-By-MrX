using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public abstract class ListItem
    {
        public EditorWindow Window { get; protected set; }

        public virtual float SelectMousePosition { get; protected set; }

        protected bool isPickedUp = false;
        protected bool isSelected = false;

        public bool WasClickedOn { get; set; } = false;
        public bool IsPickedUp => isPickedUp;
        public bool IsSelected => isSelected;

        public int Index { get; set; } = -1;

        public virtual Rect FullRect { get; protected set; }
        public virtual Rect HeaderRect { get; protected set; }

        protected abstract float DefaultHeight { get; }

        public abstract void Draw();
        public abstract float GetHeight();
        public abstract void CacheData();

        public abstract bool EqualsTo(SerializedProperty property);
        public abstract void Init(EditorWindow window, SerializedProperty property, int index);
        public abstract void WriteToProperty(SerializedProperty property);
        public virtual void ChangeIndex(int newIndex)
        {
            Index = newIndex;
        }

        public virtual Rect ReserveRect()
        {
            HeaderRect = GUILayoutUtility.GetRect(0, DefaultHeight, GUILayout.ExpandWidth(true));
            FullRect = new Rect(HeaderRect.x, HeaderRect.y, HeaderRect.width, GetHeight());

            return HeaderRect;
        }

        public virtual bool IsMouseDownInsideHeader()
        {
            var result = Event.current.type == EventType.MouseDown && Event.current.button == 0 && HeaderRect.Contains(Event.current.mousePosition);
            if (result)
            {
                Event.current.Use(); 
            }
            return result; 
        }

        public virtual void Select()
        {
            isSelected = true;
        }

        public virtual void Deselect()
        {
            isSelected = false;
        }

        public virtual void PickUp()
        {
            SelectMousePosition = Event.current.mousePosition.y;
            isPickedUp = true;
        }

        public virtual void Drop()
        {
            SelectMousePosition = 0;
            isPickedUp = false;
        }

        public virtual void Move(bool up)
        {
            if (up)
            {
                SelectMousePosition -= DefaultHeight;
            }
            else
            {
                SelectMousePosition += DefaultHeight;
            }
        }
    }
}