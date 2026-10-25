using MrX.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public enum UIButtonAudioMode { Default = 0, Custom = 1, Silent = 2 }

    [DisallowMultipleComponent, RequireComponent(typeof(Button))]
    [AddComponentMenu("MrX Studio/UI/Button Audio")]
    public sealed class UIButtonAudio : MonoBehaviour
    {
        [SerializeField] private UIButtonAudioMode mode;
        [Tooltip("Custom mode only. Use AudioData routed to the UI mixer group. Missing data stays silent.")]
        [SerializeField] private AudioData customSound;
        private Button button;

        public UIButtonAudioMode Mode { get => mode; set => mode = value; }
        public AudioData CustomSound { get => customSound; set => customSound = value; }

        private void OnEnable()
        {
            button = GetComponent<Button>();
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            var audio = GameController.AudioManager;
            if (audio == null || mode == UIButtonAudioMode.Silent) return;
            if (audio is AudioManager concrete && !concrete.IsInitialized) return;
            // Button already validates pointer/submit input. A purchase handler may
            // disable/hide the button during this same click, so do not reject it
            // by checking the resulting interactable/active state here.
            if (mode == UIButtonAudioMode.Custom)
            {
                if (customSound != null) audio.PlayAudio(customSound);
            }
            else audio.PlayButtonClick();
        }

        // Keyboard/gamepad shortcuts use the same Button event as pointer input.
        // Never call onClick.Invoke directly on a disabled or hidden button.
        public static bool Click(Button target)
        {
            if (target == null || !target.isActiveAndEnabled || !target.IsInteractable()) return false;
            target.onClick.Invoke();
            return true;
        }

        // Some selectors deliberately hide their arrow Buttons in gamepad mode.
        // The owning shortcut checks availability and runs its existing action;
        // this method supplies only that hidden button's configured audio.
        public static void PlayShortcut(Button target)
        {
            if (target == null || !target.enabled || !target.interactable) return;
            var audio = target.GetComponent<UIButtonAudio>();
            if (audio != null && audio.enabled) audio.OnClick();
        }
    }
}
