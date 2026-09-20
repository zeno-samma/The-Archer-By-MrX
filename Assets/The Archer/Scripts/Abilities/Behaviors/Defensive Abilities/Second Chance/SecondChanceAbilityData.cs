using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "SecondChance Ability Data", menuName = "October/Abilities/Defensive Abilities/SecondChance")]
    public class SecondChanceAbilityData : GenericAbilityData<SecondChanceAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_SecondChance;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_SecondChance;
        }
    }

    [System.Serializable]
    public class SecondChanceAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int revivesCount = 1;
        public int RevivesCount => revivesCount;
    }
}