using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Sniper Ability Data", menuName = "October/Abilities/Weapon Abilities/Sniper")]
    public class SniperAbilityData : GenericAbilityData<SniperAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_Sniper;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_Sniper;
        }
    }

    [System.Serializable]
    public class SniperAbilityLevel : AbilityLevel
    {
        [SerializeField] protected AnimationCurve distanceDamageMultiplierCurve;
        public AnimationCurve DistanceDamageMultiplierCurve => distanceDamageMultiplierCurve;
    }
}