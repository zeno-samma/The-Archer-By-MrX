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
        private void Start()
        {
            Service = new LuckyRewardsService(GameController.CurrenciesManager, GameController.SaveManager,
                new LuckyRewardDelivery(GameController.ArmoryManager, GameController.CurrenciesManager));
            wheelButton.onClick.AddListener(() => Open(wheelPopup)); chestButton.onClick.AddListener(() => Open(chestPopup));
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
            SetBadge(wheelFreeBadge, wheelPopup != null && Service.HasDailyFreeUse(wheelPopup.Table));
            SetBadge(chestFreeBadge, chestPopup != null && Service.HasDailyFreeUse(chestPopup.Table));
            SetBadge(wheelFreeLabel, wheelPopup != null && Service.HasDailyFreeUse(wheelPopup.Table));
            SetBadge(chestFreeLabel, chestPopup != null && Service.HasDailyFreeUse(chestPopup.Table));
        }
        private static void SetBadge(GameObject badge, bool visible)
        { if (badge != null && badge.activeSelf != visible) badge.SetActive(visible); }
        private void OnDestroy() { if (Service != null) Service.Changed -= RefreshBadges; }
        private void Open(LuckyRewardsPopup popup)
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (Service == null || !screen.IsDockEnabled || screen.CurrentPageType != MainMenuPageType.Stages) return;
            popup.Show(Service);
        }
    }
}
