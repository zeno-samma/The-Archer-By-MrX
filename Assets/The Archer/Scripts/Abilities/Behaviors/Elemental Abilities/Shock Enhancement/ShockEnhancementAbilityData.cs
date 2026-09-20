using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "ShockEnhancement Ability Data", menuName = "October/Abilities/Elemental Abilities/ShockEnhancement")]
    public class ShockEnhancementAbilityData : GenericAbilityData<ShockEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_ShockEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_ShockEnhancement;
        }
    }

    [System.Serializable]
    public class ShockEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float shockDamageMultiplier = 1f;
        public float ShockDamageMultiplier => shockDamageMultiplier;

        [SerializeField] protected float arrowSpeedMultiplier = 1f;
        public float ArrowSpeedMultiplier => arrowSpeedMultiplier;
    }
}