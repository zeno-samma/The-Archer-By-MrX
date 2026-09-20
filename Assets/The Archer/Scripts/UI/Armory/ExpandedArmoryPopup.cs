using OctoberStudio.Abilities;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using OctoberStudio.Pool;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public abstract class ExpandedArmoryPopup : MonoBehaviour
    {
        [SerializeField] protected AbilitiesDatabase abilitiesDatabase;

        [Space]
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform panelRect;

        [Header("Info")]
        [SerializeField] protected TMP_Text rarityText;
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text levelText;

        [Space]
        [SerializeField] protected StatsGrid statsGrid;

        [Header("Abilities")]
        [SerializeField] protected VerticalLayoutGroup abilitieLayout;
        [SerializeField] protected GameObject abilityCardPrefab;

        [Header("Buttons")]
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button selectButton;
        [SerializeField] protected CostButtonBehavior upgradeButton;

        [Space]
        [SerializeField] protected TMP_Text selectButtonText;

        [Space]
        [SerializeField] protected TMP_Text upgradeButtonText;
        [SerializeField] protected TMP_Text maxLvlButtonText;

        [Space]
        [SerializeField] protected ScalingLabelBehavior upgradeButtonLabel;
        [SerializeField] protected TMP_Text costText;
        [SerializeField] protected Color disabledCostTextColor;

        [Space]
        [SerializeField] protected Sprite disabledButtonSprite;

        protected PoolComponent<ArmoryAbilityCard> cardsPool;

        protected RectTransform closeButtonRect;
        protected RectTransform abilitiesRect;

        protected float defaultHeight;
        protected float heightDifference;
        protected float defaultCloseButtonPosition;

        protected Sprite selectButtonSprite;

        protected Color enabledCostTextColor;

        public event UnityAction OnPopupHidden;

        public bool IsShown => gameObject.activeSelf;

        protected virtual void Awake()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            selectButton.onClick.AddListener(OnSelectButtonClicked);
            upgradeButton.SetOnClick(OnUpgradeButtonClicked);

            defaultHeight = panelRect.sizeDelta.y;

            abilitiesRect = abilitieLayout.GetComponent<RectTransform>();
            heightDifference = defaultHeight - statsGrid.Height - abilitiesRect.sizeDelta.y;

            closeButtonRect = closeButton.GetComponent<RectTransform>();
            defaultCloseButtonPosition = closeButtonRect.anchoredPosition.y;

            selectButtonSprite = selectButton.image.sprite;

            enabledCostTextColor = costText.color;

            cardsPool = new PoolComponent<ArmoryAbilityCard>(abilityCardPrefab, 3, abilitiesRect);
        }

        protected abstract void OnSelectButtonClicked();
        protected abstract void OnUpgradeButtonClicked();

        protected virtual void SubscribeToEvents()
        {
            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancelButtonPressed;
        }

        protected virtual void UnsubscribeFromEvents()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancelButtonPressed;
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                SelectButtonsGamepad();
            }
        }

        protected virtual void SelectButtonsGamepad()
        {
            if (selectButton.enabled)
            {
                EventSystem.current.SetSelectedGameObject(selectButton.gameObject);
            }
            else if (upgradeButton.ButtonEnabled)
            {
                EventSystem.current.SetSelectedGameObject(upgradeButton.Button.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        protected abstract void InitVisuals();

        protected virtual ArmoryAbilityCard SpawnAbilityCard(AbilityType abilityType, int level)
        {
            var abilityCard = cardsPool.GetEntity();
            var data = abilitiesDatabase.GetAbility(abilityType);

            var isLocked = !IsLevelReached(level);

            abilityCard.Init(data, isLocked, level + 1);

            return abilityCard;
        }

        protected abstract bool IsLevelReached(int level);

        protected virtual void OnCloseButtonClicked()
        {
            GameController.AudioManager.PlayButtonClick();

            StartCoroutine(HideCoroutine());

            UnsubscribeFromEvents();
        }

        protected virtual void OnCancelButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnCloseButtonClicked();
        }

        protected virtual IEnumerator HideCoroutine()
        {
            closeButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            panelRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn);

            yield return new WaitForSeconds(0.1f);

            backgroundImage.DoAlpha(0f, 0.3f);

            yield return new WaitForSeconds(0.3f);

            GameController.MainMenuScreenBehavior.EnableDock();

            InvokeOnHiddenEvent();
        }

        protected virtual IEnumerator ShowCoroutine()
        {
            GameController.MainMenuScreenBehavior.DisableDock();

            gameObject.SetActive(true);

            closeButton.transform.localScale = Vector3.zero;

            backgroundImage.SetAlpha(0f);
            backgroundImage.DoAlpha(1f, 0.3f);

            panelRect.localScale = new Vector3(0, 1, 1);
            panelRect.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut).SetDelay(0.1f);

            yield return new WaitForSeconds(0.3f);

            closeButton.transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut);
        }

        protected virtual void InvokeOnHiddenEvent()
        {
            OnPopupHidden?.Invoke();
        }
    }
}