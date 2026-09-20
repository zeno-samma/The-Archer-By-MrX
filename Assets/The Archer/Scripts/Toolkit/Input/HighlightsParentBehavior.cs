using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Input
{
    public class HighlightsParentBehavior : MonoBehaviour
    {
        protected PoolComponent<RectTransform> buttonArrowPool;

        protected RectTransform rightArrow;
        protected RectTransform leftArrow;

        protected HighlightableSelectableUI highlightedButton;
        [SerializeField] GameObject buttonSelectionArrowPrefab;

        protected virtual void Awake()
        {
            buttonArrowPool = new PoolComponent<RectTransform>(buttonSelectionArrowPrefab, 2, transform, true);
        }

        public virtual void EnableArrows()
        {
            if (highlightedButton != null)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
        }

        public virtual void DisableArrows()
        {
            if (highlightedButton != null)
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }
        }

        protected virtual RectTransform GetButtonArrow()
        {
            return buttonArrowPool.GetEntity();
        }

        public virtual void Highlight(HighlightableSelectableUI button)
        {
            if (highlightedButton != null)
            {
                StopHighlighting(highlightedButton);
            }

            highlightedButton = button;

            leftArrow = GetButtonArrow();
            rightArrow = GetButtonArrow();

            leftArrow.SetParent(button.transform);
            rightArrow.SetParent(button.transform);

            leftArrow.ResetLocal();
            rightArrow.ResetLocal();

            if (GameController.InputManager.ActiveInput != InputType.UIJoystick)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }

            leftArrow.anchorMin = new Vector2(0, 0.5f);
            leftArrow.anchorMax = new Vector2(0, 0.5f);

            rightArrow.anchorMin = new Vector2(1, 0.5f);
            rightArrow.anchorMax = new Vector2(1, 0.5f);

            leftArrow.anchoredPosition = Vector2.zero;
            rightArrow.anchoredPosition = Vector2.zero;

            leftArrow.SetParent(transform);
            rightArrow.SetParent(transform);

            button.IsHighlighted = true;
        }

        public virtual void RefreshHighlight()
        {
            leftArrow.SetParent(highlightedButton.transform);
            rightArrow.SetParent(highlightedButton.transform);

            leftArrow.ResetLocal();
            rightArrow.ResetLocal();

            if (GameController.InputManager.ActiveInput != InputType.UIJoystick)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }

            leftArrow.anchorMin = new Vector2(0, 0.5f);
            leftArrow.anchorMax = new Vector2(0, 0.5f);

            rightArrow.anchorMin = new Vector2(1, 0.5f);
            rightArrow.anchorMax = new Vector2(1, 0.5f);

            leftArrow.anchoredPosition = Vector2.zero;
            rightArrow.anchoredPosition = Vector2.zero;

            leftArrow.SetParent(transform);
            rightArrow.SetParent(transform);
        }

        public virtual void StopHighlighting(HighlightableSelectableUI button)
        {
            if (rightArrow != null) rightArrow.gameObject.SetActive(false);
            if (rightArrow != null) leftArrow.gameObject.SetActive(false);

            rightArrow = null;
            leftArrow = null;

            button.IsHighlighted = false;
            highlightedButton = null;
        }
    }
}