using OctoberStudio.Input;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.UI
{
    public class ArmoryTabsPanel : MonoBehaviour
    {
        [SerializeField] protected ArmoryTabBehavior itemsTab;
        [SerializeField] protected ArmoryTabBehavior heroesTab;

        [Space]
        [SerializeField] protected GameObject leftGamepadIndicator;
        [SerializeField] protected GameObject rightGamepadIndicator;

        public event UnityAction onItemsTabSelected;
        public event UnityAction onHeroesTabSelected;

        protected virtual void Start()
        {
            itemsTab.Init(true);
            heroesTab.Init(false);

            itemsTab.onTabSelected += OnItemsTabSelected;
            heroesTab.onTabSelected += OnHeroesTabSelected;

            leftGamepadIndicator.SetActive(false);
            rightGamepadIndicator.SetActive(GameController.InputManager.ActiveInput == Input.InputType.Gamepad);
        }

        public virtual void Init()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Shoulders.performed += OnShouldersPerformed;
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                leftGamepadIndicator.SetActive(!itemsTab.IsSelected);
                rightGamepadIndicator.SetActive(!heroesTab.IsSelected);
            }
            else
            {
                leftGamepadIndicator.SetActive(false);
                rightGamepadIndicator.SetActive(false);
            }
        }

        protected virtual void OnItemsTabSelected()
        {
            itemsTab.Select();
            heroesTab.Deselect();

            onItemsTabSelected?.Invoke();
        }

        protected virtual void OnHeroesTabSelected()
        {
            heroesTab.Select();
            itemsTab.Deselect();

            onHeroesTabSelected?.Invoke();
        }

        protected virtual void OnShouldersPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (GameController.MainMenuScreenBehavior.ExpandedHeroPopup.IsShown || GameController.MainMenuScreenBehavior.ExpandedItemPopup.IsShown)
            {
                return;
            }

            var value = ctx.ReadValue<float>();
            if (value > 0)
            {
                if (!heroesTab.IsSelected)
                {
                    heroesTab.Select();
                    itemsTab.Deselect();

                    onHeroesTabSelected?.Invoke();
                }
            }
            else
            {
                if (!itemsTab.IsSelected)
                {
                    itemsTab.Select();
                    heroesTab.Deselect();

                    onItemsTabSelected?.Invoke();
                }
            }

            leftGamepadIndicator.SetActive(!itemsTab.IsSelected);
            rightGamepadIndicator.SetActive(!heroesTab.IsSelected);
        }

        public virtual void Clear()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Shoulders.performed -= OnShouldersPerformed;
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }
    }
}