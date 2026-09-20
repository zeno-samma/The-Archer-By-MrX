using OctoberStudio.Easing;
using TMPro;
using UnityEngine;

namespace OctoberStudio
{
    public class AnimatedTitleUI : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup titleCanvasGroup;
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected RectTransform titleGradientRect;
        [SerializeField] protected RectTransform titleShineRect;

        [Space]
        [SerializeField] protected TMP_Text titleText;

        [Header("Show Animation Settings")]
        [SerializeField] protected float titleRectScaleDuration = 0.3f;
        [SerializeField] protected float titleRectScaleDelay = 0.1f;

        [SerializeField] protected float titleCanvasGroupFadeInDuration = 0.5f;
        [SerializeField] protected float titleCanvasGroupFadeInDelay = 0.3f;

        [SerializeField] protected float titleGradientScaleDuration = 0.3f;
        [SerializeField] protected float titleGradientScaleDelay = 0.2f;

        [SerializeField] protected float titleLineMoveDuration = 0.5f;
        [SerializeField] protected float titleLineMoveDelay = 0.5f;

        [Header("Hide Animation Settings")]
        [SerializeField] protected float titleLineHideDuration = 0.6f;

        [SerializeField] protected float titleRectHideDelay = 0.6f;
        [SerializeField] protected float titleRectHideDuration = 0.5f;

        [SerializeField] protected float titleFadeHideDelay = 0.6f;
        [SerializeField] protected float titleFadeHideDuration = 0.5f;

        [SerializeField] protected float titleGradientHideDelay = 0.6f;
        [SerializeField] protected float titleGradientHideDuration = 0.5f;

        protected IEasingCoroutine rectScaleEasingCoroutine;
        protected IEasingCoroutine fadeInEasingCoroutine;
        protected IEasingCoroutine gradientScaleEasingCoroutine;
        protected IEasingCoroutine lineEasingCoroutine;

        protected Vector2 titleLinePosition;
        protected string titleTextValue;

        protected virtual void Awake()
        {
            titleLinePosition = titleShineRect.anchoredPosition;
            titleTextValue = titleText.text;
        }

        public virtual void SetText(string text)
        {
            titleText.text = text;
        }

        public virtual void RestoreText()
        {
            titleText.text = titleTextValue;
        }

        public virtual void Show()
        {
            rectScaleEasingCoroutine =
                titleRect.DoLocalScale(Vector3.one, titleRectScaleDuration)
                .SetDelay(titleRectScaleDelay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut);

            fadeInEasingCoroutine =
                titleCanvasGroup.DoAlpha(1, titleCanvasGroupFadeInDuration)
                .SetDelay(titleCanvasGroupFadeInDelay)
                .SetUnscaledTime(true);

            gradientScaleEasingCoroutine =
                titleGradientRect.DoLocalScale(Vector3.one, titleGradientScaleDuration)
                .SetDelay(titleGradientScaleDelay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut);

            ShowAnimatedLine();
        }

        protected virtual void ShowAnimatedLine()
        {
            var lineEndPosition = new Vector2(titleLinePosition.x * -1, titleLinePosition.y);

            lineEasingCoroutine.StopIfExists();
            lineEasingCoroutine =
                titleShineRect.DoAnchorPosition(lineEndPosition, titleLineMoveDuration)
                .SetDelay(titleLineMoveDelay + Time.unscaledDeltaTime)
                .SetUnscaledTime(true);
        }

        public virtual void Hide()
        {
            var lineEndPosition = new Vector2(titleLinePosition.x * -1, titleLinePosition.y);
            titleShineRect.anchoredPosition = titleLinePosition;

            lineEasingCoroutine =
                titleShineRect.DoAnchorPosition(lineEndPosition, titleLineHideDuration)
                .SetUnscaledTime(true);

            rectScaleEasingCoroutine =
                titleRect.DoLocalScale(new Vector3(0, 1), titleRectHideDuration)
                .SetDelay(titleRectHideDelay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineIn);

            fadeInEasingCoroutine =
                titleCanvasGroup.DoAlpha(0, titleFadeHideDuration)
                .SetDelay(titleFadeHideDelay)
                .SetUnscaledTime(true);

            gradientScaleEasingCoroutine =
                titleGradientRect.DoLocalScale(new Vector3(0, 1), titleGradientHideDuration)
                .SetDelay(titleGradientHideDelay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineIn);
        }

        public virtual void Skip()
        {
            rectScaleEasingCoroutine.StopIfExists();
            fadeInEasingCoroutine.StopIfExists();
            gradientScaleEasingCoroutine.StopIfExists();
            lineEasingCoroutine.StopIfExists();

            titleRect.localScale = Vector3.one;
            titleCanvasGroup.alpha = 1;
            titleGradientRect.localScale = Vector3.one;

            titleShineRect.anchoredPosition = titleLinePosition;
        }

        public virtual void ResetUI()
        {
            titleRect.localScale = new Vector3(0, 1f);
            titleGradientRect.localScale = new Vector3(0, 1f);
            titleCanvasGroup.alpha = 0;
            titleShineRect.anchoredPosition = titleLinePosition;
        }
    }
}