using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "October/Upgrades/Upgrades Database")]
    public class UpgradesDatabase : ScriptableObject
    {
        [SerializeField] protected List<UpgradeData> upgrades;

        public int UpgradesCount => upgrades.Count;

        public virtual UpgradeData GetUpgrade(int index)
        {
            return upgrades[index];
        }

        public virtual UpgradeData GetUpgrade(UpgradeType upgradeType)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.UpgradeType == upgradeType)
                {
                    return upgrade;
                }
            }

            return null;
        }
    }
}