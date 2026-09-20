using OctoberStudio.Abilities;
using OctoberStudio.Abilities.UI;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class HPChestUI : MonoBehaviour
    {
        [Space]
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;

        [Header("Ability")]
        [SerializeField] protected AbilityCardBehavior firstAbilityCard;
        [SerializeField] protected AbilityCardBehavior secondAbilityCard;

        [Header("Title")]
        [SerializeField] protected AnimatedTitleUI title;

        [Header("Timings")]
        [SerializeField] protected float timescaleStopDuration = 0.25f;
        [SerializeField] protected float backgroundFadeInDuration = 0.25f;

        [Space]
        [SerializeField] protected float backgroundFadeHideDelay = 0.8f;
        [SerializeField] protected float backgroundFadeHideDuration = 0.3f;

        [SerializeField] protected float timescaleHideDuration = 0.25f;
        [SerializeField] protected float timescaleHideDelay = 0.8f;

        protected UnityAction onClosed;
        public bool IsOpen { get; protected set; }

        protected virtual void Awake()
        {
            firstAbilityCard.onAbilitySelected += OnAbilityCardClicked;
            secondAbilityCard.onAbilitySelected += OnAbilityCardClicked;
        }

        public virtual void Show(AbilityData firstAbilityData, AbilityData secondAbilityData, UnityAction onClosed)
        {
            this.onClosed = onClosed;

            ResetUI();

            EasingManager.DoTimeScale(0, timescaleStopDuration);
            backgroundImage.DoAlpha(1, backgroundFadeInDuration)
                .SetUnscaledTime(true);

            title.Show();

            firstAbilityCard.gameObject.SetActive(true);
            firstAbilityCard.ShowHidden(0, false);
            firstAbilityCard.CanvasGroup.DoAlpha(1, 0.2f).SetDelay(0.3f).SetUnscaledTime(true);

            firstAbilityCard.Init(firstAbilityData, 0.5f, 0.3f);

            secondAbilityCard.gameObject.SetActive(true);
            secondAbilityCard.ShowHidden(0, false);
            secondAbilityCard.CanvasGroup.DoAlpha(1, 0.2f).SetDelay(0.45f).SetUnscaledTime(true);

            secondAbilityCard.Init(secondAbilityData, 0.5f, 0.45f);

            IsOpen = true;

            EasingManager.DoAfter(AllCardsOpened).SetOnFinish(() =>
            {
                firstAbilityCard.EnableButton();
                secondAbilityCard.EnableButton();

                if (GameController.InputManager.ActiveInput == Input.InputType.Gamepad)
                {
                    EventSystem.current.SetSelectedGameObject(firstAbilityCard.Selectable.gameObject);
                }
            });
        }

        protected virtual bool AllCardsOpened()
        {
            return !firstAbilityCard.IsAnimationActive && !secondAbilityCard.IsAnimationActive;
        }

        protected virtual void Hide()
        {
            firstAbilityCard.Hide(0, 0.3f);
            secondAbilityCard.Hide(0, 0.3f);

            title.Hide();

            backgroundImage.DoAlpha(0, backgroundFadeHideDuration)
                .SetDelay(backgroundFadeHideDelay)
                .SetUnscaledTime(true)
                .SetOnFinish(OnBackgroundHidden);

            EasingManager.DoTimeScale(1, timescaleHideDuration).SetDelay(timescaleHideDelay);
        }

        protected virtual void ResetUI()
        {
            gameObject.SetActive(true);

            backgroundImage.SetAlpha(0);

            title.ResetUI();
        }

        protected virtual void OnBackgroundHidden()
        {
            gameObject.SetActive(false);

            IsOpen = false;
            onClosed?.Invoke();
        }

        protected virtual void OnAbilityCardClicked(AbilityData abilityData)
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
                EventSystem.current.SetSelectedGameObject(firstAbilityCard.Selectable.gameObject);
            }
        }
    }
}