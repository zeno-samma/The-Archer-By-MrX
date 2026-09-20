using OctoberStudio.Abilities;
using OctoberStudio.Armory;
using OctoberStudio.Player;
using OctoberStudio.Weapon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class ArmoryManager : MonoBehaviour
    {
        [SerializeField] protected ArmoryDatabase database;
        protected ArmorySave save;

        protected Dictionary<ItemType, ItemData> EquippedItems { get; set; }
        public HeroData EquippedHero { get; protected set; }

        public event UnityAction OnItemEquipmentChanged;
        public event UnityAction OnSelectedHeroChanged;

        protected virtual void Awake()
        {
            if (!GameController.RegisterArmoryManager(this))
            {
                Destroy(gameObject);
                return;
            }
        }

        protected virtual void Start()
        {
            save = GameController.SaveManager.GetSave<ArmorySave>("Armory");
            if (!save.IsInitialized)
            {
                save.Init();
            }

            InitItems();
            InitHeroes();
        }

        protected virtual void InitItems()
        {
            EquippedItems = new Dictionary<ItemType, ItemData>();

            var itemSaves = save.GetItems();

            for (int i = 0; i < itemSaves.Count; i++)
            {
                var itemSave = itemSaves[i];

                var data = database.GetItem(itemSave.Id);
                if(data != null && itemSave.Level >= data.ItemLevelCount)
                {
                    itemSave.Level = data.ItemLevelCount - 1;
                }

                if (itemSave.IsEquipped)
                {
                    if(data == null)
                    {
                        save.RemoveItem(itemSave.Id);

                        i--;
                        continue;
                    }

                    if (!EquippedItems.ContainsKey(data.ItemType))
                    {
                        EquippedItems[data.ItemType] = data;
                    }
                    else
                    {
                        itemSave.IsEquipped = false;
                    }
                }

                itemSave.OnItemEquipped += OnItemEquippedEventFired;
            }

            for (int i = 0; i < database.ItemsCount; i++)
            {
                var itemData = database.Items[i];
                if (itemData.IsDefaultItem || itemData.IsUnlockedByDefault)
                {
                    var itemSave = save.GetItem(itemData.Id);
                    if (itemSave == null)
                    {
                        itemSave = save.AddItem(itemData.Id);

                        itemSave.IsEquipped = false;
                        itemSave.Level = 0;

                        if (itemData.IsDefaultItem && !EquippedItems.ContainsKey(itemData.ItemType))
                        {
                            EquippedItems[itemData.ItemType] = itemData;
                            itemSave.IsEquipped = true;
                        }

                        itemSave.OnItemEquipped += OnItemEquippedEventFired;
                    }
                }
            }
        }

        public virtual List<AbilityType> GetEquippedItemAndHeroAbilities()
        {
            var abilities = new List<AbilityType>();

            foreach(var itemData in EquippedItems.Values)
            {
                var itemSave = save.GetItem(itemData.Id);
                var itemLevel = itemData.GetItemLevel(itemSave.Level);
                for (int j = 0; j < itemLevel.AttachedAbilities.Count; j++)
                {
                    var ability = itemLevel.AttachedAbilities[j];
                    if (!abilities.Contains(ability))
                    {
                        abilities.Add(ability);
                    }
                }
            }

            var heroSave = save.GetHeroSave(EquippedHero.Id);
            var heroLevel = EquippedHero.GetHeroLevel(heroSave.Level);

            for (int i = 0; i < heroLevel.AttachedAbilities.Count; i++)
            {
                var ability = heroLevel.AttachedAbilities[i];
                if (!abilities.Contains(ability))
                {
                    abilities.Add(ability);
                }
            }

            return abilities;
        }

        protected virtual void OnItemEquippedEventFired(ItemSaveData itemSaveData, bool isEquipped)
        {
            var itemData = database.GetItem(itemSaveData.Id);
            var itemType = itemData.ItemType;
            if (isEquipped)
            {
                if (EquippedItems.ContainsKey(itemType))
                {
                    var oldItem = EquippedItems[itemType];

                    var oldSave = save.GetItem(oldItem.Id);
                    if (oldSave.IsEquipped) oldSave.IsEquipped = false;
                }

                EquippedItems[itemType] = itemData;
            }
            else
            {
                if (EquippedItems.ContainsKey(itemType) && itemData == EquippedItems[itemType])
                {
                    EquippedItems.Remove(itemType);
                }
            }

            OnItemEquipmentChanged?.Invoke();
        }

        public virtual ItemData GetEquippedItemData(ItemType itemType)
        {
            EquippedItems.TryGetValue(itemType, out ItemData itemData);

            return itemData;
        }


        protected virtual void InitHeroes()
        {
            var heroSaves = save.GetHeroes();

            for (int i = 0; i < database.HeroesCount; i++)
            {
                var hero = database.Heroes[i];
                var heroSaveData = save.GetHeroSave(hero.Id);

                if (hero != null && heroSaveData.Level >= hero.HeroLevelCount)
                {
                    heroSaveData.Level = hero.HeroLevelCount - 1;
                }

                if (heroSaveData.IsEquipped && EquippedHero == null)
                {
                    EquippedHero = database.GetHero(heroSaveData.Id);
                }

                if (hero.IsDefaultHero || hero.IsUnlockedByDefault)
                {
                    heroSaveData.IsUnlocked = true;

                    if (hero.IsDefaultHero && EquippedHero == null)
                    {
                        EquippedHero = hero;

                        heroSaveData.IsEquipped = true;
                    }
                }

                heroSaveData.OnHeroSelected += OnHeroSelectedEventFired;
            }

            for (int i = 0; i < heroSaves.Count; i++)
            {
                var heroSave = heroSaves[i];
                var data = database.GetHero(heroSave.Id);
                if (heroSave.IsEquipped && EquippedHero != data)
                {
                    heroSave.OnHeroSelected -= OnHeroSelectedEventFired;
                    heroSave.IsEquipped = false;
                    heroSave.OnHeroSelected += OnHeroSelectedEventFired;
                }
            }
        }

        protected virtual void OnHeroSelectedEventFired(HeroSaveData selectedHero)
        {
            if (selectedHero.IsEquipped)
            {
                EquippedHero = database.GetHero(selectedHero.Id);

                var saveHeroes = save.GetHeroes();

                for (int i = 0; i < saveHeroes.Count; i++)
                {
                    var hero = saveHeroes[i];
                    if (selectedHero != hero && hero.IsEquipped)
                    {
                        hero.IsEquipped = false;
                    }
                }

                OnSelectedHeroChanged?.Invoke();
            }
        }

        public virtual bool IsItemUnlocked(ItemData itemData)
        {
            var saveData = save.GetItem(itemData.Id);
            return saveData != null;
        }

        public virtual bool IsItemUnlocked(string id)
        {
            var saveData = save.GetItem(id);
            return saveData != null;
        }

        public virtual void UnlockItem(ItemData itemData)
        {
            save.AddItem(itemData.Id);
        }

        public virtual void UnlockItem(string id)
        {
            save.AddItem(id);
        }

        public virtual HeroSaveData GetHeroSave(string id)
        {
            return save.GetHeroSave(id);
        }

        public virtual HeroLevel GetEquippedHeroLevel()
        {
            var equippedHero = EquippedHero;
            var heroSave = GetHeroSave(equippedHero.Id);
            var level = equippedHero.GetHeroLevel(heroSave.Level);

            return level;
        }

        public virtual ItemData GetItemData(string id)
        {
            return database.GetItem(id);
        }

        public virtual List<ItemSaveData> GetItemSaveList()
        {
            return save.GetItems();
        }

        public virtual ItemLevel GetItemLevel(string id)
        {
            var itemSave = save.GetItem(id);
            var itemData = database.GetItem(id);

            var level = itemSave != null ? itemSave.Level : 0;
            var itemLevel = itemData.GetItemLevel(level);

            return itemLevel;
        }

        public virtual ItemLevel GetNextItemLevel(string id)
        {
            var itemSave = save.GetItem(id);
            var itemData = database.GetItem(id);

            var nextLevel = itemSave != null ? itemSave.Level + 1 : 0;
            var itemLevel = itemData.GetItemLevel(nextLevel);

            return itemLevel;
        }

        public virtual bool IncreaseItemLevel(string id)
        {
            var itemData = database.GetItem(id);

            return IncreaseItemLevel(itemData);
        }

        public virtual bool IncreaseItemLevel(ItemData itemData)
        {
            var itemSave = save.GetItem(itemData.Id);

            if (itemSave == null) return false;
            if (itemSave.Level >= itemData.ItemLevelCount - 1) return false;
            itemSave.Level++;

            return true;
        }

        public virtual ItemSaveData GetEquippedItemSave(ItemType itemType)
        {
            EquippedItems.TryGetValue(itemType, out ItemData itemData);

            if(itemData == null)
            {
                return null;
            }

            return save.GetItem(itemData.Id);
        }

        public virtual List<HeroData> GetAllHeroDataList()
        {
            return database.Heroes;
        }

        public virtual IHeroBehavior SpawnHero()
        {
            if (EquippedHero == null)
            {
                EquippedHero = database.GetDefaultHero();
            }

            var hero = Instantiate(EquippedHero.Prefab).GetComponent<IHeroBehavior>();
            hero.Init(EquippedHero);

            return hero;
        }

        public virtual AbstractWeaponBehavior SpawnWeapon()
        {
            var weaponSave = GetEquippedItemData(ItemType.Weapon);
            var data = database.GetItem(weaponSave.Id) as WeaponData;

            var weapon = Instantiate(data.WeaponPrefab).GetComponent<AbstractWeaponBehavior>();
            weapon.Init(data);

            return weapon;
        }

        public virtual WeaponAnimationsSet GetWeaponAnimationSet(IHeroBehavior hero, AbstractWeaponBehavior weapon)
        {
            return database.GetWeaponAnimationsSet(hero.HeroData.Id, weapon.WeaponData.Id);
        }

        public virtual WeaponAnimationsSet GetWeaponAnimationsSet(string heroId, string weaponId)
        {
            return database.GetWeaponAnimationsSet(heroId, weaponId);
        }

        public virtual ItemTypeData GetItemTypeData(ItemType itemType)
        {
            return database.GetItemTypeData(itemType);
        }

        public virtual ItemRarityData GetItemRarityData(ItemRarityType itemRarity)
        {
            return database.GetItemRarityData(itemRarity);
        }
    }
}