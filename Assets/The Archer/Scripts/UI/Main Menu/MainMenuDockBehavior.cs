using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.UI
{
    public class MainMenuDockBehavior : MonoBehaviour
    {
        [SerializeField] protected List<MainMenuDockItemBehavior> dockItems;

        [Space]
        [SerializeField] protected GameObject leftGamepadIndicator;
        [SerializeField] protected GameObject rightGamepadIndicator;

        public event UnityAction<MainMenuPageType> onPageSelected;
        public MainMenuPageType SelectedPage { get; protected set; }

        public bool IsDockEnabled { get; set; } = true;

        protected virtual void Start()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Triggers.performed += OnTriggersPerformed;
        }

        public virtual void Init(MainMenuPageType firstPageType)
        {
            dockItems.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            for (int i = 0; i < dockItems.Count; i++)
            {
                if (dockItems[i].PageType == firstPageType)
                {
                    dockItems[i].Select();
                }
                else
                {
                    dockItems[i].Deselect();
                }

                dockItems[i].onSelected += OnPageSelected;
            }

            SelectedPage = firstPageType;
            CheckGamepadIndicators();
        }

        public virtual int GetDockItemIndex(MainMenuPageType pageType)
        {
            return dockItems.FindIndex(item => item.PageType == pageType);
        }

        protected virtual void OnPageSelected(MainMenuPageType pageType)
        {
            SelectedPage = pageType;

            for (int i = 0; i < dockItems.Count; i++)
            {
                if (dockItems[i].PageType != pageType)
                {
                    if (dockItems[i].IsSelected)
                    {
                        dockItems[i].Deselect();
                    }
                }
            }

            onPageSelected?.Invoke(pageType);

            CheckGamepadIndicators();
        }

        protected virtual void CheckGamepadIndicators()
        {
            if (GameController.InputManager.ActiveInput == Input.InputType.Gamepad)
            {
                var selectedIndex = GetDockItemIndex(SelectedPage);

                leftGamepadIndicator.SetActive(selectedIndex != 0);
                rightGamepadIndicator.SetActive(selectedIndex != dockItems.Count - 1);
            }
        }

        protected virtual void OnInputChanged(Input.InputType prevInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                var selectedIndex = GetDockItemIndex(SelectedPage);

                if (selectedIndex != 0) leftGamepadIndicator.SetActive(true);
                if (selectedIndex != dockItems.Count - 1) rightGamepadIndicator.SetActive(true);
            }
            else
            {
                leftGamepadIndicator.SetActive(false);
                rightGamepadIndicator.SetActive(false);
            }
        }

        protected virtual void OnTriggersPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (!IsDockEnabled) return;

            var selectedIndex = GetDockItemIndex(SelectedPage);

            var value = ctx.ReadValue<float>();

            if (value > 0)
            {
                if (selectedIndex < dockItems.Count - 1)
                {
                    OnPageSelected(dockItems[selectedIndex + 1].PageType);
                    dockItems[selectedIndex + 1].Select();
                }
            }
            else
            {
                if (selectedIndex > 0)
                {
                    OnPageSelected(dockItems[selectedIndex - 1].PageType);
                    dockItems[selectedIndex - 1].Select();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Triggers.performed -= OnTriggersPerformed;
        }
    }
}