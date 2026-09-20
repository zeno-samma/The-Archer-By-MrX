using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class Float
    {
        [SerializeField] protected float value = 0f;
        [SerializeField] protected float min = 0f;
        [SerializeField] protected float max = 1f;

        [SerializeField] protected PropertyType propertyType = PropertyType.Constant;
        public PropertyType Type => propertyType;

        public float Min
        {
            get => propertyType == PropertyType.Constant ? value : min;
        }

        public float Max
        {
            get => propertyType == PropertyType.Constant ? value : max;
        }

        public float Value
        {
            get
            {
                if (propertyType == PropertyType.Constant)
                {
                    return value;
                }
                else if (propertyType == PropertyType.RandomBetweenTwoConstants)
                {
                    return Random.Range(min, max);
                }
                return 0f;
            }
        }

        public Float()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Float floatProperty)
        {
            return floatProperty.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Float(float value)
        {
            return new Float { value = value, propertyType = PropertyType.Constant };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Float((float min, float max) tuple)
        {
            return new Float { min = tuple.min, max = tuple.max, propertyType = PropertyType.RandomBetweenTwoConstants };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float operator *(Float one, float two)
        {
            if (one.propertyType == PropertyType.Constant) return one.value * two;
            return (one.min * two, one.max * two);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Float operator *(Float one, Float two)
        {
            if (one.propertyType == PropertyType.Constant)
            {
                if (two.propertyType == PropertyType.Constant)
                {
                    return one.value * two.value;
                }
                else
                {
                    return (one.value * two.min, one.value * two.max);
                }
            }

            if (two.propertyType == PropertyType.Constant)
            {
                return (one.min * two.value, one.max * two.value);
            }
            else
            {
                return (one.min * two.min, one.max * two.max);
            }
        }

#if UNITY_EDITOR
        public Float(SerializedProperty property)
        {
            value = property.FindPropertyRelative("value").floatValue;
            min = property.FindPropertyRelative("min").floatValue;
            max = property.FindPropertyRelative("max").floatValue;
            propertyType = (PropertyType)property.FindPropertyRelative("propertyType").intValue;
        }

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("value").floatValue = value;
            property.FindPropertyRelative("min").floatValue = min;
            property.FindPropertyRelative("max").floatValue = max;
            property.FindPropertyRelative("propertyType").intValue = (int)propertyType;
        }
#endif
    }
}