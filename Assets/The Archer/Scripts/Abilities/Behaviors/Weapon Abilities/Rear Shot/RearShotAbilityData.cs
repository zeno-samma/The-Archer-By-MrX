using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Rear Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Rear Shot")]
    public class RearShotAbilityData : GenericAbilityData<RearShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_RearShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_RearShot;
        }
    }

    [System.Serializable]
    public class RearShotAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] int backArrowsCount = 1;
        public int BackArrowsCount => backArrowsCount;

        [SerializeField, Min(0.01f)] float backArrowsDamageMultiplier = 0.8f;
        public float BackArrowsDamageMultiplier => backArrowsDamageMultiplier;
    }
}

