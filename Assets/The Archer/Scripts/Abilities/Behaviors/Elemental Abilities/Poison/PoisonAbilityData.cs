using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Poison Ability Data", menuName = "October/Abilities/Elemental Abilities/Poison")]
    public class PoisonAbilityData : GenericAbilityData<PoisonAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_Poison;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_Poison;
        }
    }

    [System.Serializable]
    public class PoisonAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField, Range(0, 1)] protected float targetDamageReductionMultiplier = 0.6f;
        public float TargetDamageReductionMultiplier => targetDamageReductionMultiplier;

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