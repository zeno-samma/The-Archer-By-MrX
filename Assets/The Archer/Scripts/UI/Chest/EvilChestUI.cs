using OctoberStudio.Abilities;
using OctoberStudio.Abilities.UI;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class EvilChestUI : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;

        [Header("Title")]
        [SerializeField] protected AnimatedTitleUI title;

        [Header("Ability")]
        [SerializeField] protected AbilityCardBehavior abilityCard;

        [Header("Description")]
        [SerializeField] protected TMP_Text descriptionText;
        [SerializeField] protected string descriptionTextFormat = "Take your reward but <color=#FA5152F)>lose</color> {0} hp";
        [SerializeField, Range(0, 1)] protected float maxHPLossProportion = 0.2f;

        [Header("Buttons")]
        [SerializeField] protected Button acceptButton;
        [SerializeField] protected Button rejectButton;

        [Header("Timings")]
        [SerializeField] protected float timescaleStopDuration = 0.25f;
        [SerializeField] protected float backgroundFadeInDuration = 0.25f;

        [Space]
        [SerializeField] protected float backgroundFadeHideDelay = 0.8f;
        [SerializeField] protected float backgroundFadeHideDuration = 0.3f;

        [SerializeField] protected float timescaleHideDuration = 0.25f;
        [SerializeField] protected float timescaleHideDelay = 0.8f;

        protected Vector2 titleLinePosition;

        protected UnityAction onClosed;

        protected AbilityData abilityData;

        public bool IsOpen { get; protected set; } = false;

        protected virtual void Awake()
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            rejectButton.onClick.AddListener(OnRejectButtonClicked);
        }

        public virtual void Show(AbilityData abilityData, UnityAction onClosed)
        {
            this.onClosed = onClosed;
            this.abilityData = abilityData;
            ResetUI();

            EasingManager.DoFloat(1, 0, timescaleStopDuration, SetTimeScale)
                .SetUnscaledTime(true);
            backgroundImage.DoAlpha(1, backgroundFadeInDuration)
                .SetUnscaledTime(true);

            title.Show();

            abilityCard.ShowHidden(0, false);
            abilityCard.CanvasGroup.DoAlpha(1, 0.2f).SetUnscaledTime(true);

            abilityCard.Init(abilityData, 0.5f);

            descriptionText.DoAlpha(1, 0.3f)
                .SetDelay(0.2f)
                .SetUnscaledTime(true);

            acceptButton.transform.DoLocalScale(Vector3.one, 0.3f)
                .SetDelay(0.6f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut)
                .SetOnFinish(() => acceptButton.enabled = true);

            rejectButton.transform.DoLocalScale(Vector3.one, 0.3f)
                .SetDelay(0.7f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut)
                .SetOnFinish(() =>
                {
                    rejectButton.enabled = true;
                    abilityCard.EnableButton();

                    if (GameController.InputManager.ActiveInput == Input.InputType.Gamepad)
                    {
                        EventSystem.current.SetSelectedGameObject(acceptButton.gameObject);
                    }
                });

            IsOpen = true;
        }

        protected virtual void Hide()
        {
            abilityCard.Hide(0, 0.3f);

            title.Hide();

            backgroundImage.DoAlpha(0, backgroundFadeHideDuration)
                .SetDelay(backgroundFadeHideDelay)
                .SetUnscaledTime(true)
                .SetOnFinish(OnBackgroundHidden);

            EasingManager.DoFloat(0, 1, timescaleHideDuration, SetTimeScale)
                .SetDelay(timescaleHideDelay)
                .SetUnscaledTime(true);

            acceptButton.transform.DoLocalScale(Vector3.zero, 0.3f)
                .SetDelay(0.0f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineIn);

            rejectButton.transform.DoLocalScale(Vector3.zero, 0.3f)
                .SetDelay(0.1f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineIn);

            descriptionText.DoAlpha(0, 0.3f)
                .SetUnscaledTime(true);

            acceptButton.enabled = false;
            rejectButton.enabled = false;
        }

        protected virtual void ResetUI()
        {
            gameObject.SetActive(true);

            backgroundImage.SetAlpha(0);

            title.ResetUI();

            descriptionText.alpha = 0;
            descriptionText.text = string.Format(descriptionTextFormat, Mathf.RoundToInt(StageController.Player.MaxHP * maxHPLossProportion));

            acceptButton.transform.localScale = Vector3.zero;
            rejectButton.transform.localScale = Vector3.zero;

            acceptButton.enabled = false;
            rejectButton.enabled = false;
        }

        protected virtual void SetTimeScale(float value)
        {
            if (value < 0) value = 0;
            Time.timeScale = value;
        }

        protected virtual void OnBackgroundHidden()
        {
            gameObject.SetActive(false);

            IsOpen = false;
            onClosed?.Invoke();
        }

        protected virtual void OnAcceptButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            if (StageController.AbilitiesManager.IsAbilityAquired(abilityData.AbilityType))
            {
                StageController.AbilitiesManager.IncreaseAbilityLevel(abilityData);
            }
            else
            {
                StageController.AbilitiesManager.AddAbility(abilityData);
            }

            var maxHpStat = StageController.Player.Stats.MaxHPStat;
            var hpLoss = maxHpStat.InitialValue * maxHPLossProportion;
            maxHpStat.ChangeInitialValue(maxHpStat.InitialValue - hpLoss);

            Hide();
        }

        protected virtual void OnRejectButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            Hide();
        }

        protected virtual void OnEnable()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnInputChanged(Input.InputType prevInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(acceptButton.gameObject);
            }
        }
    }
}