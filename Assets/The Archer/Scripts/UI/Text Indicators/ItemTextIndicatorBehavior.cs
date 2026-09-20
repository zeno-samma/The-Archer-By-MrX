using OctoberStudio.Armory;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class ItemTextIndicatorBehavior : TextIndicatorBehavior
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image gradientImage;

        protected float gradientAlpha;

        protected float heightDifference;
        protected float spacing;
        protected float gradientWidthDifference;
        protected float gradientHeightDifference;

        protected virtual void Awake()
        {
            gradientAlpha = gradientImage.color.a;

            textComponent.ForceMeshUpdate();
            var textHeight = TextHeight();

            heightDifference = textHeight - iconImage.rectTransform.sizeDelta.y;

            spacing = textComponent.rectTransform.anchoredPosition.x - iconImage.rectTransform.anchoredPosition.x - iconImage.rectTransform.sizeDelta.x;
            var combinedWidth = iconImage.rectTransform.sizeDelta.x + spacing + TextWidth();
            gradientWidthDifference = gradientImage.rectTransform.sizeDelta.x - combinedWidth;
            gradientHeightDifference = gradientImage.rectTransform.sizeDelta.y - textHeight;
        }

        public virtual void SetItem(ItemData item)
        {
            var level = GameController.ArmoryManager.GetNextItemLevel(item.Id);

            if (level != null)
            {
                var rarityData = GameController.ArmoryManager.GetItemRarityData(level.ItemRarity);
                gradientImage.color = rarityData.MainColor.SetAlpha(gradientAlpha);
            }

            textComponent.text = item.ItemName;
            textComponent.ForceMeshUpdate();

            var textheight = TextHeight();

            gradientImage.rectTransform.SetSizeDeltaY(textheight + gradientHeightDifference);
            iconImage.rectTransform.sizeDelta = Vector2.one * (textheight - heightDifference);

            var combinedWidth = iconImage.rectTransform.sizeDelta.x + spacing + TextWidth();
            iconImage.rectTransform.SetAnchoredPositionX(-combinedWidth / 2f);
            textComponent.rectTransform.SetAnchoredPositionX(-combinedWidth / 2f + iconImage.rectTransform.sizeDelta.x + spacing);
            gradientImage.rectTransform.SetSizeDeltaX(combinedWidth + gradientWidthDifference);

            var typeData = GameController.ArmoryManager.GetItemTypeData(item.ItemType);
            iconImage.sprite = typeData.ItemTypeIcon;
        }

        protected float TextWidth()
        {
            var maxLineWidth = 0f;
            for (int i = 0; i < textComponent.textInfo.lineCount; i++)
            {
                var line = textComponent.textInfo.lineInfo[i];
                float lineWidth = line.maxAdvance;
                if (lineWidth > maxLineWidth)
                    maxLineWidth = lineWidth;
            }

            return maxLineWidth;
        }

        protected float TextHeight()
        {
            var height = 0f;
            for (int i = 0; i < textComponent.textInfo.lineCount; i++)
            {
                var line = textComponent.textInfo.lineInfo[i];
                height += line.lineHeight;
            }

            return height;
        }
    }
}