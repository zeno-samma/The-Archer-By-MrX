using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Ultimate Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Ultimate Orbs")]
    public class UltimateOrbsAbilityData : GenericAbilityData<UltimateOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_UltimateOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_UltimateOrbs;
        }
    }

    [System.Serializable]
    public class UltimateOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;
    }
}