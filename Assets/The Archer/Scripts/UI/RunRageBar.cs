using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class RunRageBar : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image fill;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color chargingColor = Color.white;
        [SerializeField] private Color activeColor = new Color(1f, 0.65f, 0.3f);
        private bool showingActive;
        private int displayedValue = -1;

        public void ShowCharge(int percent)
        {
            gameObject.SetActive(true);
            fill.fillAmount = percent / 100f;
            if (!showingActive && displayedValue == percent) return;

            showingActive = false;
            displayedValue = percent;
            fill.color = label.color = chargingColor;
            icon.color = chargingColor;
            label.text = $"Rage {percent}%";
        }

        public void ShowActive(float remaining, float duration)
        {
            fill.fillAmount = Mathf.Clamp01(remaining / duration);
            int tenths = Mathf.CeilToInt(remaining * 10f);
            if (showingActive && displayedValue == tenths) return;

            showingActive = true;
            displayedValue = tenths;
            fill.color = label.color = activeColor;
            icon.color = activeColor;
            label.text = $"Rage! {tenths / 10f:0.0}s";
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
