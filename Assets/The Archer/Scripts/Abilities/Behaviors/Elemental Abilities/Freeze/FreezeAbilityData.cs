using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Freeze Ability Data", menuName = "October/Abilities/Elemental Abilities/Freeze")]
    public class FreezeAbilityData : GenericAbilityData<FreezeAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_Freeze;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_Freeze;
        }
    }

    [System.Serializable]
    public class FreezeAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField, Range(0, 1)] protected float targetSpeedMultiplier = 0;
        public float TargetSpeedMultiplier => targetSpeedMultiplier;

        [SerializeField, Range(0, 2)] protected float arrowDamageMultiplier = 0.8f;
        public float ArrowDamageMultiplier => arrowDamageMultiplier;

        [SerializeField] protected bool effectDealsDamageOverTime = false;
        public bool EffectDealsDamageOverTime => effectDealsDamageOverTime;

        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        public float EffectDamageMultiplier => effectDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;
        public float EffectDamageInterval => effectDamageInterval;
    }
}