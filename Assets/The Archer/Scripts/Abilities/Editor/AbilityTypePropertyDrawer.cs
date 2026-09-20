using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CustomPropertyDrawer(typeof(AbilityType))]
    public class AbilityTypePropertyDrawer : PropertyDrawer
    {
        protected AdvancedDropdownState state = new AdvancedDropdownState();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var buttonRect = EditorGUI.PrefixLabel(position, label);
            var labelRect = new Rect(position);
            labelRect.width -= buttonRect.width;
            
            GUI.Label(labelRect, label);

            var enumName = property.enumDisplayNames[property.enumValueIndex];
            if (enumName.Contains("_"))
            {
                enumName = enumName.Split('_')[1];
            }

            if (GUI.Button(buttonRect, enumName, EditorStyles.popup))
            {
                var dropdown = new EnumDropdown(state, property);
                dropdown.Show(position);
            }
            EditorGUI.EndProperty();
        }
    }
}