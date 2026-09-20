using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Bouncy Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Bouncy Shot")]
    public class BouncyShotAbilityData : GenericAbilityData<BouncyShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_BouncyShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_BouncyShot;
        }
    }

    [System.Serializable]
    public class BouncyShotAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] protected int bouncesCount = 1;
        public int BouncesCount => bouncesCount;

        [SerializeField, Min(0.01f)] protected float eachBounceDamageMultiplier = 1f;
        public float EachBounceDamageMultiplier => eachBounceDamageMultiplier;
    }
}