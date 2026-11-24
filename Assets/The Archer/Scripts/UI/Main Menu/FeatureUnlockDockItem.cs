using MrX.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MainMenuDockItemBehavior))]
    public sealed class FeatureUnlockDockItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Color lockedIconColor = Color.black;
        [SerializeField] private Color unlockedIconColor = Color.white;
        [SerializeField] private GameObject lockedBadge;
        [SerializeField] private TMP_Text requirementLabel;

        [SerializeField] private bool usesAccountLevel;
        internal bool UsesAccountLevel => usesAccountLevel;

        private void OnEnable()
        {
            AchievementsManager.FeatureUnlocksChanged += Refresh;
            if (usesAccountLevel) Progression.PlayerProgression.Changed += Refresh;
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            AchievementsManager.FeatureUnlocksChanged -= Refresh;
            if (usesAccountLevel) Progression.PlayerProgression.Changed -= Refresh;
        }

        internal void Refresh()
        {
            var item = GetComponent<MainMenuDockItemBehavior>();
            string featureId = item.PageType.ToString();
            var achievements = GameController.AchievementsManager;
            var requirement = achievements?.GetFeatureRequirement(featureId);
            bool unlocked = achievements?.IsFeatureUnlocked(featureId) == true;

            if (usesAccountLevel)
            {
                int minimum = Progression.PlayerProgression.RequiredLevel(item.PageType);
                unlocked = minimum > 0 ? Progression.PlayerProgression.Current.Level >= minimum
                    : requirement == null || unlocked;
                if (TryGetComponent<Button>(out var button)) button.interactable = unlocked;
            }

            if (icon != null)
                icon.color = unlocked ? unlockedIconColor : lockedIconColor;
            if (lockedBadge != null && lockedBadge != gameObject)
                lockedBadge.SetActive(!unlocked);
            if (requirementLabel != null)
            {
                requirementLabel.text = !unlocked && requirement != null
                    ? $"Mở khóa: {requirement.title}"
                    : string.Empty;
            }
        }
    }
}
