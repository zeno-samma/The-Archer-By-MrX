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
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();
        protected Canvas canvas;
        private IEasingCoroutine transitionFade;

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
            animationDefaults.Capture(pauseButton.transform, goldIndicatorRect);
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
            transitionFade.StopIfExists();
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsButtonClicked;
        }

        public virtual void HideSideUI()
        {
            pauseButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);
            goldIndicatorRect.DoLocalScale(Vector3.zero, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);
        }

        public void PrepareForResult()
        {
            transitionFade.StopIfExists();
            blackFadeImage.gameObject.SetActive(false);
            abilitySelector.CancelForResult();
            pauseWindow.gameObject.SetActive(false);
            evilChestUI.gameObject.SetActive(false); hpChestUI.gameObject.SetActive(false);
            HideSideUI();
        }

        public virtual void ShowSideUI()
        {
            pauseButton.transform.DoLocalScale(animationDefaults.Scale(pauseButton.transform), 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);
            goldIndicatorRect.DoLocalScale(animationDefaults.Scale(goldIndicatorRect), 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);
        }

        protected virtual void OnSettingsButtonClicked(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (pauseWindow.IsOpen || abilitySelector.IsOpen || evilChestUI.IsOpen || hpChestUI.IsOpen || stageFailedScreen.IsOpen || stageComleteScreen.IsOpen) return;

            UIButtonAudio.Click(pauseButton);
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

            pauseWindow.Show();
        }

        public virtual void FadeInOut(UnityAction hiddenAction, UnityAction finishAction)
        {
            if(StageController.IsResultFinalized)return;
            transitionFade.StopIfExists();
            blackFadeImage.gameObject.SetActive(true);
            blackFadeImage.SetAlpha(0);

            transitionFade = blackFadeImage.DoAlpha(1, 0.3f).SetUnscaledTime(true).SetOnFinish(() =>
            {
                hiddenAction?.Invoke();
                if(StageController.IsResultFinalized)return;

                transitionFade = blackFadeImage.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() =>
                {
                    finishAction?.Invoke();

                    blackFadeImage.gameObject.SetActive(false);
                });
            });
        }

        public virtual void FadeOut()
        {
            if(StageController.IsResultFinalized)return;
            transitionFade.StopIfExists();
            blackFadeImage.gameObject.SetActive(true);
            blackFadeImage.SetAlpha(1);
            transitionFade = blackFadeImage.DoAlpha(0, 0.2f).SetUnscaledTime(true).SetOnFinish(() => blackFadeImage.gameObject.SetActive(false));
        }

        public virtual bool HasAnyOpenedPage()
        {
            return pauseWindow.IsOpen || abilitySelector.IsOpen || evilChestUI.IsOpen || hpChestUI.IsOpen || stageFailedScreen.IsOpen || stageComleteScreen.IsOpen;
        }
    }
}
