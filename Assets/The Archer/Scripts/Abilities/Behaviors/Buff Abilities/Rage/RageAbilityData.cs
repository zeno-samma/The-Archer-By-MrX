using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Rage Ability Data", menuName = "October/Abilities/Buff Abilities/Rage")]
    public class RageAbilityData : GenericAbilityData<RageAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Buff_Rage;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Buff_Rage;
        }
    }

    [System.Serializable]
    public class RageAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(0f, 1f)] protected float abilityStartHpProportion;
        public float AbilityStartHpProportion => abilityStartHpProportion;

        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
    }
}