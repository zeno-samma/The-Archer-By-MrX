using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "Painfull Power Ability Data", menuName = "MrX Studio/Abilities/Buff Abilities/Painfull Power")]
    public class PainfulPowerAbilityData : GenericAbilityData<PainfulPowerAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Buff_PainfulPower;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Buff_PainfulPower;
        }
    }

    [System.Serializable]
    public class PainfulPowerAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float abilityDuration;
        public float AbilityDuration => abilityDuration;
    }
 }