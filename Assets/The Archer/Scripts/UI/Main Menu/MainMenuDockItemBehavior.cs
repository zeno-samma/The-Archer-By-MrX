using OctoberStudio.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuDockItemBehavior : MonoBehaviour
    {
        [SerializeField] protected MainMenuPageType pageType;
        [SerializeField] protected Button button;

        [Header("Text")]
        [SerializeField] protected TMP_Text pageNameText;

        [Header("Icon")]
        [SerializeField] protected Image iconImage;

        [Space]
        [SerializeField] protected Vector2 selectedIconSize = new Vector2(165, 165);
        [SerializeField] protected Vector2 unselectedIconSize = new Vector2(140, 140);

        [Space]
        [SerializeField] protected Vector2 selectedIconPosition = new Vector2(0, 55);
        [SerializeField] protected Vector2 unselectedIconPosition = Vector2.zero;

        public MainMenuPageType PageType => pageType;

        public event UnityAction<MainMenuPageType> onSelected;

        public bool IsSelected { get; protected set; } = false;

        protected virtual void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public virtual void Select()
        {
            if (!IsSelected)
            {
                IsSelected = true;

                iconImage.rectTransform.DoSizeDelta(selectedIconSize, 0.2f).SetEasing(EasingType.SineInOut);
                iconImage.rectTransform.DoAnchorPosition(selectedIconPosition, 0.2f).SetEasing(EasingType.SineInOut);

                pageNameText.DoAlpha(1, 0.2f);
            }
        }

        public virtual void Deselect()
        {
            if (IsSelected)
            {
                IsSelected = false;

                iconImage.rectTransform.DoSizeDelta(unselectedIconSize, 0.2f).SetEasing(EasingType.SineInOut);
                iconImage.rectTransform.DoAnchorPosition(unselectedIconPosition, 0.2f).SetEasing(EasingType.SineInOut);

                pageNameText.DoAlpha(0, 0.2f);
            }
        }

        protected virtual void OnButtonClicked()
        {
            if (!IsSelected)
            {
                GameController.AudioManager.PlayButtonClick();

                Select();
                onSelected?.Invoke(pageType);
            }
        }
    }
}