using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "LegendaryCoins Ability Data", menuName = "October/Abilities/Endgame Abilities/LegendaryCoins")]
    public class LegendaryCoinsAbilityData : GenericAbilityData<LegendaryCoinsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_LegendaryCoins;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_LegendaryCoins;
        }
    }

    [System.Serializable]
    public class LegendaryCoinsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int amountOfCoins = 10;
        public int AmountOfCoins => amountOfCoins;
    }
}