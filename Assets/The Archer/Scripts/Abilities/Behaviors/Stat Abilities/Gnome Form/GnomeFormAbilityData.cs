using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "GnomeForm Ability Data", menuName = "October/Abilities/Stat Abilities/GnomeForm")]
    public class GnomeFormAbilityData : GenericAbilityData<GnomeFormAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_GnomeForm;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_GnomeForm;
        }
    }

    [System.Serializable]
    public class GnomeFormAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float playerSizeMultiplier = 1f;
        public float PlayerSizeMultiplier => playerSizeMultiplier;

        [SerializeField] protected float attackSpeedMultiplier = 1f;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        [SerializeField] protected float movementSpeedMultiplier = 1f;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;

        [SerializeField] protected int dodgeChancePercent = 8;
        public int DodgeChancePercent => dodgeChancePercent = 8;
    }
}