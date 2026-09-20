using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Major Health Boost Ability Data", menuName = "October/Abilities/Stat Abilities/Major Health Boost")]
    public class MajorHealthBoostAbilityData : GenericAbilityData<MajorHealthBoostAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MajorHealthBoost;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MajorHealthBoost;
        }
    }

    [System.Serializable]
    public class MajorHealthBoostAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier;
        public float MaxHPMultiplier => maxHPMultiplier;
    }
}