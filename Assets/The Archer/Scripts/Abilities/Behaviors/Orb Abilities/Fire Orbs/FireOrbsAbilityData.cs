using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Fire Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Fire Orbs")]
    public class FireOrbsAbilityData : GenericAbilityData<FireOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_FireOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_FireOrbs;
        }
    }

    [System.Serializable]
    public class FireOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField] protected bool effectDealsDamageOverTime = false;
        public bool EffectDealsDamageOverTime => effectDealsDamageOverTime;

        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        public float EffectDamageMultiplier => effectDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;
        public float EffectDamageInterval => effectDamageInterval;
    }
}