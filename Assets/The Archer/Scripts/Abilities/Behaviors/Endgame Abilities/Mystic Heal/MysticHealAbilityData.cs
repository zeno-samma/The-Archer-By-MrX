using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "MysticHeal Ability Data", menuName = "October/Abilities/Endgame Abilities/MysticHeal")]
    public class MysticHealAbilityData : GenericAbilityData<MysticHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_MysticHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_MysticHeal;
        }
    }

    [System.Serializable]
    public class MysticHealAbilityLevel : AbilityLevel
    {
        [SerializeField] protected Float healingProportion;
        public Float HealingProportion => healingProportion;
    }
}