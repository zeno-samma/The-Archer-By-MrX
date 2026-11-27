using MrX.Abilities.UI;
using MrX.Easing;
using MrX.Enemy;
using MrX.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MrX.UI
{
    public class GameScreenBehavior : MonoBehaviour
    {
        protected Canvas canvas;

        [SerializeField] protected BossfightProgressbarBehavior bossProgressbar;
        [SerializeField] protected WorldSpaceTextManager worldSpaceTextManager;
        [SerializeField] protected AbilitySelectorBehavior abilitySelector;
        [SerializeField] protected PauseScreenBehavior pauseWindow;
        [SerializeField] protected WavePanelBehavior wavePanel;
        [Space]
        [SerializeField] protected Button pauseButton;
        [SerializeField] protected RectTransform goldIndicatorRect;

        [SerializeField] protected EvilChestUI evilChestUI;
        [SerializeField] protected HPChestUI hpChestUI;

        [SerializeField] protected StageObjectiveUI stageObjectiveUI;
        public StageObjectiveUI StageObjectiveUI => stageObjectiveUI;

        [Space]
        [SerializeField] protected StageFailedScreen stageFailedScreen;
        [SerializeField] protected StageCompleteScreen stageComleteScreen;

        [Space]
        [SerializeField] protected Image blackFadeImage;
        [SerializeField] private CanvasGroup lowHealthOverlay;
        [SerializeField, Range(0f, 1f)] private float lowHealthPeakOpacity = 0.09f;
        [SerializeField, Range(0f, 1f)] private float criticalHealthPeakOpacity = 0.22f;
        private float healthPulsePhase;

        [Space]
        [SerializeField] protected ExperienceUI experienceUI;
        [SerializeField] protected BossfightProgressbarBehavior bossfightProgressbar;

        public WorldSpaceTextManager WorldSpaceTextManager => worldSpaceTextManager;

        public BossfightProgressbarBehavior BossProgressbar => bossProgressbar;

        public AbilitySelectorBehavior AbiltiesSelector => abilitySelector;
        public WavePanelBehavior WavePanel => wavePanel;

        public EvilChestUI EvilChestUI => evilChestUI;
        public HPChestUI HPChestUI => hpChestUI;

        public StageFailedScreen StageFailedScreen => stageFailedScreen;
        public StageCompleteScreen StageCompleteScreen => stageComleteScreen;

        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();

            pauseButton.onClick.AddListener(OnPauseButtonClicked);

            pauseButton.transform.localScale = Vector3.zero;
            goldIndicatorRect.transform.localScale = Vector3.zero;
        }

        protected virtual void OnEnable()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsButtonClicked;
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsButtonClicked;
        }

        private void LateUpdate()
        {
            if (!lowHealthOverlay) return;

            var player = StageController.Player;
            float hp = player && player.IsPlaying ? player.NormalizedHP : 0f;
            if (hp <= 0f || hp > 0.20f || Time.timeScale <= 0f || StageController.IsResultFinalized ||
                !StageController.StageData.ShowLowHealthWarning || HasAnyOpenedPage())
            {
                lowHealthOverlay.alpha = 0f;
                healthPulsePhase = 0f;
                return;
            }

            float urgency = Mathf.InverseLerp(0.20f, 0.10f, hp);
            healthPulsePhase = Mathf.Repeat(healthPulsePhase + Time.deltaTime * Mathf.Lerp(0.9f, 1.4f, urgency), 1f);
            float pulse = 0.5f - 0.5f * Mathf.Cos(healthPulsePhase * 2f * Mathf.PI);
            float peak = Mathf.Lerp(lowHealthPeakOpacity, criticalHealthPeakOpacity, urgency);
            lowHealthOverlay.alpha = peak * Mathf.Lerp(0.22f, 1f, pulse);
        }

        public virtual void HideSideUI()
        {
            pauseButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);
            goldIndicatorRect.DoLocalScale(Vector3.zero, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);
        }

        public void PrepareForResult()
        {
            abilitySelector.CancelForResult();
            pauseWindow.gameObject.SetActive(false);
            evilChestUI.gameObject.SetActive(false); hpChestUI.gameObject.SetActive(false);
            HideSideUI();
        }

        public virtual void ShowSideUI()
        {
            pauseButton.transform.DoLocalScale(Vector3.one, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);
            goldIndicatorRect.DoLocalScale(Vector3.one, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);
        }

        protected virtual void OnSettingsButtonClicked(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (pauseWindow.IsOpen || abilitySelector.IsOpen || evilChestUI.IsOpen || hpChestUI.IsOpen || stageFailedScreen.IsOpen || stageComleteScreen.IsOpen) return;

            OnPauseButtonClicked();
        }

        protected virtual void Start()
        {
            StageController.Room.onBossfightStarted += OnBossfightStarted;

            abilitySelector.Init();
        }

        protected virtual void OnBossfightStarted(EnemyData bossData)
        {
            StageController.Room.onWaveEnded += OnBossfightEnded;

            experienceUI.Hide();
            bossfightProgressbar.Show(bossData.Name);
        }

        protected virtual void OnBossfightEnded()
        {
            StageController.Room.onWaveEnded -= OnBossfightEnded;

            bossfightProgressbar.Hide();
            experienceUI.Show();
        }

        protected virtual void OnPauseButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();
            pauseWindow.Show();
        }

        public virtual void FadeInOut(UnityAction hiddenAction, UnityAction finishAction)
        {
            blackFadeImage.gameObject.SetActive(true);
            blackFadeImage.SetAlpha(0);

            blackFadeImage.DoAlpha(1, 0.3f).SetOnFinish(() =>
            {
                hiddenAction?.Invoke();

                blackFadeImage.DoAlpha(0, 0.3f).SetOnFinish(() =>
                {
                    finishAction?.Invoke();

                    blackFadeImage.gameObject.SetActive(false);
                });
            });
        }

        public virtual void FadeOut()
        {
            blackFadeImage.gameObject.SetActive(true);
            blackFadeImage.SetAlpha(1);
            blackFadeImage.DoAlpha(0, 0.2f).SetOnFinish(() => blackFadeImage.gameObject.SetActive(false));
        }

        public virtual bool HasAnyOpenedPage()
        {
            return pauseWindow.IsOpen || abilitySelector.IsOpen || evilChestUI.IsOpen || hpChestUI.IsOpen || stageFailedScreen.IsOpen || stageComleteScreen.IsOpen;
        }
    }
}
