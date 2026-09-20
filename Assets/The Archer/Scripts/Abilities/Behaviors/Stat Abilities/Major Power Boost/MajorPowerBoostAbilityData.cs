using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Major Power Boost Ability Data", menuName = "October/Abilities/Stat Abilities/Major Power Boost")]
    public class MajorPowerBoostAbilityData : GenericAbilityData<MajorPowerBoostAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MajorPowerBoost;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MajorPowerBoost;
        }
    }

    [System.Serializable]
    public class MajorPowerBoostAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}