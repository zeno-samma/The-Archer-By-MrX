using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "TitanForm Ability Data", menuName = "October/Abilities/Stat Abilities/TitanForm")]
    public class TitanFormAbilityData : GenericAbilityData<TitanFormAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Stat_TitanForm;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Stat_TitanForm;
        }
    }

    [System.Serializable]
    public class TitanFormAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float playerSizeMultiplier = 1f;
        public float PlayerSizeMultiplier => playerSizeMultiplier;

        [SerializeField] protected float attackDamageMultiplier = 1f;
        public float AttackDamageMultiplier => attackDamageMultiplier;

        [SerializeField] protected float movementSpeedMultiplier = 1f;
        public float MovementSpeedMultiplier => movementSpeedMultiplier;

        [SerializeField] protected float maxHPMultiplier = 1f;
        public float MaxHPMultiplier => maxHPMultiplier;
    }
}