using MrX.Abilities;
using MrX.Armory;
using MrX.Extensions;
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

            statsGrid.Init(Data, SaveData.IsUnlocked, SaveData.Level);

            InitAbilities();

            var height = heightDifference + statsGrid.Height + abilitiesRect.sizeDelta.y;
            panelRect.SetSizeDeltaY(height);

            var closeButtonPosition = defaultCloseButtonPosition + (height - defaultHeight) / 2f;
            closeButtonRect.SetAnchoredPositionY(closeButtonPosition);

            if (!SaveData.IsUnlocked)
            {
                InitButtonsNotBought();
            }
            else
            {
                if (SaveData.Level == Data.HeroLevelCount - 1)
                {
                    InitButtonsMaxLevel();
                }
                else
                {
                    InitButtonsUpgradable();
                }
            if (SaveData.IsUnlocked && !SaveData.IsEquipped && !GameController.ArmoryManager.CanSelectHero(Data))
            { selectButton.interactable = false; selectButtonText.text = "Bag full"; }
            }
        }

        protected virtual void InitButtonsUpgradable()
        {
            InitButtonsMaxLevel();
            maxLvlButtonText.text=MrX.Localization.MenuLanguage.Text("Gain hero EXP by completing runs");
        }

        protected virtual void InitButtonsMaxLevel()
        {
            maxLvlButtonText.text=MrX.Localization.MenuLanguage.Text("MAX");
            if (SaveData.IsEquipped)
            {
                selectButtonText.text = "Selected";

                selectButton.interactable = false;
            }
            else
            {
                selectButtonText.text = "Select";

                selectButton.interactable = true;
            }

            upgradeButtonLabel.gameObject.SetActive(false);
            upgradeButtonText.gameObject.SetActive(false);
            maxLvlButtonText.gameObject.SetActive(true);

            upgradeButton.DisableButton();
        }

        protected virtual void InitButtonsNotBought()
        {
            selectButton.interactable = false;
            selectButtonText.text = "Select";

            upgradeButtonLabel.gameObject.SetActive(true);
            upgradeButtonText.gameObject.SetActive(true);
            maxLvlButtonText.gameObject.SetActive(false);

            upgradeButtonText.text = "Unlock";

            var firstLevel = Data.GetHeroLevel(0);
            var cost = firstLevel.Price.Amount;
            upgradeButton.SetCurrency(GameController.CurrenciesManager.GetCurrency(firstLevel.Price.CurrencyId, false));
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
            return SaveData.IsUnlocked && SaveData.Level >= level;
        }

        protected virtual void InitText()
        {
            levelText.text = $"Level {SaveData.Level + 1}";

            var heroLevel = Data.GetHeroLevel(SaveData.Level);
            var rarityData = GameController.ArmoryManager.GetItemRarityData(heroLevel.HeroRarity);

            rarityText.color = rarityData.MainColor;
            rarityText.text = heroLevel.HeroRarity.ToString();
        }

        protected override void OnSelectButtonClicked()
        {


            GameController.ArmoryManager.TrySelectHero(Data);

            InitVisuals();

            if (upgradeButton.ButtonEnabled)
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
            if(SaveData.IsUnlocked)return;
            const int targetLevel = 0;
            if (targetLevel >= Data.HeroLevelCount) return;
            var price = Data.GetHeroLevel(targetLevel).Price;
            var currency = GameController.CurrenciesManager.GetCurrency(price.CurrencyId, false);
            if (currency == null || currency.Amount < price.Amount) return;


            SaveData.IsUnlocked = true;

            currency.Withdraw(price.Amount);

            InitVisuals();

            if (!upgradeButton.ButtonEnabled)
            {
                if (!selectButton.interactable)
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
