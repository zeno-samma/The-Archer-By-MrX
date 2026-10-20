using MrX.Armory;
using MrX.Save;
using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    [System.Serializable]
    public class ArmorySave : ISave
    {
        [SerializeField] protected ItemSaveData[] items;
        protected List<ItemSaveData> Items { get; set; }

        [SerializeField] protected HeroSaveData[] heroes;
        protected List<HeroSaveData> Heroes { get; set; }

        [SerializeField] private int bagCapacity = 40;
        [SerializeField] private List<PendingEquipmentEntry> pendingEquipment = new List<PendingEquipmentEntry>();
        public int BagCapacity => System.Math.Max(40, bagCapacity);
        public int UsedSlots => Items == null ? (items == null ? 0 : items.Length) : Items.Count;
        public bool HasBagSpace => UsedSlots < BagCapacity;
        public List<PendingEquipmentEntry> PendingEquipment => pendingEquipment;
        public void SetBagCapacity(int capacity) { bagCapacity = System.Math.Max(BagCapacity, capacity); }
        public void QueueEquipment(string id)
        {
            foreach (var entry in pendingEquipment) if (entry.id == id) { if (entry.amount < int.MaxValue) entry.amount++; return; }
            pendingEquipment.Add(new PendingEquipmentEntry { id = id, amount = 1 });
        }

        public bool IsInitialized { get; protected set; }

        public virtual void Init()
        {
            IsInitialized = true;

            if (items == null)
            {
                Items = new List<ItemSaveData>();
            }
            else
            {
                Items = new List<ItemSaveData>(items);
            }

            if (heroes == null)
            {
                Heroes = new List<HeroSaveData>();
            }
            else
            {
                Heroes = new List<HeroSaveData>(heroes);
            }
            if (pendingEquipment == null) pendingEquipment = new List<PendingEquipmentEntry>();
            pendingEquipment.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.id) || entry.amount <= 0);
            // Old saves keep all equipment, even if their count exceeds 40.
            bagCapacity = System.Math.Max(BagCapacity, UsedSlots);
        }

        #region Items

        [System.Serializable] public class PendingEquipmentEntry { public string id; public int amount; }

        public virtual ItemSaveData GetItem(string id)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id == id)
                {
                    return Items[i];
                }
            }

            return null;
        }

        public virtual ItemSaveData AddItem(string id)
        {
            var itemSaveData = GetItem(id);

            if(itemSaveData == null)
            {
                itemSaveData = new ItemSaveData(id);

                Items.Add(itemSaveData);
            }

            return itemSaveData;
        }

        public virtual List<ItemSaveData> GetItems()
        {
            return Items;
        }

        public virtual void RemoveItem(string id)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id == id)
                {
                    Items.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion

        #region Heroes

        public virtual HeroSaveData GetHeroSave(string id)
        {
            for (int i = 0; i < Heroes.Count; i++)
            {
                if (Heroes[i].Id == id)
                {
                    return Heroes[i];
                }
            }

            var heroSaveData = new HeroSaveData(id);
            heroSaveData.IsEquipped = false;
            heroSaveData.Level = 0;
            heroSaveData.IsUnlocked = false;

            Heroes.Add(heroSaveData);

            return heroSaveData;
        }

        public virtual List<HeroSaveData> GetHeroes()
        {
            return Heroes;
        }

        #endregion

        public virtual void Flush()
        {
            items = Items.ToArray();
            heroes = Heroes.ToArray();
        }
    }
}
