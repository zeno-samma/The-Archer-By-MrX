using System;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [CustomPropertyDrawer(typeof(Int))]
    public class IntPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.width -= 25;

            var propertyType = property.FindPropertyRelative("propertyType").enumValueIndex;

            if (propertyType == (int)PropertyType.Constant)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label);
            }
            else
            {
                var spacing = 5f;

                var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);

                var propertyRect = new Rect(labelRect.xMax + EditorGUIUtility.standardVerticalSpacing, position.y, position.width - labelRect.width, position.height);

                var minFieldRect = new Rect(propertyRect);
                minFieldRect.width -= spacing;
                minFieldRect.width /= 2f;

                var maxFieldRect = new Rect(minFieldRect.xMax + spacing, position.y, minFieldRect.width, position.height);

                EditorGUI.LabelField(labelRect, label);

                EditorGUI.PropertyField(minFieldRect, property.FindPropertyRelative("min"), GUIContent.none);
                EditorGUI.PropertyField(maxFieldRect, property.FindPropertyRelative("max"), GUIContent.none);
            }

            var buttonPosition = new Rect(position);
            buttonPosition.x += position.width + 5;
            buttonPosition.width = 20;

            if (EditorGUI.DropdownButton(buttonPosition, new GUIContent("⮟"), FocusType.Passive))
            {
                void РandleItemClicked(object parameter)
                {
                    property.FindPropertyRelative("propertyType").enumValueIndex = (int)Enum.Parse(typeof(PropertyType), parameter.ToString());
                    property.serializedObject.ApplyModifiedProperties();
                }

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(PropertyType.Constant.ToString()), propertyType == (int)PropertyType.Constant, РandleItemClicked, PropertyType.Constant.ToString());
                menu.AddItem(new GUIContent(PropertyType.RandomBetweenTwoConstants.ToString()), propertyType == (int)PropertyType.RandomBetweenTwoConstants, РandleItemClicked, PropertyType.RandomBetweenTwoConstants.ToString());
                menu.DropDown(buttonPosition);
            }

            EditorGUI.EndProperty();
        }
    }
}