using TMPro;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class MainMenuStageSelector : HorizontalSelector<MainMenuStageIcon>
    {
        [Space]
        [SerializeField] protected TMP_Text stageNameText;
        [SerializeField] protected TMP_Text stageNumberText;

        protected MainMenuStagesPageBehavior Page { get; set; }

        protected StageDatabase stageDatabase;
        protected StageSave stageSave;

        public override int SelectedId => stageSave.SelectedStageId;

        public void Init(MainMenuStagesPageBehavior page)
        {
            Page = page;

            stageDatabase = page.StageDatabase;
            stageSave = page.StageSave;

            stageSave.onSelectedStageChanged += InitStage;

            CurrentSelectable = SpawnSelectable(stageSave.SelectedStageId);

            InitStage(stageSave.SelectedStageId);
        }

        public virtual void InitStage(int stageId)
        {
            var stage = stageDatabase.GetStageData(stageId);
            if (stage == null)
            {
                Debug.LogError($"Stage with ID {stageSave.SelectedStageId} not found in the database.");
                return;
            }

            stageNameText.text = stage.StageName;
            stageNumberText.text = $"Stage {stageId + 1}";

            CurrentSelectable.SetIcon(stage.StageImage, !stageSave.IsStageUnlocked(stage));

            InitButtons();
        }

        protected override bool IsLeftAvailable()
        {
            return !stageSave.IsFirstStageSelected;
        }

        protected override bool IsRightAvailable()
        {
            return stageSave.SelectedStageId != stageDatabase.StagesAmount - 1;
        }

        protected override void OnLeftButtonClicked()
        {
            base.OnLeftButtonClicked();

            stageSave.SetSelectedStageId(stageSave.SelectedStageId - 1);
        }

        protected override void OnRightButtonClicked()
        {
            base.OnRightButtonClicked();

            stageSave.SetSelectedStageId(stageSave.SelectedStageId + 1);
        }

        protected override MainMenuStageIcon SpawnSelectable(int stageId)
        {
            var stageData = stageDatabase.GetStageData(stageId);
            var icon = selectablesPool.GetEntity();

            return icon;
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromInputEvents();

            stageSave.onSelectedStageChanged -= InitStage;
        }
    }
}