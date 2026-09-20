using System;
using UnityEngine;

namespace OctoberStudio
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
#if UNITY_EDITOR
            if (scene != null)
            {
                var sceneAssetPath = UnityEditor.AssetDatabase.GetAssetPath(scene);
                var sceneAssetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(sceneAssetPath);

                UnityEditor.EditorBuildSettingsScene[] scenes =
                    UnityEditor.EditorBuildSettings.scenes;

                sceneIndex = -1;
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].guid.ToString() == sceneAssetGUID)
                    {
                        sceneIndex = i;
                        sceneName = scene.name;

                        break;
                    }
                }
            }
            else
            {
                sceneName = "";
            }
#endif
        }

        public virtual void OnAfterDeserialize() { }
    }
}