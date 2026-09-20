using OctoberStudio.Input;
using OctoberStudio.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OctoberStudio.UI
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

        protected override void Awake()
        {
            base.Awake();

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

                if (gamepadIndicator.activeSelf) gamepadIndicator.SetActive(false);
            }
        }

        protected virtual void OnGamepadSouthButtonPressed(InputAction.CallbackContext ctx)
        {
            if (IsSelected)
            {
                if (buyOnGamepadButtonClick)
                {
                    if (UpgradeLevel < Data.LevelsCount && upgradeButton.HasEnoughMoney)
                    {
                        OnUpgradeButtonClicked();
                    }
                }
                else
                {
                    OnInfoButtonClicked();
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
                upgradeLevelText.text = $"LVL {UpgradeLevel + 1}";
                var currencyId = Data.GetLevel(UpgradeLevel).Price.CurrencyId;
                Currency = GameController.CurrenciesManager.GetCurrency(currencyId, false);
            }

            RedrawButton();

            InitGamepadIndicator();
        }

        protected virtual void InitGamepadIndicator()
        {
            if (IsSelected && GameController.InputManager.ActiveInput == InputType.Gamepad)
            {
                if (buyOnGamepadButtonClick)
                {
                    if (UpgradeLevel < Data.LevelsCount && upgradeButton.HasEnoughMoney)
                    {
                        gamepadIndicator.SetActive(true);
                    }
                }
                else
                {
                    gamepadIndicator.SetActive(true);
                }
            }
            else
            {
                gamepadIndicator.SetActive(false);
            }
        }

        protected virtual void RedrawButton()
        {
            if (UpgradeLevel >= Data.LevelsCount)
            {
                costLabel.gameObject.SetActive(false);
                upgradedLabel.gameObject.SetActive(true);

                upgradeButton.DisableButton();
                buttonCanvasGroup.alpha = 0.5f;
            }
            else
            {
                costLabel.gameObject.SetActive(true);
                upgradedLabel.gameObject.SetActive(false);

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

            gamepadIndicator.SetActive(false);

            infoButton.onClick.RemoveListener(OnInfoButtonClicked);
            upgradeButton.RemoveOnClick(OnUpgradeButtonClicked);

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public virtual void OnUpgradeButtonClicked()
        {
            var level = Data.GetLevel(UpgradeLevel);

            upgradeButton.Currency.Withdraw(level.Price.Amount);

            GameController.UpgradesManager.IncrementUpgradeLevel(Data.UpgradeType);

            GameController.AudioManager.PlayButtonClick();
        }

        protected virtual void OnUpgradeLevelChanged(int level)
        {
            UpgradeLevel = level + 1;
            RedrawVisuals();
        }

        public virtual void OnInfoButtonClicked()
        {
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