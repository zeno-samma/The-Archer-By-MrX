using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "FreezeEnhancement Ability Data", menuName = "October/Abilities/Elemental Abilities/FreezeEnhancement")]
    public class FreezeEnhancementAbilityData : GenericAbilityData<FreezeEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_FreezeEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_FreezeEnhancement;
        }
    }

    [System.Serializable]
    public class FreezeEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float freezeDamageMultiplier = 1f;
        public float FreezeDamageMultiplier => freezeDamageMultiplier;

        [SerializeField] protected float arrowSpeedMultiplier = 1f;
        public float ArrowSpeedMultiplier => arrowSpeedMultiplier;
    }
}