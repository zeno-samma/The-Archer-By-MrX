using MrX.Save;
using UnityEngine;

namespace MrX.Pets
{
    [System.Serializable]
    public sealed class PetProfileSave : ISave
    {
        public const int FullHealth = 100;
        private const int DefeatHealthLoss = 50;

        [SerializeField] private bool initialized;
        [SerializeField] private int healthPercent = FullHealth;
        [SerializeField] private int cakes;
        [SerializeField] private bool deployed;

        public int HealthPercent => healthPercent;
        public int Cakes => cakes;
        public bool IsDeployed => deployed && HealthPercent == FullHealth;

        internal void Initialize(int startingCakes)
        {
            if (initialized) return;

            cakes = startingCakes;
            healthPercent = FullHealth;
            deployed = false;
            initialized = true;
        }

        internal bool Deploy()
        {
            if (IsDeployed || HealthPercent != FullHealth) return false;

            deployed = true;
            return true;
        }

        internal bool Recall()
        {
            if (!deployed) return false;

            deployed = false;
            return true;
        }

        internal bool Feed(int cost)
        {
            if (HealthPercent == FullHealth || cost < 1 || Cakes < cost) return false;

            cakes -= cost;
            healthPercent = FullHealth;
            return true;
        }

        internal bool ApplyDefeat()
        {
            if (!IsDeployed) return false;

            healthPercent = Mathf.Max(0, healthPercent - DefeatHealthLoss);
            deployed = false;
            return true;
        }

        internal bool AddCakes(int amount)
        {
            if (amount < 1 || amount > int.MaxValue - cakes) return false;

            cakes += amount;
            return true;
        }

        public void Flush()
        {
        }
    }
}
