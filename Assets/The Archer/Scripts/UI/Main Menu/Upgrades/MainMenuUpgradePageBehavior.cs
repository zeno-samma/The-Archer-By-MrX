using OctoberStudio.Easing;
using OctoberStudio.Pool;
using OctoberStudio.Upgrades;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuUpgradePageBehavior : MainMenuPageBehavior
    {
        [Space]
        [SerializeField] protected UpgradesDatabase database;

        [Space]
        [SerializeField] protected GameObject upgradeCardPrefab;
        [SerializeField] protected RectTransform cardsParent;

        [Space]
        [SerializeField] protected ScrollRect scrollView;

        protected List<UpgradeCardBehavior> upgradeCards = new List<UpgradeCardBehavior>();
        protected PoolComponent<UpgradeCardBehavior> upgradeCardPool;

        protected override void Awake()
        {
            base.Awake();

            upgradeCardPool = new PoolComponent<UpgradeCardBehavior>(upgradeCardPrefab, 10, cardsParent);
        }

        protected override void Start()
        {
            base.Start();

            GameController.MainMenuScreenBehavior.ExpandedUpgradePopup.onPopupHidden += OnExpandedPopupHidden;
            GameController.MainMenuScreenBehavior.ExpandedUpgradePopup.onPopupShown += OnExpandedPopupShown;
        }

        protected virtual void OnExpandedPopupHidden()
        {
            if (upgradeCards.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(upgradeCards[0].gameObject);
            }

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public virtual void OnExpandedPopupShown()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public override void MoveCenter()
        {
            base.MoveCenter();

            for (int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);

                var upgradeCard = upgradeCardPool.GetEntity();

                upgradeCard.Init(upgrade);
                upgradeCard.onCardSelected += OnCardSelected;

                upgradeCards.Add(upgradeCard);
            }

            GameController.InputManager.onInputChanged += OnInputChanged;

            EasingManager.DoNextFrame(CalculateNavigation);
        }

        protected virtual void CalculateNavigation()
        {
            var cardsInRow = -1;
            for (int i = 1; i < upgradeCards.Count; i++)
            {
                var prevCard = upgradeCards[i - 1];
                var card = upgradeCards[i];

                if (!Mathf.Approximately(prevCard.transform.position.y, card.transform.position.y))
                {
                    cardsInRow = i;
                    break;
                }
            }

            if (cardsInRow < 0)
            {
                cardsInRow = upgradeCards.Count;
            }

            for (int i = 0; i < upgradeCards.Count; i++)
            {
                var leftIndex = i - 1;
                var rightIndex = i + 1;
                var upIndex = i - cardsInRow;
                var downIndex = i + cardsInRow;

                var leftCard = leftIndex >= 0 ? upgradeCards[leftIndex] : null;
                var rightCard = rightIndex < upgradeCards.Count ? upgradeCards[rightIndex] : null;
                var upCard = upIndex >= 0 ? upgradeCards[upIndex] : null;
                var downCard = downIndex < upgradeCards.Count ? upgradeCards[downIndex] : null;

                var card = upgradeCards[i];
                card.SetNavigation(leftCard, rightCard, upCard, downCard);
            }
        }

        protected virtual void OnInputChanged(Input.InputType oldInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                if (upgradeCards.Count > 0)
                {
                    EventSystem.current.SetSelectedGameObject(upgradeCards[0].gameObject);
                }
            }
        }

        protected virtual void OnCardSelected(UpgradeCardBehavior selectedCard)
        {
            var objPosition = (Vector2)scrollView.transform.InverseTransformPoint(selectedCard.RectTransform.position);
            var scrollHeight = scrollView.GetComponent<RectTransform>().rect.height;
            var objHeight = selectedCard.RectTransform.rect.height;

            if (objPosition.y > scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y - objHeight - 37);
            }

            if (objPosition.y < -scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y + objHeight + 37);
            }
        }

        public override void OnMoveFinished()
        {
            base.OnMoveFinished();

            for (int i = 0; i < upgradeCards.Count; i++)
            {
                var upgradeCard = upgradeCards[i];
                upgradeCard.onCardSelected -= OnCardSelected;

                upgradeCard.Clear();
                upgradeCard.gameObject.SetActive(false);
            }

            GameController.InputManager.onInputChanged -= OnInputChanged;

            upgradeCards.Clear();
        }

        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();

            if (upgradeCards.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(upgradeCards[0].gameObject);
            }
        }
    }
}