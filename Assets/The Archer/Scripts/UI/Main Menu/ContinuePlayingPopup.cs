using MrX.Easing;
using MrX.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX
{
    public class ContinuePlayingPopup : MonoBehaviour
    {
        [SerializeField] protected RectTransform panelRect;
        [SerializeField] protected RectTransform titleRect;
        [SerializeField] protected Image backgroundImage;

        [Space]
        [SerializeField] protected Button yesButton;
        [SerializeField] protected Button noButton;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public bool IsOpen { get; protected set; }
        public UnityAction onPopupClosed;

        protected virtual void Start()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            EventSystem.current.SetSelectedGameObject(yesButton.gameObject);

            IsOpen = true;
        }

        public virtual void Hide()
        {
            yesButton.enabled = false;
            noButton.enabled = false;

            panelRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn);
            titleRect.DoLocalScale(new Vector3(0, 1, 1), 0.3f).SetEasing(EasingType.CubicIn).SetDelay(0.1f);
            backgroundImage.DoAlpha(0, 0.3f).SetDelay(0.2f).SetOnFinish(OnPopupHidden);
        }

        protected virtual void OnPopupHidden()
        {
            gameObject.SetActive(false);

            IsOpen = false;

            onPopupClosed?.Invoke();
        }

        protected virtual void OnEnable()
        {
            yesButton.onClick.AddListener(OnYesButtonClicked);
            noButton.onClick.AddListener(OnNoButtonClicked);

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType newInput)
        {
            if (newInput == InputType.Gamepad)
            {
                EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
            }
        }

        protected virtual void OnDisable()
        {
            yesButton.onClick.RemoveListener(OnYesButtonClicked);
            noButton.onClick.RemoveListener(OnNoButtonClicked);

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        protected virtual void OnYesButtonClicked()
        {


            GameController.LoadStage();
        }

        protected virtual void OnNoButtonClicked()
        {
            if (!ContinuePlayingSave.HasUnfinishedStageData) return;
            var saves = GameController.SaveManager as MrX.Save.SaveManager;
            saves?.BeginGameplayBatch();
            GameController.ArmoryManager.BossChests.CommitRun(ContinuePlayingSave.RunToken);
            if (ContinuePlayingSave.PickedUpItems != null)
                foreach (var drop in ContinuePlayingSave.PickedUpItems)
                {
                    if (GameController.ArmoryManager.IsItemUnlocked(drop.ItemId)) GameController.ArmoryManager.IncreaseItemLevel(drop.ItemId);
                    else GameController.ArmoryManager.UnlockItem(drop.ItemId);
                }
            ContinuePlayingSave.SavePickedUpItems(System.Array.Empty<StageSave.ItemDropSaveData>());
            ContinuePlayingSave.Disable();
            GameController.CurrenciesManager.ApplyGameplayAmounts();
            GameController.DailyQuestsManager?.FlushPending(); GameController.AchievementsManager?.FlushPending(); GameController.ArmoryManager.Crafting?.FlushPending();
            if (saves != null) saves.CommitGameplayBatch(); else GameController.SaveManager.Save(false);



            Hide();
        }
    }
}
