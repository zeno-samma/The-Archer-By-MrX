using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "RareCoins Ability Data", menuName = "October/Abilities/Endgame Abilities/RareCoins")]
    public class RareCoinsAbilityData : GenericAbilityData<RareCoinsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_RareCoins;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_RareCoins;
        }
    }

    [System.Serializable]
    public class RareCoinsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int amountOfCoins = 10;
        public int AmountOfCoins => amountOfCoins;
    }
}
