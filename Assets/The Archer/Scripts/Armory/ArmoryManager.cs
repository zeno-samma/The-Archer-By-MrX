using MrX.Abilities;
using MrX.Armory;
using MrX.Player;
using MrX.Weapon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MrX
{
    public class ArmoryManager : MonoBehaviour
    {
        [SerializeField] protected ArmoryDatabase database;
        protected ArmorySave save;
        public EquipmentForge Forge { get; private set; }
        public MrX.Crafting.CraftingManager Crafting { get; private set; }
        public InventoryBagService Bag { get; private set; }
        public Rewards.BossChestService BossChests { get; private set; }
        public event UnityAction OnInventoryChanged;

        protected Dictionary<ItemType, ItemData> EquippedItems { get; set; }
        public HeroData EquippedHero { get; protected set; }

        public event UnityAction OnItemEquipmentChanged;
        public event UnityAction OnSelectedHeroChanged;
        private bool changingHero;

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
            if (!EnsureHeroWeapon(EquippedHero))
            {
                EquippedHero = database.GetDefaultHero();
                save.GetHeroSave(EquippedHero.Id).IsEquipped = true;
                EnsureHeroWeapon(EquippedHero);
            }
            var dashboard = Resources.Load<UI.ArmoryDashboardDatabase>(UI.ArmoryDashboardDatabase.ResourcePath);
            Bag = new InventoryBagService(save, dashboard, GameController.CurrenciesManager, GameController.SaveManager, ClaimPendingEquipment);
            Forge = new EquipmentForge(database, save, GameController.CurrenciesManager, GameController.SaveManager);
            var craftingDatabase = Resources.Load<MrX.Crafting.CraftingDatabase>(MrX.Crafting.CraftingDatabase.ResourcePath);
            if (craftingDatabase != null)
            {
                Crafting = gameObject.AddComponent<MrX.Crafting.CraftingManager>();
                Crafting.Initialize(craftingDatabase, database, save, GameController.SaveManager, UnlockItem);
            }
            BossChests = new Rewards.BossChestService(this, new Rewards.LuckyRewardDelivery(this, GameController.CurrenciesManager), GameController.SaveManager);
            BossChests.RecoverPending();
        }

        protected virtual void InitItems()
        {
            EquippedItems = new Dictionary<ItemType, ItemData>();

            var itemSaves = save.GetItems();

            for (int i = 0; i < itemSaves.Count; i++)
            {
                var itemSave = itemSaves[i];

                var data = database.GetItem(itemSave.Id);
                if (data != null && itemSave.Level >= data.ItemLevelCount)
                {
                    itemSave.Level = data.ItemLevelCount - 1;
                }

                if (itemSave.IsEquipped)
                {
                    if (data == null)
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

            foreach (var itemData in EquippedItems.Values)
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
            // ==================Debug=========================
            // Debug.Log(
            //     $"[MANAGER EQUIP] Frame: {Time.frameCount}" +
            //     $" | Subscribers: {OnItemEquipmentChanged?.GetInvocationList().Length ?? 0}");
            // var handlers = OnItemEquipmentChanged?.GetInvocationList();
            // if (handlers != null)
            // {
            //     foreach (var handler in handlers)
            //     {
            //         string status = "MANAGED";

            //         if (handler.Target is UnityEngine.Object obj)
            //         {
            //             status = obj == null ? "DESTROYED" : "ALIVE";
            //         }

            //         Debug.Log(
            //             $"[SUBSCRIBER] " +
            //             $"{handler.Method.DeclaringType?.Name}." +
            //             $"{handler.Method.Name} | {status}"
            //         );
            //     }
            // }
            // ============================
            var itemData = database.GetItem(itemSaveData.Id);
            if (itemData == null) return;
            if (isEquipped && itemData is WeaponData candidate && !candidate.Supports(EquippedHero) && !changingHero)
            { itemSaveData.IsEquipped = false; return; }
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

            if (!changingHero) OnItemEquipmentChanged?.Invoke();
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
                if (hero == null) continue;
                var heroSaveData = save.GetHeroSave(hero.Id);

                if (hero != null && heroSaveData.Level >= hero.HeroLevelCount)
                {
                    heroSaveData.Level = hero.HeroLevelCount - 1;
                }

                if (heroSaveData.IsEquipped && !hero.HideInRoster && EquippedHero == null)
                {
                    EquippedHero = database.GetHero(heroSaveData.Id);
                }

                if (hero.IsDefaultHero || hero.IsUnlockedByDefault)
                {
                    heroSaveData.IsUnlocked = true;

                    if (hero.IsDefaultHero && !hero.HideInRoster && EquippedHero == null)
                    {
                        EquippedHero = hero;

                        heroSaveData.IsEquipped = true;
                    }
                }

                heroSaveData.OnHeroSelected -= OnHeroSelectedEventFired;
                heroSaveData.OnHeroSelected += OnHeroSelectedEventFired;
            }

            if (EquippedHero == null)
            {
                EquippedHero = database.GetDefaultHero();
                var fallback = save.GetHeroSave(EquippedHero.Id);
                fallback.IsUnlocked = true;
                fallback.IsEquipped = true;
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
            if (changingHero) return;
            if (selectedHero.IsEquipped)
            {
                var data = database.GetHero(selectedHero.Id);
                if (!CanSelectHero(data)) { selectedHero.IsEquipped = false; return; }
                changingHero = true;
                EquippedHero = data;
                EnsureHeroWeapon(data);

                var saveHeroes = save.GetHeroes();

                for (int i = 0; i < saveHeroes.Count; i++)
                {
                    var hero = saveHeroes[i];
                    if (selectedHero != hero && hero.IsEquipped)
                    {
                        hero.IsEquipped = false;
                    }
                }

                changingHero = false;
                OnSelectedHeroChanged?.Invoke();
            }
        }

        private WeaponData FindOwnedWeapon(HeroData hero)
        {
            var current = GetEquippedItemData(ItemType.Weapon) as WeaponData;
            if (current != null && current.Supports(hero)) return current;
            if (hero.StarterWeapon != null && save.GetItem(hero.StarterWeapon.Id) != null && hero.StarterWeapon.Supports(hero)) return hero.StarterWeapon;
            foreach (var item in database.Items)
                if (item is WeaponData weapon && weapon.Supports(hero) && save.GetItem(weapon.Id) != null) return weapon;
            return null;
        }

        public bool CanSelectHero(HeroData hero)
        {
            return hero != null && database.GetHero(hero.Id) == hero && !hero.HideInRoster && save.GetHeroSave(hero.Id).IsUnlocked &&
                (FindOwnedWeapon(hero) != null || (hero.StarterWeapon != null && database.GetItem(hero.StarterWeapon.Id) == hero.StarterWeapon && hero.StarterWeapon.Supports(hero) && save.HasBagSpace));
        }

        public bool TrySelectHero(HeroData hero)
        {
            if (!CanSelectHero(hero)) return false;
            save.GetHeroSave(hero.Id).IsEquipped = true;
            return EquippedHero == hero;
        }

        private bool EnsureHeroWeapon(HeroData hero)
        {
            if (hero == null) return false;
            var weapon = FindOwnedWeapon(hero);
            if (weapon == null && hero.StarterWeapon != null && hero.StarterWeapon.Supports(hero) && save.HasBagSpace)
            {
                weapon = hero.StarterWeapon;
                var starter = save.AddItem(weapon.Id);
                starter.OnItemEquipped -= OnItemEquippedEventFired;
                starter.OnItemEquipped += OnItemEquippedEventFired;
            }
            if (weapon == null) return false;
            var itemSave = save.GetItem(weapon.Id);
            if (!itemSave.IsEquipped) itemSave.IsEquipped = true;
            return true;
        }

        public bool CanEquipItem(ItemData item) => !(item is WeaponData weapon) || weapon.Supports(EquippedHero);

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
            // save.AddItem(itemData.Id);
            if (itemData != null)
            {
                UnlockItem(itemData.Id);
            }
        }

        public virtual void UnlockItem(string id)
        {
            if (database.GetItem(id) == null) return;
            if (save.GetItem(id) == null && !save.HasBagSpace)
            { save.QueueEquipment(id); OnInventoryChanged?.Invoke(); return; }
            // save.AddItem(id);
            var itemSave = save.AddItem(id);
            GameController.AchievementsManager?.RecordItemOwned(id);

            // Đăng ký sự kiện lắng nghe ngay khi item được mở khóa
            if (itemSave != null)
            {
                itemSave.OnItemEquipped -= OnItemEquippedEventFired; // Hủy đăng ký cũ nếu có để tránh bị duplicate
                itemSave.OnItemEquipped += OnItemEquippedEventFired; // Đăng ký sự kiện mới
            }
            Crafting?.Service.OnItemUnlocked(id);
            OnInventoryChanged?.Invoke();
        }

        public void ClaimPendingEquipment()
        {
            var pending = save.PendingEquipment;
            for (var i = 0; i < pending.Count;)
            {
                var entry = pending[i]; var data = database.GetItem(entry.id);
                if (data == null) { i++; continue; }
                var item = save.GetItem(entry.id);
                if (item == null && !save.HasBagSpace) break;
                var isNew = item == null; if (isNew) item = save.AddItem(entry.id);
                // Remove the pending unit before publishing item events.
                entry.amount--; if (entry.amount <= 0) pending.RemoveAt(i);
                if (isNew) UnlockItem(entry.id); else IncreaseItemLevel(data);
            }
            OnInventoryChanged?.Invoke();
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

            if (itemData == null)
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
            // Debug.Log($"Lấy dữ liệu độ hiếm của vật phẩm ở manager: {itemRarity}");
            return database.GetItemRarityData(itemRarity);
        }
    }
}
