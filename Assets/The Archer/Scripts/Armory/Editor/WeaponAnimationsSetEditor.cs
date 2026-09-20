using UnityEditor;

namespace OctoberStudio.Armory
{
    [CustomEditor(typeof(WeaponAnimationsSet))]
    public class WeaponAnimationsSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var typeProperty = serializedObject.FindProperty("weaponAnimationsSetType");
            var speedProperty = serializedObject.FindProperty("movementAnimationSpeed");

            EditorGUILayout.PropertyField(speedProperty);
            EditorGUILayout.PropertyField(typeProperty);
            
            if(typeProperty.intValue == (int)WeaponAnimationsSetType.Separate_Animations)
            {
                var animationsProperty = serializedObject.FindProperty("animations");
                EditorGUILayout.PropertyField(animationsProperty);
            } else
            {
                var attackAnimationLengthProperty = serializedObject.FindProperty("attackAnimationLength");
                EditorGUILayout.PropertyField(attackAnimationLengthProperty);
                if(attackAnimationLengthProperty.floatValue <= 0) attackAnimationLengthProperty.floatValue = 1;

                var animatorProperty = serializedObject.FindProperty("runtimeAnimatorController");
                EditorGUILayout.PropertyField(animatorProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}