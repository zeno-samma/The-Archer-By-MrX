using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Split Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Split Shot")]
    public class SplitShotAbilityData : GenericAbilityData<SplitShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_SplitShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_SplitShot;
        }
    }

    [System.Serializable]
    public class SplitShotAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int splitArrowsCount;
        public int SplitArrowsCount => splitArrowsCount;

        [SerializeField] protected float splitArrowDamageMultiplier;
        public float SplitArrowDamageMultiplier => splitArrowDamageMultiplier;
    }
}