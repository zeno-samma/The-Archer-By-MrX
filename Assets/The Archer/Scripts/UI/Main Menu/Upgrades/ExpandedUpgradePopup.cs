using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Upgrades;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class ExpandedUpgradePopup : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Image backgroundImage;

        [Header("Cards Settings")]
        [SerializeField] protected UpgradeCardsSelector selector;

        [Space]
        [SerializeField] protected UpgradeDescriptionBlock upgradeDescriptionBlock;

        [Header("Buttons")]
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button backgroundButton;

        protected List<UpgradeData> upgrades;
        protected int SelectedUpgradeIndex { get; set; } = 0;
        protected UpgradeData SelectedUpgradeData => upgrades[SelectedUpgradeIndex];

        public UnityAction onPopupHidden;
        public UnityAction onPopupShown;

        protected virtual void Awake()
        {
            upgrades = GameController.UpgradesManager.GetAllUpgrades();

            closeButton.onClick.AddListener(Hide);
            backgroundButton.onClick.AddListener(Hide);

            selector.onSelectableChanged += OnSelectedUpgradeChanged;
        }

        protected virtual void OnSelectedUpgradeChanged(UpgradeCardSelectable selectable)
        {
            upgradeDescriptionBlock.SetData(selectable.UpgradeCard.Data);
        }

        public virtual void Show(UpgradeData data)
        {
            gameObject.SetActive(true);

            SelectedUpgradeIndex = upgrades.IndexOf(data);

            selector.Init(SelectedUpgradeIndex);
            upgradeDescriptionBlock.SetData(data);

            StartCoroutine(ShowCoroutine());

            GameController.MainMenuScreenBehavior.DisableDock();
            selector.SubscribeToInputEvents();

            GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancelButtonPressed;

            onPopupShown?.Invoke();
        }

        protected virtual void OnCancelButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Hide();
        }

        public void Hide()
        {
            GameController.AudioManager.PlayButtonClick();

            GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancelButtonPressed;

            StartCoroutine(HideCoroutine());
        }

        protected virtual IEnumerator HideCoroutine()
        {
            selector.Hide();

            closeButton.transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            upgradeDescriptionBlock.Hide();

            yield return new WaitForSeconds(0.1f);

            backgroundImage.DoAlpha(0f, 0.3f);

            yield return new WaitForSeconds(0.3f);

            GameController.MainMenuScreenBehavior.EnableDock();
            selector.UnsubscribeFromInputEvents();

            gameObject.SetActive(false);

            onPopupHidden?.Invoke();
        }

        protected virtual IEnumerator ShowCoroutine()
        {
            closeButton.transform.localScale = Vector3.zero;

            backgroundImage.SetAlpha(0f);
            backgroundImage.DoAlpha(1f, 0.3f);

            selector.Show();

            upgradeDescriptionBlock.Show();

            yield return new WaitForSeconds(0.3f);

            closeButton.transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut);
        }
    }
}