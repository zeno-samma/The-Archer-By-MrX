using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [CustomPropertyDrawer(typeof(SceneWrapper))]
    public class SceneWrapperPropertyDrawer : PropertyDrawer
    {
        protected SerializedProperty sceneAssetProperty;
        protected SerializedProperty sceneNameProperty;
        protected SerializedProperty sceneIndexProperty;

        protected SceneAsset sceneAsset;
        protected string scenePath;
        protected string sceneGUID;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            CacheProperties(property);

            Validate();

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, sceneAssetProperty, GUIContent.none, false);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                CacheProperties(property);

                Validate();
            }

            EditorGUI.EndProperty();
        }

        protected virtual void CacheProperties(SerializedProperty property)
        {
            sceneAssetProperty = property.FindPropertyRelative("scene");
            sceneNameProperty = property.FindPropertyRelative("sceneName");
            sceneIndexProperty = property.FindPropertyRelative("sceneIndex");

            sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;

            if (sceneAsset != null)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAssetProperty.objectReferenceValue);
                sceneGUID = AssetDatabase.AssetPathToGUID(scenePath);
            }
            else
            {
                scenePath = null;
                sceneGUID = null;
            }
        }

        protected virtual void FixScene()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (sceneIndexProperty.intValue < 0)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                sceneIndexProperty.intValue = scenes.Count - 1;
            }

            scenes[sceneIndexProperty.intValue].enabled = true;

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        protected virtual void Validate()
        {
            if (sceneAsset != null)
            {
                var scenes = EditorBuildSettings.scenes;

                EditorBuildSettingsScene scene = null;

                var index = -1;
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].guid.ToString() == sceneGUID)
                    {
                        index = i;
                        scene = scenes[i];

                        break;
                    }
                }

                if (scene != null)
                {
                    if (sceneIndexProperty.intValue != index)
                    {
                        sceneIndexProperty.intValue = index;
                    }
                    if (sceneNameProperty.stringValue != sceneAsset.name)
                    {
                        sceneNameProperty.stringValue = sceneAsset.name;
                    }

                    scene.enabled = true;

                    EditorBuildSettings.scenes = scenes;
                }
                else
                {
                    FixScene();
                }
            }
            else
            {
                sceneNameProperty.stringValue = "";
            }
        }
    }
}