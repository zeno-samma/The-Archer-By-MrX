using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Piercing Strike Ability Data", menuName = "October/Abilities/Weapon Abilities/Piercing Strike")]
    public class PiercingStrikeAbilityData : GenericAbilityData<PiercingStrikeAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_PiercingStrike;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_PiercingStrike;
        }
    }

    [System.Serializable]
    public class PiercingStrikeAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float arrowDamageMultiplier;
        public float ArrowDamageMultiplier => arrowDamageMultiplier;
    }
}