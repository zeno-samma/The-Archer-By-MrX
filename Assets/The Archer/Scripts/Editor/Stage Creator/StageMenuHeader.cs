using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class StageMenuHeader
    {
        protected static readonly string defaultStageImagePath = "Assets/The Archer/Sprites/Editor/editor_ab_icon.png";
        protected static readonly float defaultHeight = 50f;
        protected static readonly float smallOffset = 2f;

        protected GUIStyle textStyle;

        public SerializedObject StageSerializedObject { get; protected set; }

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

        public void SetStageSerializedObject(SerializedObject stageSerializedObject)
        {
            StageSerializedObject = stageSerializedObject;
            

            StageNameProperty = stageSerializedObject.FindProperty("stageName");
            StageImageProperty = stageSerializedObject.FindProperty("stageImage");
        }

        public bool Draw()
        {
            if(textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            var HeaderRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            var FullRect = new Rect(HeaderRect.x, HeaderRect.y, HeaderRect.width, GetHeight());

            var rect = new Rect(FullRect);

            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
            EditorGUI.DrawRect(GetFillRect(rect), new Color(0.25f, 0.25f, 0.25f));

            var isBackButtonPressed = DrawBackButton(rect);
            DrawStageImage(rect);

            DrawStageNumber(rect);
            DrawStageName(rect);

            return isBackButtonPressed;
        }

        protected virtual bool DrawBackButton(Rect rect)
        {
            var buttonRect = new Rect(rect);
            buttonRect.x += smallOffset;
            buttonRect.y += smallOffset;
            buttonRect.width = defaultHeight - smallOffset * 2;
            buttonRect.height = defaultHeight - smallOffset * 2;

            if (GUI.Button(buttonRect, "⟵"))
            {
                return true;
            }

            return false;
        }
        protected virtual void DrawStageImage(Rect rect)
        {
            var textureRect = new Rect(rect);

            textureRect.x += defaultHeight + smallOffset * 2;
            textureRect.y += smallOffset;
            textureRect.width = defaultHeight - smallOffset * 2;
            textureRect.height = defaultHeight - smallOffset * 2;

            var texture = StageImage == null ? AssetDatabase.LoadAssetAtPath<Texture2D>(defaultStageImagePath) : StageImage.texture;
            GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, true, 0f, Color.white, 0f, 0f);
        }

        protected virtual void DrawStageNumber(Rect rect)
        {
            var stageNumberRect = new Rect(rect);
            stageNumberRect.x += defaultHeight * 2 + smallOffset * 3;
            stageNumberRect.y += smallOffset;
            stageNumberRect.width -= defaultHeight + smallOffset * 2;
            stageNumberRect.height = defaultHeight / 2 - smallOffset * 2;

            EditorGUI.LabelField(stageNumberRect, StageSerializedObject.targetObject.name, textStyle);
        }

        protected virtual void DrawStageName(Rect rect)
        {
            var stageNameRect = new Rect(rect);
            stageNameRect.x += defaultHeight * 2 + smallOffset * 3;
            stageNameRect.y += defaultHeight / 2f + smallOffset;
            stageNameRect.width -= defaultHeight * 2 + smallOffset * 2;
            stageNameRect.height = defaultHeight / 2 - smallOffset * 2;

            EditorGUI.LabelField(stageNameRect, StageName, textStyle);
        }

        public float GetHeight()
        {
            return 50f;
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
    }
}
