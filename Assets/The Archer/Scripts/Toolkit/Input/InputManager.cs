using MrX.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MrX.Input
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        [Header("References")]
        [SerializeField] HighlightsParentBehavior highlightsParent;
        public HighlightsParentBehavior Highlights => highlightsParent;

        protected InputAsset inputAsset;
        public InputAsset InputAsset => inputAsset;

        protected InputSave save;

        public InputType ActiveInput { get => save.ActiveInput; protected set => save.ActiveInput = value; }
        public Vector2 MovementValue { get; protected set; }

        public JoystickBehavior Joystick { get; protected set; }

        public event UnityAction<InputType, InputType> onInputChanged;

        protected virtual void Awake()
        {
            if (!GameController.RegisterInputManager(this))
            {
                Destroy(gameObject);
                return;
            }

            inputAsset = new InputAsset();

            Init();
        }

        protected virtual void OnEnable()
        {
            if (inputAsset != null) inputAsset.Enable();
        }

        protected virtual void OnDisable()
        {
            if (inputAsset != null) inputAsset.Disable();
            MovementValue = Vector2.zero;
        }

        public virtual void Init()
        {
            save = GameController.SaveManager.GetSave<InputSave>("Input");

            if (Gamepad.current != null)
            {
                ActiveInput = InputType.Gamepad;
            }
            else
            {
                ActiveInput = Keyboard.current != null && !Application.isMobilePlatform ? InputType.Keyboard : InputType.UIJoystick;
            }

            inputAsset.GamepadDetection.Detection.performed += GamepadDetection;
        }

        protected virtual void Update()
        {
            if (inputAsset == null || save == null) return;
            if (ActiveInput != InputType.Keyboard && Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame && !Keyboard.current.CheckStateIsAtDefaultIgnoringNoise())
            {
                SetInputType(InputType.Keyboard);
            }

            if (ActiveInput != InputType.UIJoystick &&
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame && Touchscreen.current.primaryTouch.press.isPressed))
            {
                SetInputType(InputType.UIJoystick);
            }

            if (ActiveInput == InputType.Gamepad && Gamepad.current == null)
                SetInputType(Keyboard.current != null && !Application.isMobilePlatform ? InputType.Keyboard : InputType.UIJoystick);

            // Never gate physical movement behind the last UI device. A mouse click
            // must not stop a held WASD key, and a stick need not cross a button's
            // press threshold before its analog movement can be used.
            var movement = inputAsset.Gameplay.Movement.ReadValue<Vector2>();
            var device = inputAsset.Gameplay.Movement.activeControl?.device;
            if (movement.sqrMagnitude > 0.0001f)
            {
                if (device is Gamepad) SetInputType(InputType.Gamepad);
                else if (device is Keyboard) SetInputType(InputType.Keyboard);
            }

            if (ActiveInput == InputType.UIJoystick && Joystick != null)
            {
                MovementValue = Joystick.Value;
            }
            else
            {
                MovementValue = movement;
            }
        }

        protected virtual void GamepadDetection(InputAction.CallbackContext obj)
        {
            SetInputType(InputType.Gamepad);
        }

        private void SetInputType(InputType type)
        {
            if (save == null || ActiveInput == type) return;
            var previous = ActiveInput;
            ActiveInput = type;
            if (Joystick != null)
            {
                if (type == InputType.UIJoystick) Joystick.Enable();
                else Joystick.Disable();
            }
            if (highlightsParent != null)
            {
                if (type == InputType.UIJoystick) highlightsParent.DisableArrows();
                else highlightsParent.EnableArrows();
            }
            onInputChanged?.Invoke(previous, type);
        }

        protected virtual void OnDestroy()
        {
            if (inputAsset == null) return;
            inputAsset.GamepadDetection.Detection.performed -= GamepadDetection;
            inputAsset.Dispose();
        }

        public virtual void RegisterJoystick(JoystickBehavior joystick)
        {
            Joystick = joystick;

            if (ActiveInput == InputType.UIJoystick)
            {
                joystick.Enable();
            }
            else
            {
                joystick.Disable();
            }
        }

        public virtual void RemoveJoystick()
        {
            Joystick = null;
        }
    }

    public enum InputType
    {
        UIJoystick = 1,
        Keyboard = 2,
        Gamepad = 4,
    }
}
