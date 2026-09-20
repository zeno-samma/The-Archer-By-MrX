using OctoberStudio.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class SmallRewardCard : MonoBehaviour
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text amountText;

        public void SetIcon(Sprite icon)
        {
            iconImage.sprite = icon;
        }

        public void SetAmount(int amount)
        {
            amountText.gameObject.SetActive(amount > 1);
            amountText.text = $"x{amount}";
        }

        public virtual void Show(float delay)
        {
            transform.localScale = new Vector3(0, 0.9f, 1f);
            transform.DoLocalScale(Vector3.one, 0.3f)
                .SetDelay(delay)
                .SetUnscaledTime(true)
                .SetEasing(EasingType.SineOut);
        }
    }
}