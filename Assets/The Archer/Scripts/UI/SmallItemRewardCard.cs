using MrX.Armory;
using MrX.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public class SmallItemRewardCard : SmallRewardCard
    {
        [SerializeField] private TMP_Text rewardName;
        public void SetReward(LuckyGrant reward)
        {
            var item = string.IsNullOrEmpty(reward.itemId) ? null : GameController.ArmoryManager.GetItemData(reward.itemId);
            itemTypeBackgroundImage.gameObject.SetActive(item != null);
            itemTypeIconImage.gameObject.SetActive(item != null);
            if (item != null) SetItem(item);
            SetIcon(new LuckyRewardDelivery(GameController.ArmoryManager, GameController.CurrenciesManager).Icon(reward));
            SetAmount(reward.amount); amountText.gameObject.SetActive(true);
            if (rewardName != null) rewardName.text = reward.title + (reward.pendingDelivery ? "\n(Pending delivery)" : reward.queuedEquipment ? "\n(Bag pending)" : "");
        }
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
