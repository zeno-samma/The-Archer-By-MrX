using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Ricochet Ability Data", menuName = "October/Abilities/Weapon Abilities/Ricochet")]
    public class RicochetAbilityData : GenericAbilityData<RicochetAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_Ricochet;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_Ricochet;
        }
    }

    [System.Serializable]
    public class RicochetAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int arrowRicochetCount;
        public int ArrowRicochetCount => arrowRicochetCount;

        [SerializeField] protected float eachRicochetDamageMultiplier;
        public float EachRicochetDamageMultiplier => eachRicochetDamageMultiplier;
    }
}