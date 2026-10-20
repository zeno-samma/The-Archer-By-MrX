using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "Protector Ability Data", menuName = "MrX Studio/Abilities/Defensive Abilities/Protector")]
    public class ProtectorAbilityData : GenericAbilityData<ProtectorAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_Protector;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_Protector;
        }
    }

    [System.Serializable]
    public class ProtectorAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;

        [SerializeField] protected float invincibilityDuration;
        public float InvincibilityDuration => invincibilityDuration;

        [SerializeField] protected float invincibilityCooldown;
        public float InvincibilityCooldown => invincibilityCooldown;
    }
}