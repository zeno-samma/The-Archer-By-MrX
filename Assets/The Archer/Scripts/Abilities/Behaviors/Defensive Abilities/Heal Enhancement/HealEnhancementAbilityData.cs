using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "HealEnhancement Ability Data", menuName = "October/Abilities/Defensive Abilities/HealEnhancement")]
    public class HealEnhancementAbilityData : GenericAbilityData<HealEnhancementAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_HealEnhancement;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_HealEnhancement;
        }
    }

    [System.Serializable]
    public class HealEnhancementAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;

        [SerializeField] protected float healingMultiplier;
        public float HealingMultiplier => healingMultiplier;
    }
}