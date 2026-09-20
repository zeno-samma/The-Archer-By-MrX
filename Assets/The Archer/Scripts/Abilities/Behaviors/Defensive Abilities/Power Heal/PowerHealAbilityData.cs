using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "PowerHeal Ability Data", menuName = "October/Abilities/Defensive Abilities/PowerHeal")]
    public class PowerHealAbilityData : GenericAbilityData<PowerHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_PowerHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_PowerHeal;
        }
    }

    [System.Serializable]
    public class PowerHealAbilityLevel : AbilityLevel
    {
        [SerializeField] float attackDamageMultiplier = 1f;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField, Range(0, 1)] float chance = 0.1f;
        public float Chance => chance;
    }
}