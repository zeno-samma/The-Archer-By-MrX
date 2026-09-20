using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "UltimateMeteor Ability Data", menuName = "October/Abilities/Elemental Abilities/UltimateMeteor")]
    public class UltimateMeteorAbilityData : GenericAbilityData<UltimateMeteorAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_UltimateMeteor;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_UltimateMeteor;
        }
    }

    [System.Serializable]
    public class UltimateMeteorAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float meteorSpawnDelay;
        public float MeteorSpawnDelay => meteorSpawnDelay;

        [SerializeField] protected float maxOffsetFromEnemy = 5f;
        public float MaxOffsetFromEnemy => maxOffsetFromEnemy;

        [SerializeField] protected float meteorDamageMultiplier;
        public float MeteorDamageMultiplier => meteorDamageMultiplier;
    }
}