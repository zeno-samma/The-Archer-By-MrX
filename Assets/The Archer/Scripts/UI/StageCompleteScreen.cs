using MrX.Audio;
using MrX.Easing;
using MrX.Extensions;
using MrX.Input;
using MrX.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class StageCompleteScreen : MonoBehaviour
    {
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();
        private readonly MrX.UI.InspectorVisibility inspectorVisibility = new MrX.UI.InspectorVisibility();

        protected virtual void Awake()
        {
            inspectorVisibility.Capture(continueButton);
            animationDefaults.Capture(titleRect, rewardsTitle,
                continueButton != null && continueButton.image != null ? continueButton.image.rectTransform : null,
                coinsRewardCard != null ? coinsRewardCard.transform : null);
            animationDefaults.CaptureAlpha(backgroundImage);
        }

        [SerializeField] private CompactRunResult compactResult;
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
                newCard.transform.localScale = animationDefaults.Scale(coinsRewardCard.transform);
                newCard.SetIcon(currencies[i].Data.Icon);
                newCard.gameObject.SetActive(false);

                currencyRewardCards.Add(currencies[i], newCard);
            }

            coinsRewardCard.gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            if (IsOpen) return;
            if(compactResult!=null)
            {
                StageController.FinalizeRunRewards(true);gameObject.SetActive(true);canvas.enabled=true;
                IsOpen=true;Time.timeScale=0;compactResult.Show(true);return;
            }
            StageController.FinalizeRunRewards();
            gameObject.SetActive(true);

            if (!isInitialized)
            {
                Init();
            }

            GameController.AudioManager.PlayAudio(successSound);

            var pickedUpItems = StageController.DropManager.ResultItems;
            EasingManager.DoTimeScale(0, 0.3f);

            continueButton.enabled = false;

            backgroundImage.SetAlpha(0);
            backgroundImage.DoAlpha(animationDefaults.Alpha(backgroundImage), 0.3f).SetUnscaledTime(true);

            titleRect.localScale = animationDefaults.Scale(titleRect, new Vector3(0, 0.9f, 1f));
            titleRect.DoLocalScale(animationDefaults.Scale(titleRect), 0.3f).SetDelay(0.1f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            titleGlow.SetAlpha(0);
            titleGlow.DoAlpha(glowAlpha, 0.3f).SetDelay(0.3f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            rewardsTitle.localScale = animationDefaults.Scale(rewardsTitle, new Vector3(0, 0.9f, 1f));
            rewardsTitle.DoLocalScale(animationDefaults.Scale(rewardsTitle), 0.3f).SetDelay(0.6f).SetUnscaledTime(true).SetEasing(EasingType.SineOut);

            foreach (var card in currencyRewardCards.Values)
            {
                card.gameObject.SetActive(false);
            }

            var currencies = GameController.CurrenciesManager.GetAllCurrencies(true);
            int counter = 0;
            for (int i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                int earned; StageController.DropManager.ResultCurrencies.TryGetValue(currency.Data.ID, out earned);
                if (earned > 0)
                {

                    var card = currencyRewardCards[currency];
                    card.gameObject.SetActive(true);
                    card.SetAmount(earned);
                    card.Show(0.7f + counter * 0.05f);

                    counter++;
                }
            }

            for (int i = 0; i < pickedUpItems.Count; i++)
            {
                var item = pickedUpItems[i];

                var card = cardsPool.GetEntity();
                card.SetReward(item);


                card.Show(0.7f + (counter + i) * 0.05f);
            }

            var buttonRect = continueButton.image.rectTransform;

            EasingManager.DoNextFrame(() =>
            {
                gridRect.SetSizeDeltaY(rewardsGrid.preferredHeight);
                buttonRect.SetAnchoredPositionY(gridRect.anchoredPosition.y - gridRect.sizeDelta.y - buttonPositionDifference);
            });

            buttonRect.localScale = animationDefaults.Scale(buttonRect, new Vector3(0, 0.9f, 1f));
            buttonRect.DoLocalScale(animationDefaults.Scale(buttonRect), 0.3f)
                .SetDelay(0.7f + pickedUpItems.Count * 0.05f + 0.5f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut)
                .SetOnFinish(() => inspectorVisibility.SetEnabled(continueButton, true));

            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            GameController.InputManager.onInputChanged += OnInputChanged;



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


            Clear();

            Time.timeScale = 1f;
            StageController.ReturnToMainMenu();
        }
    }
}
