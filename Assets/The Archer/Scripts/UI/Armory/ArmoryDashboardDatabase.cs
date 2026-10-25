using MrX.Currency;
using TMPro;
using UnityEngine;

namespace MrX.UI
{
    [CreateAssetMenu(menuName = "MrX Studio/Armory/Dashboard", fileName = "Armory Dashboard Database")]
    public class ArmoryDashboardDatabase : ScriptableObject
    {
        public const string ResourcePath = "Armory Dashboard Database";
        [Min(1)] public int expansionSlots = 10;
        [Min(40)] public int maximumCapacity = 200;
        public Price expansionPrice = new Price("gold", 100);
        public Sprite slotSprite, emptySlotIcon, lockIcon, blueButton, yellowButton, grayButton;
        public Material slotMaterial;
        public TMP_FontAsset font;
        [Header("Rep inventory presentation (optional)")]
        public Sprite equipmentFrame, stackFrame, emptyFrame, emptyPlus;
        public Sprite[] rarityFrames;
    }
}
