using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
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

        protected T CurrentSelectable { get; set; }
        protected List<T> SelectablesQueue { get; set; } = new List<T>();

        protected bool IsMovingRight { get; set; }

        public abstract int SelectedId { get; }

        public event UnityAction<T> onSelectableChanged;

        protected virtual void Awake()
        {
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
            if (GameController.InputManager.ActiveInput == Input.InputType.Gamepad)
            {
                leftButton.gameObject.SetActive(false);
                rightButton.gameObject.SetActive(false);

                leftGamepadIndicator.gameObject.SetActive(IsLeftAvailable());
                rightGamepadIndicator.gameObject.SetActive(IsRightAvailable());
            }
            else
            {
                leftGamepadIndicator.gameObject.SetActive(false);
                rightGamepadIndicator.gameObject.SetActive(false);

                leftButton.gameObject.SetActive(IsLeftAvailable());
                rightButton.gameObject.SetActive(IsRightAvailable());
            }
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

            GameController.AudioManager.PlayButtonClick();

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

            GameController.AudioManager.PlayButtonClick();

            onSelectableChanged?.Invoke(CurrentSelectable);
        }

        protected virtual void OnShouldersPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<float>();

            if (value > 0)
            {
                if (IsRightAvailable())
                {
                    OnRightButtonClicked();
                }
            }
            else
            {
                if (IsLeftAvailable())
                {
                    OnLeftButtonClicked();
                }
            }
        }

        protected abstract bool IsLeftAvailable();
        protected abstract bool IsRightAvailable();

        protected abstract T SpawnSelectable(int id);
    }
}