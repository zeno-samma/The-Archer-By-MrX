using MrX.Achievements;
using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class StageDifficultySelector : MonoBehaviour
    {
        [SerializeField] private Button[] choices;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private TMP_Text info;
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
            { GameController.AudioManager.PlayButtonClick(); Refresh(); }
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
                labels[i].text = Name(mode) + "\n" + MenuLanguage.Text(!unlocked ? "Locked" : save.IsDifficultyComplete(stage, mode) ? "Completed" : "First clear");
                choices[i].image.color = mode == save.SelectedDifficulty ? new Color(1, .85f, .4f) : unlocked ? Color.white : new Color(.65f, .7f, .8f);
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
