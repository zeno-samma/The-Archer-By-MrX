using OctoberStudio.Armory;
using OctoberStudio.Drop;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class EnemyDropData
    {
        [SerializeField] protected DropType dropType;
        [SerializeField] protected ItemData itemData;

        [SerializeField] protected List<HPZone> hpZones;

#if UNITY_EDITOR
        [SerializeField] protected int selectedZoneId;
#endif

        public DropType DropType => dropType;
        public ItemData ItemData => itemData;

        public float GetChance(float hpProportion)
        {
            if (hpZones.Count == 0) return 0f;
            if (hpZones.Count == 1) return hpZones[0].Chance;

            var hpSum = 0f;

            for(int i = 0; i < hpZones.Count; i++)
            {
                hpSum += hpZones[i].HP;
                if (hpProportion < hpSum)
                {
                    return hpZones[i].Chance;
                }
            }

            return 0;
        }

        public Int GetDropAmount(float hpProportion)
        {
            if (hpZones.Count == 0) return 0;
            if (hpZones.Count == 1) return hpZones[0].DropAmount;

            var hpSum = 0f;

            for (int i = 0; i < hpZones.Count; i++)
            {
                hpSum += hpZones[i].HP;
                if (hpProportion < hpSum)
                {
                    return hpZones[i].DropAmount;
                }
            }

            return 0;
        }

#if UNITY_EDITOR
        public EnemyDropData(SerializedProperty property)
        {
            dropType = (DropType)property.FindPropertyRelative("dropType").intValue;
            itemData = (ItemData)property.FindPropertyRelative("itemData").objectReferenceValue;
            
            var hpZonesProperty = property.FindPropertyRelative("hpZones");
            hpZones = new List<HPZone>(hpZonesProperty.arraySize);

            for (int i = 0; i < hpZonesProperty.arraySize; i++)
            {
                hpZones.Add(hpZonesProperty.GetArrayElementAtIndex(i));
            }            
        }

        public static implicit operator EnemyDropData(SerializedProperty property)
        {
            return new EnemyDropData(property);
        }

        public virtual void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("dropType").intValue = (int)dropType;
            property.FindPropertyRelative("itemData").objectReferenceValue = itemData;

            var hpZonesProperty = property.FindPropertyRelative("hpZones");
            hpZonesProperty.arraySize = hpZones.Count;

            for(int i = 0; i < hpZones.Count; i++)
            {
                hpZones[i].SaveToSerializedProperty(hpZonesProperty.GetArrayElementAtIndex(i));
            }
        }
#endif

        [System.Serializable]
        public class HPZone
        {
            [SerializeField] protected float hp;
            [SerializeField] protected Int dropAmount;
            [SerializeField, Range(0, 100)] protected float chance;

            public float HP => hp;
            public Int DropAmount => dropAmount;
            public float Chance => chance;

#if UNITY_EDITOR

            public HPZone(SerializedProperty property)
            {
                hp = property.FindPropertyRelative("hp").floatValue;
                dropAmount = property.FindPropertyRelative("dropAmount");
                chance = property.FindPropertyRelative("chance").floatValue;
            }

            public static implicit operator HPZone(SerializedProperty property)
            {
                return new HPZone(property);
            }

            public virtual void SaveToSerializedProperty(SerializedProperty property)
            {
                property.FindPropertyRelative("hp").floatValue = hp;
                dropAmount.SaveToSerializedProperty(property.FindPropertyRelative("dropAmount"));
                property.FindPropertyRelative("chance").floatValue = chance;
            }
#endif
        }
    }
}