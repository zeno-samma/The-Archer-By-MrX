using OctoberStudio.Easing;
using OctoberStudio.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio
{
    public class ContinuePlayingPopup : MonoBehaviour
    {
        [SerializeField] protected RectTransform panelRect;
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected Image backgroundImage;

        [Space]
        [SerializeField] protected Button yesButton;
        [SerializeField] protected Button noButton;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public bool IsOpen { get; protected set; }
        public UnityAction onPopupClosed;

        protected virtual void Start()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            EventSystem.current.SetSelectedGameObject(yesButton.gameObject);

            IsOpen = true;
        }

        public virtual void Hide()
        {
            yesButton.enabled = false;
            noButton.enabled = false;

            panelRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn);
            titleRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn).SetDelay(0.1f);
            backgroundImage.DoAlpha(0, 0.3f).SetDelay(0.2f).SetOnFinish(OnPopupHidden);
        }

        protected virtual void OnPopupHidden()
        {
            gameObject.SetActive(false);

            IsOpen = false;

            onPopupClosed?.Invoke();
        }

        protected virtual void OnEnable()
        {
            yesButton.onClick.AddListener(OnYesButtonClicked);
            noButton.onClick.AddListener(OnNoButtonClicked);

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
            }
        }

        protected virtual void OnDisable()
        {
            yesButton.onClick.RemoveListener(OnYesButtonClicked);
            noButton.onClick.RemoveListener(OnNoButtonClicked);

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnYesButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            GameController.LoadStage();
        }

        protected virtual void OnNoButtonClicked()
        {
            ContinuePlayingSave.Disable();
            GameController.CurrenciesManager.ApplyGameplayAmounts();

            GameController.AudioManager.PlayButtonClick();

            Hide();
        }
    }
}