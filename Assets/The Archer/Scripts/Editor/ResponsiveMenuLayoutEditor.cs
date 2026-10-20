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
            var layout=(ResponsiveMenuLayout)target;
            EditorGUILayout.HelpBox("Both layouts are stored here. Preview, edit referenced RectTransforms in the UI, then capture the edited positions into the chosen layout.",MessageType.Info);
            using(new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Preview Landscape"))Preview(layout,false);
                if(GUILayout.Button("Preview Portrait"))Preview(layout,true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Capture Landscape"))Capture(layout,false);
                if(GUILayout.Button("Capture Portrait"))Capture(layout,true);
                EditorGUILayout.EndHorizontal();
            }
            DrawDefaultInspector();
        }
        private static void Preview(ResponsiveMenuLayout layout,bool portrait)
        {
            Undo.RecordObjects(layout.GetComponentsInChildren<Component>(true),"Preview menu layout");
            var stage=UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if(stage!=null&&stage.prefabContentsRoot==layout.gameObject&&layout.GetComponent<MainMenuPageBehavior>()!=null)
                ((RectTransform)layout.transform).sizeDelta=portrait?new Vector2(1080,2160):new Vector2(3840,2160);
            layout.Apply(portrait);SceneView.RepaintAll();
        }
        private static void Capture(ResponsiveMenuLayout layout,bool portrait)
        {
            Undo.RecordObject(layout,"Capture menu layout");
            foreach(var entry in layout.rectangles)if(entry.target!=null){var p=ResponsiveMenuLayout.Placement.Capture(entry.target);if(portrait)entry.portrait=p;else entry.landscape=p;}
            foreach(var entry in layout.texts)if(entry.target!=null){float size=entry.target.enableAutoSizing?entry.target.fontSizeMax:entry.target.fontSize;if(portrait)entry.portrait=size;else entry.landscape=size;}
            foreach(var entry in layout.rows)if(entry.target!=null){if(portrait)entry.portrait=entry.target.preferredHeight;else entry.landscape=entry.target.preferredHeight;}
            foreach(var entry in layout.visibility)if(entry.target!=null){if(portrait)entry.portrait=entry.target.activeSelf;else entry.landscape=entry.target.activeSelf;}
            foreach(var entry in layout.grids)if(entry.target!=null){var p=entry.target.padding;var copy=new RectOffset(p.left,p.right,p.top,p.bottom);if(portrait){entry.portraitSize=entry.target.cellSize;entry.portraitPadding=copy;}else{entry.landscapeSize=entry.target.cellSize;entry.landscapePadding=copy;}}
            EditorUtility.SetDirty(layout);
        }
    }
}
