using System.Collections.Generic;

namespace OctoberStudio.Upgrades
{
    public interface IUpgradesManager
    {
        int GetUpgradeLevel(UpgradeType upgradeType);
        bool IsUpgradeAquired(UpgradeType upgradeType);
        UpgradeData GetUpgradeData(UpgradeType upgradeType);

        List<UpgradeData> GetAllUpgrades();

        void IncrementUpgradeLevel(UpgradeType upgradeType);

        void OnStageLoaded();
        void OnStageUnloaded();
    }
}