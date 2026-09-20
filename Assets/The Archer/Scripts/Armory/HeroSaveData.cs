using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    [System.Serializable]
    public class HeroSaveData
    {
        [SerializeField] protected string id;
        [SerializeField] protected bool isEquipped;
        [SerializeField] protected bool isUlocked;
        [SerializeField] protected int level;

        public string Id => id;
        public bool IsEquipped
        {
            get => isEquipped;
            set
            {
                isEquipped = value;
                OnHeroSelected?.Invoke(this);
            }
        }
        public bool IsUnlocked { get => isUlocked; set => isUlocked = value; }
        public int Level
        {
            get => level; 
            set
            { 
                level = value; 
                OnHeroLevelChanged?.Invoke(this, level);
            }
        }

        public event UnityAction<HeroSaveData> OnHeroSelected;
        public event UnityAction<HeroSaveData, int> OnHeroLevelChanged;

        public HeroSaveData(string id)
        {
            this.id = id;
            this.isEquipped = false;
            this.level = 0;
        }
    }
}