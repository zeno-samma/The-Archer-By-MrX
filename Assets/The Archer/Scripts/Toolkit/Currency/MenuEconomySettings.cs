using UnityEngine;

namespace MrX.Currency
{
    [CreateAssetMenu(menuName = "MrX Studio/Menu Economy Settings")]
    public sealed class MenuEconomySettings : ScriptableObject
    {
        public const string ResourcePath = "Menu Economy Settings";
        [Min(1)] public int maximumEnergy = 30;
        [Min(1)] public int energyPerHour = 5;
        [Min(1)] public int energyPurchaseAmount = 10;
        [Min(1)] public int energyGemPrice = 20;
        [Min(0)] public int dailyEnergyPurchaseLimit = 5;
        [Range(-12,14)] public int purchaseResetUtcOffsetHours = 7;
        [Min(1)] public int goldPurchaseAmount = 1000;
        [Min(1)] public int goldGemPrice = 10;
        public Sprite energyIcon;
    }
}
