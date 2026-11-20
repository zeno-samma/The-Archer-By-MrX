using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    using MrX.Pets;

    public sealed class PetPageBehavior : MainMenuPageBehavior
    {
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text petName;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private TMP_Text cakeLabel;
        [SerializeField] private TMP_Text statusLabel;
        [Header("Owner bonus labels")]
        [SerializeField] private TMP_Text attackBonusLabel;
        [SerializeField] private TMP_Text healthBonusLabel;
        [SerializeField] private TMP_Text criticalBonusLabel;
        [SerializeField] private TMP_Text speedBonusLabel;
        [Header("Actions")]
        [SerializeField] private Button deployButton;
        [SerializeField] private TMP_Text deployButtonLabel;
        [SerializeField] private Button feedButton;

        protected override void Awake()
        {
            base.Awake();
            if (deployButton != null)
            {
                if (deployButtonLabel == null)
                    deployButtonLabel = deployButton.GetComponentInChildren<TMP_Text>(true);

                deployButton.onClick.AddListener(OnDeployClicked);
            }
            if (feedButton != null) feedButton.onClick.AddListener(OnFeedClicked);
        }

        protected override void Start()
        {
            base.Start();
            Refresh();
        }

        private void OnEnable()
        {
            PetSystem.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            PetSystem.Changed -= Refresh;
        }

        protected override void OnMoveCenterFinished()
        {
            base.OnMoveCenterFinished();
            Refresh();
        }

        private void OnDeployClicked()
        {
            if (PetSystem.Profile?.IsDeployed == true)
                PetSystem.TryRecall();
            else
                PetSystem.TryDeploy();
        }

        private void OnFeedClicked()
        {
            PetSystem.TryFeed();
        }

        private void Refresh()
        {
            var data = PetSystem.Definition;
            var profile = PetSystem.Profile;
            bool deployed = profile?.IsDeployed == true;
            bool canDeploy = profile != null && data.CombatAbility != null &&
                profile.HealthPercent == PetProfileSave.FullHealth;

            if (deployButton != null)
                deployButton.interactable = deployed || canDeploy;

            if (deployButtonLabel != null)
                deployButtonLabel.text = deployed ? "Thu hồi" : "Xuất chiến";

            if (feedButton != null)
            {
                feedButton.interactable = profile != null && profile.HealthPercent < PetProfileSave.FullHealth &&
                    profile.Cakes >= data.CakesPerFeeding;
            }

            if (data == null || profile == null) return;

            if (portrait != null) portrait.sprite = data.Portrait;
            if (petName != null) petName.text = data.DisplayName;
            if (healthBar != null)
            {
                healthBar.minValue = 0;
                healthBar.maxValue = PetProfileSave.FullHealth;
                healthBar.SetValueWithoutNotify(profile.HealthPercent);
            }

            if (healthLabel != null) healthLabel.text = $"{profile.HealthPercent}%";
            if (cakeLabel != null) cakeLabel.text = $"Bánh ×{profile.Cakes}  |  {data.CakesPerFeeding} bánh/lần";
            if (attackBonusLabel != null) attackBonusLabel.text = $"+{data.AttackBonus:0.##}%";
            if (healthBonusLabel != null) healthBonusLabel.text = $"+{data.MaxHealthBonus:0.##}%";
            if (criticalBonusLabel != null) criticalBonusLabel.text = $"+{data.CriticalChanceBonus:0.##}%";
            if (speedBonusLabel != null) speedBonusLabel.text = $"+{data.MovementSpeedBonus:0.##}%";

            if (statusLabel != null)
            {
                if (profile.IsDeployed)
                    statusLabel.text = "Đang xuất chiến";
                else if (profile.HealthPercent < PetProfileSave.FullHealth)
                    statusLabel.text = "Cần đủ 100% HP để xuất chiến";
                else if (data.CombatAbility == null)
                    statusLabel.text = "Chưa gán Combat Ability";
                else
                    statusLabel.text = "Sẵn sàng xuất chiến";
            }
        }
    }
}
