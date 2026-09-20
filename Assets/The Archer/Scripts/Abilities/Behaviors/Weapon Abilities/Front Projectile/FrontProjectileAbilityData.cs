using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Front Projectile Ability Data", menuName = "October/Abilities/Weapon Abilities/Front Projectile")]
    public class FrontProjectileAbilityData : GenericAbilityData<FrontProjectileAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_FrontProjectile;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_FrontProjectile;
        }
    }

    [System.Serializable]
    public class FrontProjectileAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of additional front arrows. If this value is 1, Player will shoot 2 front arrows")]
        [SerializeField, Min(1)] int additionalFrontArrowsCount = 1;
        public int AdditionalFrontArrowsCount => additionalFrontArrowsCount;

        [SerializeField, Min(0.01f)] float frontArrowsDamageMultiplier = 0.8f;
        public float FrontArrowsDamageMultiplier => frontArrowsDamageMultiplier;
    }
}