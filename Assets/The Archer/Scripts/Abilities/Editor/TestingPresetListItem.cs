using OctoberStudio.Abilities;
using OctoberStudio.StageCreator;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    public class TestingPresetListItem : ListItem
    {
        protected static readonly float defaultHeight = 40f;
        protected static readonly float smallOffset = 2f;
        protected static readonly float borderOffset = 1.5f;
        protected static readonly float textureOffset = 5f;
        protected static readonly float dragOffset = 10f;

        protected static GUIStyle textStyle;

        public SerializedProperty TestingPresetProperty { get; protected set; }

        protected override float DefaultHeight => defaultHeight;

        public override void Draw()
        {
            if (textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            TestingPresetProperty.serializedObject.UpdateIfRequiredOrScript();

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

            var name = TestingPresetProperty.FindPropertyRelative("name").stringValue;
            if (name == "") name = $"Preset {Index + 1:D2}";

            EditorGUI.LabelField(GetPresetNumberRect(rect), name, textStyle);

            GUI.DrawTexture(GetDragHandleRect(rect), AbilitiesTesterWindow.Instance.DragHandleTexture);

            if (TestingPresetProperty.FindPropertyRelative("enabledInEditor").boolValue)
            {
                EditorGUI.LabelField(GetEditorRect(rect), $"Editor", EditorStyles.miniBoldLabel);
            }

            if (TestingPresetProperty.FindPropertyRelative("enabledInBuild").boolValue)
            {
                EditorGUI.LabelField(GetBuildRect(rect), $"Build", EditorStyles.miniBoldLabel);
            }
        }

        public override bool EqualsTo(SerializedProperty property)
        {
            return property.propertyPath == TestingPresetProperty.propertyPath;
        }

        public override float GetHeight()
        {
            return defaultHeight;
        }

        public override void Init(EditorWindow window, SerializedProperty property, int index)
        {
            Index = index;

            Window = window;

            TestingPresetProperty = property;
        }

        public TestingPreset PresetDataCache { get; protected set; }

        public override void CacheData()
        {
            PresetDataCache = TestingPresetProperty.GetValue<TestingPreset>();
        }

        public override void WriteToProperty(SerializedProperty property)
        {
            PresetDataCache.SaveToSerializedProperty(property);
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

        public Rect GetDragHandleRect(Rect mainRect)
        {
            var handleRect = new Rect(mainRect);

            handleRect.x += smallOffset;
            handleRect.y += dragOffset;
            handleRect.width = defaultHeight - dragOffset * 2;
            handleRect.height = defaultHeight - dragOffset * 2;

            return handleRect;
        }

        public virtual Rect GetPresetNumberRect(Rect mainRect)
        {
            var presetNumber = new Rect(mainRect);

            var offset = defaultHeight - 5 * 2;
            presetNumber.x += offset;
            presetNumber.y += defaultHeight / 2 - EditorGUIUtility.singleLineHeight / 2;
            presetNumber.width -= defaultHeight;
            presetNumber.height = defaultHeight / 2 - smallOffset * 2;

            return presetNumber;
        }

        public virtual Rect GetEditorRect(Rect mainRect)
        {
            var editorRect = new Rect(mainRect);

            editorRect.x += mainRect.width - 40;
            editorRect.y += 2f;
            editorRect.width = 40;
            editorRect.height = EditorGUIUtility.singleLineHeight;

            return editorRect;
        }

        public virtual Rect GetBuildRect(Rect mainRect)
        {
            var buildRect = new Rect(mainRect);

            buildRect.x += mainRect.width - 40;
            buildRect.y += EditorGUIUtility.singleLineHeight + 2f;
            buildRect.width = 40;
            buildRect.height = EditorGUIUtility.singleLineHeight;

            return buildRect;
        }

        public virtual void PasteValuesFrom(TestingPresetListItem other)
        {
            var waveData = other.TestingPresetProperty.GetValue<WaveData>();
            waveData.SaveToSerializedProperty(TestingPresetProperty);

            TestingPresetProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}