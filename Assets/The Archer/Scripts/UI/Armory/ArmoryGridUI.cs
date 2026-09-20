using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class ArmoryGridUI<T> : MonoBehaviour where T : ArmoryCardBehavior
    {
        [Space]
        [SerializeField] protected GameObject cardPrefab;
        [SerializeField] protected RectTransform cardsParent;

        [Space]
        [SerializeField] protected ScrollRect scrollView;

        [Space]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected float offset;
        [SerializeField] protected float duration;
        [SerializeField] protected float scaleFactor = 0.9f;

        protected List<T> cards = new List<T>();
        protected PoolComponent<T> cardsPool;

        protected IEasingCoroutine moveEasingCoroutine;
        protected IEasingCoroutine alphaEasingCoroutine;
        protected IEasingCoroutine scaleEasingCoroutine;


        protected virtual void Start()
        {
            cardsPool = new PoolComponent<T>(cardPrefab, 10, cardsParent);
        }

        public virtual void CalculateNavigation(Selectable lowestSlot)
        {
            var cardsInRow = -1;
            for (int i = 1; i < cards.Count; i++)
            {
                var prevCard = cards[i - 1];
                var card = cards[i];

                if (!Mathf.Approximately(prevCard.transform.position.y, card.transform.position.y))
                {
                    cardsInRow = i;
                    break;
                }
            }

            if (cardsInRow < 0)
            {
                cardsInRow = cards.Count;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                var leftIndex = i - 1;
                var rightIndex = i + 1;
                var upIndex = i - cardsInRow;
                var downIndex = i + cardsInRow;

                var leftCard = leftIndex >= 0 ? cards[leftIndex].Selectable : null;
                var rightCard = rightIndex < cards.Count ? cards[rightIndex].Selectable : null;
                var downCard = downIndex < cards.Count ? cards[downIndex].Selectable : null;

                Selectable upCard = null;
                if (upIndex < 0)
                {
                    upCard = lowestSlot;
                }
                else
                {
                    upCard = cards[upIndex].Selectable;
                }

                var card = cards[i];
                card.SetNavigation(leftCard, rightCard, upCard, downCard);
            }
        }

        public virtual void MoveLeft()
        {
            var offsetMin = new Vector2(-offset, 0);
            var offsetMax = new Vector2(-offset, 0);

            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = rectTransform.DoStretchedOffset(offsetMin, offsetMax, duration).SetEasing(EasingType.CubicInOut);
            scaleEasingCoroutine = rectTransform.DoLocalScale(new Vector3(1, scaleFactor, 1), duration).SetEasing(EasingType.CubicInOut);
            alphaEasingCoroutine = canvasGroup.DoAlpha(0, duration).SetDelay(0).SetOnFinish(OnMoveFinished);
        }

        public virtual void MoveRight()
        {
            var offsetMin = new Vector2(offset, 0);
            var offsetMax = new Vector2(offset, 0);

            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = rectTransform.DoStretchedOffset(offsetMin, offsetMax, duration).SetEasing(EasingType.CubicInOut);
            scaleEasingCoroutine = rectTransform.DoLocalScale(new Vector3(1, scaleFactor, 1), duration).SetEasing(EasingType.CubicInOut);
            alphaEasingCoroutine = canvasGroup.DoAlpha(0, duration).SetDelay(0).SetOnFinish(OnMoveFinished);
        }

        public virtual void MoveCenter()
        {
            moveEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            moveEasingCoroutine = rectTransform.DoStretchedOffset(Vector2.zero, Vector2.zero, duration).SetEasing(EasingType.CubicInOut).SetOnFinish(OnMoveCenterFinished);
            scaleEasingCoroutine = rectTransform.DoLocalScale(new Vector3(1, 1, 1), duration).SetEasing(EasingType.CubicInOut);
            alphaEasingCoroutine = canvasGroup.DoAlpha(1, duration);
        }

        protected virtual void OnMoveCenterFinished()
        {

        }

        public virtual void SpawnCenter()
        {
            rectTransform.SetStretchedOffset(Vector2.zero, Vector2.zero);
            rectTransform.localScale = new Vector3(1, 1, 1);

            canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        public virtual void SpawnLeft()
        {
            var offsetMin = new Vector2(-offset, 0);
            var offsetMax = new Vector2(-offset, 0);
            rectTransform.SetStretchedOffset(offsetMin, offsetMax);
            rectTransform.localScale = new Vector3(1, scaleFactor, 1);

            canvasGroup.alpha = 0f;
            gameObject.SetActive(true);
        }

        public virtual void SpawnRight()
        {
            var offsetMin = new Vector2(offset, 0);
            var offsetMax = new Vector2(offset, 0);
            rectTransform.SetStretchedOffset(offsetMin, offsetMax);
            rectTransform.localScale = new Vector3(1, scaleFactor, 1);

            canvasGroup.alpha = 0f;
            gameObject.SetActive(true);
        }

        public virtual void OnMoveFinished()
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            Clear();
        }

        public virtual void Clear()
        {
            foreach (var card in cards)
            {
                card.gameObject.SetActive(false);
                card.Clear();
            }

            cards.Clear();
        }
    }
}