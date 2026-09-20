using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuInfoPopup : MonoBehaviour
    {
        [Space]
        [SerializeField] protected StageDatabase stageDatabase;

        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Button infoButton;

        [Space]
        [SerializeField] protected RectTransform descriptionRect;
        [SerializeField] protected TMP_Text descriptionText;

        public UnityAction onPopupClosed;

        protected StageSave StageSave { get; set; }

        protected float heightDifference;

        protected IEasingCoroutine sizeEasingCoroutine;
        protected IEasingCoroutine alphaEasingCoroutine;

        protected float rectWidth;

        protected virtual void Awake()
        {
            infoButton.onClick.AddListener(Hide);

            infoButton.enabled = false;

            heightDifference = descriptionRect.sizeDelta.y - descriptionText.preferredHeight;
            rectWidth = descriptionRect.sizeDelta.x;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            UpdateText();

            sizeEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            backgroundImage.SetAlpha(0);
            alphaEasingCoroutine = backgroundImage.DoAlpha(1, 0.3f);

            var rectheight = descriptionText.preferredHeight + heightDifference;

            var startRectSize = new Vector2(rectWidth - 50, 0);
            var targetRectSize = new Vector2(rectWidth, rectheight);

            descriptionRect.sizeDelta = startRectSize;
            sizeEasingCoroutine = descriptionRect.DoSizeDelta(targetRectSize, 0.3f, 0.1f).SetEasing(EasingType.QuadOut).SetOnFinish(() => {
                infoButton.enabled = true;
                SubscribeToInputEvents();
            });
        }

        protected virtual void SubscribeToInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Y.performed += Hide;
            GameController.InputManager.InputAsset.UI.Back.performed += Hide;
        }

        protected virtual void UnsubscribeFromInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Y.performed -= Hide;
            GameController.InputManager.InputAsset.UI.Back.performed -= Hide;
        }

        protected virtual void Hide(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Hide();
        }

        protected virtual void UpdateText()
        {
            if(StageSave == null)
            {
                StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            }

            var stageData = stageDatabase.GetStageData(StageSave.SelectedStageId);
            descriptionText.SetText(stageData.UnlockDescription);
            descriptionText.ForceMeshUpdate();
        }

        protected virtual void Hide()
        {
            infoButton.enabled = false;

            UnsubscribeFromInputEvents();

            sizeEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();

            var targetRectSize = new Vector2(rectWidth - 50, 0);
            sizeEasingCoroutine = descriptionRect.DoSizeDelta(targetRectSize, 0.3f).SetEasing(EasingType.QuadIn);

            alphaEasingCoroutine = backgroundImage.DoAlpha(0, 0.3f, 0.1f).SetOnFinish(() => {
                gameObject.SetActive(false);
                onPopupClosed?.Invoke();
            });
        }

        protected virtual void OnDisable()
        {
            sizeEasingCoroutine.StopIfExists();
            alphaEasingCoroutine.StopIfExists();
        }
    }
}