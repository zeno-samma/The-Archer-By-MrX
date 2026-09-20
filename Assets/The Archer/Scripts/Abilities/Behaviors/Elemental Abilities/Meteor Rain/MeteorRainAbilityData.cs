using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "MeteorRain Ability Data", menuName = "October/Abilities/Elemental Abilities/MeteorRain")]
    public class MeteorRainAbilityData : GenericAbilityData<MeteorRainAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_MeteorRain;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_MeteorRain;
        }
    }

    [System.Serializable]
    public class MeteorRainAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float meteorSpawnDelayMultiplier;
        public float MeteorSpawnDelayMultiplier => meteorSpawnDelayMultiplier;
    }
}