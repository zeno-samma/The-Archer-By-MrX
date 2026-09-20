using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Orb Enhancement Ability Data", menuName = "October/Abilities/Orb Abilities/Orb Enhancement")]
    public class OrbEnhancementAbilityData : GenericAbilityData<OrbEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_OrbEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_OrbEnhancement;
        }
    }

    [System.Serializable] 
    public class OrbEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] protected float orbsDamageMultiplier;
        public float OrbsDamageMultiplier => orbsDamageMultiplier;
    }
}