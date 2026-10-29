using MrX.Currency;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class MenuEconomyPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text energyAmount, goldAmount, gemAmount, recoveryLabel, runCost, feedback, offerTitle, offerAmount, offerPrice;
        [SerializeField] private Button energyPlus, goldPlus, buyButton, closeButton;
        [SerializeField] private GameObject modal;
        [SerializeField] private TMP_Text purchaseLimit;
        [SerializeField] private GameObject energyGroup;
        private MenuEconomyService service;
        private bool ownsDock;
        private GameObject previousSelection;
        private string offeredCurrency;
        private float nextTick;
        private readonly InspectorVisibility visibility = new InspectorVisibility();
        private void Awake()
        {
            visibility.Capture(energyGroup);
            if (recoveryLabel != null) visibility.Capture(recoveryLabel.gameObject);
            if (runCost != null) visibility.Capture(runCost.gameObject);
            if (purchaseLimit != null) visibility.Capture(purchaseLimit.gameObject);
        }

        private void Start()
        {
            service = GameController.MenuEconomy;
            if (service == null || !service.Ready) { enabled = false; return; }
            energyPlus.onClick.AddListener(() => Open("energy")); goldPlus.onClick.AddListener(() => Open("gold"));
            buyButton.onClick.AddListener(Buy); closeButton.onClick.AddListener(Close);
            service.Energy.OnAmountChanged += WalletChanged; service.Gold.OnAmountChanged += WalletChanged; service.Gems.OnAmountChanged += WalletChanged;
            Refresh();
        }

        private void WalletChanged(CurrencySave _) { Refresh(); }
        private void Update()
        {
            if (Time.unscaledTime < nextTick || service == null) return;
            nextTick = Time.unscaledTime + 1;
            if (isActiveAndEnabled && GetComponentInParent<Canvas>().isActiveAndEnabled) RefreshTimer();
        }
        private void RefreshTimer()
        {
            if (!isActiveAndEnabled || service == null) return;
            bool energyEnabled = Development.GameFeatureSettings.IsEnabled(Development.GameFeature.Energy);
            visibility.SetVisible(energyGroup, energyEnabled);
            if (recoveryLabel != null) visibility.SetVisible(recoveryLabel.gameObject, energyEnabled);
            if (!energyEnabled && ownsDock && offeredCurrency == "energy") Close();
            int seconds = service.SecondsUntilRecovery;
            recoveryLabel.text = seconds == 0 ? "FULL" : "RECOVERS IN " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            if (ownsDock && offeredCurrency == "energy") RefreshLimit();
            var stage = GameController.MainMenuScreenBehavior?.CurrentPage as MainMenuStagesPageBehavior;
            if (runCost != null)
            {
                visibility.SetVisible(runCost.gameObject, energyEnabled && stage != null && stage.StageSave != null);
                if (stage != null && stage.StageSave != null) runCost.text = "RUN COST: " + stage.StageDatabase.GetStageData(stage.StageSave.SelectedStageId).EnergyCost + " ENERGY";
            }
        }
        private void Refresh()
        {
            if (!isActiveAndEnabled || service == null) return;
            energyAmount.text = service.Energy.Amount + " / " + service.Settings.maximumEnergy;
            goldAmount.text = service.Gold.Amount.ToString("N0"); gemAmount.text = service.Gems.Amount.ToString("N0");
            RefreshTimer();
        }

        public void ShowInsufficientEnergy()
        {
            if (!isActiveAndEnabled) return;
            Open("energy");
            feedback.text = "Not enough energy. Wait for recovery or buy more with gems.";
        }
        private void Open(string currencyId)
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!isActiveAndEnabled || service == null || screen == null || !screen.IsDockEnabled || modal.activeSelf) return;
            offeredCurrency = currencyId; ownsDock = true;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            screen.DisableDock(); (screen.CurrentPage as MainMenuStagesPageBehavior)?.SetLanguageModalInput(true);
            modal.SetActive(true); feedback.text = "";
            offerTitle.text = currencyId == "energy" ? "BUY ENERGY" : "BUY GOLD";
            offerAmount.text = "+" + (currencyId == "energy" ? service.Settings.energyPurchaseAmount : service.Settings.goldPurchaseAmount) + (currencyId == "energy" ? " ENERGY" : " GOLD");
            offerPrice.text = (currencyId == "energy" ? service.Settings.energyGemPrice : service.Settings.goldGemPrice) + " GEMS";
            RefreshLimit();
            GameController.InputManager.InputAsset.UI.Cancel.performed += Cancel;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(buyButton.gameObject);
        }
        private void Buy()
        {
            if (!isActiveAndEnabled || !ownsDock) return;
            var result = service.Buy(offeredCurrency);
            feedback.text = result == ExchangeResult.Success ? "Purchase complete!" : result == ExchangeResult.NotEnoughGems ? "Not enough gems." : result == ExchangeResult.EnergyFull ? "Not enough energy space for this pack." : result == ExchangeResult.DailyLimit ? "Daily energy purchase limit reached." : "Purchase unavailable.";
            Refresh();
        }
        private void RefreshLimit()
        {
            bool energy = offeredCurrency == "energy";
            if (purchaseLimit != null) { visibility.SetVisible(purchaseLimit.gameObject, energy); purchaseLimit.text = "PURCHASES LEFT TODAY: " + service.RemainingEnergyPurchases + " / " + service.Settings.dailyEnergyPurchaseLimit; }
            buyButton.interactable = !energy || service.RemainingEnergyPurchases > 0;
        }
        private void Cancel(InputAction.CallbackContext _) { Close(); }
        public void Close()
        {
            if (modal != null) modal.SetActive(false);
            if (!ownsDock) return;
            ownsDock = false;
            if (GameController.InputManager != null) GameController.InputManager.InputAsset.UI.Cancel.performed -= Cancel;
            var screen = GameController.MainMenuScreenBehavior;
            if (screen != null) { screen.EnableDock(); (screen.CurrentPage as MainMenuStagesPageBehavior)?.SetLanguageModalInput(false); }
            if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy) EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        private void OnDisable() { Close(); }
        private void OnDestroy()
        {
            Close();
            if (service == null) return;
            if (service.Energy != null) service.Energy.OnAmountChanged -= WalletChanged;
            if (service.Gold != null) service.Gold.OnAmountChanged -= WalletChanged;
            if (service.Gems != null) service.Gems.OnAmountChanged -= WalletChanged;
        }
    }
}
