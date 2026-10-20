using MrX.Development;
using MrX.Armory;
using MrX.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    [DefaultExecutionOrder(70)]
    public sealed class DeveloperToolsPanel : MonoBehaviour
    {
        [SerializeField] private DeveloperToolsSettings settings;
        [SerializeField] private GameObject shortcut, panel;
        [SerializeField] private Button openButton, closeButton, amountButton, goldButton, gemButton, previousItem, nextItem, grantItem;
        [SerializeField] private TMP_Text amountLabel, itemLabel, status;
        [SerializeField] private Image itemIcon;
        private DeveloperGrantService service;
        private int amountIndex = 1, itemIndex;
        private bool ownsDock;
        private GameObject previousSelection;
        private void Start()
        {
            service = new DeveloperGrantService(settings, new LuckyRewardDelivery(GameController.ArmoryManager,
                GameController.CurrenciesManager), GameController.SaveManager, Application.isEditor, Debug.isDebugBuild);
            openButton.onClick.AddListener(Open); closeButton.onClick.AddListener(Close);
            amountButton.onClick.AddListener(() => { amountIndex++; Refresh(); });
            goldButton.onClick.AddListener(() => Currency("gold")); gemButton.onClick.AddListener(() => Currency("gem"));
            previousItem.onClick.AddListener(() => SelectItem(-1)); nextItem.onClick.AddListener(() => SelectItem(1));
            grantItem.onClick.AddListener(GrantItem);
            if (settings) settings.Changed += Availability;
            Availability(); Refresh();
        }
        private void Availability()
        {
            bool available = settings && settings.IsAvailable;
            shortcut.SetActive(available);
            if (!available) Close();
        }
        private void Open()
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!settings || !settings.IsAvailable || !screen.IsDockEnabled || screen.CurrentPageType != MainMenuPageType.Stages) return;
            previousSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            panel.SetActive(true); panel.transform.SetAsLastSibling(); ownsDock = true; screen.DisableDock();
            panel.GetComponent<Canvas>().overrideSorting = true;
            GameController.InputManager.InputAsset.UI.Cancel.performed += Cancel;
            status.text = "Grants are saved to this player profile."; Refresh();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(goldButton.gameObject);
        }
        private int Amount => settings && settings.currencyAmounts != null && settings.currencyAmounts.Length > 0 ?
            settings.currencyAmounts[amountIndex % settings.currencyAmounts.Length] : 0;
        private ItemData Item => settings && settings.armory && settings.armory.ItemsCount > 0 ? settings.armory.Items[itemIndex] : null;
        private void SelectItem(int direction)
        {
            int count = settings && settings.armory ? settings.armory.ItemsCount : 0;
            itemIndex = count == 0 ? 0 : (itemIndex + direction + count) % count; Refresh();
        }
        private void Refresh()
        {
            amountIndex = settings && settings.currencyAmounts != null && settings.currencyAmounts.Length > 0 ? amountIndex % settings.currencyAmounts.Length : 0;
            if (settings && settings.armory) itemIndex = Mathf.Clamp(itemIndex, 0, Mathf.Max(0, settings.armory.ItemsCount - 1));
            amountLabel.text = "AMOUNT: " + Amount.ToString("N0") + "   >";
            goldButton.interactable = gemButton.interactable = Amount > 0;
            var item = Item; itemLabel.text = item ? item.ItemName + "\n" + (itemIndex + 1) + " / " + settings.armory.ItemsCount : "No equipment registered";
            itemIcon.sprite = item ? item.Icon : null; itemIcon.enabled = item; grantItem.interactable = item;
        }
        private void Currency(string id)
        { service.GrantCurrency(id, Amount, out var message); status.text = message; }
        private void GrantItem()
        { service.GrantItem(Item, out var message); status.text = message; }
        private void Cancel(UnityEngine.InputSystem.InputAction.CallbackContext context) => Close();
        public void Close()
        {
            panel.SetActive(false);
            if (!ownsDock) return;
            ownsDock = false;
            if (GameController.InputManager != null) GameController.InputManager.InputAsset.UI.Cancel.performed -= Cancel;
            if (GameController.MainMenuScreenBehavior != null) GameController.MainMenuScreenBehavior.EnableDock();
            if (EventSystem.current && previousSelection && previousSelection.activeInHierarchy) EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        private void OnDisable() => Close();
        private void OnDestroy() { if (settings) settings.Changed -= Availability; }
    }
}
