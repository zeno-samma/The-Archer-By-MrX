using OctoberStudio.Save;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Upgrades
{
    [System.Serializable]
    public class UpgradesSave : ISave
    {
        [SerializeField] protected UpgradeSave[] savedUpgrades;

        protected Dictionary<UpgradeType, int> upgradesLevels;

        public virtual void Init()
        {
            upgradesLevels = new Dictionary<UpgradeType, int>();

            if (savedUpgrades == null) savedUpgrades = new UpgradeSave[0];

            for (int i = 0; i < savedUpgrades.Length; i++)
            {
                var save = savedUpgrades[i];

                if (upgradesLevels.ContainsKey(save.UpgradeType))
                {
                    var savedLevel = upgradesLevels[save.UpgradeType];

                    if (save.Level > savedLevel)
                    {
                        upgradesLevels[save.UpgradeType] = save.Level;
                    }
                }
                else
                {
                    upgradesLevels.Add(save.UpgradeType, save.Level);
                }
            }
        }

        public virtual int GetUpgradeLevel(UpgradeType upgrade)
        {
            if (upgradesLevels.ContainsKey(upgrade))
            {
                return upgradesLevels[upgrade];
            }
            else
            {
                return -1;
            }
        }

        public virtual void SetUpgradeLevel(UpgradeType upgrade, int level)
        {
            if (upgradesLevels.ContainsKey(upgrade))
            {
                upgradesLevels[upgrade] = level;
            }
            else
            {
                upgradesLevels.Add(upgrade, level);
            }
        }

        public virtual void RemoveUpgrade(UpgradeType upgrade)
        {
            if (upgradesLevels.ContainsKey(upgrade))
            {
                upgradesLevels.Remove(upgrade);
            }
        }

        public virtual void Flush()
        {
            savedUpgrades = new UpgradeSave[upgradesLevels.Count];

            int i = 0;

            foreach (var upgrade in upgradesLevels.Keys)
            {
                var upgradeSave = new UpgradeSave(upgrade, upgradesLevels[upgrade]);
                savedUpgrades[i++] = upgradeSave;
            }
        }

        public virtual void Clear()
        {
            upgradesLevels.Clear();
        }

        [System.Serializable]
        protected class UpgradeSave
        {
            [SerializeField] protected UpgradeType upgradeType;
            [SerializeField] protected int level;

            public UpgradeType UpgradeType => upgradeType;
            public int Level => level;

            public UpgradeSave(UpgradeType upgradeType, int level)
            {
                this.upgradeType = upgradeType;
                this.level = level;
            }
        }
    }
}