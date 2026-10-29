using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public class MainMenuStageIcon : HorizontalSelectable
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image lockImage;
        [SerializeField] private AspectRatioFitter iconAspect;
        private readonly InspectorVisibility authoredVisibility = new InspectorVisibility();

        protected override void Awake()
        {
            base.Awake();
            if (lockImage != null) authoredVisibility.Capture(lockImage.gameObject);
        }

        public virtual void SetIcon(Sprite icon, bool isLocked)
        {
            iconImage.sprite = icon;
            if (iconAspect != null && icon != null) iconAspect.aspectRatio = icon.rect.width / icon.rect.height;
            if (lockImage != null) authoredVisibility.SetVisible(lockImage.gameObject, isLocked);
        }
    }
}
