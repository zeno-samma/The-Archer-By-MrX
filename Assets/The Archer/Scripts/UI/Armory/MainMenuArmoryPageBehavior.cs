using OctoberStudio.Easing;
using OctoberStudio.Input;
using OctoberStudio.UI.Armory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OctoberStudio.UI
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

        public override void MoveCenter()
        {
            base.MoveCenter();

            itemsTab.Init(true);
            heroesTab.Init(false);

            heroPanel.Init();

            itemsGrid.Init();
            heroesGrid.Init();

            itemsGrid.SpawnCenter();

            itemsGrid.gameObject.SetActive(true);
            heroesGrid.gameObject.SetActive(false);

            RecalculateSelection();

            tabsPanel.Init();
        }

        protected virtual void RecalculateSelection()
        {
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
            ResetSelection();
            heroesGrid.Init();
        }

        protected virtual void ResetSelection()
        {
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

        public override void OnMoveFinished()
        {
            base.OnMoveFinished();

            heroPanel.Clear();
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
            base.MoveLeft();

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public override void MoveRight()
        {
            base.MoveRight();

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnDestroy()
        {
            GameController.ArmoryManager.OnItemEquipmentChanged -= RecalculateSelection;
        }
    }
}