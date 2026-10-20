using MrX.Easing;
using MrX.Input;
using MrX.UI.Armory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MrX.UI
{
    public class MainMenuArmoryPageBehavior : MainMenuPageBehavior
    {
        [Space]
        [SerializeField] protected ArmoryHeroPanel heroPanel;
        [SerializeField] protected ArmoryTabsPanel tabsPanel;
        [SerializeField] protected ItemsGridBehavior itemsGrid;
        [SerializeField] protected HeroesGridBehavior heroesGrid;
        [SerializeField] protected RectTransform armoryPanelRect;

        [Space]
        [SerializeField] protected ArmoryTabBehavior itemsTab;
        [SerializeField] protected ArmoryTabBehavior heroesTab;
        [SerializeField] private ArmoryDashboard dashboard;

        protected override void Start()
        {
            base.Start();

            tabsPanel.onHeroesTabSelected += OnHeroesTabSelected;
            tabsPanel.onItemsTabSelected += OnItemsTabSelected;

            GameController.MainMenuScreenBehavior.ExpandedItemPopup.OnPopupHidden += ResetSelection;
            GameController.MainMenuScreenBehavior.ExpandedHeroPopup.OnPopupHidden += OnHeroPopupHidden;

            GameController.ArmoryManager.OnItemEquipmentChanged += RecalculateSelection;
        }

        protected virtual void OnItemsTabSelected()
        {
            itemsGrid.SpawnLeft();
            itemsGrid.MoveCenter();

            heroesGrid.MoveRight();

            itemsGrid.Init();

            ResetSelection();
            heroPanel.ShowSlots();
        }

        protected virtual void OnHeroesTabSelected()
        {
            itemsGrid.MoveLeft();

            heroesGrid.SpawnRight();
            heroesGrid.MoveCenter();

            heroesGrid.Init();

            ResetSelection();
            heroPanel.HideSlots();
        }

        public virtual void OpenHeroesTab()
        {
            if (dashboard != null) { dashboard.Focus(true); return; }
            heroesGrid.gameObject.SetActive(true);
            tabsPanel.SelectHeroesTab();
        }

        public override void MoveCenter()//Sau khi click vào nút Armory thì sẽ thực hiện MoveCenter để hiển thị Armory
        {
            // Debug.Log("Click Armory");
            base.MoveCenter();

            if (dashboard != null)
            {
                tabsPanel.gameObject.SetActive(false);
                itemsGrid.gameObject.SetActive(false); heroesGrid.gameObject.SetActive(false);
                heroPanel.Init(); heroPanel.ShowSlots(); dashboard.Show();
                return;
            }

            itemsTab.Init(true);
            heroesTab.Init(false);

            heroPanel.Init();
            itemsGrid.Init();
            heroesGrid.Init();

            itemsGrid.SpawnCenter();

            itemsGrid.gameObject.SetActive(true);
            heroesGrid.gameObject.SetActive(false);
            tabsPanel.Init();
            
            RecalculateSelection();//thực hiện tính toán lại các lựa chọn để đảm bảo rằng các nút điều hướng được thiết lập đúng cách.

            
        }

        protected virtual void RecalculateSelection()
        {
            if (dashboard != null) { dashboard.MarkDirty(); return; }
            EasingManager.DoNextFrame(() =>
            {
                heroPanel.CalculateNavigation(itemsGrid.GetFirstItemSelectable());

                itemsGrid.CalculateNavigation(heroPanel.GetLowestSelectable());
                heroesGrid.CalculateNavigation(null);
            });
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                ResetSelection();
            }
        }

        protected virtual void OnHeroPopupHidden()
        {
            if (dashboard != null) { dashboard.MarkDirty(); dashboard.Focus(true); return; }
            ResetSelection();
            heroesGrid.Init();
        }

        protected virtual void ResetSelection()
        {
            if (dashboard != null) { dashboard.MarkDirty(); dashboard.Focus(); return; }
            var firstSelectable = itemsTab.IsSelected ? itemsGrid.GetFirstItemSelectable() : heroesGrid.GetFirstItemSelectable();

            if (firstSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
            }
            else
            {
                var selectable = heroPanel.GetLowestSelectable();

                EventSystem.current.SetSelectedGameObject(selectable?.gameObject);
            }
        }

        public override void OnMoveFinished()///Sau khi kết thúc clear toàn bộ thông tin liên quan đến heropanel
        {
            // Debug.Log("Armory OnMoveFinished");
            base.OnMoveFinished();

            heroPanel.Clear();
            if (dashboard != null) { dashboard.Hide(); return; }
            itemsGrid.Clear();
            heroesGrid.Clear();
            tabsPanel.Clear();
        }
        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();

            ResetSelection();

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public override void MoveLeft()
        {
            if (dashboard != null) dashboard.Hide();
            base.MoveLeft();

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public override void MoveRight()
        {
            if (dashboard != null) dashboard.Hide();
            base.MoveRight();

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnDestroy()
        {
            tabsPanel.onHeroesTabSelected -= OnHeroesTabSelected;
            tabsPanel.onItemsTabSelected -= OnItemsTabSelected;
            if (GameController.MainMenuScreenBehavior != null)
            {
                GameController.MainMenuScreenBehavior.ExpandedItemPopup.OnPopupHidden -= ResetSelection;
                GameController.MainMenuScreenBehavior.ExpandedHeroPopup.OnPopupHidden -= OnHeroPopupHidden;
            }
            if (GameController.ArmoryManager != null) GameController.ArmoryManager.OnItemEquipmentChanged -= RecalculateSelection;
            if (GameController.InputManager != null) GameController.InputManager.onInputChanged -= OnInputChanged;
        }
    }
}
