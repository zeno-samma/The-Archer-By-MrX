using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class IconGridItem : EditorGridItem<Sprite>
    {
        protected UnityAction onClickAction;

        public bool IsSelected { get; set; }

        protected GUIContent content;

        public override void Draw(Rect rect, UnityAction<Sprite> onClick)
        {
            if(content == null)
            {
                content = new GUIContent(Source.texture, Source.name);
            }

            var buttonRect = new Rect(rect.x, rect.y, rect.width, rect.height);

            if (IsSelected)
            {
                EditorGUI.DrawRect(rect, Color.yellow);
                buttonRect.x += 2;
                buttonRect.y += 2;
                buttonRect.width -= 4;
                buttonRect.height -= 4;
            }

            if (GUI.Button(buttonRect, content))
            {
                onClick?.Invoke(Source);
            }
        }
    }
}