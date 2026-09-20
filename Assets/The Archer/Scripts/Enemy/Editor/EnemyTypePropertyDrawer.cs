using OctoberStudio.Enemy;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    [CustomPropertyDrawer(typeof(EnemyType))]
    public class EnemyTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.width -= 30;

            EditorGUI.PropertyField(position, property, label);

            var buttonPosition = new Rect(position);
            buttonPosition.x += position.width + 5;
            buttonPosition.width = 25;

            if (GUI.Button(buttonPosition, "•"))
            {
                EnemyTypeWindow.ShowWindow();
            }

            EditorGUI.EndProperty();
        }
    }
}