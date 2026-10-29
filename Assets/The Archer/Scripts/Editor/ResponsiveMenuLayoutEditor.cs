using MrX.UI;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(ResponsiveMenuLayout))]
    public sealed class ResponsiveMenuLayoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var layout = (ResponsiveMenuLayout)target;
            EditorGUILayout.HelpBox("Chọn bố cục Ngang/Dọc, rồi sửa trực tiếp RectTransform, TMP, Image trong Hierarchy. Auto Capture lưu chỉnh sửa vào đúng profile; không cần nhập lại tọa độ trong danh sách.", MessageType.Info);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Edit Landscape")) Preview(layout, false);
                if (GUILayout.Button("Edit Portrait")) Preview(layout, true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Đang chỉnh", layout.editorPortrait ? "Portrait / Dọc" : "Landscape / Ngang");
                bool capture = EditorGUILayout.Toggle("Auto Capture Inspector", layout.captureInspectorEdits);
                if (capture != layout.captureInspectorEdits)
                {
                    Undo.RecordObject(layout, "Auto capture menu layout");
                    layout.captureInspectorEdits = capture;
                    MenuLayoutInspectorCapture.MarkChanged(layout);
                }
                if (GUILayout.Button("Capture Current Layout")) Capture(layout, layout.editorPortrait);
            }
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "editorPortrait", "captureInspectorEdits");
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying) Preview(layout, layout.editorPortrait);
            }
            else serializedObject.ApplyModifiedProperties();
        }
        public static void Preview(ResponsiveMenuLayout layout, bool portrait)
        {
            Undo.RecordObjects(layout.GetComponentsInChildren<Component>(true), "Preview menu layout");
            layout.editorPortrait = portrait;
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot == layout.gameObject && layout.GetComponent<MainMenuPageBehavior>() != null)
                ((RectTransform)layout.transform).sizeDelta = portrait ? new Vector2(1080,2160) : new Vector2(3840,2160);
            layout.Apply(portrait);
            MenuLayoutInspectorCapture.MarkChanged(layout);
            SceneView.RepaintAll();
        }
        public static void Capture(ResponsiveMenuLayout layout, bool portrait)
        {
            Undo.RecordObject(layout, "Capture menu layout");
            MenuLayoutInspectorCapture.CaptureTargets(layout, portrait, null);
            MenuLayoutInspectorCapture.MarkChanged(layout);
        }
    }

    [InitializeOnLoad]
    public static class MenuLayoutInspectorCapture
    {
        static bool capturing;
        static MenuLayoutInspectorCapture()
        {
            Undo.postprocessModifications += Modified;
            Editor.finishedDefaultHeaderGUI += Header;
        }
        static UndoPropertyModification[] Modified(UndoPropertyModification[] changes)
        {
            if (capturing || EditorApplication.isPlayingOrWillChangePlaymode) return changes;
            capturing = true;
            try
            {
                foreach (var change in changes)
                {
                    var obj = change.currentValue.target;
                    var c = obj as Component;
                    var go = obj as GameObject;
                    var t = c != null ? c.transform : go != null ? go.transform : null;
                    if (t == null) continue;
                    var layout = t.GetComponentInParent<ResponsiveMenuLayout>(true);
                    if (layout == null || !layout.captureInspectorEdits || EditorUtility.IsPersistent(layout)) continue;
                    // Capture only authored properties. Sprite/text content and generated layout values are not placement edits.
                    string path = change.currentValue.propertyPath;
                    bool relevant = obj is RectTransform && (path.StartsWith("m_Anchor") || path.StartsWith("m_Offset") || path.StartsWith("m_SizeDelta") || path.StartsWith("m_Pivot") || path.StartsWith("m_LocalScale"))
                        || obj is TMPro.TMP_Text && (path.Contains("fontSize") || path.Contains("AutoSizing"))
                        || obj is UnityEngine.UI.Image && path.Contains("PixelsPerUnitMultiplier")
                        || obj is UnityEngine.UI.LayoutElement && path.Contains("PreferredHeight")
                        || obj is UnityEngine.UI.GridLayoutGroup && (path.Contains("CellSize") || path.Contains("Padding"))
                        || obj is GameObject && path == "m_IsActive";
                    if (!relevant) continue;
                    // Postprocess runs while Unity is flushing recorded diffs. Record the complete
                    // profile now so its inverse joins the same undo group as the edited target.
                    Undo.RegisterCompleteObjectUndo(layout, "Capture Inspector edit into menu profile");
                    CaptureTargets(layout, layout.editorPortrait, obj);
                    MarkChanged(layout);
                }
            }
            finally { capturing = false; }
            return changes;
        }
        public static void CaptureTargets(ResponsiveMenuLayout layout, bool portrait, Object only)
        {
            foreach (var e in layout.rectangles) if (e.target != null && (only == null || only == e.target)) {var p=ResponsiveMenuLayout.Placement.Capture(e.target);if(portrait)e.portrait=p;else e.landscape=p;}
            foreach (var e in layout.texts) if (e.target != null && (only == null || only == e.target)) {float n=e.target.enableAutoSizing?e.target.fontSizeMax:e.target.fontSize;e.autoSize=e.target.enableAutoSizing;if(portrait){e.portrait=n;e.portraitMinimum=e.target.fontSizeMin;}else{e.landscape=n;e.landscapeMinimum=e.target.fontSizeMin;}}
            foreach (var e in layout.rows) if (e.target != null && (only == null || only == e.target)) {if(portrait)e.portrait=e.target.preferredHeight;else e.landscape=e.target.preferredHeight;}
            foreach (var e in layout.visibility) if (e.target != null && (only == null || only == e.target)) {if(portrait)e.portrait=e.target.activeSelf;else e.landscape=e.target.activeSelf;}
            foreach (var e in layout.grids) if (e.target != null && (only == null || only == e.target)) {var p=e.target.padding;var copy=new RectOffset(p.left,p.right,p.top,p.bottom);if(portrait){e.portraitSize=e.target.cellSize;e.portraitPadding=copy;}else{e.landscapeSize=e.target.cellSize;e.landscapePadding=copy;}}
            foreach (var e in layout.images) if (e.target != null && (only == null || only == e.target)) {if(portrait)e.portrait=e.target.pixelsPerUnitMultiplier;else e.landscape=e.target.pixelsPerUnitMultiplier;}
        }
        public static void MarkChanged(ResponsiveMenuLayout layout)
        {
            EditorUtility.SetDirty(layout);
            if (PrefabUtility.IsPartOfPrefabInstance(layout)) PrefabUtility.RecordPrefabInstancePropertyModifications(layout);
        }
        static void Header(Editor editor)
        {
            if (Application.isPlaying || editor.targets.Length != 1) return;
            var c=editor.target as Component;var g=editor.target as GameObject;
            var t=c!=null?c.transform:g!=null?g.transform:null;if(t==null)return;
            var layout=t.GetComponentInParent<ResponsiveMenuLayout>(true);if(layout==null||t==layout.transform)return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField((layout.editorPortrait?"Dọc":"Ngang")+(layout.captureInspectorEdits?" · Auto Capture":" · Manual Capture"),EditorStyles.miniLabel);
            if(GUILayout.Button("Layout Settings",EditorStyles.miniButton,GUILayout.Width(100)))Selection.activeGameObject=layout.gameObject;
            EditorGUILayout.EndHorizontal();
        }
    }
}
