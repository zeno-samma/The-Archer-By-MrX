using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuPageBehavior : MonoBehaviour
    {
        [SerializeField] protected MainMenuPageType pageType;
        public MainMenuPageType PageType => pageType;

        [SerializeField] protected RectTransform pageRect;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Canvas canvas;

        protected CanvasScaler canvasScaler;
        protected float width;

        protected IEasingCoroutine moveEasingCoroutine;
        protected IEasingCoroutine alphaEasingCoroutine;

        protected virtual void Awake()
        {
            canvasScaler = GetComponentInParent<CanvasScaler>();
        }

        protected virtual void Start()
        {
            var resolution = canvasScaler.referenceResolution;

            var aspectRatio = Camera.main.aspect;
            width = resolution.x;

            if (canvasScaler.matchWidthOrHeight == 1)
            {
                width = resolution.y * aspectRatio;
            }
        }

        public virtual void MoveLeft()
        {
            var offsetMin = new Vector2(-width, 0);
            var offsetMax = new Vector2(-width, 0);

            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = pageRect.DoStretchedOffset(offsetMin, offsetMax, 0.3f).SetEasing(EasingType.CubicInOut);
            alphaEasingCoroutine = canvasGroup.DoAlpha(0, 0.2f).SetDelay(0.1f).SetOnFinish(OnMoveFinished);
        }

        public virtual void MoveRight()
        {
            var offsetMin = new Vector2(width, 0);
            var offsetMax = new Vector2(width, 0);

            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = pageRect.DoStretchedOffset(offsetMin, offsetMax, 0.3f).SetEasing(EasingType.CubicInOut);
            alphaEasingCoroutine = canvasGroup.DoAlpha(0, 0.2f).SetDelay(0.1f).SetOnFinish(OnMoveFinished);
        }

        public virtual void MoveCenter()
        {
            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = pageRect.DoStretchedOffset(Vector2.zero, Vector2.zero, 0.3f).SetEasing(EasingType.CubicInOut).SetOnFinish(OnMoveCenterFinished);
            alphaEasingCoroutine = canvasGroup.DoAlpha(1, 0.2f);
        }

        protected virtual void OnMoveCenterFinished()
        {

        }

        public virtual void SpawnLeft()
        {
            var offsetMin = new Vector2(-width, 0);
            var offsetMax = new Vector2(-width, 0);
            pageRect.SetStretchedOffset(offsetMin, offsetMax);

            canvasGroup.alpha = 0f;
            canvas.enabled = true;
        }

        public virtual void SpawnRight()
        {
            var offsetMin = new Vector2(width, 0);
            var offsetMax = new Vector2(width, 0);
            pageRect.SetStretchedOffset(offsetMin, offsetMax);

            canvasGroup.alpha = 0f;
            canvas.enabled = true;
        }

        public virtual void OnMoveFinished()
        {
            canvasGroup.alpha = 0f;
            canvas.enabled = false;
        }
    }
}