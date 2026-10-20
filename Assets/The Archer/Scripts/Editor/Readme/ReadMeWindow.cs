using MrX.PipelineSwitcher;
using UnityEditor;
using UnityEngine;

namespace MrX.ReadMe
{
    public class ReadMeWindow : EditorWindow
    {
        protected ReadMe readme;

        [MenuItem("Tools/MrX Studio/Readme", priority = 2000)]
        public static void OpenWindow()
        {
            string[] guids = AssetDatabase.FindAssets("t:ReadMe");
            if (guids.Length == 0)
            {
                EditorGUILayout.LabelField("Cannot Find Readme asset");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var readme = AssetDatabase.LoadAssetAtPath<ReadMe>(path);

            var width = 350f;
            if(readme != null)
            {
                width = readme.ReadmeBanner.width / (float)readme.ReadmeBanner.height * 150;
            }

            ReadMeWindow wnd = GetWindow<ReadMeWindow>();
            wnd.titleContent = new GUIContent("MrX Studio | Readme");
            wnd.minSize = new Vector2(width, 400);
        }

        protected virtual void OnEnable()
        {
            if(readme == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ReadMe");
                if (guids.Length == 0)
                {
                    EditorGUILayout.LabelField("Cannot Find Readme asset");
                    return;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                readme = AssetDatabase.LoadAssetAtPath<ReadMe>(path);
            }
        }

        public virtual void OnGUI()
        {
            EditorGUILayout.LabelField("MrX Studio — The Archer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Project tools, configuration and testing guide.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Open MrX Studio Tools Guide")) MrX.EditorTools.StudioToolsGuide.Open();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Original template reference", EditorStyles.boldLabel);
            if (readme == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ReadMe");
                if (guids.Length == 0)
                {
                    EditorGUILayout.LabelField("Cannot Find Readme asset");
                    return;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                readme = AssetDatabase.LoadAssetAtPath<ReadMe>(path);
            }

            if (readme == null) return;

            EditorGUILayout.BeginHorizontal();

            var rect = GUILayoutUtility.GetRect(readme.ReadmeBanner.width / (float)readme.ReadmeBanner.height * 150, 150);
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
