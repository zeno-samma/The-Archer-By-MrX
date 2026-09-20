using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Minor Health Boost Ability Data", menuName = "October/Abilities/Stat Abilities/Minor Health Boost")]
    public class MinorHealthBoostAbilityData : GenericAbilityData<MinorHealthBoostAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MinorHealthBoost;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MinorHealthBoost;
        }
    }

    [System.Serializable]
    public class MinorHealthBoostAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;
    }
}