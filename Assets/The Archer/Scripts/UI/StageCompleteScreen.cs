using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageCompleteScreen : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected Image titleGlow;

        [Space]
        [SerializeField] protected RectTransform rewardsTitle;
        [SerializeField] protected GridLayoutGroup rewardsGrid;

        [Space]
        [SerializeField] protected Button continueButton;

        [SerializeField] protected SmallRewardCard coinsRewardCard;
        [SerializeField] protected GameObject itemRewardCardPrefab;

        [Space]
        [SerializeField] protected AudioData successSound;

        protected PoolComponent<SmallItemRewardCard> cardsPool;
        protected Dictionary<CurrencySave, SmallRewardCard> currencyRewardCards = new Dictionary<CurrencySave, SmallRewardCard>();

        protected RectTransform gridRect;

        protected float buttonPositionDifference;
        protected float glowAlpha;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public bool IsOpen { get; protected set; } = false;
        protected bool isInitialized;

        protected virtual void Init()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            gridRect = rewardsGrid.GetComponent<RectTransform>();

            CreateCurrencyRewardCards();
            cardsPool = new PoolComponent<SmallItemRewardCard>(itemRewardCardPrefab, 5, gridRect);

            buttonPositionDifference = gridRect.anchoredPosition.y - gridRect.sizeDelta.y - continueButton.image.rectTransform.anchoredPosition.y;

            glowAlpha = titleGlow.color.a;

            continueButton.onClick.AddListener(OnContinueButtonClicked);

            isInitialized = true;
        }

        protected virtual void CreateCurrencyRewardCards()
        {
            var currencies = GameController.CurrenciesManager.GetAllCurrencies(true);

            currencyRewardCards.Add(currencies[0], coinsRewardCard);
            coinsRewardCard.SetIcon(currencies[0].Data.Icon);

            for (int i = 1; i < currencies.Count; i++)
            {
                var newCard = Instantiate(coinsRewardCard, coinsRewardCard.transform.parent);
                newCard.transform.localScale = Vector3.one;
                newCard.SetIcon(currencies[i].Data.Icon);
                newCard.gameObject.SetActive(false);

                currencyRewardCards.Add(currencies[i], newCard);
            }

            coinsRewardCard.gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (!isInitialized)
            {
                Init();
            }

            GameController.AudioManager.PlayAudio(successSound);

            var pickedUpItems = StageController.DropManager.PickedUpItems;
            pickedUpItems.Sort((item1, item2) =>
            {
                var level1 = GameController.ArmoryManager.GetItemLevel(item1.ItemId);
                var level2 = GameController.ArmoryManager.GetItemLevel(item2.ItemId);

                return level2.ItemRarity.CompareTo(level1.ItemRarity);
            });

            EasingManager.DoTimeScale(0, 0.3f);

            continueButton.enabled = false;

            backgroundImage.SetAlpha(0);
            backgroundImage.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            titleRect.localScale = new Vector3(0, 0.9f, 1f);
            titleRect.DoLocalScale(Vector3.one, 0.3f).SetDelay(0.1f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            titleGlow.SetAlpha(0);
            titleGlow.DoAlpha(glowAlpha, 0.3f).SetDelay(0.3f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            rewardsTitle.localScale = new Vector3(0, 0.9f, 1f);
            rewardsTitle.DoLocalScale(Vector3.one, 0.3f).SetDelay(0.6f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            foreach (var card in currencyRewardCards.Values)
            {
                card.gameObject.SetActive(false);
            }

            var currencies = GameController.CurrenciesManager.GetAllCurrencies(true);
            int counter = 0;
            for (int i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                if (currency.Amount > 0)
                {

                    var card = currencyRewardCards[currency];
                    card.gameObject.SetActive(true);
                    card.SetAmount(currency.Amount);
                    card.Show(0.7f + counter * 0.05f);

                    counter++;
                }
            }

            for (int i = 0; i < pickedUpItems.Count; i++)
            {
                var item = GameController.ArmoryManager.GetItemData(pickedUpItems[i].ItemId);

                var card = cardsPool.GetEntity();
                card.SetItem(item);
                card.SetAmount(1);

                card.Show(0.7f + (counter + i) * 0.05f);
            }

            var buttonRect = continueButton.image.rectTransform;

            EasingManager.DoNextFrame(() =>
            {
                gridRect.SetSizeDeltaY(rewardsGrid.preferredHeight);
                buttonRect.SetAnchoredPositionY(gridRect.anchoredPosition.y - gridRect.sizeDelta.y - buttonPositionDifference);
            });

            buttonRect.localScale = new Vector3(0, 0.9f, 1f);
            buttonRect.DoLocalScale(Vector3.one, 0.3f)
                .SetDelay(0.7f + pickedUpItems.Count * 0.05f + 0.5f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut)
                .SetOnFinish(() => continueButton.enabled = true);

            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            GameController.InputManager.onInputChanged += OnInputChanged;

            ContinuePlayingSave.Disable();

            IsOpen = true;
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            }
        }

        protected virtual void Clear()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnContinueButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            Clear();

            Time.timeScale = 1f;
            StageController.ReturnToMainMenu();
        }
    }
}