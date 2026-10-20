using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "MeteorStrike Ability Data", menuName = "MrX Studio/Abilities/Elemental Abilities/MeteorStrike")]
    public class MeteorStrikeAbilityData : GenericAbilityData<MeteorStrikeAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Elemental_MeteorStrike;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Elemental_MeteorStrike;
        }
    }

    [System.Serializable]
    public class MeteorStrikeAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float meteorSpawnDelay;
        public float MeteorSpawnDelay => meteorSpawnDelay;

        [SerializeField] protected float meteorDamageMultiplier;
        public float MeteorDamageMultiplier => meteorDamageMultiplier;
    }
}