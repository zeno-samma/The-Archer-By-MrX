using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "RareHeal Ability Data", menuName = "October/Abilities/Endgame Abilities/RareHeal")]
    public class RareHealAbilityData : GenericAbilityData<RareHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_RareHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_RareHeal;
        }
    }

    [System.Serializable]
    public class RareHealAbilityLevel : AbilityLevel
    {
        [SerializeField] protected Float healingProportion;
        public Float HealingProportion => healingProportion;
    }
}