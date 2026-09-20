using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Trifecta Ability Data", menuName = "October/Abilities/Stat Abilities/Trifecta")]
    public class TrifectaAbilityData : GenericAbilityData<TrifectaAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_Trifecta;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_Trifecta;
        }
    }

    [System.Serializable]
    public class TrifectaAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;
    }
}