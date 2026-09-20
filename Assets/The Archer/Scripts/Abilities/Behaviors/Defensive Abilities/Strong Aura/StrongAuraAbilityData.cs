using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "StrongAura Ability Data", menuName = "October/Abilities/Defensive Abilities/StrongAura")]
    public class StrongAuraAbilityData : GenericAbilityData<StrongAuraAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_StrongAura;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_StrongAura;
        }
    }

    [System.Serializable]
    public class StrongAuraAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float abilityRadius;
        public float AbilityRadius => abilityRadius;

        [SerializeField] protected float enemyDamageReceivedMultiplierInsideRadius;
        public float EnemyDamageReceivedMultiplierInsideRadius => enemyDamageReceivedMultiplierInsideRadius;

        [SerializeField] protected float enemyDamageMultiplierWhenEntering;
        public float EnemyDamageMultiplierWhenEntering => enemyDamageMultiplierWhenEntering;
    }
}