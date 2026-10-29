using MrX.Achievements;
using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class StageDifficultySelector : MonoBehaviour
    {
        [SerializeField, Tooltip("Opt in to gameplay state colors. Off preserves Image/TMP colors authored in Inspector.")] private bool useStateColors;
        [SerializeField] private Button[] choices;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private TMP_Text info;
        // Legacy references are no longer applied over each choice's authored Image.
        [SerializeField, HideInInspector] private Sprite selectedFrame, idleFrame;
        [SerializeField] private bool compactLabels;
        [Header("Difficulty state colors")]
        [SerializeField] private Color selectedColor = new Color(1, .85f, .4f);
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(.65f, .7f, .8f);
        [SerializeField] private Color lockedFrameColor = new Color(.64f, .7f, .8f);
        private StageSave save;
        private MainMenuStagesPageBehavior page;
        public static string Name(StageDifficulty mode) => MenuLanguage.Text(mode == StageDifficulty.Normal ? "Normal" : mode == StageDifficulty.Hard ? "Hard" : "Nightmare");
        public static string AchievementId(string stageId, StageDifficulty mode) => "stage-difficulty:" + stageId + ":" + (int)mode;
        private void Start()
        {
            page = GetComponentInParent<MainMenuStagesPageBehavior>();
            save = GameController.SaveManager.GetSave<StageSave>("Stage");
            for (int i = 0; i < choices.Length; i++) { var mode = (StageDifficulty)i; choices[i].onClick.AddListener(() => Select(mode)); }
            save.onSelectedStageChanged += StageChanged; MenuLanguage.Changed += Refresh;
            if (GameController.AchievementsManager != null) GameController.AchievementsManager.Changed += Refresh;
            Refresh();
        }
        private void StageChanged(int _) { Refresh(); }
        private void Select(StageDifficulty mode)
        {
            if (!GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            if (save.SelectDifficulty(page.StageDatabase.GetStageData(save.SelectedStageId), mode))
            {  Refresh(); }
        }
        private void Refresh()
        {
            if (save == null || page == null) return;
            var stage = page.StageDatabase.GetStageData(save.SelectedStageId);
            if (!save.CanPlayDifficulty(stage, save.SelectedDifficulty)) save.SelectDifficulty(stage, StageDifficulty.Normal);
            for (int i = 0; i < choices.Length; i++)
            {
                var mode = (StageDifficulty)i; bool unlocked = save.CanPlayDifficulty(stage, mode);
                choices[i].interactable = unlocked;
                labels[i].text = Name(mode) + (compactLabels ? "" : "\n" + MenuLanguage.Text(!unlocked ? "Locked" : save.IsDifficultyComplete(stage, mode) ? "Completed" : "First clear"));
                if (useStateColors) choices[i].image.color = mode == save.SelectedDifficulty ? selectedColor : unlocked ? idleColor : lockedColor;
            }
            var definition = GameController.AchievementsManager?.Database.achievements.Find(a => a.id == AchievementId(stage.StageId, save.SelectedDifficulty));
            var manager = GameController.AchievementsManager;
            info.text = definition == null ? "" : manager.Save.IsClaimed(definition.id) ? MenuLanguage.Text("Reward claimed") :
                string.Format(MenuLanguage.Text(manager.CanClaim(definition) ? "Reward ready: {0} gems · Claim in Achievements" : "First-clear reward: {0} gems · Claim in Achievements"), definition.reward.Amount);
        }
        private void OnDestroy()
        {
            if (save != null) save.onSelectedStageChanged -= StageChanged;
            MenuLanguage.Changed -= Refresh;
            if (GameController.AchievementsManager != null) GameController.AchievementsManager.Changed -= Refresh;
        }
    }
}
