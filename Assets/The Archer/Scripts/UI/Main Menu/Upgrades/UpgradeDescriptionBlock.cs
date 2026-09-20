using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Upgrades;
using TMPro;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class UpgradeDescriptionBlock : MonoBehaviour
    {
        [SerializeField] protected TMP_Text descriptionText;
        [SerializeField] protected TMP_Text statsText;
        [SerializeField] protected RectTransform statsBackgroundRect;
        [SerializeField] protected float backgroundPadding = 10f;
        [SerializeField] protected float spacing = 10f;
        protected UpgradeData Data { get; set; }

        public void SetData(UpgradeData data)
        {
            if (Data != null)
            {
                Data.onUpgradeLevelChanged -= OnUpgradeLevelChanged;
            }

            Data = data;

            descriptionText.text = data.Description;

            data.onUpgradeLevelChanged += OnUpgradeLevelChanged;

            var level = GameController.UpgradesManager.GetUpgradeLevel(data.UpgradeType);
            OnUpgradeLevelChanged(level);
        }

        protected virtual void OnUpgradeLevelChanged(int level)
        {
            if (level == Data.LevelsCount - 1)
            {
                statsBackgroundRect.gameObject.SetActive(false);
            }
            else
            {
                statsBackgroundRect.gameObject.SetActive(true);

                float value;
                if (level >= 0)
                {
                    value = Data.GetLevel(level).Value;
                }
                else
                {
                    value = Data.InitialValue;
                }

                var nextValue = Data.GetLevel(level + 1).Value;

                statsText.text = string.Format(Data.StatsText, value, nextValue);
            }

            RecalculateBackgroundRect();
        }

        protected virtual void RecalculateBackgroundRect()
        {
            descriptionText.ForceMeshUpdate();
            statsText.ForceMeshUpdate();

            statsBackgroundRect.sizeDelta = statsBackgroundRect.sizeDelta.SetX(statsText.preferredWidth + backgroundPadding * 2);

            descriptionText.rectTransform.sizeDelta = new Vector2(descriptionText.rectTransform.sizeDelta.x + 1, TextHeight() + 1);
            statsBackgroundRect.anchoredPosition = Vector2.down * (TextHeight() + spacing);
        }

        public virtual void Show()
        {
            descriptionText.alpha = 0f;
            statsText.alpha = 0f;
            statsBackgroundRect.localScale = Vector3.zero;

            descriptionText.DoAlpha(1f, 0.3f).SetDelay(0.3f);
            statsText.DoAlpha(1f, 0.3f).SetDelay(0.3f);
            statsBackgroundRect.DoLocalScale(Vector3.one, 0.3f).SetDelay(0.3f).SetEasing(EasingType.CubicOut);
        }

        public virtual void Hide()
        {
            descriptionText.DoAlpha(0f, 0.3f);
            statsText.DoAlpha(0f, 0.3f);
            statsBackgroundRect.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            Data.onUpgradeLevelChanged -= OnUpgradeLevelChanged;
        }

        protected float TextWidth()
        {
            var maxLineWidth = 0f;
            for (int i = 0; i < descriptionText.textInfo.lineCount; i++)
            {
                var line = descriptionText.textInfo.lineInfo[i];
                float lineWidth = line.maxAdvance;
                if (lineWidth > maxLineWidth)
                    maxLineWidth = lineWidth;
            }

            return maxLineWidth;
        }

        protected float TextHeight()
        {
            var height = 0f;
            for (int i = 0; i < descriptionText.textInfo.lineCount; i++)
            {
                var line = descriptionText.textInfo.lineInfo[i];
                height += line.lineHeight;
            }

            return height;
        }
    }
}