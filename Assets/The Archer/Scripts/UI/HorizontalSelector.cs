using MrX.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MrX.UI
{
    public abstract class HorizontalSelector<T> : MonoBehaviour where T : HorizontalSelectable
    {
        [SerializeField] protected GameObject selectablePrefab;
        [SerializeField] protected RectTransform selectablesParent;

        [Space]
        [SerializeField] protected Button leftButton;
        [SerializeField] protected Button rightButton;

        [Space]
        [SerializeField] protected GameObject leftGamepadIndicator;
        [SerializeField] protected GameObject rightGamepadIndicator;

        protected PoolComponent<T> selectablesPool;
        private readonly InspectorVisibility inspectorVisibility = new InspectorVisibility();

        protected T CurrentSelectable { get; set; }
        protected List<T> SelectablesQueue { get; set; } = new List<T>();

        protected bool IsMovingRight { get; set; }

        public abstract int SelectedId { get; }

        public event UnityAction<T> onSelectableChanged;

        protected virtual void Awake()
        {
            inspectorVisibility.Capture(leftButton.gameObject);
            inspectorVisibility.Capture(rightButton.gameObject);
            inspectorVisibility.Capture(leftGamepadIndicator);
            inspectorVisibility.Capture(rightGamepadIndicator);
            selectablesPool = new PoolComponent<T>(selectablePrefab, 3, selectablesParent);

            leftButton.onClick.AddListener(OnLeftButtonClicked);
            rightButton.onClick.AddListener(OnRightButtonClicked);
        }

        protected virtual void Start()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        protected virtual void ClearQueue()
        {
            for (int i = SelectablesQueue.Count - 1; i >= 0; i--)
            {
                if (!SelectablesQueue[i].gameObject.activeSelf)
                {
                    SelectablesQueue.RemoveRange(0, i + 1);
                    break;
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (GameController.InputManager != null)
            {
                GameController.InputManager.onInputChanged -= OnInputChanged;
                GameController.InputManager.onInputChanged += OnInputChanged;
            }
        }

        protected virtual void OnDisable()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        public virtual void UnsubscribeFromInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Shoulders.performed -= OnShouldersPerformed;
        }

        public virtual void SubscribeToInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Shoulders.performed += OnShouldersPerformed;
        }

        protected virtual void OnInputChanged(Input.InputType oldInput, Input.InputType newInput)
        {
            Debug.Log("Input changed");
            InitButtons();
        }

        protected virtual void InitButtons()
        {
            bool gamepad = GameController.InputManager.ActiveInput == Input.InputType.Gamepad;
            inspectorVisibility.SetVisible(leftButton.gameObject, !gamepad && IsLeftAvailable());
            inspectorVisibility.SetVisible(rightButton.gameObject, !gamepad && IsRightAvailable());
            inspectorVisibility.SetVisible(leftGamepadIndicator, gamepad && IsLeftAvailable());
            inspectorVisibility.SetVisible(rightGamepadIndicator, gamepad && IsRightAvailable());
        }

        protected virtual void OnLeftButtonClicked()
        {
            ClearQueue();
            var isMoving = SelectablesQueue.Count > 0;

            SelectablesQueue.Add(CurrentSelectable);
            CurrentSelectable = SpawnSelectable(SelectedId - 1);

            CurrentSelectable.RectTransform.anchoredPosition = new Vector2(-700, 0);
            CurrentSelectable.MoveCenter(isMoving && !IsMovingRight ? 0.2f : 0);

            SelectablesQueue[^1].MoveRight();
            IsMovingRight = true;



            onSelectableChanged?.Invoke(CurrentSelectable);
        }

        protected virtual void OnRightButtonClicked()
        {
            ClearQueue();
            var isMoving = SelectablesQueue.Count > 0;

            SelectablesQueue.Add(CurrentSelectable);
            CurrentSelectable = SpawnSelectable(SelectedId + 1);

            CurrentSelectable.RectTransform.anchoredPosition = new Vector2(700, 0);
            CurrentSelectable.MoveCenter(isMoving && IsMovingRight ? 0.2f : 0);

            SelectablesQueue[^1].MoveLeft();
            IsMovingRight = false;



            onSelectableChanged?.Invoke(CurrentSelectable);
        }

        protected virtual void OnShouldersPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<float>();

            if (value > 0)
            {
                if (IsRightAvailable())
                {
                    UIButtonAudio.PlayShortcut(rightButton);
                    OnRightButtonClicked();
                }
            }
            else
            {
                if (IsLeftAvailable())
                {
                    UIButtonAudio.PlayShortcut(leftButton);
                    OnLeftButtonClicked();
                }
            }
        }

        protected abstract bool IsLeftAvailable();
        protected abstract bool IsRightAvailable();

        protected abstract T SpawnSelectable(int id);
    }
}
