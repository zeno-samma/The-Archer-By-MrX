using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Vampiric Orbs Ability Data", menuName = "October/Abilities/Orb Abilities/Vampiric Orbs")]
    public class VampiricOrbsAbilityData : GenericAbilityData<VampiricOrbsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Orb_VampiricOrbs;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Orb_VampiricOrbs;
        }
    }

    [System.Serializable]
    public class VampiricOrbsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float orbDamageMultiplier;
        public float OrbDamageMultiplier => orbDamageMultiplier;

        [SerializeField] protected float vampirismMultiplier;
        public float VampirismMultiplier => vampirismMultiplier;
    }
}