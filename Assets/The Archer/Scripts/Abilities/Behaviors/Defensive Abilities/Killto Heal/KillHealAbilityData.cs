using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Kill To Heal Ability Data", menuName = "October/Abilities/Defensive Abilities/Kill To Heal")]
    public class KillHealAbilityData : GenericAbilityData<KillToHealAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_KilltoHeal;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_KilltoHeal;
        }
    }

    [System.Serializable]
    public class KillToHealAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(0, 1)] protected float healChance;
        public float HealChance => healChance;

        [SerializeField, Range(0, 1)] protected float maxHealthProportionHeal;
        public float MaxHealthProportionHeal => maxHealthProportionHeal;
    }
}