using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Boss Slayer Ability Data", menuName = "October/Abilities/Stat Abilities/Boss Slayer")]
    public class BossSlayerAbilityData : GenericAbilityData<BossSlayerAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_BossSlayer;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_BossSlayer;
        }
    }

    [System.Serializable]
    public class BossSlayerAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float damageToBossesMultiplier = 1.2f;
        public float DamageToBossesMultiplier => damageToBossesMultiplier;
    }
}