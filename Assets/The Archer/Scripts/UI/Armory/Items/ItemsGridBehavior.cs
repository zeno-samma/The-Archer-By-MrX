using OctoberStudio.Armory;
using System.Collections.Generic;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class ItemsGridBehavior : ArmoryGridUI<ItemCardBehavior>
    {
        public Selectable GetFirstItemSelectable()
        {
            if (cards.Count > 0)
            {
                return cards[0].Selectable;
            }
            return null;
        }

        public virtual void Init()
        {
            Clear();

            GameController.ArmoryManager.OnItemEquipmentChanged += Init;

            var itemsSaves = GameController.ArmoryManager.GetItemSaveList();
            var itemPairs = new List<ItemSaveDataPair>(itemsSaves.Count);
            for (int i = 0; i < itemsSaves.Count; i++)
            {
                var save = itemsSaves[i];
                var data = GameController.ArmoryManager.GetItemData(itemsSaves[i].Id);

                if (data != null)
                {
                    itemPairs.Add(new ItemSaveDataPair
                    {
                        save = save,
                        data = data,
                    });
                }
            }

            itemPairs.Sort(ItemsComparator);

            for (int i = 0; i < itemPairs.Count; i++)
            {
                var itemSave = itemPairs[i].save;
                var itemData = itemPairs[i].data;

                if (itemSave.IsEquipped) continue;

                var card = cardsPool.GetEntity();
                card.SetData(itemData, itemSave);
                cards.Add(card);
            }
        }

        protected virtual int ItemsComparator(ItemSaveDataPair first, ItemSaveDataPair second)
        {
            var firstLevel = first.data.GetItemLevel(first.save.Level);
            var secondLevel = second.data.GetItemLevel(second.save.Level);

            var rarity = secondLevel.ItemRarity - firstLevel.ItemRarity;

            if (rarity != 0) return rarity;

            return first.data.ItemType - second.data.ItemType;
        }

        public override void Clear()
        {
            base.Clear();

            GameController.ArmoryManager.OnItemEquipmentChanged -= Init;
        }

        protected class ItemSaveDataPair
        {
            public ItemSaveData save;
            public ItemData data;
        }
    }
}