using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "RagePotion Ability Data", menuName = "October/Abilities/Buff Abilities/RagePotion")]
    public class RageStarAbilityData : GenericAbilityData<RageStarAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Buff_RageStar;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Buff_RageStar;
        }
    }

    [System.Serializable]
    public class RageStarAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float abilityDuration = 4f;
        public float AbilityDuration => abilityDuration;

        [SerializeField] protected float starSpawnInterval = 5f;
        public float StarSpawnInterval => starSpawnInterval;

        [SerializeField] protected int critChangeIncreasePercent = 25;
        public int CritChangeIncreasePercent => critChangeIncreasePercent;

        [SerializeField] protected float attackSpeedMultiplier = 1.5f;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
    }
}