using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Cure Ability Data", menuName = "October/Abilities/Defensive Abilities/Cure")]
    public class CureAbilityData : GenericAbilityData<CureAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_Cure;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_Cure;
        }
    }

    [System.Serializable]
    public class CureAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;

        [SerializeField, Range(0, 1)] protected float maxHealthProportionHeal;
        public float MaxHealthProportionHeal => maxHealthProportionHeal;
    }
}