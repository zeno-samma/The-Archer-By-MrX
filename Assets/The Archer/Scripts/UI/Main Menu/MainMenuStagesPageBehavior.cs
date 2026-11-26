using MrX.Easing;
using MrX.Input;
using MrX.UI.Armory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class MainMenuStagesPageBehavior : MainMenuPageBehavior
    {
        [Space]
        [SerializeField] protected StageDatabase stageDatabase;
        [SerializeField] protected ArmoryHeroPanel heroPanel;
        [SerializeField] protected ArmoryTabsPanel tabsPanel;
        [SerializeField] protected ItemsGridBehavior itemsGrid;
        [SerializeField] protected HeroesGridBehavior heroesGrid;
        [SerializeField] protected ArmoryTabBehavior itemsTab;
        [SerializeField] protected ArmoryTabBehavior heroesTab;

        public StageDatabase StageDatabase => stageDatabase;

        [Header("Stage Data")]
        [SerializeField] protected MainMenuStageSelector stageSelector;
        [SerializeField] private GameObject difficultySelector;
        [SerializeField] private Button[] difficultyButtons;
        [SerializeField] private TMP_Text[] difficultyLabels;
        [SerializeField] private GameObject stageLockNotice;
        [SerializeField] private TMP_Text stageLockText;
        private StageDifficulty selectedDifficulty;
        [SerializeField] private Color difficultyColor = new Color(0.06f, 0.18f, 0.33f);
        [SerializeField] private Color selectedDifficultyColor = new Color(0.04f, 0.48f, 0.70f);

        [Header("Buttons")]
        [SerializeField] protected Button playButton;
        [SerializeField] protected RectTransform playButtonShineLine;
        [SerializeField] protected Sprite playButtonEnabledSprite;
        [SerializeField] protected Sprite playButtonDisabledSprite;

        [Space]
        [SerializeField] protected CostButtonBehavior purchaseButton;

        [Space]
        [SerializeField] protected Button settingsButton;
        // [SerializeField] protected GameObject settingsGamepadIndicator;

        [Space]
        [SerializeField] protected Button infoButton;
        [SerializeField] protected GameObject infoGamepadIndicator;

        [Space]
        [SerializeField] protected TMP_Text attemptsText;
        [SerializeField] protected string attemptsFormat = "Attempts Made {0}";

        public StageSave StageSave { get; protected set; }
        public ContinuePlayingSave ContinuePlayingSave { get; protected set; }

        protected PurchasedCondition PurchaseCondition { get; set; }

        protected override void Start()// bắt đầu khi trang được hiển thị
        {

            base.Start();

            InitSaves();

            CheckFirstStageUnlockStatus();
            int initialStageIndex = StageDifficultyTrial.GetInitialStageIndex(stageDatabase, StageSave);
            TryUnlockStages();

            StageSave.onSelectedStageChanged += InitStage;

            CheckContinuePlayingPopup();
            if (StageDifficultyTrial.IsActive && !ContinuePlayingSave.HasUnfinishedStageData)
                StageSave.SetSelectedStageId(initialStageIndex);

            OnMoveCenterFinished();

            stageSelector.Init(this);

            AddListenersToButtons();
            // heroPanel.gameObject.SetActive(false);
        }

        protected virtual void InitSaves()
        {
            StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
            // Migrate legacy runs before resetting the menu selection.
            if (ContinuePlayingSave.HasUnfinishedStageData && string.IsNullOrEmpty(ContinuePlayingSave.StageContentId))
                ContinuePlayingSave.RememberStage(StageDatabase.GetStageData(Mathf.Clamp(StageSave.SelectedStageId,0,StageDatabase.StagesAmount-1)).StageId);
            if (!StageDifficultyTrial.IsActive) StageSave.SetSelectedStageId(0);
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
                StageSave.SetSelectedStageId(0);

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
            for (int i = StageDatabase.StagesAmount - 1; i >= 0; i--)
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

                if (CheckStageUnlockStatus(stageData, i))
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

            if (stageData.UsesDifficulties)
            {
                var gate = stageData.GetDifficultyUnlock(stageIndex);
                if (gate == null || !gate.IsMet(stageDatabase, stageIndex, StageSave)) return false;
                StageSave.UnlockStage(stageData);
                return true;
            }

            bool hasRegularCondition = false;
            foreach (var condition in stageData.UnlockConditions)
            {
                if (condition == null || condition is ClearStageThroughDifficultyCondition) continue;
                hasRegularCondition = true;
                if (condition.IsMet(stageDatabase, stageIndex, StageSave))
                {
                    StageSave.UnlockStage(stageData);
                    return true;
                }
            }

            // If the stage does not have conditions, we use default PreviousStageCompletedCondition
            if (!hasRegularCondition)
            {
                if (new PreviousStageCompletedCondition().IsMet(stageDatabase, stageIndex, StageSave))
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
            }
            else
            {
                InitButtonsWithoutPurchaseButton(stageData);
            }

            InitAttemptsText(stageData);
            InitInfoButton(stageData);
            selectedDifficulty = StageDifficulty.Basic;
            if (stageData.UsesDifficulties && StageSave.IsStageUnlocked(stageData))
            {
                while (selectedDifficulty < StageDifficulty.Hard &&
                    StageSave.CanPlayDifficulty(stageData, selectedDifficulty + 1))
                    selectedDifficulty++;
            }

            RefreshStageLockNotice(stageData, stageId);
            RefreshDifficultySelector(stageData);
        }

        private void RefreshStageLockNotice(StageData stage, int stageIndex)
        {
            if (stageLockNotice == null) return;

            var gate = StageSave.IsStageUnlocked(stage) ? null : stage.GetDifficultyUnlock(stageIndex);
            if (gate != null)
                stageLockText.text = gate.GetDescription(stageDatabase, stageIndex);
            stageLockNotice.SetActive(gate != null);
        }

        public void SelectDifficulty(int value)
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!screen.IsDockEnabled || screen.CurrentPage != this) return;
            var stage = stageDatabase.GetStageData(StageSave.SelectedStageId);
            var difficulty = (StageDifficulty)value;
            if (!stage.UsesDifficulties || !StageSave.CanPlayDifficulty(stage, difficulty)) return;

            selectedDifficulty = difficulty;
            GameController.AudioManager.PlayButtonClick();
            RefreshDifficultySelector(stage);
        }

        private void RefreshDifficultySelector(StageData stage)
        {
            if (difficultySelector == null) return;
            bool canSelectDifficulty = stage.UsesDifficulties && StageSave.IsStageUnlocked(stage);
            difficultySelector.SetActive(canSelectDifficulty);
            if (!canSelectDifficulty) return;

            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                var difficulty = (StageDifficulty)i;
                bool available = StageSave.CanPlayDifficulty(stage, difficulty);
                bool cleared = StageSave.IsDifficultyCompleted(stage, difficulty);
                difficultyButtons[i].interactable = available;
                difficultyButtons[i].image.color = available && selectedDifficulty == difficulty
                    ? selectedDifficultyColor : difficultyColor;
                difficultyLabels[i].text = !available ? $"{difficulty}\nLocked" :
                    cleared ? $"{difficulty}\nCleared" : difficulty.ToString();
            }
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
            }
            else
            {
                infoButton.gameObject.SetActive(true);
                infoGamepadIndicator.gameObject.SetActive(GameController.InputManager.ActiveInput == InputType.Gamepad);
            }
        }

        protected virtual void InitButtonsWithPurchaseButton(StageData stageData)
        {
            if (!StageSave.IsStageUnlocked(stageData))
            {
                PurchaseCondition = stageData.UsesDifficulties ? null : stageData.GetPurchaseUnlockCondition();
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

            // Debug.Log("Click stage");
            if (!GameController.MainMenuScreenBehavior.ContinuePlayingPopup.IsOpen)
            {
                SubscribeToInputEvents();
                SelectPlayButton();
            }
            // Debug.Log("[STAGE] Init Hero Panel");
            heroPanel.Init();
            // itemsTab.Init(true);
            // heroesTab.Init(false);

            // heroPanel.Init();
            // itemsGrid.Init();
            // heroesGrid.Init();

            // itemsGrid.SpawnCenter();

            // itemsGrid.gameObject.SetActive(true);
            // heroesGrid.gameObject.SetActive(false);
        }

        public override void OnMoveFinished()
        {
            // Debug.Log("[STAGE] Clear Hero Panel");
            heroPanel.Clear();
            // heroPanel.Clear();
            // itemsGrid.Clear();
            // heroesGrid.Clear();
            // tabsPanel.Clear();
            base.OnMoveFinished();
            UnsubscribeFromInputEvents();
        }

        public virtual void OnSettingsPopupClosed()
        {
            GameController.MainMenuScreenBehavior.SettingsPopup.onPopupClosed -= OnSettingsPopupClosed;
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

        public void SetLanguageModalInput(bool blocked)
        {
            if (blocked) UnsubscribeFromInputEvents();
            else if (GameController.MainMenuScreenBehavior.CurrentPage == this) SubscribeToInputEvents();
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
            var screen = GameController.MainMenuScreenBehavior;
            if (!screen.IsDockEnabled || screen.CurrentPage != this) return;
            GameController.AudioManager.PlayButtonClick();
            var stage = StageDatabase.GetStageData(StageSave.SelectedStageId);
            if (!StageSave.CanPlayDifficulty(stage, selectedDifficulty)) return;
            if (!GameController.TryStartNewRun(stage, selectedDifficulty))
                GetComponentInChildren<MenuEconomyPanel>(true)?.ShowInsufficientEnergy();
        }

        protected virtual void OnPurchaseButtonClicked()
        {
            if (PurchaseCondition == null) return;
            var stageData = StageDatabase.GetStageData(StageSave.SelectedStageId);
            if (stageData.UsesDifficulties) return;

            PurchaseCondition.Currency.Withdraw(PurchaseCondition.Cost);

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

            // settingsGamepadIndicator.SetActive(inputType == InputType.Gamepad);

            if (infoButton != null && infoButton.gameObject.activeSelf)
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
