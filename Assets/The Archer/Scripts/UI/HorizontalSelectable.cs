using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class HorizontalSelectable : MonoBehaviour
    {
        public RectTransform RectTransform { get; protected set; }

        [SerializeField] protected float moveDuration = 0.3f;
        [SerializeField] protected float moveDistance = 700f;
        [SerializeField] protected EasingType moveEasingType = EasingType.CubicInOut;

        protected IEasingCoroutine moveEasingCoroutine;

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void MoveCenter(float delay)
        {
            moveEasingCoroutine.StopIfExists();
            moveEasingCoroutine = RectTransform.DoAnchorPosition(Vector2.zero, moveDuration).SetEasing(moveEasingType).SetDelay(delay).SetOnFinish(OnMoveCenterFinished);
        }

        public virtual void MoveLeft()
        {
            moveEasingCoroutine.StopIfExists();
            moveEasingCoroutine = RectTransform.DoAnchorPosition(new Vector2(-moveDistance, 0), moveDuration).SetEasing(moveEasingType).SetOnFinish(OnMoveAwayFinished);
        }

        public virtual void MoveRight()
        {
            moveEasingCoroutine.StopIfExists();
            moveEasingCoroutine = RectTransform.DoAnchorPosition(new Vector2(moveDistance, 0), moveDuration).SetEasing(moveEasingType).SetOnFinish(OnMoveAwayFinished);
        }

        protected virtual void OnMoveAwayFinished()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnMoveCenterFinished()
        {

        }
    }
}