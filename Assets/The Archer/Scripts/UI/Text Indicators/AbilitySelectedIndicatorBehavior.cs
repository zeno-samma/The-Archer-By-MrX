using OctoberStudio.Abilities;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class AbilitySelectedIndicatorBehavior : TextIndicatorBehavior
    {
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

            gradientWidthDifference = gradientImage.rectTransform.sizeDelta.x - TextWidth();
            gradientHeightDifference = gradientImage.rectTransform.sizeDelta.y - textHeight;
        }

        public virtual void SetAbilityType(AbilityType abilityType)
        {
            var abilityData = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(abilityType);
            var rarityData = StageController.AbilitiesManager.AbilitiesDatabase.GetRarityData(abilityData.Rarity);

            textComponent.text = abilityData.Title;
            textComponent.ForceMeshUpdate();

            gradientImage.rectTransform.SetSizeDeltaY(TextHeight() + gradientHeightDifference);
            gradientImage.rectTransform.SetSizeDeltaX(TextWidth() + gradientWidthDifference);

            gradientImage.color = rarityData.Color.SetAlpha(gradientAlpha);
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