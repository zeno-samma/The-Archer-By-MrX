using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "MysticCoins Ability Data", menuName = "October/Abilities/Endgame Abilities/MysticCoins")]
    public class MysticCoinsAbilityData : GenericAbilityData<MysticCoinsAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Endgame_MysticCoins;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Endgame_MysticCoins;
        }
    }

    [System.Serializable]   
    public class MysticCoinsAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int amountOfCoins = 10;
        public int AmountOfCoins => amountOfCoins;
    }
}