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
        [SerializeField] private int experience;
        public int Experience=>Mathf.Max(0,experience);
        public void AddExperience(Armory.HeroData hero,RunProgressionSettings config,int amount)
        {
            if(!IsUnlocked||hero==null||config==null||amount<=0)return;
            long available=(long)Experience+amount;
            while(Level<hero.HeroLevelCount-1)
            {int required=config.RequiredExperience(hero,Level);if(available<required)break;available-=required;Level++;}
            experience=Level>=hero.HeroLevelCount-1?0:(int)System.Math.Min(int.MaxValue,available);
        }

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
