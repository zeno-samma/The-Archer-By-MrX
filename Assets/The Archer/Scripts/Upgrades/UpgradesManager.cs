using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Upgrades
{
    public class UpgradesManager : MonoBehaviour, IUpgradesManager
    {
        [SerializeField] protected UpgradesDatabase database;

        protected UpgradesSave save;

        protected List<UpgradeBehavior> acquiredUpgrades = new List<UpgradeBehavior>();

        protected virtual void Awake()
        {
            if (!GameController.RegisterUpgradesManager(this))
            {
                Destroy(gameObject);

                return;
            }

            save = GameController.SaveManager.GetSave<UpgradesSave>("Upgrades Save");
            save.Init();

            for (int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);

                if (GetUpgradeLevel(upgrade.UpgradeType) < upgrade.DevStartLevel)
                {
                    save.SetUpgradeLevel(upgrade.UpgradeType, upgrade.DevStartLevel);
                }
            }
        }

        public virtual void OnStageLoaded()
        {
            for (int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);
                var upgradeLevel = save.GetUpgradeLevel(upgrade.UpgradeType);
                if (upgradeLevel >= 0)
                {
                    var upgradeBehavior = Instantiate(upgrade.Prefab).GetComponent<UpgradeBehavior>();

                    upgradeBehavior.Init(upgrade);
                    upgradeBehavior.SetUpgradeLevel(upgradeLevel);

                    acquiredUpgrades.Add(upgradeBehavior);
                }
            }
        }

        public virtual void OnStageUnloaded()
        {
            for (int i = 0; i < acquiredUpgrades.Count; i++)
            {
                acquiredUpgrades[i].Clear();
            }

            acquiredUpgrades.Clear();
        }

        public virtual List<UpgradeData> GetAllUpgrades()
        {
            var upgrades = new List<UpgradeData>();
            for (int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);
                upgrades.Add(upgrade);
            }

            return upgrades;
        }

        public virtual void IncrementUpgradeLevel(UpgradeType upgradeType)
        {
            var level = save.GetUpgradeLevel(upgradeType);
            save.SetUpgradeLevel(upgradeType, level + 1);

            var upgradeData = database.GetUpgrade(upgradeType);
            upgradeData.SetLevel(level + 1);
        }

        public virtual int GetUpgradeLevel(UpgradeType upgradeType)
        {
            return save.GetUpgradeLevel(upgradeType);
        }

        public virtual bool IsUpgradeAquired(UpgradeType upgradeType)
        {
            var level = save.GetUpgradeLevel(upgradeType);

            return level != -1;
        }

        public virtual UpgradeData GetUpgradeData(UpgradeType upgradeType)
        {
            return database.GetUpgrade(upgradeType);
        }

        public virtual float GetUpgadeValue(UpgradeType upgradeType)
        {
            var data = GetUpgradeData(upgradeType);
            var level = GetUpgradeLevel(upgradeType);

            if (level >= 0)
            {
                return data.GetLevel(level).Value;
            }

            return 0;
        }
    }
}