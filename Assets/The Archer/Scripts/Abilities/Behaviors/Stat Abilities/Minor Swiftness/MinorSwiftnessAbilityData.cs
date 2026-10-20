using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "Minor Swiftness Ability Data", menuName = "MrX Studio/Abilities/Stat Abilities/Minor Swiftness")]
    public class MinorSwiftnessAbilityData : GenericAbilityData<MinorSwiftnessAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MinorSwiftness;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MinorSwiftness;
        }
    }

    [System.Serializable]
    public class MinorSwiftnessAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float movementSpeedMultiplier;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;
    }
}