using MrX.Rewards;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(LuckyRewardTable))]
    public sealed class LuckyRewardTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var table = (LuckyRewardTable)target;
            EditorGUILayout.Space();
            if (!table.IsValid(out var total))
            { EditorGUILayout.HelpBox("Invalid table: use a stable ID, positive gem price, unique reward IDs, valid quantities and positive total weight.", MessageType.Error); return; }
            EditorGUILayout.LabelField("Normalized chances", EditorStyles.boldLabel);
            foreach (var prize in table.prizes)
                if (prize != null && prize.enabled && prize.weight > 0) EditorGUILayout.LabelField(prize.id, (prize.weight / total * 100).ToString("0.###") + "%");
            EditorGUILayout.HelpBox("One reward per purchase. Weights do not need to sum to 100. New equipment uses the existing bag; max-level duplicates use the entry's gold compensation.", MessageType.Info);
        }
    }
}
