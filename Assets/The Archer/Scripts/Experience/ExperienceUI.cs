using OctoberStudio.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio
{
    public class ExperienceUI : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        [SerializeField] protected RectMask2D rectMask;
        [SerializeField] protected TMP_Text levelText;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DoAlpha(1f, 0.3f);
        }

        public virtual void Hide()
        {
            canvasGroup.DoAlpha(0f, 0.3f).SetOnFinish(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public virtual void SetProgress(float progress)
        {
            var padding = rectMask.padding;
            padding.z = rectMask.rectTransform.rect.width * (1 - progress);
            rectMask.padding = padding;
        }

        public virtual void SetLevelText(int levelNumber)
        {
            levelText.text = $"LVL {levelNumber}";
        }
    }
}