using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "CommonCoins Ability Data", menuName = "October/Abilities/Endgame Abilities/CommonCoins")]
    public class CommonCoinsAbilityData : GenericAbilityData<CommonCoinsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_CommonCoins;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_CommonCoins;
        }
    }

    [System.Serializable]
    public class CommonCoinsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int amountOfCoins = 10;
        public int AmountOfCoins => amountOfCoins;
    }
}