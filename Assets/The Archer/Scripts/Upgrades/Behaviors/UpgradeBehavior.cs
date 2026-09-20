using UnityEngine;

namespace OctoberStudio.Upgrades
{
    public class UpgradeBehavior : MonoBehaviour
    {
        public UpgradeData Data { get; protected set; }
        public UpgradeLevel Level { get; protected set; }
        public int LevelID { get; protected set; }

        protected virtual void Awake()
        {

        }

        public virtual void Init(UpgradeData data)
        {
            Data = data;

            data.onUpgradeLevelChanged += SetUpgradeLevel;
        }

        public virtual void SetUpgradeLevel(int level)
        {
            LevelID = level;
            Level = Data.GetLevel(level);
        }

        public virtual void Clear()
        {
            Data.onUpgradeLevelChanged -= SetUpgradeLevel;

            Destroy(gameObject);
        }
    }
}