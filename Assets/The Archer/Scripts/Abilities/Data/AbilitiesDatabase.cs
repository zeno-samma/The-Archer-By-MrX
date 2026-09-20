using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Abilities Database", menuName = "October/Abilities/Database")]
    public class AbilitiesDatabase : ScriptableObject
    {
        [SerializeField] protected List<AbilityData> abilities;

        [Space]
        [SerializeField] protected List<AbilityRarityData> rarities;

        public int AbilitiesCount => abilities.Count;
        public int RaritiesCount => rarities.Count;

        public virtual AbilityData GetAbility(int index)
        {
            return abilities[index];
        }

        public virtual AbilityData GetAbility(AbilityType type)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];

                if (ability.AbilityType == type) return ability;
            }

            return null;
        }

        public virtual AbilityRarityData GetRarityData(int index)
        {
            return rarities[index];
        }

        public virtual List<AbilityRarityData> GetRarities()
        {
            return rarities;
        }

        public virtual AbilityRarityData GetRarityData(AbilityRarity rarity)
        {
            for (int i = 0; i < rarities.Count; i++)
            {
                var rarityData = rarities[i];

                if (rarityData.Rarity == rarity) return rarityData;
            }

            return null;
        }
    }
}