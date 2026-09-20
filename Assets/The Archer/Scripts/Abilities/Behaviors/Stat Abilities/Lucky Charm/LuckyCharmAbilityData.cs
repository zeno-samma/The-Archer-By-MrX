using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Lucky Charm Ability Data", menuName = "October/Abilities/Stat Abilities/Lucky Charm")]
    public class LuckyCharmAbilityData : GenericAbilityData<LuckyCharmAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_LuckyCharm;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_LuckyCharm;
        }
    }

    [System.Serializable]
    public class LuckyCharmAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float maxHPMultiplier = 1.1f;
        public float MaxHPMultiplier => maxHPMultiplier;

        [SerializeField, Range(0, 1)] protected float chance = 0.1f;
        public float Chance => chance;
    }
}