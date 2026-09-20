using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Minor Attack Speed Boost Ability Data", menuName = "October/Abilities/Stat Abilities/Minor Attack Speed Boost")]
    public class MinorAttackSpeedBoostAbilityData : GenericAbilityData<MinorAttackSpeedBoostAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MinorAttackSpeedBoost;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MinorAttackSpeedBoost;
        }
    }

    [System.Serializable]
    public class MinorAttackSpeedBoostAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
    }
}