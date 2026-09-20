using System.Diagnostics.CodeAnalysis;
using UnityEditor;

namespace OctoberStudio
{
    public class StageDataProcessor : AssetModificationProcessor
    {
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Called by Unity via reflection")]
        private static void OnWillCreateAsset(string path)
        {
            if (!path.EndsWith(".asset"))
                return;

            EditorApplication.delayCall += () =>
            {
                var scriptable = AssetDatabase.LoadAssetAtPath<StageData>(path);
                if (scriptable != null)
                {
                    GenerateAndAssignId(scriptable);
                }
            };
        }

        private static void GenerateAndAssignId(StageData scriptable)
        {
            var serializedObject = new SerializedObject(scriptable);

            var idProperty = serializedObject.FindProperty("stageId");
            idProperty.stringValue = System.Guid.NewGuid().ToString();

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(scriptable);
        }
    }
}