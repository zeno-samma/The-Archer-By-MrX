using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "Spirit Lord Ability Data", menuName = "MrX Studio/Abilities/Spirit Abilities/Spirit Lord")]
    public class SpiritLordAbilityData : GenericAbilityData<SpiritLordAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Spirit_SpiritLord;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Spirit_SpiritLord;
        }
    }

    [System.Serializable]
    public class SpiritLordAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float spiritAttackDelay = 2f;
        public float SpiritAttackDelay => spiritAttackDelay;

        [SerializeField] protected float spiritDamageMultiplier;
        public float SpiritDamageMultiplier => spiritDamageMultiplier;
    }
}