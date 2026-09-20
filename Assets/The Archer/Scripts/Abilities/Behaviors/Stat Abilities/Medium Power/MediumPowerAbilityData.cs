using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Medium Power Ability Data", menuName = "October/Abilities/Stat Abilities/Medium Power")]
    public class MediumPowerAbilityData : GenericAbilityData<MediumPowerAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MediumPower;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MediumPower;
        }
    }

    [System.Serializable]
    public class MediumPowerAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier = 1.25f;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}