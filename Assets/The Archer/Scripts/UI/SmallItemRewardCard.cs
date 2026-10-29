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
        protected override void Awake()
        {
            base.Awake();
            if (itemTypeBackgroundImage != null) inspectorVisibility.Capture(itemTypeBackgroundImage.gameObject);
            if (itemTypeIconImage != null) inspectorVisibility.Capture(itemTypeIconImage.gameObject);
        }
        public void SetReward(LuckyGrant reward)
        {
            var item = string.IsNullOrEmpty(reward.itemId) ? null : GameController.ArmoryManager.GetItemData(reward.itemId);
            inspectorVisibility.SetVisible(itemTypeBackgroundImage.gameObject, item != null);
            inspectorVisibility.SetVisible(itemTypeIconImage.gameObject, item != null);
            if (item != null) SetItem(item);
            SetIcon(new LuckyRewardDelivery(GameController.ArmoryManager, GameController.CurrenciesManager).Icon(reward));
            SetAmount(reward.amount); inspectorVisibility.SetVisible(amountText.gameObject, true);
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
