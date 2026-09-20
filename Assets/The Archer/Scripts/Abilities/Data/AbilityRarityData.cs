using UnityEngine;

namespace OctoberStudio.Abilities
{
    [System.Serializable]
    public class AbilityRarityData
    {
        [SerializeField] protected AbilityRarity rarity;
        public AbilityRarity Rarity => rarity;

        [SerializeField] protected Color color;
        public Color Color => color;

        [SerializeField] protected string name;
        public string Name => name;

        [SerializeField, Range(0, 100)] protected int chance = 100;
        public int Chance => chance;
    }
}