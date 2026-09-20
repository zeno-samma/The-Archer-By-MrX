using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Ignite Ability Data", menuName = "October/Abilities/Elemental Abilities/Ignite")]
    public class IgniteAbilityData : GenericAbilityData<IgniteAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_Ignite;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_Ignite;
        }
    }

    [System.Serializable]
    public class IgniteAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

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