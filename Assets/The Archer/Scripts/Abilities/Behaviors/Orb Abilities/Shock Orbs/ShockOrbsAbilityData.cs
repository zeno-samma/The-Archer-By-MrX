using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "ShockOrbs Ability Data", menuName = "October/Abilities/Orb Abilities/ShockOrbs")]
    public class ShockOrbsAbilityData : GenericAbilityData<ShockOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_ShockOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_ShockOrbs;
        }
    }

    [System.Serializable]
    public class ShockOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;

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