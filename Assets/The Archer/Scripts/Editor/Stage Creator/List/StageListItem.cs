using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class StageListItem : ListItem
    {
        protected static readonly string prefabsPath = "Assets/The Archer/Prefabs/Stages/";
        protected static readonly string defaultStageImagePath = "Assets/The Archer/Sprites/Editor/editor_ab_icon.png";
        protected static readonly float defaultHeight = 50f;
        protected static readonly float smallOffset = 2f;

        protected static GUIStyle textStyle;

        public StageCreatorWindow StageCreator { get; protected set; }

        public SerializedObject StageObject { get; protected set; }
        public Object TargetObject => StageObject.targetObject;

        public SerializedProperty RoomsProperty { get; protected set; }

        public SerializedProperty StageNameProperty { get; protected set; }
        public string StageName
        {
            get => StageNameProperty.stringValue;
            set => StageNameProperty.stringValue = value;
        }

        public SerializedProperty StageImageProperty { get; protected set; }
        public Sprite StageImage
        {
            get => StageImageProperty.objectReferenceValue as Sprite;
            set => StageImageProperty.objectReferenceValue = value;
        }

        protected override float DefaultHeight => defaultHeight;

        public string PrefabsPath => prefabsPath + StageFileName;
        public string StageFileName => $"Stage {Index + 1:D3}";

        public Rect RoomsRect { get; set; }

        public override Rect ReserveRect()
        {
            base.ReserveRect();

            RoomsRect = new Rect(FullRect.x, FullRect.y + HeaderRect.height, FullRect.width, FullRect.height - HeaderRect.height);

            return HeaderRect;
        }

        public override void Draw()
        {
            if(textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };

            StageObject.UpdateIfRequiredOrScript();

            if (Event.current.type == EventType.ContextClick && HeaderRect.Contains(Event.current.mousePosition))
            {
                StageCreator.ContextManager.ShowContextMenu(this);
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

            var texture = StageImage == null ? AssetDatabase.LoadAssetAtPath<Texture2D>(defaultStageImagePath) : StageImage.texture;

            GUI.DrawTexture(GetTextureRect(rect), texture, ScaleMode.ScaleToFit, true, 0f, Color.white, 0f, 0f);

            EditorGUI.LabelField(GetStageNumberRect(rect), StageObject.targetObject.name, textStyle);
            EditorGUI.LabelField(GetStageNameRect(rect), StageName, textStyle);

            GUI.DrawTexture(GetDragHandleRect(rect), StageCreator.DragHandleTexture);
        }

        public override void Init(EditorWindow window, SerializedProperty property, int index)
        {
            Index = index;
            Window = window;
            StageCreator = window as StageCreatorWindow;

            StageObject = new SerializedObject(property.objectReferenceValue);
            StageNameProperty = StageObject.FindProperty("stageName");
            StageImageProperty = StageObject.FindProperty("stageImage");
            RoomsProperty = StageObject.FindProperty("rooms");
        }

        public override bool EqualsTo(SerializedProperty property)
        {
            return StageObject.targetObject == property.objectReferenceValue;
        }

        public override float GetHeight()
        {
            return defaultHeight;
        }

        #region Rects

        public Rect GetDragHandleRect(Rect mainRect)
        {
            var handleRect = new Rect(mainRect);

            handleRect.x += smallOffset;
            handleRect.y += 10;
            handleRect.width = defaultHeight - 10 * 2;
            handleRect.height = defaultHeight - 10 * 2;

            return handleRect;
        }

        public Rect GetFillRect(Rect mainRect)
        {
            var fillRect = new Rect(mainRect);
            fillRect.x += smallOffset;
            fillRect.y += smallOffset;
            fillRect.width -= smallOffset * 2;
            fillRect.height -= smallOffset * 2;
            return fillRect;
        }

        public Rect GetTextureRect(Rect mainRect)
        {
            var textureRect = new Rect(mainRect);

            var offset = defaultHeight - 10 * 2;

            textureRect.x += smallOffset + offset;
            textureRect.y += smallOffset;
            textureRect.width = defaultHeight - smallOffset * 2;
            textureRect.height = defaultHeight - smallOffset * 2;

            return textureRect;
        }

        public Rect GetStageNumberRect(Rect mainRect)
        {
            var stageNumberRect = new Rect(mainRect);

            var offset = defaultHeight - 10 * 2 + defaultHeight;
            stageNumberRect.x += offset;
            stageNumberRect.y += smallOffset;
            stageNumberRect.width -= defaultHeight;
            stageNumberRect.height = defaultHeight / 2 - smallOffset * 2;

            return stageNumberRect;
        }

        public Rect GetStageNameRect(Rect mainRect)
        {
            var stageNumberRect = new Rect(mainRect);

            var offset = defaultHeight - 10 * 2 + defaultHeight;
            stageNumberRect.x += offset;
            stageNumberRect.y += defaultHeight / 2f + smallOffset;
            stageNumberRect.width -= defaultHeight;
            stageNumberRect.height = defaultHeight / 2 - smallOffset * 2;

            return stageNumberRect;
        }

        #endregion

        public override void CacheData()
        {
            
        }

        public override void WriteToProperty(SerializedProperty property)
        {
            property.objectReferenceValue = StageObject.targetObject;
        }

        public virtual void PasteValuesFrom(StageListItem other)
        {
            StageObject.CopyFromSerializedObject(other.StageObject);
        }
    }
}