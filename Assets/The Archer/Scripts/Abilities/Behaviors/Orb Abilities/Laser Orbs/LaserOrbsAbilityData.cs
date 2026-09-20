using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Laser Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Laser Orbs")]
    public class LaserOrbsAbilityData : GenericAbilityData<LaserOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_LaserOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_LaserOrbs;
        }
    }

    [System.Serializable]
    public class LaserOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(0)] protected float orbDamageMultiplier = 1f;
        public float OrbDamageMultiplier => orbDamageMultiplier;

        [SerializeField, Min(0)] protected float laserDamageMultiplier = 0.5f;
        public float LaserDamageMultiplier => laserDamageMultiplier;

        [SerializeField, Min(0.1f)] protected float laserActiveDuration = 1f;
        public float LaserActiveDuration => laserActiveDuration;

        [SerializeField, Min(0.1f)] protected float laserDisabledDuration = 5f; 
        public float LaserDisabledDuration => laserDisabledDuration;
    }
}