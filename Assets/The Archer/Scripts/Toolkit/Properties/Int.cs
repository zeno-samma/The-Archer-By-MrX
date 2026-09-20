using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class Int
    {
        [SerializeField] protected int value = 0;
        [SerializeField] protected int min = 0;
        [SerializeField] protected int max = 1;

        [SerializeField] protected PropertyType propertyType = PropertyType.Constant;

        public Int()
        {

        }

        public int Value
        {
            get
            {
                if (propertyType == PropertyType.Constant)
                {
                    return value;
                }
                else if (propertyType == PropertyType.RandomBetweenTwoConstants)
                {
                    return Random.Range(min, max + 1);
                }
                return 0;
            }
        }

        public static implicit operator int(Int floatProperty)
        {
            return floatProperty.Value;
        }

        public static implicit operator Int(int value)
        {
            return new Int { value = value, propertyType = PropertyType.Constant };
        }

        public static implicit operator Int((int min, int max) tuple)
        {
            return new Int { min = tuple.min, max = tuple.max, propertyType = PropertyType.RandomBetweenTwoConstants };
        }

#if UNITY_EDITOR

        public Int(SerializedProperty property)
        {
            value = property.FindPropertyRelative("value").intValue;
            min = property.FindPropertyRelative("min").intValue;
            max = property.FindPropertyRelative("max").intValue;
            propertyType = (PropertyType)property.FindPropertyRelative("propertyType").intValue;
        }

        public static implicit operator Int(SerializedProperty property)
        {
            return new Int(property);
        }

        public virtual void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("value").intValue = value;
            property.FindPropertyRelative("min").intValue = min;
            property.FindPropertyRelative("max").intValue = max;
            property.FindPropertyRelative("propertyType").intValue = (int)propertyType;
        }

#endif
    }
}