using MrX.Development;
using MrX.Armory;
using MrX.Rewards;
using MrX.Progression;
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
        [SerializeField] private Button addXpButton, resetXpButton;
        [SerializeField] private TMP_Text addXpLabel;
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
            if (addXpButton) addXpButton.onClick.AddListener(() => ChangeXp(false));
            if (resetXpButton) resetXpButton.onClick.AddListener(() => ChangeXp(true));
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
            status.text = addXpButton ? XpStatus() : "Grants are saved to this player profile.";
            Refresh();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(goldButton.gameObject);
        }
        private int Amount => settings && settings.currencyAmounts != null && settings.currencyAmounts.Length > 0 ?
            settings.currencyAmounts[amountIndex % settings.currencyAmounts.Length] : 0;
        private HeroShardDefinition Shard => settings ? settings.heroShard : null;
        private int EquipmentOffset => Shard ? 1 : 0;
        private int EntryCount => EquipmentOffset + (settings && settings.armory ? settings.armory.ItemsCount : 0);
        private bool IsShardSelected => Shard && itemIndex == 0;
        private ItemData Item => !IsShardSelected && itemIndex < EntryCount
            ? settings.armory.Items[itemIndex - EquipmentOffset] : null;
        private void SelectItem(int direction)
        {
            int count = EntryCount;
            itemIndex = count == 0 ? 0 : (itemIndex + direction + count) % count; Refresh();
        }
        private void Refresh()
        {
            amountIndex = settings && settings.currencyAmounts != null && settings.currencyAmounts.Length > 0 ? amountIndex % settings.currencyAmounts.Length : 0;
            itemIndex = Mathf.Clamp(itemIndex, 0, Mathf.Max(0, EntryCount - 1));
            amountLabel.text = "AMOUNT: " + Amount.ToString("N0") + "   >";
            goldButton.interactable = gemButton.interactable = Amount > 0;
            if (addXpButton) addXpButton.interactable = Amount > 0;
            if (addXpLabel) addXpLabel.text = $"+{Amount:N0} XP";
            var item = Item;
            var shard = IsShardSelected ? Shard : null;
            var heroSave = shard && shard.Hero ? GameController.ArmoryManager.GetHeroSave(shard.Hero.Id) : null;
            itemLabel.text = shard && shard.Hero
                ? $"{shard.Hero.Name} Hero Shard\n{heroSave?.Shards ?? 0} owned · +{shard.GrantAmount}"
                : item ? $"{item.ItemName}\n{itemIndex + 1} / {EntryCount}" : "No reward registered";
            itemIcon.sprite = shard ? shard.Icon : item ? item.Icon : null;
            itemIcon.enabled = itemIcon.sprite != null;
            grantItem.interactable = heroSave != null || item != null;
        }
        private static string XpStatus() =>
            $"Account XP: {PlayerProgression.TotalXp:N0} | Lv. {PlayerProgression.Current.Level}";

        private void ChangeXp(bool reset)
        {
            if (!settings || !settings.IsAvailable) return;
            status.text = PlayerProgression.ChangeDeveloperXp(Amount, reset)
                ? XpStatus() + " (saved)" : "Cannot change XP during an active run.";
        }

        private void Currency(string id)
        { service.GrantCurrency(id, Amount, out var message); status.text = message; }
        private void GrantItem()
        {
            if (!IsShardSelected)
            {
                service.GrantItem(Item, out var message);
                status.text = message;
                return;
            }

            if (!settings.IsAvailable) return;
            var shard = Shard;
            var heroSave = shard.Hero ? GameController.ArmoryManager.GetHeroSave(shard.Hero.Id) : null;
            if (heroSave?.TryAddShards(shard.GrantAmount) != true)
            {
                status.text = "Cannot grant Hero Shards.";
                return;
            }

            GameController.SaveManager.Save(false);
            status.text = $"+{shard.GrantAmount} {shard.Hero.Name} Hero Shards (saved)";
            Refresh();
        }
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
