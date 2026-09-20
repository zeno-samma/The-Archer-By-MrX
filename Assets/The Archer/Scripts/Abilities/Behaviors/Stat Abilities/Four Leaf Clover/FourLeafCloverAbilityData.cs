using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Four Leaf Clover Ability Data", menuName = "October/Abilities/Stat Abilities/Four Leaf Clover")]
    public class FourLeafCloverAbilityData : GenericAbilityData<FourLeafCloverAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_FourLeafClover;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_FourLeafClover;
        }
    }

    [System.Serializable]
    public class FourLeafCloverAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(0, 100)] public int chanceToIncreaseRarityPrecent = 25;
        public int ChanceToIncreaseRarityPercent => chanceToIncreaseRarityPrecent;

        [SerializeField, Range(0, 100)] protected int dodgeChancePercent = 8;
        public int DodgeChancePercent => dodgeChancePercent = 8;
    }
}