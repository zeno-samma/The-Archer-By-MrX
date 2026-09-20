using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Audio
{
    [CustomEditor(typeof(AudioData))]
    public class AudioDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var audioGroupProperty = serializedObject.FindProperty("audioGroup");
            EditorGUILayout.PropertyField(audioGroupProperty);

            var audioDataTypeProperty = serializedObject.FindProperty("audioDataType");
            EditorGUILayout.PropertyField(audioDataTypeProperty);
            var audioDataType = (AudioDataType)audioDataTypeProperty.intValue;

            var cooldownProperty = serializedObject.FindProperty("cooldown");
            EditorGUILayout.PropertyField(cooldownProperty);

            EditorGUILayout.Space(5);

            var isPitchCurveActiveProperty = serializedObject.FindProperty("isPitchCurveActive");

            var isPitchActiveContent = new GUIContent();

            if (isPitchCurveActiveProperty.boolValue)
            {
                isPitchActiveContent.text = "Pitch Curve Enabled";
                isPitchActiveContent.tooltip = "Untoggle to disable pitch curve";
            }
            else
            {
                isPitchActiveContent.text = "Enabled Pitch Curve";
                isPitchActiveContent.tooltip = "Toggle to enable pitch curve";
            }

            isPitchCurveActiveProperty.boolValue = EditorGUILayout.Toggle(isPitchActiveContent, isPitchCurveActiveProperty.boolValue);
            if (isPitchCurveActiveProperty.boolValue)
            {
                var pitchResetCooldownProperty = serializedObject.FindProperty("pitchResetCooldown");
                EditorGUILayout.PropertyField(pitchResetCooldownProperty);

                var pitchCurveProperty = serializedObject.FindProperty("pitchCurve");
                EditorGUILayout.PropertyField(pitchCurveProperty);
                //pitchCurveProperty.animationCurveValue = EditorGUILayout.CurveField("Pitch Curve", pitchCurveProperty.animationCurveValue);
            }

            EditorGUILayout.Space(5);

            var vibrateOnPlayProperty = serializedObject.FindProperty("vibrateOnPlay");
            EditorGUILayout.PropertyField(vibrateOnPlayProperty);

            EditorGUILayout.Space(5);

            var samplesProperty = serializedObject.FindProperty("samples");
            if (samplesProperty.arraySize <= 0) samplesProperty.arraySize = 1;

            if (audioDataType == AudioDataType.Single)
            {
                DrawSingleInspectorGUI(samplesProperty);
            }
            else
            {
                EditorGUILayout.PropertyField(samplesProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawSingleInspectorGUI(SerializedProperty samplesProperty)
        {
            var firstSampleProperty = samplesProperty.GetArrayElementAtIndex(0);

            var clipProperty = firstSampleProperty.FindPropertyRelative("clip");
            var volumeProperty = firstSampleProperty.FindPropertyRelative("volume");
            var pitchProperty = firstSampleProperty.FindPropertyRelative("pitch");

            EditorGUILayout.PropertyField(clipProperty);
            EditorGUILayout.PropertyField(volumeProperty);
            EditorGUILayout.PropertyField(pitchProperty);
        }
    }
}