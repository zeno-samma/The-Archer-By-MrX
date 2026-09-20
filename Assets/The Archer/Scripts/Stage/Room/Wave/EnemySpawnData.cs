using OctoberStudio.Drop;
using OctoberStudio.Enemy;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [SerializeField] protected EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        [SerializeField] protected TransformData transformData;
        public TransformData TransformData => transformData;

        [SerializeField] protected EnemyOverrideData overrideData;
        public EnemyOverrideData OverrideData => overrideData;

        public EnemySpawnData(EnemyType enemyType, Transform instanceTransform)
        {
            this.enemyType = enemyType;
            transformData = new TransformData(instanceTransform);
            if (instanceTransform.TryGetComponent<EnemyOverridesBehavior>(out var enemyOverridebehavior))
            {
                overrideData = new EnemyOverrideData(enemyOverridebehavior);
            }
        }

#if UNITY_EDITOR
        public EnemySpawnData(SerializedProperty property)
        {
            enemyType = (EnemyType)property.FindPropertyRelative("enemyType").intValue;

            transformData = new TransformData(property.FindPropertyRelative("transformData"));

            overrideData = new EnemyOverrideData(property.FindPropertyRelative("overrideData"));

        }

        public static implicit operator EnemySpawnData(SerializedProperty property)
        {
            return new EnemySpawnData(property);
        }

        public virtual void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("enemyType").intValue = (int)enemyType;
            transformData.SaveToSerializedProperty(property.FindPropertyRelative("transformData"));
            overrideData.SaveToSerializedProperty(property.FindPropertyRelative("overrideData"));
        }
#endif
    }

    [System.Serializable]
    public class EnemyOverrideData
    {
        [Tooltip("This drop will spawn in addition to the main drop of the enemy")]
        [SerializeField] protected List<EnemyDropData> additionalDrop;

        [Tooltip("This drop will override the main drop of the eney on DropType basis " +
            "(if there is a 'Heal' drop in the main enemy drop, but no 'Heal' drop in Drop Overrides, the enemy will still drop 'Heal')")]
        [SerializeField] protected List<EnemyDropData> dropOverrides;

        [Tooltip("This drop will remove drop from the main enemy drop")]
        [SerializeField] protected List<DropType> removeDrop = new List<DropType>();

        [Space]
        [SerializeField] protected bool overrideHP = false;
        [SerializeField] protected Float hp = 100;

        [SerializeField] protected bool overrideDamage = false;
        [SerializeField] protected Float damage = 1;

        public List<EnemyDropData> AdditionalDrop => additionalDrop;
        public List<EnemyDropData> DropOverrides => dropOverrides;
        public List<DropType> RemoveDrop => removeDrop;

        public bool OverrideHP => overrideHP;
        public Float HP => hp;
        public bool OverrideDamage => overrideDamage;
        public Float Damage => damage;

        public EnemyOverrideData(EnemyOverridesBehavior overridesBehavior)
        {
            additionalDrop = overridesBehavior.AdditionalDrop;
            dropOverrides = overridesBehavior.DropOverrides;
            removeDrop = overridesBehavior.RemoveDrop;

            overrideHP = overridesBehavior.OverrideHP;
            hp = overridesBehavior.HP;

            overrideDamage = overridesBehavior.OverrideDamage;
            damage = overridesBehavior.Damage;
        }

        public virtual float ApplyDamageOverride(Float baseDamage)
        {
            return overrideDamage ? damage : baseDamage;
        }

        public virtual float ApplyHPOverride(Float baseHP)
        {
            return overrideHP ? hp : baseHP;
        }

#if UNITY_EDITOR
        public EnemyOverrideData(SerializedProperty property)
        {
            var additionalDropProperty = property.FindPropertyRelative("additionalDrop");
            var dropOverridesProperty = property.FindPropertyRelative("dropOverrides");
            var removeDropProperty = property.FindPropertyRelative("removeDrop");
            var overrideHPProperty = property.FindPropertyRelative("overrideHP");
            var hpProperty = property.FindPropertyRelative("hp");
            var overrideDamageProperty = property.FindPropertyRelative("overrideDamage");
            var damageProperty = property.FindPropertyRelative("damage");

            additionalDrop = new List<EnemyDropData>(additionalDropProperty.arraySize);
            for (int i = 0; i < additionalDropProperty.arraySize; i++)
            {
                var dropProperty = additionalDropProperty.GetArrayElementAtIndex(i);
                additionalDrop.Add(new EnemyDropData(dropProperty));
            }

            dropOverrides = new List<EnemyDropData>(dropOverridesProperty.arraySize);
            for (int i = 0; i < dropOverridesProperty.arraySize; i++)
            {
                var dropProperty = dropOverridesProperty.GetArrayElementAtIndex(i);
                dropOverrides.Add(new EnemyDropData(dropProperty));
            }

            removeDrop = new List<DropType>(removeDropProperty.arraySize);
            for (int i = 0; i < removeDropProperty.arraySize; i++)
            {
                var dropProperty = removeDropProperty.GetArrayElementAtIndex(i);
                removeDrop.Add((DropType)dropProperty.intValue);
            }

            overrideHP = overrideHPProperty.boolValue;
            hp = new Float(hpProperty);

            overrideDamage = overrideDamageProperty.boolValue;
            damage = new Float(damageProperty);
        }

        public virtual void SaveToSerializedProperty(SerializedProperty property)
        {
            var additionalDropProperty = property.FindPropertyRelative("additionalDrop");
            var dropOverridesProperty = property.FindPropertyRelative("dropOverrides");
            var removeDropProperty = property.FindPropertyRelative("removeDrop");
            var overrideHPProperty = property.FindPropertyRelative("overrideHP");
            var hpProperty = property.FindPropertyRelative("hp");
            var overrideDamageProperty = property.FindPropertyRelative("overrideDamage");
            var damageProperty = property.FindPropertyRelative("damage");

            additionalDropProperty.arraySize = additionalDrop.Count;
            for (int i = 0; i < additionalDrop.Count; i++)
            {
                additionalDrop[i].SaveToSerializedProperty(additionalDropProperty.GetArrayElementAtIndex(i));
            }

            dropOverridesProperty.arraySize = dropOverrides.Count;
            for (int i = 0; i < dropOverrides.Count; i++)
            {
                dropOverrides[i].SaveToSerializedProperty(dropOverridesProperty.GetArrayElementAtIndex(i));
            }

            removeDropProperty.arraySize = removeDrop.Count;
            for (int i = 0; i < removeDrop.Count; i++)
            {
                removeDropProperty.GetArrayElementAtIndex(i).intValue = (int)removeDrop[i];
            }

            overrideHPProperty.boolValue = overrideHP;
            hp.SaveToSerializedProperty(hpProperty);
            overrideDamageProperty.boolValue = overrideDamage;
            damage.SaveToSerializedProperty(damageProperty);
        }
#endif
    }
}