using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class ScalingLabelBehavior : MonoBehaviour
    {
        [SerializeField] protected TMP_Text label;
        [SerializeField] protected Image icon;
        [SerializeField] protected AligmentType aligment;

        protected RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        protected float spacing;

        public float Width => icon.rectTransform.sizeDelta.x + spacing + label.rectTransform.sizeDelta.x;

        public int Amount { get; protected set; }

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            spacing = label.rectTransform.anchoredPosition.x - label.rectTransform.sizeDelta.x / 2 - icon.rectTransform.anchoredPosition.x - icon.rectTransform.sizeDelta.x / 2;
        }

        public virtual void SetAmount(int amount)
        {
            Amount = amount;
            label.SetText(amount.ToString());
            RecalculatePositions();
        }

        public virtual void SetSprite(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        protected virtual void RecalculatePositions()
        {
            label.SetSizeDeltaX(label.preferredWidth);

            var iconWidth = icon.rectTransform.sizeDelta.x;
            var textWidth = label.rectTransform.sizeDelta.x;
            var width = iconWidth + spacing + textWidth;

            switch (aligment)
            {
                case AligmentType.Center:

                    icon.SetAnchoredPositionX(-width / 2f + iconWidth / 2f);
                    label.SetAnchoredPositionX(width / 2f - textWidth / 2f);
                    break;

                case AligmentType.Left:

                    icon.SetAnchoredPositionX(iconWidth / 2f);
                    label.SetAnchoredPositionX(iconWidth + spacing + textWidth / 2f);

                    break;

                case AligmentType.Right:

                    icon.SetAnchoredPositionX(-textWidth - spacing - iconWidth / 2f);
                    label.SetAnchoredPositionX(-textWidth / 2f);

                    break;
            }
        }

        public enum AligmentType
        {
            Left, Center, Right
        }
    }
}