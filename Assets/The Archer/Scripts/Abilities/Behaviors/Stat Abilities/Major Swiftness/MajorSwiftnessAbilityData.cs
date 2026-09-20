using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Major Swiftness Ability Data", menuName = "October/Abilities/Stat Abilities/Major Swiftness")]
    public class MajorSwiftnessAbilityData : GenericAbilityData<MajorSwiftnessAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_MajorSwiftness;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_MajorSwiftness;
        }
    }

    [System.Serializable]
    public class MajorSwiftnessAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float movementSpeedMultiplier;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;
    }
}