using MrX.Rewards;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(BossChestRewards))]
    public sealed class BossChestRewardsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var table = (BossChestRewards)target;
            if (!table.IsValid(out var total))
            { EditorGUILayout.HelpBox("Use equipment/fragments, registered ItemData, unique IDs, valid quantities and a positive total weight.", MessageType.Error); return; }
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Normalized chances", EditorStyles.boldLabel);
            foreach (var prize in table.prizes)
                if (prize != null && prize.enabled && prize.weight > 0) EditorGUILayout.LabelField(prize.id, (prize.weight / total * 100).ToString("0.###") + "%");
            EditorGUILayout.HelpBox("One chest and one reward per boss. Materials belong in Lucky Wheel / Gem Chest tables.", MessageType.Info);
        }
    }
}
