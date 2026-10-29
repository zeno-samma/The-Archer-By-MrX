using MrX.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    [DefaultExecutionOrder(50)]
    public sealed class StageLuckyRewards : MonoBehaviour
    {
        [SerializeField] private Button wheelButton, chestButton;
        [SerializeField] private LuckyRewardsPopup wheelPopup, chestPopup;
        [SerializeField] private GameObject wheelFreeBadge, chestFreeBadge;
        [SerializeField] private GameObject wheelFreeLabel, chestFreeLabel;
        public LuckyRewardsService Service { get; private set; }
        private float nextRefresh;
        private readonly InspectorVisibility visibility = new InspectorVisibility();
        private void Awake()
        {
            visibility.Capture(wheelFreeBadge); visibility.Capture(chestFreeBadge);
            visibility.Capture(wheelFreeLabel); visibility.Capture(chestFreeLabel);
        }
        private void Start()
        {
            Service = new LuckyRewardsService(GameController.CurrenciesManager, GameController.SaveManager,
                new LuckyRewardDelivery(GameController.ArmoryManager, GameController.CurrenciesManager));
            wheelButton.onClick.AddListener(OpenWheel); chestButton.onClick.AddListener(OpenChest);
            Service.Changed += RefreshBadges;
            RefreshBadges();
        }
        private void Update()
        {
            if (Service == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1;
            RefreshBadges();
        }
        private void RefreshBadges()
        {
            if (!isActiveAndEnabled || Service == null) return;
            visibility.SetVisible(wheelFreeBadge, wheelPopup != null && Service.HasDailyFreeUse(wheelPopup.Table));
            visibility.SetVisible(chestFreeBadge, chestPopup != null && Service.HasDailyFreeUse(chestPopup.Table));
            visibility.SetVisible(wheelFreeLabel, wheelPopup != null && Service.HasDailyFreeUse(wheelPopup.Table));
            visibility.SetVisible(chestFreeLabel, chestPopup != null && Service.HasDailyFreeUse(chestPopup.Table));
        }
        private void OpenWheel() { Open(wheelPopup); }
        private void OpenChest() { Open(chestPopup); }
        private void OnDestroy()
        {
            if (Service != null) Service.Changed -= RefreshBadges;
            if (wheelButton != null) wheelButton.onClick.RemoveListener(OpenWheel);
            if (chestButton != null) chestButton.onClick.RemoveListener(OpenChest);
        }
        private void Open(LuckyRewardsPopup popup)
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (!isActiveAndEnabled || Service == null || screen == null || popup == null || !screen.IsDockEnabled || screen.CurrentPageType != MainMenuPageType.Stages) return;
            popup.Show(Service);
        }
    }
}
