using MrX.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public class SmallRewardCard : MonoBehaviour
    {
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();
        protected readonly InspectorVisibility inspectorVisibility = new InspectorVisibility();

        protected virtual void Awake()
        {
            animationDefaults.Capture(transform);
            inspectorVisibility.Capture(iconImage);
            if (amountText != null) inspectorVisibility.Capture(amountText.gameObject);
        }

        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text amountText;

        public void SetIcon(Sprite icon)
        {
            iconImage.sprite = icon;
            inspectorVisibility.SetVisible(iconImage, icon != null);
        }

        public void SetAmount(int amount)
        {
            inspectorVisibility.SetVisible(amountText.gameObject, amount > 1);
            amountText.text = $"x{amount}";
        }

        public virtual void Show(float delay)
        {
            transform.localScale = animationDefaults.Scale(transform, new Vector3(0, 0.9f, 1f));
            transform.DoLocalScale(animationDefaults.Scale(transform), 0.3f)
                .SetDelay(delay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut);
        }
    }
}
