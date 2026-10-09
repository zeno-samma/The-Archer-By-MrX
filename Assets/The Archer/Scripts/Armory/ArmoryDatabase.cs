using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Armory
{
    [CreateAssetMenu(fileName = "Armory Database", menuName = "October/Armory/Armory Database")]
    public class ArmoryDatabase : ScriptableObject
    {
        [SerializeField] protected List<ItemData> items;
        [SerializeField] protected List<HeroData> heroes;
        [SerializeField] protected List<ItemRarityData> itemRarities;
        [SerializeField] protected List<ItemTypeData> itemTypes;
        [SerializeField] protected WeaponAnimationSetDataList weaponAnimationSetData;

        public int ItemsCount => items.Count;
        public List<ItemData> Items => items;

        public int HeroesCount => heroes.Count;
        public List<HeroData> Heroes => heroes;

        public int ItemRaritiesCount => itemRarities.Count;
        public List<ItemRarityData> ItemRarities => itemRarities;

        public int ItemTypesCount => itemTypes.Count;
        public List<ItemTypeData> ItemTypes => itemTypes;

        public virtual List<ItemData> GetItems(ItemType itemType)
        {
            return items.Where(item => item.ItemType == itemType).ToList();
        }

        public virtual ItemData GetItem(string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if(items[i].Id == id) return items[i];  
            }

            return null;
        }

        public virtual HeroData GetHero(string id)
        {
            return heroes.FirstOrDefault(hero => hero.Id == id);
        }

        public virtual HeroData GetDefaultHero()
        {
            var defaultHero = heroes.FirstOrDefault(hero => hero.IsDefaultHero);

            if(defaultHero != null) return defaultHero;

            defaultHero = heroes.FirstOrDefault(hero => hero.IsUnlockedByDefault);

            if(defaultHero != null) return defaultHero;

            Debug.LogError("No default hero found in the database.");
            return heroes[0];
        }

        public virtual WeaponAnimationsSet GetWeaponAnimationsSet(string heroId, string weaponId)
        {
            var set = weaponAnimationSetData.Sets.FirstOrDefault(s => s.WeaponId == weaponId && s.HeroId == heroId);
            
            if (set == null)
            {
                Debug.LogError($"No weapon animation set found for hero '{heroId}' and weapon '{weaponId}'.");
                return null;
            }

            return set.AnimationsSet;
        }

        public virtual ItemRarityData GetItemRarityData(ItemRarityType rarityType)
        {
            return itemRarities.FirstOrDefault(rarity => rarity.RarityType == rarityType);
        }

        public virtual ItemTypeData GetItemTypeData(ItemType itemType)
        {
            return itemTypes.FirstOrDefault(type => type.ItemType == itemType);
        }

        protected virtual void OnValidate()
        {
            for(int i = 0; i < items.Count - 1; i++)
            {
                if (items[i] == null) continue;
                for(int j = i + 1; j < items.Count; j++)
                {
                    if (items[j] == null) continue;
                    if (items[i].Id == items[j].Id)
                    {
                        items[j].RecalculateId();
                    }
                }
            }

            for(int i = 0; i < heroes.Count - 1; i++)
            {
                if (heroes[i] == null) continue;
                for(int j = i + 1; j < heroes.Count; j++)
                {
                    if (heroes[j] == null) continue;
                    if (heroes[i].Id == heroes[j].Id)
                    {
                        heroes[j].RecalculateId();
                    }
                }
            }

            weaponAnimationSetData.Validate(this);
        }
    }

    [System.Serializable]
    public class ItemRarityData
    {
        [SerializeField] protected ItemRarityType rarityType;

        [Space]
        [SerializeField] protected Color mainColor;
        [SerializeField] protected Color secondaryColor;

        [Space]
        [SerializeField] protected Sprite wideBorderBackground;
        [SerializeField] protected Sprite narrowBorderBackground;

        public ItemRarityType RarityType => rarityType;

        public Color MainColor => mainColor;
        public Color SecondaryColor => secondaryColor;

        public Sprite WideBorderBackground => wideBorderBackground;
        public Sprite NarrowBorderBackground => narrowBorderBackground;
    }

    [System.Serializable]
    public class ItemTypeData
    {
        [SerializeField] protected ItemType itemType;
        [SerializeField] protected Sprite itemTypeIcon;

        public ItemType ItemType => itemType;
        public Sprite ItemTypeIcon => itemTypeIcon;
    }
}