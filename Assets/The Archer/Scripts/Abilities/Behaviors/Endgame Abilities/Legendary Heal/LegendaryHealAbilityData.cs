using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "LegendaryHeal Ability Data", menuName = "October/Abilities/Endgame Abilities/LegendaryHeal")]
    public class LegendaryHealAbilityData : GenericAbilityData<LegendaryHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_LegendaryHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_LegendaryHeal;
        }
    }

    [System.Serializable]
    public class LegendaryHealAbilityLevel : AbilityLevel
    {
        [SerializeField] protected Float healingProportion;
        public Float HealingProportion => healingProportion;
    }
}