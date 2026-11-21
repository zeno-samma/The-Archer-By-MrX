using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.Progression
{
    public sealed class PlayerProgressionHeader : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text xpLabel;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Image xpFill;

        private void OnEnable()
        {
            PlayerProgression.Changed += Refresh;
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            PlayerProgression.Changed -= Refresh;
        }

        public void Refresh()
        {
            var progress = PlayerProgression.Current;
            float fraction = progress.IsMaxLevel ? 1f : (float)progress.XpIntoLevel / progress.XpToNextLevel;

            if (levelLabel != null)
                levelLabel.text = progress.Level.ToString();
            if (xpLabel != null)
                xpLabel.text = progress.IsMaxLevel ? "MAX" : $"{progress.XpIntoLevel}/{progress.XpToNextLevel}";

            if (xpSlider != null)
            {
                xpSlider.SetValueWithoutNotify(Mathf.Lerp(xpSlider.minValue, xpSlider.maxValue, fraction));
            }
            else if (xpFill != null)
            {
                xpFill.fillAmount = fraction;
            }
        }
    }
}
