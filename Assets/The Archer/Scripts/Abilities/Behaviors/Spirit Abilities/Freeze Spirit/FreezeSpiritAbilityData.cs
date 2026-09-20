using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "FreezeSpirit Ability Data", menuName = "October/Abilities/Spirit Abilities/Freeze Spirit")]
    public class FreezeSpiritAbilityData : GenericAbilityData<FreezeSpiritAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_FreezeSpirit;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_FreezeSpirit;
        }
    }

    [System.Serializable]
    public class FreezeSpiritAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float spiritAttackDelay = 2f;
        public float SpiritAttackDelay => spiritAttackDelay;

        [SerializeField] protected float spiritDamageMultiplier;
        public float SpiritDamageMultiplier => spiritDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField, Range(0, 1)] protected float targetSpeedMultiplier = 0;
        public float TargetSpeedMultiplier => targetSpeedMultiplier;

        [SerializeField] protected bool effectDealsDamageOverTime = false;
        public bool EffectDealsDamageOverTime => effectDealsDamageOverTime;

        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        public float EffectDamageMultiplier => effectDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;
        public float EffectDamageInterval => effectDamageInterval;
    }
}