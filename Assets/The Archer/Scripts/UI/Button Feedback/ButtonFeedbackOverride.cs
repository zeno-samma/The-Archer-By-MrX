using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI.Feedback
{
    [AddComponentMenu("UI/Button Feedback Override")]
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class ButtonFeedbackOverride : MonoBehaviour
    {
        [Tooltip("Optional child to scale while keeping the button's own hit area.")]
        [SerializeField] private RectTransform visual;
        [SerializeField] private ButtonFeedbackStyle style = new ButtonFeedbackStyle();

        public RectTransform Visual => visual;
        public ButtonFeedbackStyle Style => style;
    }
}
