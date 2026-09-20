using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "PoisonEnhancement Ability Data", menuName = "October/Abilities/Elemental Abilities/PoisonEnhancement")]
    public class PoisonEnhancementAbilityData : GenericAbilityData<PoisonEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_PoisonEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_PoisonEnhancement;
        }
    }

    [System.Serializable]
    public class PoisonEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float poisonDamageMultiplier = 1f;
        public float PoisonDamageMultiplier => poisonDamageMultiplier;

        [SerializeField] protected float arrowSpeedMultiplier = 1f;
        public float ArrowSpeedMultiplier => arrowSpeedMultiplier;
    }
}