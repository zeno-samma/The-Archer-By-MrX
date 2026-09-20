using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Poison Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Poison Orbs")]
    public class PoisonOrbsAbilityData : GenericAbilityData<PoisonOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_PoisonOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_PoisonOrbs;
        }
    }

    [System.Serializable]
    public class PoisonOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField, Range(0, 1)] protected float targetDamageReductionMultiplier = 0.6f;
        public float TargetDamageReductionMultiplier => targetDamageReductionMultiplier;

        [SerializeField] protected bool effectDealsDamageOverTime = false;
        public bool EffectDealsDamageOverTime => effectDealsDamageOverTime;

        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        public float EffectDamageMultiplier => effectDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;
        public float EffectDamageInterval => effectDamageInterval;
    }
}