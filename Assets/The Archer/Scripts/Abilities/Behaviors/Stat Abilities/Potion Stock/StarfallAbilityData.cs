using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Starfall Ability Data", menuName = "October/Abilities/Stat Abilities/Starfall")]
    public class StarfallAbilityData : GenericAbilityData<StarfallAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_Starfall;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_Starfall;
        }
    }

    [System.Serializable]
    public class StarfallAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float starsSpawnIntervalMultiplier = 0.75f;
        public float StarsSpawnIntervalMultiplier => starsSpawnIntervalMultiplier;
    }
}