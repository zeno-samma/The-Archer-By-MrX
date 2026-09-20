using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class StatIndicator : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform;

        [Space]
        [SerializeField] protected Image iconImage;

        [Space]
        [SerializeField] protected TMP_Text valueText;
        [SerializeField] protected string singleValueFormat = "{0}";
        [SerializeField] protected string multipleValueFormat = "{0} > <color=#44CB42>{1}</color>";

        public RectTransform RectTransform => rectTransform;

        protected float spacing;
        protected float padding;

        protected virtual void Awake()
        {
            var width = rectTransform.sizeDelta.x;
            valueText.rectTransform.sizeDelta = new Vector2(valueText.preferredWidth + 1, valueText.rectTransform.sizeDelta.y);

            var textWidth = valueText.rectTransform.sizeDelta.x;
            var iconWidth = iconImage.rectTransform.sizeDelta.x;

            var iconPosition = iconImage.rectTransform.anchoredPosition.x;
            var textPosition = valueText.rectTransform.anchoredPosition.x;

            spacing = Mathf.Abs(iconPosition - textPosition) - iconWidth / 2 - textWidth / 2;
            padding = (width - iconWidth - textWidth - spacing) / 2;
        }

        public virtual void Init(Sprite icon, float value)
        {
            iconImage.sprite = icon;

            valueText.text = string.Format(singleValueFormat, value);
            UpdateSize();
        }

        public virtual void Init(Sprite icon, float value, float nextValue)
        {
            iconImage.sprite = icon;

            valueText.text = string.Format(multipleValueFormat, value, nextValue);
            UpdateSize();
        }

        protected virtual void UpdateSize()
        {
            valueText.ForceMeshUpdate();

            valueText.rectTransform.sizeDelta = new Vector2(valueText.preferredWidth + 1, valueText.rectTransform.sizeDelta.y);

            var textWidth = valueText.rectTransform.sizeDelta.x;
            var iconWidth = iconImage.rectTransform.sizeDelta.x;

            var contentWidth = iconWidth + spacing + textWidth;
            rectTransform.SetSizeDeltaX(padding + contentWidth + padding);

            var iconPosition = -contentWidth / 2 + iconWidth / 2;
            iconImage.rectTransform.SetAnchoredPositionX(iconPosition);
            valueText.rectTransform.SetAnchoredPositionX(iconPosition + iconWidth / 2 + spacing + textWidth / 2);
        }
    }
}