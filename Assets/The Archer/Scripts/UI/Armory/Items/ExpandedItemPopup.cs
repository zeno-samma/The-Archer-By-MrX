using OctoberStudio.Abilities;
using OctoberStudio.Armory;
using OctoberStudio.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OctoberStudio.UI.Armory
{
    public class ExpandedItemPopup : ExpandedArmoryPopup
    {
        [Space]
        [SerializeField] protected ItemCardBehavior itemCard;

        public ItemData Data { get; protected set; }
        public ItemSaveData SaveData { get; protected set; }

        public virtual void Show(ItemData data, ItemSaveData saveData)
        {
            gameObject.SetActive(true);

            Data = data;
            SaveData = saveData;

            InitVisuals();

            StartCoroutine(ShowCoroutine());

            SubscribeToEvents();

            SelectButtonsGamepad();
        }

        protected override IEnumerator HideCoroutine()
        {
            yield return base.HideCoroutine();

            itemCard.Clear();
            gameObject.SetActive(false);
        }

        protected override void InitVisuals()
        {
            nameText.text = Data.ItemName;

            InitText();

            itemCard.Clear();
            itemCard.SetData(Data, SaveData);

            statsGrid.Init(Data, true, SaveData.Level);

            InitAbilities();

            var height = heightDifference + statsGrid.Height + abilitiesRect.sizeDelta.y;
            panelRect.SetSizeDeltaY(height);

            var closeButtonPosition = defaultCloseButtonPosition + (height - defaultHeight) / 2f;
            closeButtonRect.SetAnchoredPositionY(closeButtonPosition);

            if (SaveData.Level == Data.ItemLevelCount - 1)
            {
                InitButtonsMaxLevel();
            }
            else
            {
                InitButtonsUpgradable();
            }
        }

        protected virtual void InitButtonsUpgradable()
        {
            var nextLevel = Data.GetItemLevel(SaveData.Level + 1);

            if (SaveData.IsEquipped)
            {
                selectButtonText.text = "Unequip";

                if (Data.ItemType == ItemType.Weapon)
                {
                    selectButton.enabled = false;
                    selectButton.image.sprite = disabledButtonSprite;
                }
                else
                {
                    selectButton.enabled = true;
                    selectButton.image.sprite = selectButtonSprite;
                }
            }
            else
            {
                selectButtonText.text = "Equip";

                selectButton.enabled = true;
                selectButton.image.sprite = selectButtonSprite;
            }

            upgradeButtonLabel.gameObject.SetActive(true);
            upgradeButtonText.gameObject.SetActive(true);
            maxLvlButtonText.gameObject.SetActive(false);

            upgradeButtonText.text = "Upgrade";

            var cost = nextLevel.Price.Amount;
            upgradeButton.SetCurrency(GameController.CurrenciesManager.GetCurrency(nextLevel.Price.CurrencyId, false));
            upgradeButton.SetCost(cost);

            if (upgradeButton.HasEnoughMoney)
            {
                costText.color = enabledCostTextColor;
            }
            else
            {
                costText.color = disabledCostTextColor;
            }
        }

        protected virtual void InitButtonsMaxLevel()
        {
            if (SaveData.IsEquipped)
            {
                selectButtonText.text = "Unequip";

                if (Data.ItemType == ItemType.Weapon)
                {
                    selectButton.enabled = false;
                    selectButton.image.sprite = disabledButtonSprite;
                }
                else
                {
                    selectButton.enabled = true;
                    selectButton.image.sprite = selectButtonSprite;
                }
            }
            else
            {
                selectButtonText.text = "Equip";

                selectButton.enabled = true;
                selectButton.image.sprite = selectButtonSprite;
            }

            upgradeButtonLabel.gameObject.SetActive(false);
            upgradeButtonText.gameObject.SetActive(false);
            maxLvlButtonText.gameObject.SetActive(true);

            upgradeButton.DisableButton();
        }

        protected virtual void InitAbilities()
        {
            cardsPool.DisableAllEntities();

            var abilities = new List<AbilityType>();
            var cards = new List<ArmoryAbilityCard>();

            for (int i = 0; i < Data.ItemLevelCount; i++)
            {
                var heroLevel = Data.GetItemLevel(i);
                if (heroLevel.AttachedAbilities != null && heroLevel.AttachedAbilities.Count > 0)
                {
                    foreach (var abilityType in heroLevel.AttachedAbilities)
                    {
                        if (!abilities.Contains(abilityType))
                        {
                            abilities.Add(abilityType);
                            cards.Add(SpawnAbilityCard(abilityType, i));
                        }
                    }
                }
            }

            var spacing = abilitieLayout.spacing;

            if (abilities.Count > 0)
            {
                var height = cards[0].Height * abilities.Count + spacing * (abilities.Count - 1);
                abilitiesRect.SetSizeDeltaY(height);
            }
            else
            {
                abilitiesRect.SetSizeDeltaY(0f);
            }
        }

        protected override bool IsLevelReached(int level)
        {
            return SaveData.Level >= level;
        }

        protected virtual void InitText()
        {
            levelText.text = $"Level {SaveData.Level + 1}";

            var itemLevel = Data.GetItemLevel(SaveData.Level);
            var rarityData = GameController.ArmoryManager.GetItemRarityData(itemLevel.ItemRarity);

            rarityText.color = rarityData.MainColor;
            rarityText.text = itemLevel.ItemRarity.ToString();
        }

        protected override void OnSelectButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();
            SaveData.IsEquipped = !SaveData.IsEquipped;

            InitVisuals();

            if (Data.ItemType == ItemType.Weapon)
            {
                if (upgradeButton.ButtonEnabled)
                {
                    EventSystem.current.SetSelectedGameObject(upgradeButton.Button.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        protected override void OnUpgradeButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();
            SaveData.Level++;

            var cost = Data.GetItemLevel(SaveData.Level).Price.Amount;
            upgradeButton.Currency.Withdraw(cost);

            InitVisuals();

            if (!upgradeButton.ButtonEnabled)
            {
                if (SaveData.IsEquipped && Data.ItemType == ItemType.Weapon)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(selectButton.gameObject);
                }
            }
        }
    }
}