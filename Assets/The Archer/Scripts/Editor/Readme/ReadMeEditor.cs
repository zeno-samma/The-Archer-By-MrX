using OctoberStudio.PipelineSwitcher;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.ReadMe
{
    [CustomEditor(typeof(ReadMe))]
    public class ReadMeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var readme = (ReadMe) target;

            EditorGUILayout.BeginHorizontal();

            var rect = GUILayoutUtility.GetRect(readme.ReadmeBanner.width / readme.ReadmeBanner.height * 200, 200);
            GUI.DrawTexture(rect, readme.ReadmeBanner, ScaleMode.ScaleToFit);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(readme.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(readme.DocumentationURL))
            {
                if (EditorGUILayout.LinkButton("Open Documentation"))
                    Application.OpenURL(readme.DocumentationURL);
            }

            if (!string.IsNullOrEmpty(readme.DiscordURL))
            {
                if (EditorGUILayout.LinkButton("Open Discord"))
                    Application.OpenURL(readme.DiscordURL);
            }

            if (!string.IsNullOrEmpty(readme.AssetURL))
            {
                if (EditorGUILayout.LinkButton("Open Asset Store"))
                    Application.OpenURL(readme.AssetURL);
            }

            if (!string.IsNullOrEmpty(readme.Email))
            {
                EditorGUILayout.SelectableLabel(readme.Email);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(readme.URPDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Open Pipeline Switcher"))
            {
                PipelineSwitcherWindow.OpenWindow();
            }
        }
    }
}