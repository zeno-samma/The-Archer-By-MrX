using MrX.Easing;
using MrX.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class PauseScreenBehavior : MonoBehaviour
    {
        [SerializeField] protected Image backgroundImage;

        [Header("Title")]
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected float titleRectScaleDuration = 0.3f;
        [SerializeField] protected float titleRectScaleDelay = 0.1f;
        [SerializeField] protected AnimationCurve titleRectScaleEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Abilities")]
        [SerializeField] protected AbilitiesGridBehavior abilitiesGrid;
        [SerializeField] protected RectTransform abilitiesRect;
        [SerializeField] protected float abilitiesRectScaleDuration = 0.3f;
        [SerializeField] protected float abilitiesRectScaleDelay = 0.2f;
        [SerializeField] protected AnimationCurve abilitiesRectScaleEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Settings")]
        [SerializeField] protected ToggleBehavior soundToggle;
        [SerializeField] protected ToggleBehavior musicToggle;
        [SerializeField] protected ToggleBehavior vibrationToggle;

        [Space]
        [SerializeField] protected RectTransform settingsRect;
        [SerializeField] protected float settingsRectScaleDuration = 0.3f;
        [SerializeField] protected float settingsRectScaleDelay = 0.3f;
        [SerializeField] protected AnimationCurve settingsRectScaleEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Buttons")]
        [SerializeField] protected RectTransform buttonsRect;
        [SerializeField] protected float buttonsRectScaleDuration = 0.3f;
        [SerializeField] protected float buttonsRectScaleDelay = 0.4f;
        [SerializeField] protected AnimationCurve buttonsRectScaleEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Space]
        [SerializeField] protected Button resumeButton;
        [SerializeField] protected RectTransform resumeButtonShineRect;
        [SerializeField] protected float resumeButtonShineDuration = 0.5f;
        [SerializeField] protected float resumeButtonShineDelay = 0.1f;

        [Space]
        [SerializeField] protected Button quitButton;
        [SerializeField] protected RectTransform quitButtonShineRect;
        [SerializeField] protected float quitButtonShineDuration = 0.5f;
        [SerializeField] protected float quitButtonShineDelay = 0.1f;

        [Header("Run Rewards")]
        [SerializeField] private GridLayoutGroup runRewards;
        [SerializeField] private TMPro.TMP_Text experienceAmount;
        [SerializeField] private SmallRewardCard goldReward;
        [SerializeField] private TMPro.TMP_Text goldAmount;
        [SerializeField] private SmallItemRewardCard itemRewardPrefab;
        private readonly System.Collections.Generic.List<SmallItemRewardCard> itemRewardCards =
            new System.Collections.Generic.List<SmallItemRewardCard>();

        protected float backgroundAlpha;

        public bool IsOpen { get; protected set; } = false;
        protected bool IsShowAnimationEnded { get; set; } = false;
        public UnityAction onClosed;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        protected Vector2 resumeShineRectPosition;
        protected Vector2 quitShineRectPosition;

        protected virtual void Awake()
        {
            backgroundAlpha = backgroundImage.color.a;

            resumeButton.onClick.AddListener(OnResumeButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);

            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            resumeShineRectPosition = resumeButtonShineRect.anchoredPosition;
            quitShineRectPosition = quitButtonShineRect.anchoredPosition;
        }

        protected virtual void OnEnable()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsGamepadButtonPressed;
            GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancelGamepadButtonPerformed;
            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        protected virtual void OnInputChanged(Input.InputType prevInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(soundToggle.gameObject);
            }
        }

        protected virtual void OnSettingsGamepadButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (IsShowAnimationEnded)
            {
                OnResumeButtonClicked();
            }
        }

        protected virtual void OnCancelGamepadButtonPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (IsShowAnimationEnded)
            {
                OnResumeButtonClicked();
            }
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsGamepadButtonPressed;
            GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancelGamepadButtonPerformed;
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void Start()
        {
            soundToggle.SetToggle(GameController.AudioManager.SoundVolume != 0);
            musicToggle.SetToggle(GameController.AudioManager.MusicVolume != 0);
            vibrationToggle.SetToggle(GameController.VibrationManager.IsVibrationEnabled);

            soundToggle.onChanged += OnSoundToggleChanged;
            musicToggle.onChanged += OnMusicToggleChanged;
            vibrationToggle.onChanged += OnVibrationToggleChanged;
        }

        public virtual void Show()
        {
            IsShowAnimationEnded = false;

            gameObject.SetActive(true);

            quitButton.enabled = false;
            resumeButton.enabled = false;

            EasingManager.DoTimeScale(0, 0.25f);

            backgroundImage.SetAlpha(0);
            backgroundImage.DoAlpha(backgroundAlpha, 0.25f)
                .SetUnscaledTime(true);

            titleRect.localScale = Vector3.zero;
            titleRect.DoLocalScale(Vector3.one, titleRectScaleDuration)
                .SetDelay(titleRectScaleDelay)
                .SetUnscaledTime(true)
                .SetEasingCurve(titleRectScaleEasing);

            abilitiesRect.localScale = Vector3.zero;
            abilitiesRect.DoLocalScale(Vector3.one, abilitiesRectScaleDuration)
                .SetDelay(abilitiesRectScaleDelay)
                .SetUnscaledTime(true)
                .SetEasingCurve(abilitiesRectScaleEasing);

            abilitiesGrid.Show();
            StageController.PauseResultStatistics();
            RefreshRunRewards();

            settingsRect.localScale = Vector3.zero;
            settingsRect.DoLocalScale(Vector3.one, settingsRectScaleDuration)
                .SetDelay(settingsRectScaleDelay)
                .SetUnscaledTime(true)
                .SetEasingCurve(settingsRectScaleEasing);

            buttonsRect.localScale = Vector3.zero;
            buttonsRect.DoLocalScale(Vector3.one, buttonsRectScaleDuration)
                .SetDelay(buttonsRectScaleDelay)
                .SetUnscaledTime(true)
                .SetEasingCurve(buttonsRectScaleEasing)
                .SetOnFinish(() =>
                {
                    EventSystem.current.SetSelectedGameObject(soundToggle.gameObject);
                    IsShowAnimationEnded = true;
                });

            resumeButtonShineRect.anchoredPosition = resumeShineRectPosition;
            resumeButtonShineRect.DoAnchorPosition(resumeShineRectPosition * new Vector3(-1, 1, 1), resumeButtonShineDuration)
                .SetDelay(resumeButtonShineDelay)
                .SetUnscaledTime(true)
                .SetOnFinish(() =>
                {
                    resumeButtonShineRect.anchoredPosition = resumeShineRectPosition;
                    resumeButton.enabled = true;
                });

            quitButtonShineRect.anchoredPosition = quitShineRectPosition;
            quitButtonShineRect.DoAnchorPosition(quitShineRectPosition * new Vector3(-1, 1, 1), quitButtonShineDuration)
                .SetDelay(quitButtonShineDelay)
                .SetUnscaledTime(true)
                .SetOnFinish(() =>
                {
                    quitButtonShineRect.anchoredPosition = quitShineRectPosition;
                    quitButton.enabled = true;
                });

            IsOpen = true;

            EventSystem.current.SetSelectedGameObject(null);
        }

        private void RefreshRunRewards()
        {
            if (runRewards == null) return;

            experienceAmount.text = $"+{Progression.PlayerProgression.PendingAwardXp:N0}";
            var gold = GameController.CurrenciesManager.GetCurrency("gold", true);
            if (gold != null) goldReward.SetIcon(gold.Data.Icon);
            goldAmount.text = (gold?.Amount ?? 0).ToString("N0");

            var counts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var drop in StageController.DropManager.PickedUpItems)
            {
                counts.TryGetValue(drop.ItemId, out int count);
                counts[drop.ItemId] = count + 1;
            }

            int used = 0;
            foreach (var entry in counts)
            {
                var item = GameController.ArmoryManager.GetItemData(entry.Key);
                if (item == null) continue;
                ShowItemReward(used++, new Rewards.LuckyGrant
                {
                    kind = Rewards.LuckyRewardKind.Equipment,
                    itemId = item.Id,
                    title = item.ItemName,
                    amount = entry.Value
                });
            }

            var bossChests = GameController.ArmoryManager.BossChests;
            if (bossChests != null)
                foreach (var grant in bossChests.GetCollected(ContinuePlayingSave.RunToken))
                    ShowItemReward(used++, grant);

            for (int i = used; i < itemRewardCards.Count; i++)
                itemRewardCards[i].gameObject.SetActive(false);

            var rect = (RectTransform)runRewards.transform;
            int columns = Mathf.Min(7, used + 2);
            int rows = Mathf.CeilToInt((used + 2) / (float)columns);
            float width = (rect.rect.width - (columns - 1) * runRewards.spacing.x) / columns;
            float height = (rect.rect.height - (rows - 1) * runRewards.spacing.y) / rows;
            float cellSize = Mathf.Max(1, Mathf.Min(130, width, height));
            runRewards.constraintCount = columns;
            runRewards.cellSize = new Vector2(cellSize, cellSize);
            float horizontalRoom = rect.rect.width - runRewards.padding.horizontal
                - columns * cellSize - (columns - 1) * runRewards.spacing.x;
            float verticalRoom = rect.rect.height - runRewards.padding.vertical
                - rows * cellSize - (rows - 1) * runRewards.spacing.y;
            float extraSize = Mathf.Min(cellSize * 0.5f, runRewards.spacing.x, horizontalRoom, verticalRoom);
            if (rows > 1)
                extraSize = Mathf.Min(extraSize, runRewards.spacing.y);
            var scale = Vector3.one * (1f + Mathf.Max(0f, extraSize) / cellSize);
            foreach (Transform reward in runRewards.transform)
                if (reward.gameObject.activeSelf)
                    reward.localScale = scale;

        }

        private void ShowItemReward(int index, Rewards.LuckyGrant grant)
        {
            if (index == itemRewardCards.Count)
                itemRewardCards.Add(Instantiate(itemRewardPrefab, runRewards.transform));
            var card = itemRewardCards[index];
            card.SetReward(grant);
            card.gameObject.SetActive(true);
        }

        protected virtual void OnSoundToggleChanged(bool soundEnabled)
        {
            GameController.AudioManager.SoundVolume = soundEnabled ? 1 : 0;
        }

        protected virtual void OnMusicToggleChanged(bool musicEnabled)
        {
            GameController.AudioManager.MusicVolume = musicEnabled ? 1 : 0;
        }

        protected virtual void OnVibrationToggleChanged(bool vibrationEnabled)
        {
            GameController.VibrationManager.IsVibrationEnabled = vibrationEnabled;
        }

        protected virtual void OnResumeButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            StageController.ResumeResultStatistics();
            EasingManager.DoTimeScale(1, 0.25f);
            gameObject.SetActive(false);

            IsOpen = false;

            onClosed?.Invoke();
        }

        protected virtual void OnQuitButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            Time.timeScale = 1f;

            gameObject.SetActive(false);

            StageController.ForceFail();
        }
    }
}