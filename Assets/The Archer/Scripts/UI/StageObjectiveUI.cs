using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageObjectiveUI : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Float fadeInDuration = 0.5f;
        [SerializeField] protected Float stayDuration = 1.5f;
        [SerializeField] protected Float fadeOutDuration = 0.5f;

        [Space]        
        [SerializeField] protected TMP_Text stageIdText;
        [SerializeField] protected string stageIdFormat = "Stage {0}";
        [SerializeField] protected Image leftDecorImage;
        [SerializeField] protected Image rightDecorImage;

        [Space]
        [SerializeField] protected TMP_Text objectiveText;

        protected float leftOffset;
        protected float rightOffset;

        protected virtual void Awake()
        {
            var prefWidth = stageIdText.preferredWidth;

            leftOffset = leftDecorImage.rectTransform.anchoredPosition.x - prefWidth / 2f;
            rightOffset = rightDecorImage.rectTransform.anchoredPosition.x - prefWidth / 2f;
        }

        public virtual void Show(StageData stageData, int stageId, UnityAction onComplete)
        {
            gameObject.SetActive(true);

            stageIdText.text = string.Format(stageIdFormat, stageId + 1);
            stageIdText.ForceMeshUpdate();

            var halfWidth = stageIdText.preferredWidth / 2f;
            leftDecorImage.rectTransform.SetAnchoredPositionX(halfWidth + leftOffset);
            rightDecorImage.rectTransform.SetAnchoredPositionX(halfWidth + rightOffset);

            objectiveText.text = stageData.StageObjective;

            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1, fadeInDuration);
            EasingManager.DoAfter(fadeInDuration + stayDuration, () => Hide(onComplete));
        }

        protected virtual void Hide(UnityAction onComplete)
        {
            canvasGroup.DoAlpha(0, fadeOutDuration).SetOnFinish(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}