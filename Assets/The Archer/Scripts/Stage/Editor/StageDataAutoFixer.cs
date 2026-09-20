using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [InitializeOnLoad]
    public static class StageDataAutoFixer
    {
        static StageDataAutoFixer()
        {
            EditorApplication.delayCall += RunCheck;
        }

        private static void RunCheck()
        {
            var guids = AssetDatabase.FindAssets("t:StageData");

            var changed = false;

            var stages = new List<StageData>(guids.Length);

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                StageData stage = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (stage == null) continue;

                if (string.IsNullOrEmpty(stage.StageId))
                {
                    AssignId(stage);
                    changed = true;
                }

                stages.Add(stage);
            }

            for(int i = 0; i < stages.Count - 1; i++)
            {
                for (int j = i + 1; j < stages.Count; j++)
                {
                    if (stages[i].StageId == stages[j].StageId)
                    {
                        AssignId(stages[j]);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[StagelData] Missing IDs were auto-generated.");
            }
        }

        private static void AssignId(StageData stage)
        {
            var newId = System.Guid.NewGuid().ToString();

            var serializedObject = new SerializedObject(stage);
            var stageIdProperty = serializedObject.FindProperty("stageId");

            stageIdProperty.stringValue = newId;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(stage);
        }
    }
}