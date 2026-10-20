using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "Minor Spee dIncrease Ability Data", menuName = "MrX Studio/Abilities/Stat Abilities/Minor Speed Increase")]
    public class MinorSpeedIncreaseAbilityData : GenericAbilityData<MinorSpeedIncreaseAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MinorSpeedIncrease;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MinorSpeedIncrease;
        }
    }

    [System.Serializable]
    public class MinorSpeedIncreaseAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float movementSpeedMultiplier;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;
    }
}