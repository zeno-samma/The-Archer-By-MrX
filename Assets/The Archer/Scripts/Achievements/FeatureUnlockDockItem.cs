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

        private void OnEnable()
        {
            AchievementsManager.FeatureUnlocksChanged += Refresh;
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            AchievementsManager.FeatureUnlocksChanged -= Refresh;
        }

        internal void Refresh()
        {
            string featureId = GetComponent<MainMenuDockItemBehavior>().PageType.ToString();
            var achievements = GameController.AchievementsManager;
            var requirement = achievements?.GetFeatureRequirement(featureId);
            bool unlocked = achievements?.IsFeatureUnlocked(featureId) == true;

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
