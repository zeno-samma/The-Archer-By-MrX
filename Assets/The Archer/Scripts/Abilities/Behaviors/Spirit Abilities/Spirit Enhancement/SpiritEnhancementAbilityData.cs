using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Spirit Enhancement Ability Data", menuName = "October/Abilities/Spirit Abilities/Spirit Enhancement")]
    public class SpiritEnhancementAbilityData : GenericAbilityData<SpiritEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_SpiritEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_SpiritEnhancement;
        }
    }

    [System.Serializable]
    public class SpiritEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] float spiritsDamageMultiplier;
        public float SpiritsDamageMultiplier => spiritsDamageMultiplier;
    }
}