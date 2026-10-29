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

        [Tooltip("Opt in to change icon size/position and label opacity when selecting tabs. Leave disabled to keep the Inspector presentation.")]
        [SerializeField] private bool animateSelectionLayout;
        [Space]
        [SerializeField] protected Vector2 selectedIconSize = new Vector2(165, 165);
        [SerializeField] protected Vector2 unselectedIconSize = new Vector2(140, 140);

        [Space]
        [SerializeField] protected Vector2 selectedIconPosition = new Vector2(0, 55);
        [SerializeField] protected Vector2 unselectedIconPosition = Vector2.zero;
        [SerializeField] private Image selectedBackdrop;
        [SerializeField, Range(0, 1)] private float inactiveLabelAlpha;
        private readonly InspectorVisibility inspectorVisibility = new InspectorVisibility();

        public MainMenuPageType PageType => pageType;

        public event UnityAction<MainMenuPageType> onSelected;

        public bool IsSelected { get; protected set; } = false;

        public virtual void Configure(MainMenuPageType type, string title, Sprite icon)
        {
            pageType = type;
            pageNameText.text = title;
            if (icon != null) iconImage.sprite = icon;
#if UNITY_EDITOR
            // Layout defaults belong to authoring tools, never runtime data binding.
            if (Application.isPlaying) return;
            gameObject.name = title + " Dock Item";
            pageNameText.textWrappingMode = TextWrappingModes.NoWrap;
            pageNameText.enableAutoSizing = true;
            pageNameText.fontSizeMin = 12;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
#endif
        }

        protected virtual void Awake()
        {
            inspectorVisibility.Capture(selectedBackdrop);
            button.onClick.AddListener(OnButtonClicked);
        }

        public virtual void Select()
        {
            if (!IsSelected)
            {
                IsSelected = true;
                inspectorVisibility.SetVisible(selectedBackdrop, true);

                if (!animateSelectionLayout) return;
                iconImage.rectTransform.DoSizeDelta(selectedIconSize, 0.2f).SetEasing(EasingType.SineInOut);
                iconImage.rectTransform.DoAnchorPosition(selectedIconPosition, 0.2f).SetEasing(EasingType.SineInOut);

                pageNameText.DoAlpha(1, 0.2f);
            }
        }
        // phương thức này được gọi khi một item khác được chọn, và item này sẽ bị bỏ chọn. Nó sẽ thay đổi trạng thái IsSelected thành false, và thực hiện các hiệu ứng chuyển đổi kích thước và vị trí của icon, cũng như làm mờ text của tên trang.
        public virtual void Deselect()
        {
            inspectorVisibility.SetVisible(selectedBackdrop, false);
            if (IsSelected)
            {
                IsSelected = false;

                if (!animateSelectionLayout) return;
                iconImage.rectTransform.DoSizeDelta(unselectedIconSize, 0.2f).SetEasing(EasingType.SineInOut);
                iconImage.rectTransform.DoAnchorPosition(unselectedIconPosition, 0.2f).SetEasing(EasingType.SineInOut);

                pageNameText.DoAlpha(inactiveLabelAlpha, 0.2f);
            }
        }

        protected virtual void OnButtonClicked()
        {
            if (GameController.MainMenuScreenBehavior != null && !GameController.MainMenuScreenBehavior.IsDockEnabled) return;
            if (!IsSelected)
            {


                Select();
                onSelected?.Invoke(pageType);
            }
        }
    }
}
