using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "IgniteEnhancement Ability Data", menuName = "October/Abilities/Elemental Abilities/IgniteEnhancement")]
    public class IgniteEnhancementAbilityData : GenericAbilityData<IgniteEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_IgniteEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_IgniteEnhancement;
        }
    }

    [System.Serializable]
    public class IgniteEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float igniteDamageMultiplier = 1f;
        public float IgniteDamageMultiplier => igniteDamageMultiplier;

        [SerializeField] protected float arrowSpeedMultiplier = 1f;
        public float ArrowSpeedMultiplier => arrowSpeedMultiplier;
    }
}