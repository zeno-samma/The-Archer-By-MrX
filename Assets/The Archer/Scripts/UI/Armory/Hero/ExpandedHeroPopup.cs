using MrX.Abilities;
using MrX.Armory;
using MrX.Extensions;
using MrX.Progression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MrX.UI.Armory
{
    public class ExpandedHeroPopup : ExpandedArmoryPopup
    {
        [Space]
        [SerializeField] protected HeroCardBehavior heroCard;

        public HeroData Data { get; protected set; }
        public HeroSaveData SaveData { get; protected set; }

        private int EffectiveLevelIndex => HeroProgression.GetEffectiveLevelIndex(Data, SaveData);

        public virtual void Show(HeroData data, HeroSaveData saveData)
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

            heroCard.Clear();
            gameObject.SetActive(false);
        }

        protected override void InitVisuals()
        {
            nameText.text = Data.Name;

            InitText();

            heroCard.Clear();
            heroCard.SetData(Data, SaveData);

            statsGrid.Init(Data, SaveData.IsUnlocked, EffectiveLevelIndex);

            InitAbilities();

            var height = heightDifference + statsGrid.Height + abilitiesRect.sizeDelta.y;
            panelRect.SetSizeDeltaY(height);

            var closeButtonPosition = defaultCloseButtonPosition + (height - defaultHeight) / 2f;
            closeButtonRect.SetAnchoredPositionY(closeButtonPosition);

            if (SaveData.IsUnlocked)
                InitButtonsMaxLevel();
            else
                InitButtonsNotBought();
        }

        protected virtual void InitButtonsUpgradable()
        {
            InitButtonsMaxLevel();
        }

        protected virtual void InitButtonsMaxLevel()
        {
            if (SaveData.IsEquipped)
            {
                selectButtonText.text = "Selected";

                selectButton.enabled = false;
                selectButton.image.sprite = disabledButtonSprite;
            }
            else
            {
                selectButtonText.text = "Select";

                selectButton.enabled = true;
                selectButton.image.sprite = selectButtonSprite;
            }

            upgradeButtonLabel.gameObject.SetActive(false);
            upgradeButtonText.gameObject.SetActive(false);
            maxLvlButtonText.gameObject.SetActive(false);

            upgradeButton.Button.interactable = false;
            upgradeButton.DisableButton();
        }

        protected virtual void InitButtonsNotBought()
        {
            selectButton.enabled = false;
            selectButton.image.sprite = disabledButtonSprite;
            selectButtonText.text = "Select";

            upgradeButton.gameObject.SetActive(true);
            upgradeButtonLabel.gameObject.SetActive(true);
            upgradeButtonText.gameObject.SetActive(true);
            maxLvlButtonText.gameObject.SetActive(false);

            upgradeButtonText.text = "Unlock";

            var price = Data.GetHeroLevel(0).Price;
            var currency = GameController.CurrenciesManager.GetCurrency(price.CurrencyId, false);
            bool hasValidPrice = currency != null && price.Amount >= 0;
            upgradeButton.Button.interactable = hasValidPrice;

            if (!hasValidPrice)
            {
                upgradeButton.DisableButton();
                costText.color = disabledCostTextColor;
                return;
            }

            upgradeButton.SetCurrency(currency);
            upgradeButton.SetCost(price.Amount);

            costText.color = upgradeButton.HasEnoughMoney ? enabledCostTextColor : disabledCostTextColor;
        }

        protected virtual void InitAbilities()
        {
            cardsPool.DisableAllEntities();

            var abilities = new List<AbilityType>();
            var cards = new List<ArmoryAbilityCard>();

            for (int i = 0; i < Data.HeroLevelCount; i++)
            {
                var heroLevel = Data.GetHeroLevel(i);
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
            return SaveData.IsUnlocked && EffectiveLevelIndex >= level;
        }

        protected virtual void InitText()
        {
            int levelIndex = EffectiveLevelIndex;
            levelText.text = $"Level {levelIndex + 1}";

            var heroLevel = Data.GetHeroLevel(levelIndex);
            var rarityData = GameController.ArmoryManager.GetItemRarityData(heroLevel.HeroRarity);

            rarityText.color = rarityData.MainColor;
            rarityText.text = heroLevel.HeroRarity.ToString();
        }

        protected override void OnSelectButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            SaveData.IsEquipped = true;

            InitVisuals();

            if (upgradeButton.ButtonEnabled && upgradeButton.Button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(upgradeButton.Button.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        protected override void OnUpgradeButtonClicked()
        {
            var hero = SaveData;
            if (hero == null || hero.IsUnlocked || Data == null || Data.HeroLevelCount == 0) return;

            var price = Data.GetHeroLevel(0).Price;
            var currency = GameController.CurrenciesManager.GetCurrency(price.CurrencyId, false);
            if (currency == null || !currency.TryWithdrawAndCommit(price.Amount, () => hero.IsUnlocked = true))
                return;

            GameController.AudioManager.PlayButtonClick();
            InitVisuals();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(selectButton.enabled ? selectButton.gameObject : null);
        }
    }
}
