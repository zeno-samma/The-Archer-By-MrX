using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Flying Weapon Ability Data", menuName = "October/Abilities/Weapon Abilities/Flying Weapon")]
    public class FlyingWeaponAbilityData : GenericAbilityData<FlyingWeaponAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_FlyingWeapon;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_FlyingWeapon;
        }
    }

    [System.Serializable]
    public class FlyingWeaponAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier = 0.6f;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float attackDamageMultiplier = 0.6f;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}