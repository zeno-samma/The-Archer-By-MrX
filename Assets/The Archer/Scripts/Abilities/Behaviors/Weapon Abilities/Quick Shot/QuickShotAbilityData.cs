using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Quick Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Quick Shot")]
    public class QuickShotAbilityData : GenericAbilityData<QuickShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_QuickShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_QuickShot;
        }
    }

    [System.Serializable]
    public class QuickShotAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackSpeedMultiplier;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float attackDamageMultiplier;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float arrowSizeMultiplier;
        public float ArrowSizeMultiplier => arrowSizeMultiplier;

        [SerializeField] protected int arrowSpreadAngle;
        public int ArrowSpreadAngle => arrowSpreadAngle;
    }
}