using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Heal Ability Data", menuName = "October/Abilities/Defensive Abilities/Heal")]
    public class HealAbilityData : GenericAbilityData<HealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_Heal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_Heal;
        }
    }

    [System.Serializable]
    public class HealAbilityLevel : AbilityLevel
    {
        [SerializeField] protected Float healingProportion;
        public Float HealingProportion => healingProportion;
    }
}