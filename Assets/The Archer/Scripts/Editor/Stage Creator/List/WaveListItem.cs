using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class WaveListItem : ListItem
    {
        protected static readonly float defaultHeight = 40f;
        protected static readonly float smallOffset = 2f;
        protected static readonly float borderOffset = 1.5f;
        protected static readonly float textureOffset = 5f;
        protected static readonly float dragOffset = 10f;

        protected static GUIStyle textStyle;

        public SerializedProperty WaveProperty { get; protected set; }

        protected override float DefaultHeight => defaultHeight;

        public override void Draw()
        {
            if(textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            WaveProperty.serializedObject.UpdateIfRequiredOrScript();

            if (Event.current.type == EventType.ContextClick && HeaderRect.Contains(Event.current.mousePosition))
            {
                StageCreatorWindow.Instance.ContextManager.ShowContextMenu(this);
                Event.current.Use();
            }

            var rect = new Rect(FullRect);

            if (isPickedUp || isSelected)
            {
                if (isPickedUp)
                    rect.y += Event.current.mousePosition.y - SelectMousePosition;

                EditorGUI.DrawRect(rect, new Color(0.05f, 0.05f, 0.05f));
                EditorGUI.DrawRect(GetFillRect(rect), new Color(0.3f, 0.3f, 0.3f));
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.05f, 0.05f, 0.05f));
                EditorGUI.DrawRect(GetFillRect(rect), new Color(0.15f, 0.15f, 0.15f));
            }

            var groupId = WaveProperty.FindPropertyRelative("groupId").intValue;

            if (groupId > 0)
            {
                var color = StageCreatorWindow.Instance.StageDatabase.GetRandomGroupColor(groupId - 1);
                var groupRect = GetGroupRect(rect);

                GUI.DrawTexture(groupRect, StageCreatorWindow.Instance.CircleTexture, ScaleMode.ScaleToFit, true, 0, color, 0, 0);

                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                };
                EditorGUI.LabelField(groupRect, groupId.ToString(), style);

            }

            var icon = WaveProperty.FindPropertyRelative("icon").objectReferenceValue;
            if(icon is Texture iconTexture)
            {
                GUI.DrawTexture(GetTextureRect(rect), iconTexture);
            } else if(icon is Sprite iconSprite)
            {
                GUI.DrawTexture(GetTextureRect(rect), iconSprite.texture);
            }

            EditorGUI.LabelField(GetWaveNumberRect(rect), $"Wave {Index + 1:D2}", textStyle);

            GUI.DrawTexture(GetDragHandleRect(rect), StageCreatorWindow.Instance.DragHandleTexture);
        }

        public override bool EqualsTo(SerializedProperty property)
        {
            return property.propertyPath == WaveProperty.propertyPath;
        }

        public override float GetHeight()
        {
            return defaultHeight;
        }

        public override void Init(EditorWindow window, SerializedProperty property, int index)
        {
            Index = index;

            Window = window;

            WaveProperty = property;
        }

        public WaveData WaveDataCache { get; protected set; }

        public override void CacheData()
        {
            WaveDataCache = WaveProperty.GetValue<WaveData>();
        }

        public override void WriteToProperty(SerializedProperty property)
        {
            WaveDataCache.SaveToSerializedProperty(property);
        }

        public virtual Rect GetFillRect(Rect mainRect)
        {
            var fillRect = new Rect(mainRect);
            fillRect.x += borderOffset;
            fillRect.y += borderOffset;
            fillRect.width -= borderOffset * 2;
            fillRect.height -= borderOffset * 2;
            return fillRect;
        }

        public virtual Rect GetGroupRect(Rect mainRect)
        {
            var groupRect = new Rect(mainRect);

            groupRect.x += groupRect.width - defaultHeight;
            groupRect.y += textureOffset;
            groupRect.width = defaultHeight - textureOffset * 2;
            groupRect.height = defaultHeight - textureOffset * 2;

            return groupRect;
        }

        public Rect GetDragHandleRect(Rect mainRect)
        {
            var handleRect = new Rect(mainRect);

            handleRect.x += smallOffset;
            handleRect.y += dragOffset;
            handleRect.width = defaultHeight - dragOffset * 2;
            handleRect.height = defaultHeight - dragOffset * 2;

            return handleRect;
        }

        public Rect GetTextureRect(Rect mainRect)
        {
            var textureRect = new Rect(mainRect);

            var offset = defaultHeight - dragOffset * 2;

            textureRect.x += textureOffset + offset;
            textureRect.y += textureOffset;
            textureRect.width = defaultHeight - textureOffset * 2;
            textureRect.height = defaultHeight - textureOffset * 2;

            return textureRect;
        }

        public virtual Rect GetWaveNumberRect(Rect mainRect)
        {
            var stageNumberRect = new Rect(mainRect);

            var offset = defaultHeight - 5 * 2 + defaultHeight;
            stageNumberRect.x += offset;
            stageNumberRect.y += defaultHeight / 2 - EditorGUIUtility.singleLineHeight / 2;
            stageNumberRect.width -= defaultHeight;
            stageNumberRect.height = defaultHeight / 2 - smallOffset * 2;

            return stageNumberRect;
        }

        public virtual void PasteValuesFrom(WaveListItem other)
        {
            var waveData = other.WaveProperty.GetValue<WaveData>();
            waveData.SaveToSerializedProperty(WaveProperty);

            WaveProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}