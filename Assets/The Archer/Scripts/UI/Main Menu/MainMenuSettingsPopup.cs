using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuSettingsPopup : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform panelRect;

        [Space]
        [SerializeField] protected ToggleBehavior soundToggle;
        [SerializeField] protected ToggleBehavior musicToggle;
        [SerializeField] protected ToggleBehavior vibrationToggle;

        [Space]
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button quitButton;

        public UnityAction onPopupClosed;

        protected virtual void Start()
        {
            soundToggle.SetToggle(GameController.AudioManager.SoundVolume > 0.1f);
            musicToggle.SetToggle(GameController.AudioManager.MusicVolume > 0.1f);
            vibrationToggle.SetToggle(GameController.VibrationManager.IsVibrationEnabled);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            backgroundImage.SetAlpha(0);
            backgroundImage.DoAlpha(1, 0.3f);

            panelRect.localScale = new Vector3(0, 1, 1);
            panelRect.DoLocalScale(Vector3.one, 0.3f).SetDelay(0.1f).SetEasing(EasingType.CubicOut);

            closeButton.enabled = false;
            quitButton.enabled = false;

            closeButton.transform.localScale = Vector3.zero;
            closeButton.transform.DoLocalScale(Vector3.one, 0.3f).SetDelay(0.2f).SetEasing(EasingType.CubicOut).SetOnFinish(() =>
            {
                closeButton.enabled = true;
                quitButton.enabled = true;
            });

            EventSystem.current.SetSelectedGameObject(soundToggle.gameObject);
        }

        public virtual void Hide()
        {
            closeButton.enabled = false;
            quitButton.enabled = false;

            closeButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            panelRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn).SetDelay(0.1f);

            backgroundImage.DoAlpha(0, 0.3f).SetDelay(0.2f).SetOnFinish(() =>
            {
                gameObject.SetActive(false);
                onPopupClosed?.Invoke();
            });
        }

        protected virtual void OnEnable()
        {
            soundToggle.onChanged += OnSoundToggleChanged;
            musicToggle.onChanged += OnMusicToggleChanged;
            vibrationToggle.onChanged += OnVibrationToggleChanged;

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancelButtonPressed;
        }

        private void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(musicToggle.gameObject);
            }
        }

        protected virtual void OnCancelButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnCloseButtonClicked();
        }

        protected virtual void OnDisable()
        {
            soundToggle.onChanged -= OnSoundToggleChanged;
            musicToggle.onChanged -= OnMusicToggleChanged;
            vibrationToggle.onChanged -= OnVibrationToggleChanged;

            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);

            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancelButtonPressed;
        }

        protected virtual void OnSoundToggleChanged(bool value)
        {
            GameController.AudioManager.SoundVolume = value ? 1 : 0;
        }

        protected virtual void OnMusicToggleChanged(bool value)
        {
            GameController.AudioManager.MusicVolume = value ? 1 : 0;
        }

        protected virtual void OnVibrationToggleChanged(bool value)
        {
            GameController.VibrationManager.IsVibrationEnabled = value;
        }

        protected virtual void OnCloseButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            Hide();
        }

        protected virtual void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}