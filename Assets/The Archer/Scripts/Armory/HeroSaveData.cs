using UnityEngine;
using UnityEngine.Events;

namespace MrX
{
    [System.Serializable]
    public class HeroSaveData
    {
        [SerializeField] protected string id;
        [SerializeField] protected bool isEquipped;
        [SerializeField] protected bool isUlocked;
        [SerializeField] protected int level;

        [SerializeField, Min(0)] private int shards;

        public string Id => id;
        public int Shards => shards;
        public int ShardsToUpgrade => 5 * (Mathf.Max(0, level) + 1);

        public bool TryAddShards(int amount)
        {
            if (amount <= 0 || shards > int.MaxValue - amount) return false;
            shards += amount;
            return true;
        }

        public bool CanUpgradeWithShards(int levelCount) =>
            IsUnlocked && level >= 0 && level < levelCount - 1 && shards >= ShardsToUpgrade;

        public bool TryUpgradeWithShards(int levelCount)
        {
            if (!CanUpgradeWithShards(levelCount)) return false;
            shards -= ShardsToUpgrade;
            Level++;
            return true;
        }
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
                var gainedLevels = value - level;
                level = value; 
                if (IsUnlocked && gainedLevels > 0)
                {
                    GameController.DailyQuestsManager?.Record(Quests.DailyQuestType.UpgradeHero, gainedLevels);
                    GameController.AchievementsManager?.Record(Achievements.AchievementMetric.HeroUpgrades, gainedLevels);
                }
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
