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
        public LuckyRewardsService Service { get; private set; }
        private void Start()
        {
            Service = new LuckyRewardsService(GameController.CurrenciesManager, GameController.SaveManager,
                new LuckyRewardDelivery(GameController.ArmoryManager, GameController.CurrenciesManager));
            wheelButton.onClick.AddListener(() => Open(wheelPopup)); chestButton.onClick.AddListener(() => Open(chestPopup));
        }
        private void Open(LuckyRewardsPopup popup)
        {
            var screen = GameController.MainMenuScreenBehavior;
            if (Service == null || !screen.IsDockEnabled || screen.CurrentPageType != MainMenuPageType.Stages) return;
            popup.Show(Service);
        }
    }
}
