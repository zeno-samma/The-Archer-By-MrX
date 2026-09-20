using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Critical Strike Ability Data", menuName = "October/Abilities/Stat Abilities/Critical Strike")]
    public class CriticalStrikeAbilityData : GenericAbilityData<CriticalStrikeAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_CriticalStrike;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_CriticalStrike;
        }
    }

    [System.Serializable]
    public class CriticalStrikeAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float critDamageMultiplier = 1.5f;
        public float CritDamageMultiplier => critDamageMultiplier;

        [SerializeField] protected int critChanceIncreasePercent = 10;
        public int CritChanceIncreasePercent => critChanceIncreasePercent;
    }
}