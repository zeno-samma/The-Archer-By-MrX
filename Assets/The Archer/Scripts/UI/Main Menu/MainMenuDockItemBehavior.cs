using MrX.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MrX.UI
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

        public virtual void Configure(MainMenuPageType type, string title, Sprite icon)
        {
            pageType = type;
            gameObject.name = title + " Dock Item";
            pageNameText.text = title;
            pageNameText.textWrappingMode = TextWrappingModes.NoWrap;
            pageNameText.enableAutoSizing = true;
            pageNameText.fontSizeMin = 12;
            if (icon != null) iconImage.sprite = icon;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        }

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
        // phương thức này được gọi khi một item khác được chọn, và item này sẽ bị bỏ chọn. Nó sẽ thay đổi trạng thái IsSelected thành false, và thực hiện các hiệu ứng chuyển đổi kích thước và vị trí của icon, cũng như làm mờ text của tên trang.
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
            if (GameController.MainMenuScreenBehavior != null && !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            if (!IsSelected)
            {
                GameController.AudioManager.PlayButtonClick();

                Select();
                onSelected?.Invoke(pageType);
            }
        }
    }
}
