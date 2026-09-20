using OctoberStudio.Easing;
using OctoberStudio.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuStagesPageBehavior : MainMenuPageBehavior
    {
        [Space]
        [SerializeField] protected StageDatabase stageDatabase;
        public StageDatabase StageDatabase => stageDatabase;

        [Header("Stage Data")]
        [SerializeField] protected MainMenuStageSelector stageSelector;

        [Header("Buttons")]
        [SerializeField] protected Button playButton;
        [SerializeField] protected RectTransform playButtonShineLine;
        [SerializeField] protected Sprite playButtonEnabledSprite;
        [SerializeField] protected Sprite playButtonDisabledSprite;

        [Space]
        [SerializeField] protected CostButtonBehavior purchaseButton;

        [Space]
        [SerializeField] protected Button settingsButton;
        [SerializeField] protected GameObject settingsGamepadIndicator;

        [Space]
        [SerializeField] protected Button infoButton;
        [SerializeField] protected GameObject infoGamepadIndicator;

        [Space]
        [SerializeField] protected TMP_Text attemptsText;
        [SerializeField] protected string attemptsFormat = "Attempts Made {0}";

        public StageSave StageSave { get; protected set; }
        public ContinuePlayingSave ContinuePlayingSave { get; protected set; }

        protected PurchasedCondition PurchaseCondition { get; set; }

        protected override void Start()
        {
            base.Start();

            InitSaves();

            CheckFirstStageUnlockStatus();
            TryUnlockStages();

            StageSave.onSelectedStageChanged += InitStage;

            CheckContinuePlayingPopup();

            OnMoveCenterFinished();

            stageSelector.Init(this);

            AddListenersToButtons();
        }

        protected virtual void InitSaves()
        {
            StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
        }

        protected virtual void AddListenersToButtons()
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            playButton.onClick.AddListener(OnPlayerButtonClicked);

            if (purchaseButton != null)
            {
                purchaseButton.SetOnClick(OnPurchaseButtonClicked);
            }

            if (infoButton != null)
            {
                infoButton.onClick.AddListener(OnInfoButtonClicked);
            }
        }

        protected virtual void CheckContinuePlayingPopup()
        {
            if (!ContinuePlayingSave.HasUnfinishedStageData || ContinuePlayingSave.HP <= 0)
            {
                var lastUnlockedStageId = GetLastUnlockedStageId();
                if(lastUnlockedStageId >= StageDatabase.StagesAmount)
                {
                    lastUnlockedStageId = StageDatabase.StagesAmount - 1;
                }

                StageSave.SetSelectedStageId(lastUnlockedStageId);

                SubscribeToInputEvents();
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            }
            else
            {
                GameController.MainMenuScreenBehavior.ContinuePlayingPopup.Show();
                GameController.MainMenuScreenBehavior.ContinuePlayingPopup.onPopupClosed += OnContinuePlayingPopupClosed;

                GameController.MainMenuScreenBehavior.DisableDock();
            }
        }

        protected virtual void CheckFirstStageUnlockStatus()
        {
            var firstStage = StageDatabase.GetStageData(0);

            if (!StageSave.IsStageUnlocked(firstStage))
            {
                StageSave.UnlockStage(firstStage);
            }
        }

        protected virtual int GetLastUnlockedStageId()
        {
            for(int i = StageDatabase.StagesAmount - 1; i >= 0; i--)
            {
                var stageData = StageDatabase.GetStageData(i);
                if (StageSave.IsStageUnlocked(stageData)) return i;
            }

            return 0;
        }

        protected virtual void TryUnlockStages()
        {
            var unlocked = false;

            for (int i = 1; i < StageDatabase.StagesAmount; i++)
            {
                var stageData = StageDatabase.GetStageData(i);

                if(CheckStageUnlockStatus(stageData, i))
                {
                    unlocked = true;
                }
            }

            // Unlocking a stage might have fulfilled a condition for other stage
            if (unlocked) TryUnlockStages();
        }

        protected virtual bool CheckStageUnlockStatus(StageData stageData, int stageIndex)
        {
            if (StageSave.IsStageUnlocked(stageData)) return false;

            for(int i = 0; i < stageData.UnlockConditions.Count; i++)
            {
                var condition = stageData.UnlockConditions[i];

                if(condition.IsMet(stageDatabase, stageIndex, StageSave))
                {
                    StageSave.UnlockStage(stageData);
                    return true;
                }
            }

            // If the stage does not have conditions, we use default PreviousStageCompletedCondition
            if (stageData.UnlockConditions.Count == 0)
            {
                if(new PreviousStageCompletedCondition().IsMet(stageDatabase, stageIndex, StageSave))
                {
                    StageSave.UnlockStage(stageData);
                    return true;
                }
            }

            return false;
        }

        public virtual void SelectPlayButton()
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        public virtual void SelectPurchaseButton()
        {
            EventSystem.current.SetSelectedGameObject(purchaseButton.gameObject);
        }

        protected virtual void InitStage(int stageId)
        {
            var stageData = stageDatabase.GetStageData(stageId);
            if (stageData == null)
            {
                Debug.LogError($"Stage with ID {StageSave.SelectedStageId} not found in the database.");
                return;
            }

            if (purchaseButton != null)
            {
                InitButtonsWithPurchaseButton(stageData);
            } else
            {
                InitButtonsWithoutPurchaseButton(stageData);
            }

            InitAttemptsText(stageData);
            InitInfoButton(stageData);
        }

        protected virtual void InitAttemptsText(StageData stageData)
        {
            if (attemptsText == null) return;

            var attemptsCount = StageSave.GetStageAttempts(stageData);
            if (attemptsCount > 0)
            {
                attemptsText.gameObject.SetActive(true);
                attemptsText.text = string.Format(attemptsFormat, attemptsCount);
            }
            else
            {
                attemptsText.gameObject.SetActive(false);
            }
        }

        protected virtual void InitInfoButton(StageData stageData)
        {
            if (infoButton == null) return;

            if (StageSave.IsStageUnlocked(stageData))
            {
                infoButton.gameObject.SetActive(false);
            } else
            {
                infoButton.gameObject.SetActive(true);
                infoGamepadIndicator.gameObject.SetActive(GameController.InputManager.ActiveInput == InputType.Gamepad);
            }
        }

        protected virtual void InitButtonsWithPurchaseButton(StageData stageData)
        {
            if (!StageSave.IsStageUnlocked(stageData))
            {
                PurchaseCondition = stageData.GetPurchaseUnlockCondition();
                if (PurchaseCondition == null)
                {
                    purchaseButton.gameObject.SetActive(false);

                    playButton.gameObject.SetActive(true);
                    playButton.interactable = false;
                    playButton.image.sprite = playButtonDisabledSprite;
                }
                else
                {
                    purchaseButton.SetCurrency(PurchaseCondition.Currency);
                    purchaseButton.SetCost(PurchaseCondition.Cost);

                    playButton.gameObject.SetActive(false);

                    purchaseButton.gameObject.SetActive(true);

                    if (purchaseButton.ButtonEnabled)
                    {
                        SelectPurchaseButton();
                    }
                }
            }
            else
            {
                purchaseButton.gameObject.SetActive(false);

                playButton.gameObject.SetActive(true);
                playButton.interactable = true;
                playButton.image.sprite = playButtonEnabledSprite;

                SelectPlayButton();
            }
        }

        protected virtual void InitButtonsWithoutPurchaseButton(StageData stageData)
        {
            playButton.gameObject.SetActive(true);

            if (!StageSave.IsStageUnlocked(stageData))
            {
                playButton.interactable = false;
                playButton.image.sprite = playButtonDisabledSprite;
            }
            else
            {
                playButton.interactable = true;
                playButton.image.sprite = playButtonEnabledSprite;

                SelectPlayButton();
            }
        }

        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();

            if (!GameController.MainMenuScreenBehavior.ContinuePlayingPopup.IsOpen)
            {
                var linePosition = playButtonShineLine.anchoredPosition;
                var targetPosition = new Vector2(-linePosition.x, linePosition.y);

                playButtonShineLine.DoAnchorPosition(targetPosition, 0.5f).SetOnFinish(() => playButtonShineLine.anchoredPosition = linePosition);

                SubscribeToInputEvents();
                SelectPlayButton();
            }
        }

        public override void OnMoveFinished()
        {
            base.OnMoveFinished();

            UnsubscribeFromInputEvents();
        }

        public virtual void OnSettingsPopupClosed()
        {
            GameController.MainMenuScreenBehavior.SettingsPopup.onPopupClosed -= OnSettingsPopupClosed;
            SubscribeToInputEvents();

            GameController.MainMenuScreenBehavior.EnableDock();

            if(GameController.InputManager.ActiveInput == InputType.Gamepad)
            {
                if(purchaseButton != null && purchaseButton.gameObject.activeSelf)
                {
                    if (purchaseButton.ButtonEnabled) SelectPurchaseButton();
                } else
                {
                    if (playButton.interactable) SelectPlayButton();
                }
            }
        }

        public virtual void OnInfoPopupClosed()
        {
            GameController.MainMenuScreenBehavior.InfoPopup.onPopupClosed -= OnInfoPopupClosed;
            SubscribeToInputEvents();

            GameController.MainMenuScreenBehavior.EnableDock();

            if (GameController.InputManager.ActiveInput == InputType.Gamepad)
            {
                if (purchaseButton != null && purchaseButton.gameObject.activeSelf)
                {
                    if (purchaseButton.ButtonEnabled) SelectPurchaseButton();
                }
                else
                {
                    if (playButton.interactable) SelectPlayButton();
                }
            }
        }

        public virtual void OnContinuePlayingPopupClosed()
        {
            GameController.MainMenuScreenBehavior.ContinuePlayingPopup.onPopupClosed -= OnContinuePlayingPopupClosed;
            SubscribeToInputEvents();

            GameController.MainMenuScreenBehavior.EnableDock();

            SelectPlayButton();
        }

        protected virtual void SubscribeToInputEvents()
        {
            UnsubscribeFromInputEvents();

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
            GameController.InputManager.InputAsset.UI.Y.performed += OnInfoInputClicked;

            stageSelector.SubscribeToInputEvents();
        }

        protected virtual void UnsubscribeFromInputEvents()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
            GameController.InputManager.InputAsset.UI.Y.performed -= OnInfoInputClicked;

            stageSelector.UnsubscribeFromInputEvents();
        }

        protected virtual void OnPlayerButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            ContinuePlayingSave.Disable();
            GameController.CurrenciesManager.ApplyGameplayAmounts();

            StageSave.IncrementStageAttempts(StageDatabase.GetStageData(StageSave.SelectedStageId));

            GameController.LoadStage();
        }

        protected virtual void OnPurchaseButtonClicked()
        {
            if (PurchaseCondition == null) return;

            PurchaseCondition.Currency.Withdraw(PurchaseCondition.Cost);

            var stageData = StageDatabase.GetStageData(StageSave.SelectedStageId);
            StageSave.UnlockStage(stageData);

            purchaseButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.interactable = true;
            playButton.image.sprite = playButtonEnabledSprite;

            InitStage(StageSave.SelectedStageId);
            stageSelector.InitStage(StageSave.SelectedStageId);
        }

        protected virtual void OnSettingsInputClicked(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnSettingsButtonClicked();
        }

        protected virtual void OnSettingsButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            GameController.MainMenuScreenBehavior.SettingsPopup.Show();
            GameController.MainMenuScreenBehavior.SettingsPopup.onPopupClosed += OnSettingsPopupClosed;

            UnsubscribeFromInputEvents();

            GameController.MainMenuScreenBehavior.DisableDock();
        }

        protected virtual void OnInfoInputClicked(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnInfoButtonClicked();
        }

        protected virtual void OnInfoButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            GameController.MainMenuScreenBehavior.InfoPopup.Show();
            GameController.MainMenuScreenBehavior.InfoPopup.onPopupClosed += OnInfoPopupClosed;

            UnsubscribeFromInputEvents();

            GameController.MainMenuScreenBehavior.DisableDock();
        }

        protected virtual void OnInputChanged(InputType prevInputType, InputType inputType)
        {
            if (prevInputType == InputType.UIJoystick)
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            }

            settingsGamepadIndicator.SetActive(inputType == InputType.Gamepad);

            if(infoButton != null && infoButton.gameObject.activeSelf) 
            {
                infoGamepadIndicator.SetActive(inputType == InputType.Gamepad);
            }
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromInputEvents();

            StageSave.onSelectedStageChanged -= InitStage;
        }
    }
}