using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class LoadingScreenProgress : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text progressText;

        private int displayedPercentage = -1;

        private void Awake()
        {
            slider.interactable = false;
            SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            slider.SetValueWithoutNotify(Mathf.Lerp(slider.minValue, slider.maxValue, progress));

            int percentage = Mathf.FloorToInt(progress * 100f);
            if (percentage == displayedPercentage) return;

            displayedPercentage = percentage;
            progressText.text = $"Loading... {percentage}%";
        }
    }
}
