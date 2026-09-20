using OctoberStudio.Easing;
using OctoberStudio.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OctoberStudio
{
    public class SelectableGamepadIndicator : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] protected GameObject gamepadIndicator;
        public bool IsSelected { get; protected set; }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            gamepadIndicator.SetActive(newInput == InputType.Gamepad && IsSelected);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (GameController.InputManager.ActiveInput == InputType.Gamepad)
            {
                gamepadIndicator.SetActive(true);
            }

            IsSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            IsSelected = false;

            gamepadIndicator.SetActive(false);
        }

        public virtual void Select()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        protected virtual void OnEnable()
        {
            if (GameController.InputManager != null)
            {
                GameController.InputManager.onInputChanged += OnInputChanged;
            }
            else
            {
                EasingManager.DoNextFrame(() => GameController.InputManager.onInputChanged += OnInputChanged);
            }

            bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
            gamepadIndicator.SetActive(isSelected && GameController.InputManager.ActiveInput == InputType.Gamepad);
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;

            gamepadIndicator.SetActive(false);
        }
    }
}