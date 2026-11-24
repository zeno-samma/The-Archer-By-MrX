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
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected Image titleGlow;

        [Space]
        [SerializeField] protected RectTransform rewardsTitle;
        [SerializeField] protected GridLayoutGroup rewardsGrid;
        [SerializeField] private RectTransform remainingHealthFill;
        [SerializeField] private TMPro.TMP_Text remainingHealthLabel;

        [Space]
        [SerializeField] protected Button continueButton;

        [SerializeField] protected SmallRewardCard coinsRewardCard;
        [SerializeField] private TMPro.TMP_Text experienceAmount;
        [SerializeField] private TMPro.TMP_Text totalDamageAmount;
        [SerializeField] private TMPro.TMP_Text dpsAmount;
        [SerializeField] protected GameObject itemRewardCardPrefab;

        [Space]
        [SerializeField] protected AudioData successSound;

        protected PoolComponent<SmallItemRewardCard> cardsPool;
        protected Dictionary<CurrencySave, SmallRewardCard> currencyRewardCards = new Dictionary<CurrencySave, SmallRewardCard>();

        private readonly List<SmallItemRewardCard> displayedLootCards = new List<SmallItemRewardCard>();

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
            if (IsOpen) return;
            var player = StageController.Player;
            float remainingHealth = player != null ? Mathf.Clamp01(player.NormalizedHP) : 0f;
            StageController.FinalizeRunRewards();
            if (remainingHealthFill != null && remainingHealthLabel != null)
            {
                remainingHealthFill.anchorMax = new Vector2(remainingHealth, 1f);
                remainingHealthFill.gameObject.SetActive(remainingHealth > 0f);
                remainingHealthLabel.text = $"HP REMAINING  {Mathf.RoundToInt(remainingHealth * 100f)}%";
            }
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
            if (experienceAmount != null)
                experienceAmount.text = $"+{StageController.ResultProfileXp:N0}";
            if (totalDamageAmount != null)
                totalDamageAmount.text = StageController.ResultDamage.ToString("N0");
            if (dpsAmount != null)
                dpsAmount.text = StageController.ResultDps.ToString("N0");
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
                displayedLootCards.Add(card);


                card.Show(0.7f + (counter + i) * 0.05f);
            }

            var buttonRect = continueButton.image.rectTransform;

            EasingManager.DoNextFrame(() =>
            {
                gridRect.SetSizeDeltaY(rewardsGrid.preferredHeight);
                // The grid owns the positions of Claim and x2 Gold.
                var buttonGrid = continueButton.transform.parent.GetComponent<GridLayoutGroup>();
                if (buttonGrid == null || !buttonGrid.isActiveAndEnabled)
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



            IsOpen = true;
        }

        internal void ShowDoubledRewards(int awardedXp, Dictionary<Rewards.LuckyGrant, int> equipmentAwarded)
        {
            if (experienceAmount != null)
                experienceAmount.text = $"+{StageController.ResultProfileXp + awardedXp:N0}";

            foreach (var entry in currencyRewardCards)
                if (StageController.DropManager.ResultCurrencies.TryGetValue(entry.Key.Data.ID, out int earned) && earned > 0)
                    entry.Value.SetAmount((int)System.Math.Min(int.MaxValue, 2L * earned));

            var loot = StageController.DropManager.ResultItems;
            for (int i = 0; i < displayedLootCards.Count; i++)
            {
                var reward = loot[i];
                int bonus = reward.kind == Rewards.LuckyRewardKind.Equipment
                    ? equipmentAwarded[reward] : reward.amount;
                int total = (int)System.Math.Min(int.MaxValue, (long)reward.amount + bonus);
                if (total > 1) displayedLootCards[i].SetAmount(total);
            }
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