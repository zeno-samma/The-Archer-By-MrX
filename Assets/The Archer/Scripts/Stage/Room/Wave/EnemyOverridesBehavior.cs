using OctoberStudio.Drop;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class EnemyOverridesBehavior : MonoBehaviour
    {
        [Tooltip("This drop will spawn in addition to the main drop of the enemy")]
        [SerializeField] protected List<EnemyDropData> additionalDrop = new List<EnemyDropData>();

        [Tooltip("This drop will override the main drop of the eney on DropType basis " +
            "(if there is a 'Heal' drop in the main enemy drop, but no 'Heal' drop in Drop Overrides, the enemy will still drop 'Heal')")]
        [SerializeField] protected List<EnemyDropData> dropOverrides = new List<EnemyDropData>();

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

        public virtual void Init(EnemyOverrideData overrideData)
        {
            additionalDrop = overrideData.AdditionalDrop;
            dropOverrides = overrideData.DropOverrides;
            removeDrop = overrideData.RemoveDrop;

            overrideHP = overrideData.OverrideHP;
            hp = overrideData.HP;

            overrideDamage = overrideData.OverrideDamage;
            damage = overrideData.Damage;
        }
    }
}