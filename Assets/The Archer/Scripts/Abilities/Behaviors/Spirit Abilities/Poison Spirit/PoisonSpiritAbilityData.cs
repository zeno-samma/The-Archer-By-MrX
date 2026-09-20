using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "PoisonSpirit Ability Data", menuName = "October/Abilities/Spirit Abilities/PoisonSpirit")]
    public class PoisonSpiritAbilityData : GenericAbilityData<PoisonSpiritAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_PoisonSpirit;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_PoisonSpirit;
        }
    }

    [System.Serializable]
    public class PoisonSpiritAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float spiritAttackDelay = 2f;
        public float SpiritAttackDelay => spiritAttackDelay;

        [SerializeField] protected float spiritDamageMultiplier;
        public float SpiritDamageMultiplier => spiritDamageMultiplier;

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
