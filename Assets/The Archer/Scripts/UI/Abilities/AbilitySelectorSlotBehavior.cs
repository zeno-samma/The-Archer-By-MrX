using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.Abilities.UI
{
    public class AbilitySelectorSlotBehavior : MonoBehaviour
    {
        [SerializeField] protected GameObject abilityCardPrefab;
        [SerializeField] protected Image gradientImage;

        [Header("Scrolling")]
        [SerializeField] protected RectTransform slotRect;
        [SerializeField] protected RectTransform scrollContentRect;
        [SerializeField] protected float scrollSpeed = 2000f;
        [SerializeField] protected float hiddenCardMargin = 100;
        [SerializeField] protected float scrollingSpacing = 25f;
        [SerializeField] protected float fadeSize = 200;

        [Space]
        [SerializeField] protected AnimationCurve otherCardsFadeCurve;
        [SerializeField] protected AnimationCurve mainCardMovementCurve;
        [SerializeField] protected float mainCardMovementDuration;

        [Space]
        [SerializeField] protected List<RarityData> rarityData;

        [Header("Flash")]
        [SerializeField] protected float colorChangeDuration = 0.1f;
        [SerializeField] protected float flashInDuration = 0.15f;
        [SerializeField] protected float flashOutDuration = 0.25f;

        [SerializeField] protected float flashGradientScale = 1.3f;
        [SerializeField] protected float flashCardsScale = 1.2f;

        [SerializeField] protected EasingType flashInEasing = EasingType.SineOut;
        [SerializeField] protected EasingType flashOutEasing = EasingType.SineInOut;

        protected float mainCardSpawnTime;

        protected PoolComponent<AbilityCardBehavior> cardsPool;

        protected Color initialGradientColor;

        protected float cardsScale = 1f;
        protected bool isStopped = false;

        protected IEasingCoroutine flashColorEasingCoroutine;
        protected IEasingCoroutine flashGradientScaleEasingCoroutine;
        protected IEasingCoroutine flashCardsScaleEasingCoroutine;

        protected Coroutine scrollingCoroutine;
        protected List<CardData> cards = new List<CardData>();

        public AbilityData AbilityData { get; protected set; }
        public AbilityCardBehavior MainCard { get; protected set; }

        protected Vector2 cardPosition;
        protected float nextCardSpawnTriggerY;
        protected AbilityCardBehavior highestCard;

        public float LastCardSpawnTime { get; protected set; }

        protected virtual void Awake()
        {
            cardsPool = new PoolComponent<AbilityCardBehavior>(abilityCardPrefab, 3, transform, true);

            initialGradientColor = gradientImage.color;
        }

        protected virtual void SetCardsScale(float value)
        {
            cardsScale = value;
        }

        public void StopScrolling()
        {
            isStopped = true;

            flashColorEasingCoroutine.StopIfExists();
            flashGradientScaleEasingCoroutine.StopIfExists();
            flashCardsScaleEasingCoroutine.StopIfExists();

            cardsScale = 1f;

            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Card.RectTransform.localScale = Vector3.one;
            }
        }

        public virtual void StartScrolling(AbilityData data, int id)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Card.gameObject.SetActive(false);
            }

            cards.Clear();
            MainCard = null;
            highestCard = null;

            AbilityData = data;
            scrollingCoroutine = StartCoroutine(ScrollingCoroutine(data, id));
        }

        protected virtual IEnumerator ScrollingCoroutine(AbilityData data, int id)
        {
            isStopped = false;

            gradientImage.color = initialGradientColor;
            gradientImage.SetAlpha(0);

            gradientImage.DoAlpha(initialGradientColor.a, 0.3f).SetUnscaledTime(true);

            highestCard = cardsPool.GetEntity();

            highestCard.ShowHidden(hiddenCardMargin);

            var scrollContentSize = scrollContentRect.sizeDelta;
            var cardSize = highestCard.RectTransform.sizeDelta;

            cardPosition = new Vector2(0, scrollContentSize.y / 2f + cardSize.y / 2f - hiddenCardMargin);
            var adjustedCardPosition = new Vector2(0, scrollContentSize.y / 2f + cardSize.y / 2f + cardSize.y / 4f * id - hiddenCardMargin);

            highestCard.RectTransform.anchoredPosition = adjustedCardPosition;

            var cardHeightOffset = new Vector2(0, cardSize.y);

            highestCard.SetAlphaParameters(200, 200 - fadeSize, true);
            highestCard.CanvasGroup.alpha = 1;

            nextCardSpawnTriggerY = cardPosition.y - cardSize.y + hiddenCardMargin * 2 - scrollingSpacing;
            var cardDisappearTriggerY = -cardPosition.y;
            var flipCardTriggerY = nextCardSpawnTriggerY - fadeSize;

            var highestCardData = new CardData
            {
                Card = highestCard,
                flipped = false
            };

            cards.Add(highestCardData);

            var scrollSpeed = this.scrollSpeed;

            MainCard = null;

            LastCardSpawnTime = Time.unscaledTime;

            while (true)
            {
                yield return null;

                bool shouldBreak = false;

                if (isStopped && MainCard != null)
                {
                    var time = Time.unscaledTime - mainCardSpawnTime;
                    var t = time / mainCardMovementDuration;
                    if (t > 1)
                    {
                        t = 1;
                        shouldBreak = true;
                    }

                    var y = Mathf.LerpUnclamped(cardPosition.y, 0, mainCardMovementCurve.Evaluate(t));
                    var difference = MainCard.RectTransform.anchoredPosition.y - y;

                    scrollSpeed = difference / Time.unscaledDeltaTime;

                    var otherCardsAlpha = otherCardsFadeCurve.Evaluate(t);

                    for (int i = 0; i < cards.Count; i++)
                    {
                        var card = cards[i];

                        if (card.Card != MainCard)
                        {
                            card.Card.CanvasGroup.alpha = otherCardsAlpha;
                        }
                    }
                }

                for (int i = 0; i < cards.Count; i++)
                {
                    var cardData = cards[i];
                    var card = cardData.Card;

                    card.RectTransform.anchoredPosition += Vector2.down * scrollSpeed * Time.unscaledDeltaTime;

                    if (card != MainCard)
                    {
                        card.transform.localScale = Vector3.one * cardsScale;
                    }

                    if (card.RectTransform.anchoredPosition.y < cardDisappearTriggerY)
                    {
                        card.gameObject.SetActive(false);
                        cards.RemoveAt(i);
                        i--;
                    }

                    if (card.RectTransform.anchoredPosition.y < flipCardTriggerY && !cardData.flipped)
                    {
                        cardData.flipped = true;
                        card.SetAlphaParameters(-650 + fadeSize, -650, false);
                    }
                }

                if (highestCard.RectTransform.anchoredPosition.y < nextCardSpawnTriggerY)
                {
                    var difference = highestCard.RectTransform.anchoredPosition.y - nextCardSpawnTriggerY;

                    highestCard = cardsPool.GetEntity();
                    highestCard.ShowHidden(hiddenCardMargin);
                    highestCard.RectTransform.anchoredPosition = cardPosition + new Vector2(0, difference);
                    highestCard.SetAlphaParameters(200, 200 - fadeSize, true);
                    highestCard.CanvasGroup.alpha = 1;

                    LastCardSpawnTime = Time.unscaledTime;

                    if (isStopped && MainCard == null)
                    {
                        MainCard = highestCard;
                        MainCard.Init(data, mainCardMovementDuration, id * 0.25f);
                        mainCardSpawnTime = Time.unscaledTime;

                        var halfDuration = mainCardMovementDuration / 2;
                        gradientImage.DoAlpha(0, halfDuration).SetDelay(halfDuration).SetUnscaledTime(true);

                    }

                    cards.Add(new CardData() { Card = highestCard });
                }

                if (shouldBreak) break;
            }
        }

        public virtual void DoFlash(AbilityRarity rarity)
        {
            var data = GetRarityData(rarity);

            flashColorEasingCoroutine =
                gradientImage.DoColor(data.GradientColor, colorChangeDuration)
                .SetUnscaledTime(true);

            flashGradientScaleEasingCoroutine =
                gradientImage.rectTransform.DoLocalScale(Vector3.one * flashGradientScale, flashInDuration)
                .SetUnscaledTime(true)
                .SetEasing(flashInEasing)
                .SetOnFinish(OnFlashInAminationEnded);

            flashCardsScaleEasingCoroutine =
                EasingManager.DoFloat(1, flashCardsScale, flashInDuration, SetCardsScale)
                .SetUnscaledTime(true)
                .SetEasing(flashInEasing);
        }

        protected virtual void OnFlashInAminationEnded()
        {
            flashGradientScaleEasingCoroutine =
                gradientImage.rectTransform.DoLocalScale(Vector3.one, flashOutDuration)
                .SetUnscaledTime(true)
                .SetEasing(flashOutEasing);

            flashCardsScaleEasingCoroutine =
                EasingManager.DoFloat(flashCardsScale, 1f, flashOutDuration, SetCardsScale)
                .SetUnscaledTime(true)
                .SetEasing(flashOutEasing);
        }

        protected virtual RarityData GetRarityData(AbilityRarity rarity)
        {
            foreach (var data in rarityData)
            {
                if (data.Rarity == rarity)
                {
                    return data;
                }
            }
            return null;
        }

        protected class CardData
        {
            public AbilityCardBehavior Card { get; set; }

            public bool flipped = false;
        }

        [System.Serializable]
        protected class RarityData
        {
            [SerializeField] protected AbilityRarity rarity;
            [SerializeField] protected Color gradientColor;

            public AbilityRarity Rarity => rarity;
            public Color GradientColor => gradientColor;
        }
    }
}