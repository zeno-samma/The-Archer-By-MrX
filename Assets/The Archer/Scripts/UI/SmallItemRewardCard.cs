using OctoberStudio.Armory;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class SmallItemRewardCard : SmallRewardCard
    {
        [SerializeField] protected Image rarityBackgroundImage;
        [SerializeField] protected Image itemTypeBackgroundImage;
        [SerializeField] protected Image itemTypeIconImage;

        public void SetItem(ItemData itemData)
        {
            var itemLevel = itemData.GetItemLevel(0);
            var itemRarity = itemLevel.ItemRarity;
            var itemType = itemData.ItemType;

            var rarityData = GameController.ArmoryManager.GetItemRarityData(itemRarity);
            var itemTypeData = GameController.ArmoryManager.GetItemTypeData(itemType);

            itemTypeBackgroundImage.color = rarityData.SecondaryColor;
            itemTypeIconImage.sprite = itemTypeData.ItemTypeIcon;
            rarityBackgroundImage.sprite = rarityData.NarrowBorderBackground;

            SetIcon(itemData.Icon);
        }
    }
}