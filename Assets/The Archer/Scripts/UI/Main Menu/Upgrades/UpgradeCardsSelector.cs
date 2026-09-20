using OctoberStudio.Easing;
using OctoberStudio.Upgrades;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OctoberStudio.UI
{
    public class UpgradeCardsSelector : HorizontalSelector<UpgradeCardSelectable>
    {
        protected List<UpgradeData> upgrades;

        protected int selectedId;
        public override int SelectedId => selectedId;

        protected override void Awake()
        {
            base.Awake();
            upgrades = GameController.UpgradesManager.GetAllUpgrades();
        }

        public virtual void Init(int selectedId)
        {
            this.selectedId = selectedId;

            CurrentSelectable = SpawnSelectable(selectedId);
            EventSystem.current.SetSelectedGameObject(CurrentSelectable.UpgradeCard.gameObject);

            InitButtons();
        }

        public virtual void Show()
        {
            leftButton.transform.localScale = Vector3.zero;
            rightButton.transform.localScale = Vector3.zero;

            leftGamepadIndicator.transform.localScale = Vector3.zero;
            rightGamepadIndicator.transform.localScale = Vector3.zero;

            selectablesParent.localScale = Vector3.zero;

            leftButton.transform.DoLocalScale(new Vector3(-1, 1, 1), 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.2f);
            rightButton.transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.2f);

            leftGamepadIndicator.transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.2f);
            rightGamepadIndicator.transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.2f);

            selectablesParent.localScale = new Vector3(0, 1, 1);
            selectablesParent.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.1f);
        }

        public void Hide()
        {
            leftButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);
            rightButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            leftGamepadIndicator.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);
            rightGamepadIndicator.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            selectablesParent.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn).SetOnFinish(OnHidden);
        }

        protected virtual void OnHidden()
        {
            CurrentSelectable.Clear();

            for (int i = 0; i < SelectablesQueue.Count; i++)
            {
                SelectablesQueue[i].Clear();
            }

            SelectablesQueue.Clear();
        }

        protected override void OnLeftButtonClicked()
        {
            base.OnLeftButtonClicked();

            selectedId--;
            InitButtons();
        }

        protected override void OnRightButtonClicked()
        {
            base.OnRightButtonClicked();

            selectedId++;
            InitButtons();
        }

        protected override bool IsLeftAvailable()
        {
            return selectedId > 0;
        }

        protected override bool IsRightAvailable()
        {
            return selectedId < upgrades.Count - 1;
        }

        protected override UpgradeCardSelectable SpawnSelectable(int id)
        {
            var selectable = selectablesPool.GetEntity();
            selectable.UpgradeCard.Init(upgrades[id]);

            selectable.UpgradeCard.SetNavigation(null, null, null, null);

            return selectable;
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromInputEvents();
        }
    }
}