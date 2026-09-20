using OctoberStudio.Abilities;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class TestingPreset
    {
        [SerializeField] protected string name;
        [SerializeField] protected bool enabledInEditor = false;
        [SerializeField] protected bool enabledInBuild = false;

        public bool EnabledInEditor => enabledInEditor;
        public bool EnabledInBuild => enabledInBuild;

        [SerializeField] List<AbilityDev> abilities = new List<AbilityDev>();
        public List<AbilityDev> Abilities => abilities;

#if UNITY_EDITOR

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            var nameProperty = property.FindPropertyRelative("name");
            var enabledInEditorProperty = property.FindPropertyRelative("enabledInEditor");
            var enabledInBuildProperty = property.FindPropertyRelative("enabledInBuild");

            nameProperty.stringValue = name;
            enabledInEditorProperty.boolValue = enabledInEditor;
            enabledInBuildProperty.boolValue = enabledInBuild;

            var abilitiesProperty = property.FindPropertyRelative("abilities");
            abilitiesProperty.arraySize = abilities.Count;
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].SaveToSerializedProperty(abilitiesProperty.GetArrayElementAtIndex(i));
            }
        }
#endif
    }

    [System.Serializable]
    public class AbilityDev
    {
        public AbilityType abilityType;
        public int level;

#if UNITY_EDITOR

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            var abilityTypeProperty = property.FindPropertyRelative("abilityType");
            var levelProperty = property.FindPropertyRelative("level");

            abilityTypeProperty.intValue = (int)abilityType;
            levelProperty.intValue = level;
        }
#endif
    }
}

