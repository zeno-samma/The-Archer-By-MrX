using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [CustomPropertyDrawer(typeof(AbilityDev))]
    public class AbilityDevPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var abilityProperty = property.FindPropertyRelative("abilityType");
            var levelProperty = property.FindPropertyRelative("level");

            EditorGUI.BeginProperty(position, label, property);

            var abilityPosition = new Rect(position);
            abilityPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(abilityPosition, abilityProperty);

            var levelPosition = new Rect(position);
            levelPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            levelPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(levelPosition, levelProperty);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }

}