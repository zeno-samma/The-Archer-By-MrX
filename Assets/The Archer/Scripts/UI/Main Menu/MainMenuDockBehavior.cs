using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MrX.UI
{
    public class MainMenuDockBehavior : MonoBehaviour
    {
        [SerializeField] protected List<MainMenuDockItemBehavior> dockItems;

        [Space]
        [SerializeField] protected GameObject leftGamepadIndicator;
        [SerializeField] protected GameObject rightGamepadIndicator;
        private readonly InspectorVisibility inspectorVisibility = new InspectorVisibility();

        public event UnityAction<MainMenuPageType> onPageSelected;
        public MainMenuPageType SelectedPage { get; protected set; }

        public bool IsDockEnabled { get; set; } = true;

#if UNITY_EDITOR
        public virtual void RegisterDailyQuests(Sprite icon)
        { RegisterPageSlot(MainMenuPageType.DailyQuests, "QUESTS", icon, "ui_5"); }

        public virtual void RegisterAchievements(Sprite icon)
        { RegisterPageSlot(MainMenuPageType.Achievements, "ACHIEVEMENTS", icon, "ui_6"); }

        public virtual void RegisterForge(Sprite icon)
        { RegisterPageSlot(MainMenuPageType.Forge, "FORGE", icon, "ui_4"); }

        private void RegisterPageSlot(MainMenuPageType type, string title, Sprite icon, string slotSprite)
        {
            if (dockItems.Exists(item => item.PageType == type)) return;
            // The existing scroll-art slot is a visual placeholder, not a registered
            // tab. Replace that slot instead of appending a seventh layout cell.
            Transform placeholder = null;
            foreach (var image in GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (image.sprite == null || image.sprite.name != slotSprite) continue;
                placeholder = image.transform;
                while (placeholder.parent != transform && placeholder.parent != null) placeholder = placeholder.parent;
                break;
            }
            var questTab = Instantiate(dockItems[dockItems.Count - 1], dockItems[0].transform.parent);
            if (placeholder != null)
            {
                questTab.transform.SetSiblingIndex(placeholder.GetSiblingIndex());
                placeholder.gameObject.SetActive(false);
            }
            questTab.gameObject.SetActive(true);
            questTab.Configure(type, title, icon);
            dockItems.Add(questTab);
        }

#endif

        public virtual bool TrySelectPage(MainMenuPageType pageType)
        {
            if (!IsDockEnabled) return false;
            var item = dockItems.Find(tab => tab.PageType == pageType);
            if (item == null) return false;
            item.Select();
            OnPageSelected(pageType);
            return true;
        }

        protected virtual void Awake()
        {
            inspectorVisibility.Capture(leftGamepadIndicator);
            inspectorVisibility.Capture(rightGamepadIndicator);
        }

        protected virtual void Start()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Triggers.performed += OnTriggersPerformed;
        }

        public virtual void Init(MainMenuPageType firstPageType)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
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

                inspectorVisibility.SetVisible(leftGamepadIndicator, selectedIndex != 0);
                inspectorVisibility.SetVisible(rightGamepadIndicator, selectedIndex != dockItems.Count - 1);
            }
        }

        protected virtual void OnInputChanged(Input.InputType prevInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                var selectedIndex = GetDockItemIndex(SelectedPage);

                if (selectedIndex != 0) inspectorVisibility.SetVisible(leftGamepadIndicator, true);
                if (selectedIndex != dockItems.Count - 1) inspectorVisibility.SetVisible(rightGamepadIndicator, true);
            }
            else
            {
                inspectorVisibility.SetVisible(leftGamepadIndicator, false);
                inspectorVisibility.SetVisible(rightGamepadIndicator, false);
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
