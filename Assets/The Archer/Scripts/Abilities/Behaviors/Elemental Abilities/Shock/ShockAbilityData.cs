using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Shock Ability Data", menuName = "October/Abilities/Elemental Abilities/Shock")]
    public class ShockAbilityData : GenericAbilityData<ShockAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_Shock;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_Shock;
        }
    }

    [System.Serializable]
    public class ShockAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        public float EffectDuration => effectDuration;

        [SerializeField] protected GameObject shockSpreadProjectulePrefab;
        public GameObject ShockSpreadProjectulePrefab => shockSpreadProjectulePrefab;

        [SerializeField, Min(0)] protected float shockSpreadRadius = 1;
        public float ShockSpreadRadius => shockSpreadRadius;

        [SerializeField, Range(0, 1)] protected float shockSpreadDamageMultiplier;
        public float ShockSpreadDamageMultiplier => shockSpreadDamageMultiplier;

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