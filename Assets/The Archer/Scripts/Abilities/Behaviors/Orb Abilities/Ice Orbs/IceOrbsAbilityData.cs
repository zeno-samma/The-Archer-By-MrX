using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Ice Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Ice Orbs")]
    public class IceOrbsAbilityData : GenericAbilityData<IceOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_IceOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_IceOrbs;
        }
    }

    [System.Serializable]
    public class IceOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;

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