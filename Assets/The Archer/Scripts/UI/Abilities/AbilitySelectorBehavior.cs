using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.Abilities.UI
{
    public class AbilitySelectorBehavior : MonoBehaviour
    {
        [SerializeField] protected AbilitiesDatabase abilitiesDatabase;

        [Space]
        [SerializeField] protected Canvas canvas;

        [Space]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Button skipButton;
        [SerializeField] protected CostButtonBehavior rerollButton;
        [SerializeField] protected RectTransform goldIndicator;

        [Space]
        [SerializeField] AnimatedTitleUI title;
        [Space]

        [SerializeField] protected CanvasGroup rarityCanvasGroup;
        [SerializeField] protected Image rarityBackgroundImage;
        [SerializeField] protected RectTransform rarityRect;
        [SerializeField] protected TMP_Text rarityText;
        [SerializeField] protected AnimationCurve rarityScaleCurve;

        [Space]
        [SerializeField] protected GameObject abilitySlotPrefab;
        [SerializeField] protected Transform slotsHolder;

        [Space]
        [SerializeField] protected List<RarityData> rarityData;

        [Header("Animation Settings")]
        [SerializeField] protected float timescaleStopDuration = 0.25f;
        [SerializeField] protected float backgroundFadeInDuration = 0.25f;

        [Space]
        [SerializeField] protected float slotInitialDelay = 0.6f;
        [SerializeField] protected float slotStepDelay = 0.13f;

        [Space]
        [SerializeField] protected float flashInitialDelay = 1.5f;
        [SerializeField] protected float flashStepDelay = 1f;

        [SerializeField] protected float stopScrollingDelay = 1f;
        [SerializeField] protected float stopScrollingStepDelay = 0.5f;

        [Space]
        [SerializeField] protected float rarityScaleInDuration = 0.1f;
        [SerializeField] protected float rarityScaleOutDuration = 0.3f;
        [SerializeField] protected float rarityFlashScale = 1.5f;
        [SerializeField] protected float rarityAnimationScale = 1.2f;

        [Header("Hide Animation Settings")]
        [SerializeField] protected float rarityScaleHideDelay = 0.6f;
        [SerializeField] protected float rarityScaleHideDuration = 0.5f;

        [SerializeField] protected float backgroundFadeHideDelay = 0.8f;
        [SerializeField] protected float backgroundFadeHideDuration = 0.3f;

        [SerializeField] protected float timescaleHideDuration = 0.25f;
        [SerializeField] protected float timescaleHideDelay = 0.8f;

        [Header("Audio")]
        [SerializeField] protected AudioData popupSound;
        [SerializeField] protected AudioData rarityJumpSound;
        [SerializeField] protected AudioData ambientSound;
        [SerializeField] protected float ambientSoundFadeOutDuration;
        [SerializeField] protected float ambientSoundStartDelay;
        [Tooltip("Ambient Sound will stop playing when the stots are starting stopping + this delay")]
        [SerializeField] protected float ambiendSoundEndDelay;

        protected IEasingCoroutine ambientSoundDelayEasingCoroutine;
        protected AudioSource ambientSoundAudioSource;

        protected PoolComponent<AbilitySelectorSlotBehavior> slotsPool;

        public bool IsOpen { get; protected set; } = false;

        protected List<AbilitySelectorSlotBehavior> slots = new List<AbilitySelectorSlotBehavior>();

        protected IEasingCoroutine timeScaleEasingCoroutine;
        protected IEasingCoroutine backgroundAlphaEasingCoroutine;

        protected IEasingCoroutine rarityAlphaEasingCoroutine;
        protected IEasingCoroutine rarityScaleEasingCoroutine;

        protected Coroutine showCoroutine;
        protected AbilityRarity Rarity { get; set; }

        public UnityAction onClosed;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public virtual void Init()
        {
            slotsPool = new PoolComponent<AbilitySelectorSlotBehavior>(abilitySlotPrefab, 3, slotsHolder, true);
            skipButton.onClick.AddListener(OnSkipButtonClicked);

            StageController.ExperienceManager.onXpLevelChanged += OnXpLevelChanged;
        }

        protected virtual void Start()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            rerollButton.SetOnClick(OnRerollButtonClicked);
            
            ResetUI();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);

            title.RestoreText();

            var selectorAbilities = StageController.AbilitiesManager.GetAbilitiesForSelector();

            if (selectorAbilities.Count > 0)
            {
                showCoroutine = StartCoroutine(ShowCoroutine(selectorAbilities, selectorAbilities[0].Rarity));
            }
        }

        public virtual void Show(AbilityRarity rarity, string titleText, UnityAction onClosed)
        {
            gameObject.SetActive(true);

            title.SetText(titleText);

            var selectorAbilities = StageController.AbilitiesManager.GetAbilitiesForSelector(rarity);
            if (selectorAbilities.Count > 0)
            {
                showCoroutine = StartCoroutine(ShowCoroutine(selectorAbilities, selectorAbilities[0].Rarity));
            }

            this.onClosed = onClosed;
        }

        protected virtual void OnXpLevelChanged(int level)
        {
            gameObject.SetActive(true);
            if (IsOpen)
            {
                StartCoroutine(WaitForSelectorToHide(Open));
            }
            else
            {
                StartCoroutine(WaitForDrop(Open));
            }

        }

        protected virtual IEnumerator WaitForSelectorToHide(UnityAction action)
        {
            yield return new WaitUntil(() => !IsOpen);

            action?.Invoke();
        }

        protected virtual IEnumerator WaitForDrop(UnityAction action)
        {
            if (!StageController.DropManager.AllWaveDropPickedUp())
            {
                yield return new WaitUntil(() => StageController.DropManager.AllWaveDropPickedUp());
            }

            if (StageController.DropManager.HasAliveIndicators)
            {
                yield return new WaitUntil(() => !StageController.DropManager.HasAliveIndicators);
            }

            action?.Invoke();
        }

        public virtual IEnumerator ShowCoroutine(List<AbilityData> abilities, AbilityRarity rarity)
        {
            IsOpen = true;
            Rarity = rarity;

            StageController.GameScreen.HideSideUI();

            InitAudio();
            ResetUI();

            timeScaleEasingCoroutine = EasingManager.DoTimeScale(0, timescaleStopDuration);

            backgroundAlphaEasingCoroutine =
                backgroundImage.DoAlpha(1, backgroundFadeInDuration)
                .SetUnscaledTime(true);

            title.Show();

            yield return new WaitForSecondsRealtime(slotInitialDelay);

            InitSlots(abilities);

            skipButton.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(flashInitialDelay);

            rarityAlphaEasingCoroutine =
                rarityCanvasGroup.DoAlpha(1, 0.5f)
                .SetUnscaledTime(true);

            var rarityIndex = (int)rarity;
            for (int i = 0; i <= rarityIndex; i++)
            {
                var data = rarityData[i];

                DoRarityFlash(data);

                if (i != rarityIndex) yield return new WaitForSecondsRealtime(flashStepDelay);
            }

            yield return new WaitForSecondsRealtime(stopScrollingDelay);
            yield return CompleteShowCoroutine();
        }

        protected virtual IEnumerator CompleteShowCoroutine()
        {
            yield return new WaitUntil(CardsInRightOrder);

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.StopScrolling();
            }

            skipButton.gameObject.SetActive(false);

            ambientSoundDelayEasingCoroutine.StopIfExists();
            EasingManager.DoAfter(ambiendSoundEndDelay, StopAmbientSound).SetUnscaledTime(true);

            yield return new WaitUntil(AllCardsEndedAnimations);

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.MainCard.onAbilitySelected += OnAbilitySelected;
                slot.MainCard.EnableButton();
            }

            InitRerollButton();
            InitNavigation();

            rarityScaleEasingCoroutine =
                rarityRect.DoLocalScale(Vector3.one, 0.25f)
                .SetEasing(EasingType.SineInOut)
                .SetUnscaledTime(true);

            showCoroutine = null;

            rerollButton.ButtonEnabled = false;
            rerollButton.transform
                .DoLocalScale(Vector3.one, 0.3f)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.CubicOut)
                .SetOnFinish(() => rerollButton.RecalculateVisuals());

            goldIndicator.transform.DoLocalScale(Vector3.one, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);

            ambientSoundDelayEasingCoroutine.StopIfExists();
            StopAmbientSound();
        }

        protected virtual void OnSkipButtonClicked()
        {
            skipButton.gameObject.SetActive(false);

            DisableCoroutines();

            Time.timeScale = 0f;
            backgroundImage.SetAlpha(1);

            title.Skip();

            var data = rarityData[(int)Rarity];

            InitRarityData(data);

            rarityCanvasGroup.alpha = 1;
            rarityRect.localScale = Vector3.one;

            StartCoroutine(CompleteShowCoroutine());
        }

        protected virtual void OnRerollButtonClicked()
        {
            if (rerollButton.WithdrawCost())
            {
                GameController.AudioManager.PlayButtonClick();

                EventSystem.current.SetSelectedGameObject(null);

                var newAbilities = StageController.AbilitiesManager.GetAbilitiesForSelector(Rarity);

                for (int i = 0; i < slots.Count; i++)
                {
                    var card = slots[i].MainCard;
                    var ability = i >= newAbilities.Count ? null : newAbilities[i];

                    card.ChangeAbility(ability, i * 0.3f);
                }

                rerollButton.ButtonEnabled = false;
                rerollButton.transform.DoLocalScale(new Vector3(0, 0.6f, 1f), 0.4f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);

                EasingManager.DoAfter(AllCardsEndedAnimations).SetOnFinish(FinishReroll).SetUnscaledTime(true);

                ContinuePlayingSave.RerollsCount++;
            }
        }

        protected virtual void FinishReroll()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].MainCard.EnableButton();
            }

            if (slots.Count > 1)
            {
                EventSystem.current.SetSelectedGameObject(slots[1].MainCard.Selectable.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(slots[0].MainCard.Selectable.gameObject);
            }
        }

        protected virtual void DisableCoroutines()
        {
            if (showCoroutine != null) StopCoroutine(showCoroutine);

            timeScaleEasingCoroutine.StopIfExists();
            backgroundAlphaEasingCoroutine.StopIfExists();

            rarityAlphaEasingCoroutine.StopIfExists();
            rarityScaleEasingCoroutine.StopIfExists();
        }

        protected virtual void InitSlots(List<AbilityData> abilities)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                var abilityData = abilities[i];

                var slot = slotsPool.GetEntity();

                slot.StartScrolling(abilityData, i);

                slots.Add(slot);
            }
        }

        protected virtual void InitRerollButton()
        {
            var currencyId = StageController.StageData.AbilityRerollPrice.CurrencyId;
            var currency = GameController.CurrenciesManager.GetCurrency(currencyId, true);

            if(currency == null)
            {
                currency = GameController.CurrenciesManager.GetDefaultCurrency(true);
            }
            rerollButton.SetCurrency(currency);
            
            var cost = StageController.StageData.AbilityRerollPrice.Amount;
            for (int i = 0; i < ContinuePlayingSave.RerollsCount; i++) {
                cost = Mathf.RoundToInt(cost * StageController.StageData.NextAbilityRerollMultiplier);
            }

            rerollButton.SetCost(cost);

            rerollButton.Currency.OnAmountChanged += OnGoldAmountChanged;
        }

        protected virtual void InitNavigation()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                if (i != 0)
                {
                    navigation.selectOnLeft = slots[i - 1].MainCard.Selectable;
                }

                if (i != slots.Count - 1)
                {
                    navigation.selectOnRight = slots[i + 1].MainCard.Selectable;
                }

                if (rerollButton.HasEnoughMoney)
                {
                    navigation.selectOnDown = rerollButton.Selectable;
                }

                slot.MainCard.Selectable.navigation = navigation;
            }

            var buttonNavigation = new Navigation();
            buttonNavigation.mode = Navigation.Mode.Explicit;
            if (slots.Count > 1)
            {
                buttonNavigation.selectOnUp = slots[1].MainCard.Selectable;
            }
            else
            {
                buttonNavigation.selectOnUp = slots[0].MainCard.Selectable;
            }

            rerollButton.Selectable.navigation = buttonNavigation;

            if (slots.Count > 1)
            {
                EventSystem.current.SetSelectedGameObject(slots[1].MainCard.Selectable.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(slots[0].MainCard.Selectable.gameObject);
            }
        }

        protected virtual void InitRarityData(RarityData data)
        {
            rarityText.text = data.RarityName;
            rarityText.color = data.RarityTextColor;

            rarityBackgroundImage.color = data.RarityBackgroundColor;
            rarityBackgroundImage.rectTransform.sizeDelta = new Vector2(data.RarityBackgroundWidth, rarityBackgroundImage.rectTransform.sizeDelta.y);
        }

        protected virtual void DoRarityFlash(RarityData data)
        {
            slots.ForEach(slot => slot.DoFlash(data.Rarity));

            InitRarityData(data);

            rarityScaleEasingCoroutine =
                rarityRect.DoLocalScale(Vector3.one * rarityFlashScale, rarityScaleInDuration)
                .SetEasingCurve(rarityScaleCurve)
                .SetUnscaledTime(true)
                .SetOnFinish(OnRarityFlashScaleFinished);

            GameController.AudioManager.PlayAudio(rarityJumpSound);
        }

        protected virtual void OnRarityFlashScaleFinished()
        {
            rarityScaleEasingCoroutine =
                rarityRect.DoLocalScale(Vector3.one * rarityAnimationScale, rarityScaleOutDuration)
                .SetEasing(EasingType.SineOut)
                .SetUnscaledTime(true);
        }

        protected virtual void InitAudio()
        {
            GameController.AudioManager.PlayAudio(popupSound);

            ambientSoundDelayEasingCoroutine = EasingManager.DoAfter(ambientSoundStartDelay, () =>
            {
                ambientSoundAudioSource = GameController.AudioManager.PlayAudio(ambientSound);
                if (ambientSoundAudioSource != null)
                {
                    ambientSoundAudioSource.loop = true;
                }
            }).SetUnscaledTime(true);
        }

        protected virtual void StopAmbientSound()
        {
            if (ambientSoundAudioSource != null)
            {
                ambientSoundAudioSource.DoVolume(0, ambientSoundFadeOutDuration).SetUnscaledTime(true).SetOnFinish(() =>
                {
                    ambientSoundAudioSource.loop = false;
                    ambientSoundAudioSource.Stop();
                    ambientSoundAudioSource = null;
                });
            }
        }

        protected virtual bool AllCardsEndedAnimations()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var card = slot.MainCard;

                if (card == null) return false;

                if (card.IsAnimationActive) return false;
            }

            return true;
        }

        protected virtual bool CardsInRightOrder()
        {
            for (int i = 0; i < slots.Count - 1; i++)
            {
                if (slots[i + 1].LastCardSpawnTime <= slots[i].LastCardSpawnTime) return false;
            }

            return true;
        }

        protected virtual void OnAbilitySelected(AbilityData abilityData)
        {
            if (StageController.AbilitiesManager.IsAbilityAquired(abilityData.AbilityType))
            {
                StageController.AbilitiesManager.IncreaseAbilityLevel(abilityData);
            }
            else
            {
                StageController.AbilitiesManager.AddAbility(abilityData);
            }

            var index = slots.FindIndex(x => x.MainCard.AbilityData.AbilityType == abilityData.AbilityType);
            var card = slots[index];

            slots.RemoveAt(index);
            slots.Insert(0, card);

            var text = StageController.GameScreen.WorldSpaceTextManager.SpawnText(StageController.Player.transform, Vector3.zero, abilityData.Title, WorldSpaceTextType.AbiltiySelect);

            if (text is AbilitySelectedIndicatorBehavior indicator)
            {
                indicator.SetAbilityType(abilityData.AbilityType);
            }

            Hide();
        }

        protected virtual void Hide()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var card = slot.MainCard;
                card.onAbilitySelected -= OnAbilitySelected;
                card.Hide(0.1f * i, (slots.Count - i - 1) * 0.1f);
            }

            title.Hide();

            rarityAlphaEasingCoroutine =
                rarityCanvasGroup.DoAlpha(0, rarityScaleHideDuration)
                .SetDelay(rarityScaleHideDelay)
                .SetUnscaledTime(true);

            backgroundAlphaEasingCoroutine =
                backgroundImage.DoAlpha(0, backgroundFadeHideDuration)
                .SetDelay(backgroundFadeHideDelay)
                .SetUnscaledTime(true)
                .SetOnFinish(OnBackgroundHidden);

            rerollButton.ButtonEnabled = false;
            rerollButton.transform.DoLocalScale(new Vector3(0, 0.6f, 1f), 0.4f).SetUnscaledTime(true).SetEasing(EasingType.CubicIn);

            goldIndicator.transform.DoLocalScale(new Vector3(0, 0.6f, 1f), 0.3f).SetUnscaledTime(true).SetEasing(EasingType.CubicOut);

            StageController.GameScreen.ShowSideUI();

            timeScaleEasingCoroutine =
                EasingManager.DoTimeScale(1, timescaleHideDuration)
                .SetDelay(timescaleHideDelay);

            rerollButton.Currency.OnAmountChanged -= OnGoldAmountChanged;
        }

        protected virtual void OnBackgroundHidden()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.gameObject.SetActive(false);
            }

            slots.Clear();

            gameObject.SetActive(false);

            IsOpen = false;

            onClosed?.Invoke();
        }

        protected virtual void ResetUI()
        {
            gameObject.SetActive(true);

            backgroundImage.SetAlpha(0);

            title.ResetUI();

            rarityCanvasGroup.alpha = 0;

            rerollButton.transform.localScale = new Vector3(0, 0.6f, 1);
            rerollButton.ButtonEnabled = false;

            goldIndicator.transform.localScale = new Vector3(0, 0.6f, 1f);

            skipButton.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Submit.performed += GamepadSkipClicked;
            GameController.InputManager.InputAsset.UI.Cancel.performed += GamepadSkipClicked;
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;

            GameController.InputManager.InputAsset.UI.Submit.performed -= GamepadSkipClicked;
            GameController.InputManager.InputAsset.UI.Cancel.performed -= GamepadSkipClicked;
        }

        protected virtual void OnGoldAmountChanged(CurrencySave currency)
        {
            EasingManager.DoNextFrame(InitNavigation);
        }

        protected virtual void GamepadSkipClicked(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (skipButton.gameObject.activeSelf)
            {
                OnSkipButtonClicked();
            }
        }

        protected virtual void OnInputChanged(Input.InputType prevInput, Input.InputType newInput)
        {
            if (newInput == Input.InputType.Gamepad)
            {
                if (slots.Count > 0 && AllCardsEndedAnimations())
                {
                    if (slots.Count > 1)
                    {
                        EventSystem.current.SetSelectedGameObject(slots[1].MainCard.Selectable.gameObject);
                    }
                    else
                    {
                        EventSystem.current.SetSelectedGameObject(slots[0].MainCard.Selectable.gameObject);
                    }
                }
            }
        }

        protected virtual void OnDestroy()
        {
            StageController.ExperienceManager.onXpLevelChanged -= OnXpLevelChanged;
        }

        [System.Serializable]
        protected class RarityData
        {
            [SerializeField] protected AbilityRarity rarity;
            [SerializeField] protected string rarityName;
            [SerializeField] protected Color rarityTextColor;
            [SerializeField] protected Color rarityBackgroundColor;
            [SerializeField] protected float rarityBackgroundWidth;

            public AbilityRarity Rarity => rarity;
            public string RarityName => rarityName;

            public Color RarityTextColor => rarityTextColor;
            public Color RarityBackgroundColor => rarityBackgroundColor;
            public float RarityBackgroundWidth => rarityBackgroundWidth;
        }
    }
}