using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Multi Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Multi Shot")]
    public class MultiShotAbilityData : GenericAbilityData<MultiShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_MultiShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_MultiShot;
        }
    }

    [System.Serializable] 
    public class MultiShotAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int additionalShotsCount = 1;
        public int AdditionalShotsCount => additionalShotsCount;

        [SerializeField] protected float attackDamageMultiplier = 0.8f;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float attackSpeedMultiplier = 0.8f;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
    }
}