using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    // Selection, rather than Update, keeps off-screen gamepad rewards visible.
    public class AchievementRowFocus : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform row;

        public void Initialize(ScrollRect list, RectTransform rowRect) { scroll = list; row = rowRect; }

        public void OnSelect(BaseEventData eventData)
        {
            if (scroll == null || row == null) return;
            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, row);
            var viewport = scroll.viewport.rect;
            var offset = bounds.max.y > viewport.yMax ? viewport.yMax - bounds.max.y :
                bounds.min.y < viewport.yMin ? viewport.yMin - bounds.min.y : 0;
            if (Mathf.Abs(offset) < .01f) return;
            scroll.StopMovement();
            var position = scroll.content.anchoredPosition;
            position.y = Mathf.Clamp(position.y + offset, 0, Mathf.Max(0, scroll.content.rect.height - viewport.height));
            scroll.content.anchoredPosition = position;
        }
    }
}
