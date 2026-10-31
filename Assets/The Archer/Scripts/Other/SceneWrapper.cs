using System;
using UnityEngine;

namespace MrX
{
    [Serializable]
    public class SceneWrapper : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] protected UnityEditor.SceneAsset scene;
        public UnityEditor.SceneAsset Scene => scene;
#endif

        [SerializeField] protected string sceneName;
        public string SceneName => sceneName;

        [SerializeField] protected int sceneIndex = -1;
        public int SceneIndex => sceneIndex;

        public virtual void OnBeforeSerialize()
        {
            // Unity may serialize on a background thread or during domain backup.
            // Never call AssetDatabase / EditorBuildSettings from this callback.
        }

        public virtual void OnAfterDeserialize() { }
    }
}

#if UNITY_EDITOR
// Synchronize the serialized runtime fields while the reference is drawn in
// the Inspector (on the Editor main thread), not during serialization.
[UnityEditor.CustomPropertyDrawer(typeof(MrX.SceneWrapper))]
internal sealed class SceneWrapperDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property,
        GUIContent label)
    {
        UnityEditor.EditorGUI.BeginProperty(position, label, property);

        var sceneProperty = property.FindPropertyRelative("scene");
        var nameProperty = property.FindPropertyRelative("sceneName");
        var indexProperty = property.FindPropertyRelative("sceneIndex");

        if (sceneProperty != null && nameProperty != null && indexProperty != null)
        {
            UnityEditor.EditorGUI.PropertyField(position, sceneProperty, label);

            // Keep fields accurate when the scene is changed, renamed, removed,
            // or when enabled scenes in Build Settings are reordered.
            if (!sceneProperty.hasMultipleDifferentValues)
            {
                var sceneAsset = sceneProperty.objectReferenceValue as UnityEditor.SceneAsset;
                string path = sceneAsset != null
                    ? UnityEditor.AssetDatabase.GetAssetPath(sceneAsset)
                    : string.Empty;

                string newName = string.IsNullOrEmpty(path)
                    ? string.Empty
                    : System.IO.Path.GetFileNameWithoutExtension(path);
                int newIndex = -1;

                if (!string.IsNullOrEmpty(path))
                {
                    int enabledIndex = 0;
                    foreach (var buildScene in UnityEditor.EditorBuildSettings.scenes)
                    {
                        if (!buildScene.enabled)
                            continue;

                        if (buildScene.path == path)
                        {
                            newIndex = enabledIndex;
                            break;
                        }
                        enabledIndex++;
                    }
                }

                if (nameProperty.stringValue != newName)
                    nameProperty.stringValue = newName;
                if (indexProperty.intValue != newIndex)
                    indexProperty.intValue = newIndex;
            }
        }
        else
        {
            UnityEditor.EditorGUI.LabelField(position, label.text, "SceneWrapper fields not found");
        }

        UnityEditor.EditorGUI.EndProperty();
    }
}
#endif
