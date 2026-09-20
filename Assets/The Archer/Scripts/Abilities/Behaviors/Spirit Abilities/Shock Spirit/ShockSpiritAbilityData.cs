using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "ShockSpirit Ability Data", menuName = "October/Abilities/Spirit Abilities/ShockSpirit")]
    public class ShockSpiritAbilityData : GenericAbilityData<ShockSpiritAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_ShockSpirit;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_ShockSpirit;
        }
    }

    [System.Serializable]
    public class ShockSpiritAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float spiritAttackDelay = 2f;
        public float SpiritAttackDelay => spiritAttackDelay;

        [SerializeField] protected float spiritDamageMultiplier;
        public float SpiritDamageMultiplier => spiritDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField] protected GameObject shockSpreadProjectulePrefab;
        public GameObject ShockSpreadProjectulePrefab => shockSpreadProjectulePrefab;

        [SerializeField, Min(0)] protected float shockSpreadRadius = 1;
        public float ShockSpreadRadius => shockSpreadRadius;

        [SerializeField, Range(0, 1)] protected float shockSpreadDamageMultiplier;
        public float ShockSpreadDamageMultiplier => shockSpreadDamageMultiplier;

        [SerializeField] protected bool effectDealsDamageOverTime = false;
        public bool EffectDealsDamageOverTime => effectDealsDamageOverTime;

        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        public float EffectDamageMultiplier => effectDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;
        public float EffectDamageInterval => effectDamageInterval;
    }
}