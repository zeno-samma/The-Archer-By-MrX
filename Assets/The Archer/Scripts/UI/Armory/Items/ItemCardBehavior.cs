using OctoberStudio.Armory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class ItemCardBehavior : ArmoryCardBehavior
    {
        [Space]
        [SerializeField] protected Image itemTypeImage;
        [SerializeField] protected Image itemTypeBackgroundImage;

        [Space]
        [SerializeField] protected List<ItemTypeData> itemTypeDataList;

        public ItemSaveData SaveData { get; protected set; }
        public ItemData ItemData { get; protected set; }
        public ItemLevel ItemLevel { get; protected set; }

        public RectTransform RectTransform { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void SetData(ItemData data, ItemSaveData saveData)
        {
            ItemData = data;
            SaveData = saveData;

            iconImage.sprite = data.Icon;

            ItemLevel = data.GetItemLevel(saveData.Level);

            levelText.text = (saveData.Level + 1).ToString();

            InitRarityVisuals(ItemLevel.ItemRarity);

            itemTypeImage.sprite = GetItemTypeSprite(data.ItemType);

            SaveData.OnItemLevelChanged += OnItemLevelChanged;
        }

        protected virtual void OnItemLevelChanged(ItemSaveData saveData, int level)
        {
            ItemLevel = ItemData.GetItemLevel(level);
            levelText.text = (level + 1).ToString();
            InitRarityVisuals(ItemLevel.ItemRarity);
        }

        protected override void InitRarityVisuals(ItemRarityType rarityType)
        {
            base.InitRarityVisuals(rarityType);

            itemTypeBackgroundImage.color = rarityBackgroundImage.color;
        }

        protected virtual Sprite GetItemTypeSprite(ItemType itemType)
        {
            for (int i = 0; i < itemTypeDataList.Count; i++)
            {
                if (itemTypeDataList[i].itemType == itemType)
                {
                    return itemTypeDataList[i].iconSprite;
                }
            }
            return null;
        }

        public override void Clear()
        {
            base.Clear();

            if (SaveData != null)
            {
                SaveData.OnItemLevelChanged -= OnItemLevelChanged;
            }
        }

        protected override void OnButtonClicked()
        {
            GameController.MainMenuScreenBehavior.ExpandedItemPopup.Show(ItemData, SaveData);
        }

        [System.Serializable]
        protected class ItemTypeData
        {
            public ItemType itemType;
            public Sprite iconSprite;
        }
    }
}