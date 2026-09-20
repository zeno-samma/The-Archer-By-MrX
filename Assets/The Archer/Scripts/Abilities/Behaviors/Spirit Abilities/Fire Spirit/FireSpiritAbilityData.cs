using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "FireSpirit Ability Data", menuName = "October/Abilities/Spirit Abilities/Fire Spirit")]
    public class FireSpiritAbilityData : GenericAbilityData<FireSpiritAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_FireSpirit;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_FireSpirit;
        }
    }

    [System.Serializable]
    public class FireSpiritAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float spiritAttackDelay = 2f;
        public float SpiritAttackDelay => spiritAttackDelay;

        [SerializeField] protected float spiritDamageMultiplier;
        public float SpiritDamageMultiplier => spiritDamageMultiplier;

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