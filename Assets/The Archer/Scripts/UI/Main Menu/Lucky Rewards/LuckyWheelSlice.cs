using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class LuckyWheelSlice : MonoBehaviour
    {
        public Image sector, icon;
        public TMP_Text label;
        public void Bind(float start, float fraction, Sprite sprite, string text, int index)
        {
            sector.fillAmount = fraction; sector.rectTransform.localEulerAngles = new Vector3(0, 0, -start);
            sector.color = index % 2 == 0 ? new Color(.89f, .96f, 1) : new Color(.27f, .57f, 1);
            float angle = (start + fraction * 180) * Mathf.Deg2Rad;
            var position = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            icon.sprite = sprite; icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = Vector2.one * .5f + position * .31f;
            icon.rectTransform.anchoredPosition = Vector2.zero;
            // Percentages belong to the reward list, not the decorative wheel.
            if (label) label.gameObject.SetActive(false);
        }
    }
}
