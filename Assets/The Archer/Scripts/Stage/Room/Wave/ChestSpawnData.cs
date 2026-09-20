using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class ChestSpawnData
    {
        // The unique id of an enemy (GUID)
        [SerializeField] protected ChestType chestType;
        public ChestType ChestType => chestType;

        [SerializeField] protected TransformData transformData;

        public TransformData TransformData => transformData;

        public ChestSpawnData(ChestType chestType, Transform instanceTransform)
        {
            this.chestType = chestType;
            transformData = new TransformData(instanceTransform);
        }

#if UNITY_EDITOR
        public ChestSpawnData(SerializedProperty property)
        {
            chestType = (ChestType)property.FindPropertyRelative("chestType").intValue;

            transformData = new TransformData(property.FindPropertyRelative("transformData"));
        }

        public static implicit operator ChestSpawnData(SerializedProperty property)
        {
            return new ChestSpawnData(property);
        }

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("chestType").intValue = (int)chestType;
            transformData.SaveToSerializedProperty(property.FindPropertyRelative("transformData"));
        }

#endif
    }
}
