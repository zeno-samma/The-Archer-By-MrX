using MrX.Development;
using UnityEditor;
using UnityEngine;
namespace MrX.EditorTools
{
    [CustomEditor(typeof(DeveloperToolsSettings))]
    public sealed class DeveloperToolsSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var settings = (DeveloperToolsSettings)target;
            if (settings.features == null) return;
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Công tắc tính năng game", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Các công tắc này độc lập với nút bật công cụ DEV. Online hiện chỉ là cấu hình dự phòng, chưa có dịch vụ mạng.", MessageType.Info);
            var features = new SerializedObject(settings.features); features.Update();
            EditorGUILayout.PropertyField(features.FindProperty("energyEnabled"), new GUIContent("Bật hệ thống thể lực"));
            EditorGUILayout.PropertyField(features.FindProperty("onlineEnabled"), new GUIContent("Online (dự phòng)"));
            using (new EditorGUI.DisabledScope(!features.FindProperty("onlineEnabled").boolValue))
            {
                EditorGUILayout.PropertyField(features.FindProperty("cloudSaveEnabled"), new GUIContent("Cloud save (dự phòng)"));
                EditorGUILayout.PropertyField(features.FindProperty("onlineLeaderboardsEnabled"), new GUIContent("BXH online (dự phòng)"));
            }
            features.ApplyModifiedProperties();
        }
    }
}
