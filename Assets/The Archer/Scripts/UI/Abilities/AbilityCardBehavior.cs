using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.Abilities.UI
{
    public class AbilityCardBehavior : MonoBehaviour
    {
        // Shader properties
        protected static readonly int _MaxAlphaV = Shader.PropertyToID("_MaxAlphaV");
        protected static readonly int _MinAlphaV = Shader.PropertyToID("_MinAlphaV");
        protected static readonly int _FlipAlpha = Shader.PropertyToID("_FlipAlpha");

        [SerializeField] protected Button button;

        [Space]
        [SerializeField] protected RectTransform cardBackRect;
        [SerializeField] protected RectTransform cardFrontRect;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Image cardBackSideImage;

        [Space]
        [SerializeField] protected Image gradientImage;

        [Space]
        [SerializeField] protected List<Image> maskedImages;

        [Space]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image cardBackImage;
        [SerializeField] protected Image nameBadgeImage;
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text descriptionText;
        [SerializeField] protected Transform upgradeIndicator;

        [Space]
        [SerializeField] protected Sprite cardBackSprite;
        [SerializeField] protected Sprite cardBackMovingSprite;

        [Header("Colors")]
        [SerializeField] protected List<RarityData> colorData;

        [Header("Animation Settings")]
        [SerializeField] protected AnimationCurve scaleCurve;
        [SerializeField] protected AnimationCurve backScaleCurve;
        [SerializeField] protected AnimationCurve frontScaleCurve;

        [Space]
        [SerializeField] protected float scaleDuration = 0.35f;
        [Tooltip("The bigger it is, the earlier scale will start")]
        [SerializeField] protected float scaleDelayReduction = 0.15f;

        [Space]
        [SerializeField] protected float backRotationDuration = 0.15f;
        [SerializeField] protected float frontRotationDuration = 0.2f;
        [Tooltip("The bigger it is, the earlier rotation will start")]
        [SerializeField] protected float rotationDelayReduction = 0.15f;

        [Space]
        [SerializeField] protected float gradientStretchDuration = 0.35f;
        [Tooltip("The bigger it is, the earlier stretching will start")]
        [SerializeField] protected float gradientStretchDelayReduction = 0.6f;

        [Space]
        [SerializeField] protected float hideDuration = 0.3f;

        [Space]
        [SerializeField] protected AudioData cardRevealedSound;
        [SerializeField] protected float cardRevealSoundDelay;

        public RectTransform RectTransform { get; protected set; }
        public RectTransform CardBackRect => cardBackRect;

        public CanvasGroup CanvasGroup => canvasGroup;

        public AbilityData AbilityData { get; protected set; }

        public event UnityAction<AbilityData> onAbilitySelected;

        public Selectable Selectable => button;

        protected Vector2 gradientOffsetMin;
        protected Vector2 gradientOffsetMax;

        protected IEasingCoroutine scaleEasingCoroutine;
        protected IEasingCoroutine backRotationEasingCoroutine;
        protected IEasingCoroutine frontRotationEasingCoroutine;
        protected IEasingCoroutine gradientStretchEasingCoroutine;

        public bool IsAnimationActive { get; protected set; }

        protected bool shouldShowUpgrade;

        protected virtual void Awake()
        {
            gradientOffsetMin = gradientImage.rectTransform.offsetMin;
            gradientOffsetMax = gradientImage.rectTransform.offsetMax;

            button.onClick.AddListener(OnAbilitySelected);

            RectTransform = GetComponent<RectTransform>();

            for (int i = 0; i < maskedImages.Count; i++)
            {
                // Create a new material instance for each masked image to avoid shared state issues
                var maskedImage = maskedImages[i];
                maskedImage.material = new Material(maskedImage.material);
            }
        }

        protected virtual void SetData(AbilityData data)
        {
            AbilityData = data;

            iconImage.sprite = data.Icon;
            nameText.text = data.Title;
            descriptionText.text = data.Description;

            var rarityData = GetRarityData(data.Rarity);
            gradientImage.color = rarityData.GradientColor;
            cardBackImage.sprite = rarityData.CardBackSprite;
            nameBadgeImage.sprite = rarityData.NameBadgeSprite;

            shouldShowUpgrade = !data.IsRepeatedAbility && !data.IsEndgameAbility && StageController.AbilitiesManager.IsAbilityAquired(data.AbilityType);
        }

        protected virtual void OnAbilitySelected()
        {
            GameController.AudioManager.PlayButtonClick();
            onAbilitySelected?.Invoke(AbilityData);
        }

        public virtual void SetAlphaParameters(float maxAlphaV, float minAlphaV, bool fipAlpha)
        {
            for (int i = 0; i < maskedImages.Count; i++)
            {
                // Set shader properties for each masked image
                var maskedImage = maskedImages[i];
                maskedImage.material.SetFloat(_MinAlphaV, minAlphaV);
                maskedImage.material.SetFloat(_MaxAlphaV, maxAlphaV);
                maskedImage.material.SetInt(_FlipAlpha, fipAlpha ? 1 : 0);
            }
        }

        public virtual void ShowHidden(float margin, bool moving = true)
        {
            ResetUI();

            // Set the card back image to the correct size based on the margin
            var aspect = cardBackRect.rect.size.x / cardBackRect.rect.size.y;
            cardBackRect.SetStretchedOffset(new Vector2(margin * aspect, margin), new Vector2(-margin * aspect, -margin));

            cardBackSideImage.sprite = moving ? cardBackMovingSprite : cardBackSprite;
        }

        public virtual void Init(AbilityData data, float duration, float delay = 0)
        {
            IsAnimationActive = true;
            button.enabled = false;

            SetData(data);

            cardFrontRect.SetStretchedOffset(Vector2.zero, Vector2.zero);

            cardBackSideImage.sprite = cardBackSprite;

            var offsetMin = cardBackRect.offsetMin;
            var offsetMax = cardBackRect.offsetMax;

            var scaleX = (RectTransform.sizeDelta.x - offsetMin.x + offsetMax.x) / RectTransform.sizeDelta.x;
            var scaleY = (RectTransform.sizeDelta.y - offsetMin.y + offsetMax.y) / RectTransform.sizeDelta.y;

            CardBackRect.SetStretchedOffset(Vector2.zero, Vector2.zero);

            transform.localScale = new Vector3(scaleX, scaleY, 1);

            scaleEasingCoroutine =
                transform.DoLocalScale(Vector3.one, scaleDuration)
                .SetDelay(duration - scaleDelayReduction + delay)
                .SetEasingCurve(scaleCurve)
                .SetUnscaledTime(true);

            // "Rotating" card back
            backRotationEasingCoroutine =
                cardBackRect.DoLocalEulerAngles(new Vector3(0, 90, 0), backRotationDuration)
                .SetDelay(duration - rotationDelayReduction + delay)
                .SetUnscaledTime(true)
                .SetEasingCurve(backScaleCurve)
                .SetOnFinish(OnCardBackScaledToZero);

            // "Rotating" card front
            frontRotationEasingCoroutine =
                cardFrontRect.DoLocalEulerAngles(Vector3.zero, frontRotationDuration)
                .SetDelay(duration - rotationDelayReduction + delay + backRotationDuration)
                .SetEasingCurve(frontScaleCurve)
                .SetUnscaledTime(true);

            // Scale the gradient image to its original size
            gradientStretchEasingCoroutine =
                gradientImage.rectTransform.DoStretchedOffset(gradientOffsetMin, gradientOffsetMax, gradientStretchDuration)
                .SetDelay(duration - gradientStretchDelayReduction + delay)
                .SetEasing(EasingType.CubicOut)
                .SetUnscaledTime(true)
                .SetOnFinish(OnGradientStretched);

            if (shouldShowUpgrade)
            {
                upgradeIndicator.gameObject.SetActive(true);
                upgradeIndicator.DoLocalScale(Vector3.one, 0.4f)
                    .SetUnscaledTime(true)
                    .SetEasing(EasingType.CubicOut);
            }

            EasingManager.DoAfter(cardRevealSoundDelay + delay, () => GameController.AudioManager.PlayAudio(cardRevealedSound)).SetUnscaledTime(true);
        }

        public virtual void ChangeAbility(AbilityData newAbilityData, float delay)
        {
            StartCoroutine(ChangeAbilityCoroutine(newAbilityData, delay));
        }

        protected IEnumerator ChangeAbilityCoroutine(AbilityData newAbilityData, float delay)
        {
            button.enabled = false;
            IsAnimationActive = true;

            yield return new WaitForSecondsRealtime(delay);

            if (shouldShowUpgrade)
            {
                upgradeIndicator.gameObject.SetActive(true);
                upgradeIndicator.DoLocalScale(new Vector3(0, 0.6f, 1f), 0.3f)
                    .SetUnscaledTime(true)
                    .SetEasing(EasingType.CubicIn);
            }

            yield return cardFrontRect.DoLocalEulerAngles(new Vector3(0, 90, 0), 0.15f)
                .SetEasingCurve(frontScaleCurve)
                .SetUnscaledTime(true);

            cardBackRect.gameObject.SetActive(true);
            cardFrontRect.gameObject.SetActive(false);

            SetData(newAbilityData);

            cardBackRect.eulerAngles = new Vector3(0, 90, 0);
            yield return cardBackRect.DoLocalEulerAngles(new Vector3(0, -90, 0), 0.3f)
                .SetUnscaledTime(true)
                .SetEasingCurve(backScaleCurve);

            cardBackRect.gameObject.SetActive(false);
            cardFrontRect.gameObject.SetActive(true);

            if (shouldShowUpgrade)
            {
                upgradeIndicator.gameObject.SetActive(true);
                upgradeIndicator.DoLocalScale(Vector3.one, 0.4f)
                    .SetUnscaledTime(true)
                    .SetEasing(EasingType.CubicOut);
            }

            yield return cardFrontRect.DoLocalEulerAngles(Vector3.zero, 0.2f)
                .SetEasingCurve(frontScaleCurve)
                .SetUnscaledTime(true);

            IsAnimationActive = false;
        }

        protected virtual void OnCardBackScaledToZero()
        {
            cardBackRect.gameObject.SetActive(false);
            cardFrontRect.gameObject.SetActive(true);
        }

        protected virtual void OnGradientStretched()
        {
            IsAnimationActive = false;
        }

        public virtual void EnableButton()
        {
            button.enabled = true;
        }

        public void Hide(float delay, float disableDelay)
        {
            button.enabled = false;

            cardFrontRect.DoStretchedOffset(new Vector2(0, -300), new Vector2(0, -300), hideDuration).SetDelay(delay).SetUnscaledTime(true).SetEasing(EasingType.SineIn);
            canvasGroup.DoAlpha(0, hideDuration - 0.1f).SetDelay(delay + 0.1f).SetUnscaledTime(true).SetOnFinish(() => Disable(disableDelay));
        }

        protected virtual void Disable(float delay = 0)
        {
            EasingManager.DoAfter(delay, () =>
            {
                gameObject.SetActive(false);
                cardFrontRect.SetStretchedOffset(Vector2.zero, Vector2.zero);
            });
        }

        protected virtual void ResetUI()
        {
            cardBackRect.gameObject.SetActive(true);
            cardFrontRect.gameObject.SetActive(false);

            gradientImage.rectTransform.offsetMin = Vector2.zero;
            gradientImage.rectTransform.offsetMax = Vector2.zero;

            canvasGroup.alpha = 0;

            cardBackRect.localEulerAngles = Vector3.zero;
            cardFrontRect.localEulerAngles = new Vector3(0, 90, 0);

            button.enabled = false;

            upgradeIndicator.gameObject.SetActive(false);
            upgradeIndicator.localScale = new Vector3(0, 0.6f, 1);
        }

        protected virtual RarityData GetRarityData(AbilityRarity rarity)
        {
            foreach (var data in colorData)
            {
                if (data.Rarity == rarity)
                {
                    return data;
                }
            }
            return null;
        }

        [System.Serializable]
        protected class RarityData
        {
            [SerializeField] protected AbilityRarity rarity;
            [SerializeField] protected Sprite cartBackSprite;
            [SerializeField] protected Sprite nameBadgeSprite;
            [SerializeField] protected Color gradientColor;

            public AbilityRarity Rarity => rarity;
            public Color GradientColor => gradientColor;
            public Sprite CardBackSprite => cartBackSprite;
            public Sprite NameBadgeSprite => nameBadgeSprite;
        }
    }
}