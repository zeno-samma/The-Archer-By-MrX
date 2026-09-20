using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Spirit Rage Ability Data", menuName = "October/Abilities/Spirit Abilities/Spirit Rage")]
    public class SpiritRageAbilityData : GenericAbilityData<SpiritRageAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_SpiritRage;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_SpiritRage;
        }
    }

    [System.Serializable]
    public class SpiritRageAbilityLevel : AbilityLevel
    {
        [SerializeField] float spiritsDamageMultiplier;
        public float SpiritsDamageMultiplier => spiritsDamageMultiplier;

        [SerializeField] float spiritsAttackDelayMultiplier;
        public float SpiritsAttackDelayMultiplier => spiritsAttackDelayMultiplier;
    }
}