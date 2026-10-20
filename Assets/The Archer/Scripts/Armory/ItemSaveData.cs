using UnityEngine;
using UnityEngine.Events;

namespace MrX.Armory
{
    [System.Serializable]
    public class ItemSaveData
    {
        [SerializeField] protected string id;
        [SerializeField] protected bool isEquipped;
        [SerializeField] protected int level;

        public string Id => id;
        public bool IsEquipped
        {
            get => isEquipped; 
            set
            {
                isEquipped = value;
                OnItemEquipped?.Invoke(this, value);
            }
        }
        public int Level
        {
            get => level;
            set
            {
                var gainedLevels = value - level;
                level = value; 
                if (gainedLevels > 0)
                    GameController.AchievementsManager?.Record(Achievements.AchievementMetric.ItemUpgrades, gainedLevels);
                OnItemLevelChanged?.Invoke(this, level);
            }
        }

        public event UnityAction<ItemSaveData, bool> OnItemEquipped;
        public event UnityAction<ItemSaveData, int> OnItemLevelChanged;

        public ItemSaveData(string id)
        {
            this.id = id;
            this.isEquipped = false;
            this.level = 0;
        }
    }
}
