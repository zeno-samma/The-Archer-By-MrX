using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Combo Ability Data", menuName = "October/Abilities/Buff Abilities/Combo")]
    public class ComboAbilityData : GenericAbilityData<ComboAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Buff_Combo;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Buff_Combo;
        }
    }

    [System.Serializable]
    public class ComboAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float abilityDuration;
        public float AbilityDuration => abilityDuration;

        [SerializeField] protected float attackDamageMultiplierPerEnemyKilled;
        public float AttackDamageMultiplierPerEnemyKilled => attackDamageMultiplierPerEnemyKilled;

        [SerializeField] protected float maxAttackDamageMultiplier;
        public float MaxAttackDamageMultiplier => maxAttackDamageMultiplier;
    }
}