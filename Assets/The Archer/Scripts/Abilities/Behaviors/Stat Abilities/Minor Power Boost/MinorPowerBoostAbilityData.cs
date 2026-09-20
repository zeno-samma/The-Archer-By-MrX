using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Minor Power Boost Ability Data", menuName = "October/Abilities/Stat Abilities/Minor Power Boost")]
    public class MinorPowerBoostAbilityData : GenericAbilityData<MinorPowerBoostAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MinorPowerBoost;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MinorPowerBoost;
        }
    }

    [System.Serializable]
    public class MinorPowerBoostAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}