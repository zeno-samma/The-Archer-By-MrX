using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "CommonHeal Ability Data", menuName = "October/Abilities/Endgame Abilities/CommonHeal")]
    public class CommonHealAbilityData : GenericAbilityData<CommonHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_CommonHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_CommonHeal;
        }
    }

    [System.Serializable]
    public class CommonHealAbilityLevel : AbilityLevel
    {
        [SerializeField] protected Float healingProportion;
        public Float HealingProportion => healingProportion;
    }
}