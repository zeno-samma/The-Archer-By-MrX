using MrX.Easing;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MrX.UI
{
    public class UpgradeCardSelectable : HorizontalSelectable
    {
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();

        protected override void Awake()
        {
            animationDefaults.Capture(transform);
            animationDefaults.CaptureAlpha(canvasGroup);
            base.Awake();
        }

        [SerializeField] protected CanvasGroup canvasGroup;

        [Space]
        [SerializeField] protected EasingType scaleInEasingType;
        [SerializeField] protected EasingType fadeInEasingType;

        [Space]
        [SerializeField] protected EasingType scaleOutEasingType;
        [SerializeField] protected EasingType fadeOutEasingType;
        [Space]
        [SerializeField] protected UpgradeCardBehavior upgradeCard;
        public UpgradeCardBehavior UpgradeCard => upgradeCard;

        protected IEasingCoroutine fadeEasingCoroutine;
        protected IEasingCoroutine scaleEasingCoroutine;

        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();

            EventSystem.current.SetSelectedGameObject(upgradeCard.gameObject);
        }

        public override void MoveCenter(float delay)
        {
            base.MoveCenter(delay);

            if (!scaleEasingCoroutine.ExistsAndActive())
            {
                RectTransform.localScale = animationDefaults.Scale(RectTransform, new Vector3(0, 1, 1));
            }

            scaleEasingCoroutine.StopIfExists();
            scaleEasingCoroutine = RectTransform.DoLocalScale(animationDefaults.Scale(RectTransform), moveDuration).SetDelay(delay).SetEasing(scaleInEasingType);

            if (!fadeEasingCoroutine.ExistsAndActive())
            {
                canvasGroup.alpha = 0;
            }

            fadeEasingCoroutine.StopIfExists();
            fadeEasingCoroutine = canvasGroup.DoAlpha(animationDefaults.Alpha(canvasGroup), moveDuration).SetDelay(delay).SetEasing(fadeInEasingType);
        }

        public override void MoveLeft()
        {
            base.MoveLeft();

            MoveAway();
        }

        public override void MoveRight()
        {
            base.MoveRight();

            MoveAway();
        }

        protected virtual void MoveAway()
        {
            scaleEasingCoroutine.StopIfExists();
            scaleEasingCoroutine = RectTransform.DoLocalScale(animationDefaults.Scale(RectTransform, new Vector3(0, 1, 1)), moveDuration).SetEasing(scaleOutEasingType);

            fadeEasingCoroutine.StopIfExists();
            fadeEasingCoroutine = canvasGroup.DoAlpha(0, moveDuration).SetEasing(fadeOutEasingType);
        }

        public virtual void Clear()
        {
            scaleEasingCoroutine.StopIfExists();
            fadeEasingCoroutine.StopIfExists();
            moveEasingCoroutine.StopIfExists();

            canvasGroup.alpha = animationDefaults.Alpha(canvasGroup);
            RectTransform.localScale = animationDefaults.Scale(RectTransform);
            RectTransform.anchoredPosition = Vector2.zero;

            upgradeCard.Clear();

            gameObject.SetActive(false);
        }
    }
}