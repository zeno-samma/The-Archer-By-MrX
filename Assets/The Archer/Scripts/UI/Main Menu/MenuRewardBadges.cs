using MrX.Achievements;
using MrX.Quests;
using UnityEngine;

namespace MrX.UI
{
    // Views are drawn in Menu Dock.prefab. Managers own progress and claim state.
    [DefaultExecutionOrder(100)]
    public sealed class MenuRewardBadges : MonoBehaviour
    {
        [SerializeField] private GameObject dailyQuestsBadge;
        [SerializeField] private GameObject achievementsBadge;
        private DailyQuestsManager quests;
        private AchievementsManager achievements;
        private bool started;
        private readonly InspectorVisibility visibility = new InspectorVisibility();

        private void Awake() { visibility.Capture(dailyQuestsBadge); visibility.Capture(achievementsBadge); }

        private void Start() { started = true; Bind(); }
        private void OnEnable() { if (started) Bind(); }
        private void Bind()
        {
            Unbind();
            quests = GameController.DailyQuestsManager;
            achievements = GameController.AchievementsManager;
            if (quests != null) quests.Changed += Refresh;
            if (achievements != null) achievements.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (!isActiveAndEnabled) return;
            visibility.SetVisible(dailyQuestsBadge, quests != null && quests.HasClaimableRewards);
            visibility.SetVisible(achievementsBadge, achievements != null && achievements.ClaimableCount > 0);
        }

        private void Unbind()
        {
            if (quests != null) quests.Changed -= Refresh;
            if (achievements != null) achievements.Changed -= Refresh;
            quests = null;
            achievements = null;
        }
        private void OnDisable() { Unbind(); }
    }
}
