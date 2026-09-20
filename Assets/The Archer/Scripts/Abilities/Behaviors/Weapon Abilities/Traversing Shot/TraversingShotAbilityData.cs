using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "TraversingShot Ability Data", menuName = "October/Abilities/Weapon Abilities/TraversingShot")]
    public class TraversingShotAbilityData : GenericAbilityData<TraversingShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_TraversingShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_TraversingShot;
        }
    }

    [System.Serializable]
    public class TraversingShotAbilityLevel : AbilityLevel
    {
        [SerializeField] protected float attackDamageMultiplier = 1.25f;
        public float AttackDamageMultiplier => attackDamageMultiplier;
    }
}