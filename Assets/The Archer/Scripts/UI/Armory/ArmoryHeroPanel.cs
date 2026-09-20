using OctoberStudio.Armory;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Weapon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class ArmoryHeroPanel : MonoBehaviour
    {
        [SerializeField] protected HeroPreviewBehavior heroPreviewBehavior;

        [Space]
        [SerializeField] protected GameObject itemCardPrefab;
        [SerializeField] protected RectTransform cardsParent;

        [Space]
        [SerializeField] protected RectTransform statsPanelRect;
        [SerializeField] protected ScalingLabelBehavior damageLabel;
        [SerializeField] protected ScalingLabelBehavior hpLabel;

        [Space]
        [SerializeField] protected List<SlotData> slots;

        protected List<ItemCardBehavior> equippedCards = new List<ItemCardBehavior>();

        protected PoolComponent<ItemCardBehavior> cardsPool;

        protected float statsPanelPadding;
        protected float statsPanelSpacing;

        protected virtual void Start()
        {
            cardsPool = new PoolComponent<ItemCardBehavior>(itemCardPrefab, slots.Count, cardsParent);

            statsPanelSpacing = hpLabel.RectTransform.anchoredPosition.x - damageLabel.RectTransform.anchoredPosition.x - damageLabel.Width;
            statsPanelPadding = (statsPanelRect.sizeDelta.x - damageLabel.Width - hpLabel.Width - statsPanelSpacing) / 2f;
        }

        public virtual void Init()
        {
            heroPreviewBehavior.Clear();
            heroPreviewBehavior.Show();

            GameController.ArmoryManager.OnItemEquipmentChanged += OnItemEquipmentChanged;
            GameController.ArmoryManager.OnSelectedHeroChanged += OnSelectedHeroChanged;

            InitSlots();

            RecalculateStats();

            SetSlotsScale(1f);
        }

        protected virtual void OnSelectedHeroChanged()
        {
            RecalculateStats();
        }

        protected virtual void OnItemEquipmentChanged()
        {
            InitSlots();

            RecalculateStats();
        }

        protected virtual void RecalculateStats()
        {
            var heroLevel = GameController.ArmoryManager.GetEquippedHeroLevel();

            var damage = heroLevel.GetStatValue(StatType.Damage);
            var hp = heroLevel.GetStatValue(StatType.HP);

            for (int i = 0; i < equippedCards.Count; i++)
            {
                var card = equippedCards[i];
                var itemLevel = GameController.ArmoryManager.GetItemLevel(card.ItemData.Id);

                damage += itemLevel.GetStatValue(StatType.Damage);
                hp += itemLevel.GetStatValue(StatType.HP);
            }

            damageLabel.SetAmount((int)damage);
            hpLabel.SetAmount((int)hp);

            statsPanelRect.SetSizeDeltaX(damageLabel.Width + hpLabel.Width + statsPanelSpacing + statsPanelPadding * 2f);
            damageLabel.RectTransform.SetAnchoredPositionX(statsPanelPadding);
            hpLabel.RectTransform.SetAnchoredPositionX(damageLabel.RectTransform.anchoredPosition.x + damageLabel.Width + statsPanelSpacing);
        }

        protected virtual void InitSlots()
        {
            ClearSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var data = GameController.ArmoryManager.GetEquippedItemData(slot.ItemType);
                if (data != null)
                {
                    var card = cardsPool.GetEntity();
                    var itemSave = GameController.ArmoryManager.GetEquippedItemSave(slot.ItemType);

                    card.SetData(data, itemSave);
                    card.RectTransform.anchoredPosition = slot.SlotRect.anchoredPosition;
                    card.RectTransform.sizeDelta = slot.SlotRect.sizeDelta;

                    card.transform.position = slot.SlotRect.position;

                    equippedCards.Add(card);

                    if (slot.ItemType == ItemType.Weapon)
                    {
                        heroPreviewBehavior.SetWeaponData(data as WeaponData);
                    }
                }
            }
        }

        protected IEasingCoroutine slotsScaleEasingCoroutine;
        protected float slotsScale = 1f;
        public virtual void ShowSlots()
        {
            slotsScaleEasingCoroutine.StopIfExists();

            slotsScaleEasingCoroutine = EasingManager.DoFloat(slotsScale, 1, 0.2f, SetSlotsScale).SetEasing(EasingType.CubicOut);
        }

        protected virtual void SetSlotsScale(float value)
        {
            slotsScale = value;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.SlotRect.localScale = new Vector3(value, value, value);
            }

            for (int i = 0; i < equippedCards.Count; i++)
            {
                var card = equippedCards[i];
                card.RectTransform.localScale = new Vector3(value, value, value);
            }
        }

        public virtual void HideSlots()
        {
            slotsScaleEasingCoroutine.StopIfExists();

            slotsScaleEasingCoroutine = EasingManager.DoFloat(slotsScale, 0, 0.2f, SetSlotsScale).SetEasing(EasingType.CubicIn);
        }

        public virtual void CalculateNavigation(Selectable firstCard)
        {
            if (equippedCards.Count == 0)
            {
                return;
            }

            for (int i = 0; i < equippedCards.Count; i++)
            {
                var card = equippedCards[i];

                var topCards = equippedCards.Where(c => c.RectTransform.position.y > card.RectTransform.position.y)
                    .OrderBy(c => Mathf.Abs(c.RectTransform.position.y - card.RectTransform.position.y) * 10 + Mathf.Abs(c.RectTransform.position.x - card.RectTransform.position.x))
                    .FirstOrDefault();

                var leftCard = equippedCards
                    .FindAll(c => c.RectTransform.position.x < card.RectTransform.position.x)
                    .OrderBy(c => Mathf.Abs(card.RectTransform.position.x - c.RectTransform.position.x) * 10 + Mathf.Abs(c.RectTransform.position.y - card.RectTransform.position.y))
                    .FirstOrDefault();

                var rightCard = equippedCards
                    .FindAll(c => c.RectTransform.position.x > card.RectTransform.position.x)
                    .OrderBy(c => Mathf.Abs(card.RectTransform.position.x - c.RectTransform.position.x) * 10 + Mathf.Abs(c.RectTransform.position.y - card.RectTransform.position.y))
                    .FirstOrDefault();

                var bottomCards = equippedCards
                    .FindAll(c => c.RectTransform.position.y < card.RectTransform.position.y)
                    .OrderBy(c => Mathf.Abs(card.RectTransform.position.y - c.RectTransform.position.y) * 10 + Mathf.Abs(c.RectTransform.position.x - card.RectTransform.position.x))
                    .FirstOrDefault();

                var leftSelectable = leftCard != null ? leftCard.Selectable : null;
                var rightSelectable = rightCard != null ? rightCard.Selectable : null;
                var upSelectable = topCards != null ? topCards.Selectable : null;
                var downSelectable = bottomCards != null ? bottomCards.Selectable : firstCard;

                card.SetNavigation(leftSelectable, rightSelectable, upSelectable, downSelectable);
            }
        }

        public virtual Selectable GetLowestSelectable()
        {
            if (equippedCards.Count == 0)
            {
                return null;
            }

            equippedCards.Sort(PositionsComparator);

            return equippedCards[0].Selectable;
        }

        protected virtual int PositionsComparator(ItemCardBehavior card1, ItemCardBehavior card2)
        {
            var compareY = card1.RectTransform.position.y.CompareTo(card2.RectTransform.position.y);

            if (compareY != 0)
            {
                return compareY;
            }

            return card1.RectTransform.position.x.CompareTo(card2.RectTransform.position.x);
        }

        public virtual void Clear()
        {
            heroPreviewBehavior.Clear();

            ClearSlots();

            GameController.ArmoryManager.OnItemEquipmentChanged -= OnItemEquipmentChanged;
            GameController.ArmoryManager.OnSelectedHeroChanged -= OnSelectedHeroChanged;
        }

        protected virtual void ClearSlots()
        {
            foreach (var card in equippedCards)
            {
                card.gameObject.SetActive(false);
                card.Clear();
            }

            equippedCards.Clear();
        }

        [System.Serializable]
        protected class SlotData
        {
            [SerializeField] protected ItemType itemType;
            [SerializeField] protected RectTransform slotRect;

            public ItemType ItemType => itemType;
            public RectTransform SlotRect => slotRect;
        }
    }
}