using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Bullseye Ability Data", menuName = "October/Abilities/Weapon Abilities/Bullseye")]
    public class BullseyeAbilityData : GenericAbilityData<BullseyeAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_Bullseye;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_Bullseye;
        }
    }

    [System.Serializable]
    public class BullseyeAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier = 1.25f;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float magnetismMultiplier = 1.25f;
        public float MagnetismMultiplier => magnetismMultiplier;
    }
}