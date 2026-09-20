using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public abstract class AbilityData : ScriptableObject
    {
        [Tooltip("The unique identifier of an ability")]
        [SerializeField] protected AbilityType type;
        public AbilityType AbilityType => type;

        [Tooltip("Needs at least one of this abilities to appear in Selector Window")]
        [SerializeField] protected List<AbilityType> prerequisites;
        public List<AbilityType> Prerequisites => prerequisites;

        [Tooltip("The grouping of an ability")]
        [SerializeField] protected AbilityRarity rarity;
        public AbilityRarity Rarity => rarity;

        [Tooltip("This ability's level is always 1. Can be picked multiple times")]
        [SerializeField] protected bool isRepeatedAbiltiy;
        public bool IsRepeatedAbility => isRepeatedAbiltiy;

        [Tooltip("This ability will only show up if there are no abilities of the similar rarity left")]
        [SerializeField] protected bool isEndgameAbility;
        public bool IsEndgameAbility => isEndgameAbility;

        [Tooltip("Shoud be short, no more than two words")]
        [SerializeField] string title;
        public string Title => title;

        [Tooltip("Keep it brief but informative")]
        [SerializeField] string description;
        public string Description => description;

        [Tooltip("Image that will appear on the ui")]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [Tooltip("Prefab with the implementation of the ability")]
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        public abstract AbilityLevel[] Levels { get; }
        public int LevelsCount => Levels.Length;

        public event UnityAction<int> onAbilityUpgraded;

        public virtual void Upgrade(int level)
        {
            onAbilityUpgraded?.Invoke(level);
        }

        public virtual AbilityLevel GetLevel(int index)
        {
            return Levels[index];
        }
    }
}