using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Close Combat Ability Data", menuName = "October/Abilities/Weapon Abilities/Close Combat")]
    public class CloseCombatAbilityData : GenericAbilityData<CloseCombatAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_CloseCombat;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_CloseCombat;
        }
    }

    [System.Serializable]
    public class CloseCombatAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float arrowRangeMultiplier;
        public float ArrowRangeMultiplier => arrowRangeMultiplier;

        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}