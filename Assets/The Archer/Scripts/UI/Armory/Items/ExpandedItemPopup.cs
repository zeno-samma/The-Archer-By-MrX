using MrX.Abilities;
using MrX.Armory;
using MrX.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace MrX.UI.Armory
{
    public class ExpandedItemPopup : ExpandedArmoryPopup
    {
        [Space]
        [SerializeField] protected ItemCardBehavior itemCard;

        [Header("Delete Item")]
        [SerializeField] private Button deleteButton;
        [SerializeField] private TMP_Text deleteLabel;
        [SerializeField] private GameObject deleteConfirmation;
        [SerializeField] private TMP_Text deletePrompt;
        [SerializeField] private Button cancelDeleteButton;
        [SerializeField] private Button confirmDeleteButton;

        protected override void Awake()
        {
            base.Awake();
            if (deleteButton == null) return;
            deleteButton.onClick.AddListener(ShowDeleteConfirmation);
            cancelDeleteButton.onClick.AddListener(CancelDeletion);
            confirmDeleteButton.onClick.AddListener(ConfirmDeletion);
        }

        private void ShowDeleteConfirmation()
        {
            RefreshDeleteButton();
            if (!deleteButton.interactable) return;

            var rarity = Data.GetItemLevel(SaveData.Level).ItemRarity;
            deletePrompt.text = $"Delete {Data.ItemName}?\n{rarity} · Lv. {SaveData.Level + 1}\nThe item and its upgrades will be lost.\nA queued reward may fill the freed slot.";
            deleteConfirmation.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(cancelDeleteButton.gameObject);
        }

        private void CancelDeletion()
        {
            deleteConfirmation.SetActive(false);
            RefreshDeleteButton();
            if (deleteButton.interactable)
                EventSystem.current?.SetSelectedGameObject(deleteButton.gameObject);
            else
                SelectButtonsGamepad();
        }

        private void ConfirmDeletion()
        {
            if (!deleteConfirmation.activeSelf) return;
            if (GameController.ArmoryManager.TryDeleteItem(Data.Id))
            {
                deleteConfirmation.SetActive(false);
                OnCloseButtonClicked();
            }
            else
            {
                CancelDeletion();
            }
        }

        private void RefreshDeleteButton()
        {
            if (deleteButton == null) return;
            string reason = GameController.ArmoryManager.GetItemDeletionBlockReason(Data.Id);
            deleteButton.interactable = reason == null;
            deleteLabel.text = reason ?? "Delete item";
        }

        protected override void SelectButtonsGamepad()
        {
            if (deleteConfirmation != null && deleteConfirmation.activeSelf)
                EventSystem.current?.SetSelectedGameObject(cancelDeleteButton.gameObject);
            else
                base.SelectButtonsGamepad();
        }

        protected override void OnCancelButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (deleteConfirmation != null && deleteConfirmation.activeSelf)
                CancelDeletion();
            else
                base.OnCancelButtonPressed(context);
        }

        protected override void OnCloseButtonClicked()
        {
            if (deleteConfirmation != null) deleteConfirmation.SetActive(false);
            base.OnCloseButtonClicked();
        }

        public ItemData Data { get; protected set; }
        public ItemSaveData SaveData { get; protected set; }
        private float nextUpgradeTime;

        public virtual void Show(ItemData data, ItemSaveData saveData)
        {
            gameObject.SetActive(true);

            Data = data;
            SaveData = saveData;
            if (deleteConfirmation != null) deleteConfirmation.SetActive(false);

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

            RefreshDeleteButton();
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
            // Debug.Log($"[EQUIP BUTTON] Clicked | Frame: {Time.frameCount}");
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
            // Debug.Log("[EQUIP] BUTTON FINISHED");
        }

        protected override void OnUpgradeButtonClicked()
        {
            if (Time.unscaledTime < nextUpgradeTime) return;
            var result = GameController.ArmoryManager.Forge.TryUpgrade(Data.Id, SaveData.Level);
            if (result != ForgeResult.Success) { InitVisuals(); return; }
            nextUpgradeTime = Time.unscaledTime + .35f;
            GameController.AudioManager.PlayButtonClick();

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
