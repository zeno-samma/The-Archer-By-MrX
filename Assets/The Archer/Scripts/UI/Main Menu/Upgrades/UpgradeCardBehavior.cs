using MrX.Input;
using MrX.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MrX.UI
{
    public class UpgradeCardBehavior : Selectable
    {
        [SerializeField] protected Image upgradeImage;

        [Space]
        [SerializeField] protected TMP_Text upgradeTitleText;
        [SerializeField] protected TMP_Text upgradeLevelText;

        [Space]
        [SerializeField] protected Button infoButton;
        [SerializeField] protected CostButtonBehavior upgradeButton;
        [SerializeField] protected CanvasGroup buttonCanvasGroup;
        [SerializeField] protected ScalingLabelBehavior costLabel;
        [SerializeField] protected GameObject upgradedLabel;

        [Space]
        [SerializeField] protected GameObject gamepadIndicator;

        [Space]
        [SerializeField] protected bool buyOnGamepadButtonClick = false;

        public UpgradeData Data { get; protected set; }
        public int UpgradeLevel { get; protected set; }
        public bool IsSelected { get; protected set; } = false;

        public RectTransform RectTransform { get; protected set; }

        public event UnityAction<UpgradeCardBehavior> onCardSelected;
        public CurrencySave Currency { get; protected set; }
        private readonly InspectorVisibility authoredVisibility = new InspectorVisibility();

        protected override void Awake()
        {
            base.Awake();
            authoredVisibility.Capture(gamepadIndicator);
            if (costLabel != null) authoredVisibility.Capture(costLabel.gameObject);
            authoredVisibility.Capture(upgradedLabel);

            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void Init(UpgradeData data)
        {
            GameController.InputManager.InputAsset.UI.A.performed += OnGamepadSouthButtonPressed;
            GameController.InputManager.onInputChanged += OnInputChanged;

            Data = data;
            Data.onUpgradeLevelChanged += OnUpgradeLevelChanged;

            RedrawVisuals();

            if (GameController.InputManager.ActiveInput != InputType.Gamepad)
            {
                // If we are not on gamepad, we need to add listeners to the buttons
                infoButton.onClick.RemoveListener(OnInfoButtonClicked);
                upgradeButton.RemoveOnClick(OnUpgradeButtonClicked);

                infoButton.onClick.AddListener(OnInfoButtonClicked);
                upgradeButton.SetOnClick(OnUpgradeButtonClicked);
            }
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (!isActiveAndEnabled) return;
            if (newInput == InputType.Gamepad)
            {
                infoButton.onClick.RemoveListener(OnInfoButtonClicked);
                upgradeButton.RemoveOnClick(OnUpgradeButtonClicked);

                InitGamepadIndicator();
            }
            else
            {
                // If we are switching to keyboard/mouse, we need to re-add listeners
                infoButton.onClick.RemoveListener(OnInfoButtonClicked);
                upgradeButton.RemoveOnClick(OnUpgradeButtonClicked);

                infoButton.onClick.AddListener(OnInfoButtonClicked);
                upgradeButton.SetOnClick(OnUpgradeButtonClicked);

                authoredVisibility.SetVisible(gamepadIndicator, false);
            }
        }

        protected virtual void OnGamepadSouthButtonPressed(InputAction.CallbackContext ctx)
        {
            if (!isActiveAndEnabled) return;
            if (IsSelected)
            {
                if (buyOnGamepadButtonClick)
                {
                    if (UpgradeLevel < Data.LevelsCount && upgradeButton.HasEnoughMoney)
                    {
                        UIButtonAudio.Click(upgradeButton.Button);
                    }
                }
                else
                {
                    UIButtonAudio.Click(infoButton);
                }
            }
        }

        protected virtual void RedrawVisuals()
        {
            upgradeImage.sprite = Data.Icon;
            upgradeTitleText.text = Data.Title;

            UpgradeLevel = GameController.UpgradesManager.GetUpgradeLevel(Data.UpgradeType) + 1;

            if (UpgradeLevel >= Data.LevelsCount)
            {
                upgradeLevelText.text = "MAX";
            }
            else
            {
                upgradeLevelText.text = $"Lv. {UpgradeLevel + 1}";
                var currencyId = Data.GetLevel(UpgradeLevel).Price.CurrencyId;
                Currency = GameController.CurrenciesManager.GetCurrency(currencyId, false);
            }

            RedrawButton();

            InitGamepadIndicator();
        }

        protected virtual void InitGamepadIndicator()
        {
            if (!isActiveAndEnabled) return;
            bool visible = IsSelected && GameController.InputManager.ActiveInput == InputType.Gamepad &&
                (!buyOnGamepadButtonClick || Data != null && UpgradeLevel < Data.LevelsCount && upgradeButton.HasEnoughMoney);
            authoredVisibility.SetVisible(gamepadIndicator, visible);
        }

        protected virtual void RedrawButton()
        {
            if (UpgradeLevel >= Data.LevelsCount)
            {
                authoredVisibility.SetVisible(costLabel.gameObject, false);
                authoredVisibility.SetVisible(upgradedLabel, true);

                upgradeButton.DisableButton();
                buttonCanvasGroup.alpha = 0.5f;
            }
            else
            {
                authoredVisibility.SetVisible(costLabel.gameObject, true);
                authoredVisibility.SetVisible(upgradedLabel, false);

                var level = Data.GetLevel(UpgradeLevel);
                upgradeButton.SetCurrency(Currency);
                upgradeButton.SetCost(level.Price.Amount);

                if (upgradeButton.HasEnoughMoney)
                {
                    buttonCanvasGroup.alpha = 1f;
                }
                else
                {
                    buttonCanvasGroup.alpha = 0.75f;
                }
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            IsSelected = true;

            InitGamepadIndicator();

            onCardSelected?.Invoke(this);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            IsSelected = false;

            InitGamepadIndicator();
        }

        public virtual void Clear()
        {
            GameController.InputManager.InputAsset.UI.A.performed -= OnGamepadSouthButtonPressed;

            Data.onUpgradeLevelChanged -= OnUpgradeLevelChanged;

            IsSelected = false;

            authoredVisibility.SetVisible(gamepadIndicator, false);

            infoButton.onClick.RemoveListener(OnInfoButtonClicked);
            upgradeButton.RemoveOnClick(OnUpgradeButtonClicked);

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public virtual void OnUpgradeButtonClicked()
        {
            if (!isActiveAndEnabled) return;
            var level = Data.GetLevel(UpgradeLevel);

            upgradeButton.Currency.Withdraw(level.Price.Amount);

            GameController.UpgradesManager.IncrementUpgradeLevel(Data.UpgradeType);


        }

        protected virtual void OnUpgradeLevelChanged(int level)
        {
            UpgradeLevel = level + 1;
            if (!isActiveAndEnabled) return;
            RedrawVisuals();
        }

        public virtual void OnInfoButtonClicked()
        {
            if (!isActiveAndEnabled) return;
            GameController.MainMenuScreenBehavior.ExpandedUpgradePopup.Show(Data);
        }

        public virtual void SetNavigation(Selectable selectOnLeft, Selectable selectOnRight, Selectable selectOnUp, Selectable selectOnDown)
        {
            var navigation = this.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            navigation.selectOnRight = selectOnRight;
            navigation.selectOnLeft = selectOnLeft;
            navigation.selectOnUp = selectOnUp;
            navigation.selectOnDown = selectOnDown;

            this.navigation = navigation;
        }
    }
}
