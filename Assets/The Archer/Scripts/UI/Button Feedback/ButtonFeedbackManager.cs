using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MrX.UI.Feedback
{
    [Serializable]
    public sealed class ButtonFeedbackStyle
    {
        [Tooltip("Enable feedback for buttons using this style.")]
        public bool enabled = true;
        [Min(1f)] public float hoverScale = 1.08f;
        [Range(.5f, 1f)] public float clickScale = .94f;
        [Min(.01f)] public float responseTime = .07f;
        [Min(0f)] public float clickDuration = .13f;
        public bool brightenOnHover = true;
        [Range(1f, 1.8f)] public float hoverBrightness = 1.28f;
    }

    [AddComponentMenu("UI/Button Feedback Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public sealed class ButtonFeedbackManager : MonoBehaviour
    {
        [SerializeField] private ButtonFeedbackStyle defaults = new ButtonFeedbackStyle();

        private sealed class ButtonState
        {
            public Button button;
            public RectTransform visual;
            public Graphic graphic;
            public ButtonFeedbackOverride localOverride;
            public ButtonFeedbackStyle style;
            public Vector3 restingScale;
            public Vector3 appliedScale;
            public float multiplier = 1f;
            public float clickUntil;
            public Material originalMaterial;
            public Material hoverMaterial;
            public float appliedBrightness;
            public UnityAction click;
        }

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private readonly List<RaycastResult> hits = new List<RaycastResult>();
        private readonly Dictionary<Button, ButtonState> tracked = new Dictionary<Button, ButtonState>();
        private readonly List<Button> finished = new List<Button>();
        private EventSystem eventSystem;
        private PointerEventData pointer;
        private Button touchButton;

        private void LateUpdate()
        {
            if (eventSystem != EventSystem.current)
            {
                eventSystem = EventSystem.current;
                pointer = eventSystem != null ? new PointerEventData(eventSystem) : null;
            }

            Button hovered = null;
            Button selected = null;
            if (pointer != null && Application.isFocused)
            {
                if (Mouse.current != null) hovered = FindButton(Mouse.current.position.ReadValue());
                var touch = Touchscreen.current?.primaryTouch;
                if (touch != null && touch.press.wasPressedThisFrame)
                    touchButton = FindButton(touch.position.ReadValue());
                else if (touch == null || !touch.press.isPressed)
                    touchButton = null;

                var selection = eventSystem.currentSelectedGameObject;
                if (selection != null) selected = selection.GetComponent<Button>();
            }
            else touchButton = null;

            Track(hovered);
            Track(touchButton);
            Track(selected);
            finished.Clear();
            foreach (var pair in tracked)
            {
                var state = pair.Value;
                if (!IsUsable(state.button) || !state.visual || !Application.isFocused)
                {
                    finished.Add(pair.Key);
                    continue;
                }

                var activeOverride = GetActiveOverride(state.localOverride);
                state.style = activeOverride != null ? activeOverride.Style : defaults;
                if (!state.style.enabled || state.visual != GetVisual(state.button, activeOverride))
                {
                    finished.Add(pair.Key);
                    continue;
                }

                bool isHovered = state.button == hovered;
                bool clicked = Time.unscaledTime < state.clickUntil;
                var multiplier = clicked ? state.style.clickScale : isHovered ? state.style.hoverScale : 1f;
                UpdateVisual(state, multiplier, isHovered);
                if (!isHovered && !clicked && state.button != selected && state.button != touchButton && state.multiplier == 1f)
                    finished.Add(pair.Key);
            }
            foreach (var button in finished) Release(button, tracked[button]);
        }

        private Button FindButton(Vector2 position)
        {
            if (position.x < 0f || position.y < 0f || position.x >= Screen.width || position.y >= Screen.height)
                return null;
            pointer.position = position;
            hits.Clear();
            eventSystem.RaycastAll(pointer, hits);
            if (hits.Count == 0) return null;
            var receiver = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            return receiver != null ? receiver.GetComponent<Button>() : null;
        }

        private void Track(Button button)
        {
            if (!IsUsable(button) || tracked.ContainsKey(button)) return;
            var localOverride = button.GetComponent<ButtonFeedbackOverride>();
            var activeOverride = GetActiveOverride(localOverride);
            var style = activeOverride != null ? activeOverride.Style : defaults;
            if (!style.enabled) return;

            var visual = GetVisual(button, activeOverride);
            var state = new ButtonState
            {
                button = button,
                visual = visual,
                graphic = button.targetGraphic,
                localOverride = localOverride,
                style = style,
                restingScale = visual.localScale,
                appliedScale = visual.localScale
            };
            state.click = () => state.clickUntil = Time.unscaledTime + state.style.clickDuration;
            button.onClick.AddListener(state.click);
            tracked.Add(button, state);
        }

        private static ButtonFeedbackOverride GetActiveOverride(ButtonFeedbackOverride localOverride) =>
            localOverride != null && localOverride.isActiveAndEnabled ? localOverride : null;

        private static RectTransform GetVisual(Button button, ButtonFeedbackOverride activeOverride) =>
            activeOverride != null && activeOverride.Visual != null ? activeOverride.Visual : (RectTransform)button.transform;

        private static bool IsUsable(Button button) => button != null && button.isActiveAndEnabled && button.IsInteractable();

        private static void UpdateVisual(ButtonState state, float target, bool hovered)
        {
            if (state.visual.localScale != state.appliedScale)
                state.restingScale = state.visual.localScale;
            float step = Time.unscaledDeltaTime / Mathf.Max(.01f, state.style.responseTime);
            state.multiplier = Mathf.Lerp(state.multiplier, target, step);
            if (Mathf.Abs(state.multiplier - target) < .001f) state.multiplier = target;
            state.appliedScale = state.restingScale * state.multiplier;
            state.visual.localScale = state.appliedScale;
            UpdateMaterial(state, hovered && state.style.brightenOnHover ? state.style.hoverBrightness : 1f);
        }

        private static void UpdateMaterial(ButtonState state, float brightness)
        {
            if (!state.graphic) return;
            if (state.hoverMaterial && state.graphic.material == state.hoverMaterial && state.appliedBrightness == brightness)
                return;
            RestoreMaterial(state);
            var original = state.graphic.material;
            if (brightness <= 1f || !original || !original.HasProperty(ColorProperty)) return;

            var tint = original.GetColor(ColorProperty);
            var material = new Material(original) { hideFlags = HideFlags.DontSave };
            material.SetColor(ColorProperty, new Color(tint.r * brightness, tint.g * brightness, tint.b * brightness, tint.a));
            state.originalMaterial = original == state.graphic.defaultMaterial ? null : original;
            state.hoverMaterial = material;
            state.appliedBrightness = brightness;
            state.graphic.material = material;
        }

        private static void RestoreMaterial(ButtonState state)
        {
            if (!state.hoverMaterial) return;
            if (state.graphic && state.graphic.material == state.hoverMaterial) state.graphic.material = state.originalMaterial;
            Destroy(state.hoverMaterial);
            state.hoverMaterial = null;
        }

        private void Release(Button button, ButtonState state)
        {
            if (button) button.onClick.RemoveListener(state.click);
            if (state.visual && state.visual.localScale == state.appliedScale) state.visual.localScale = state.restingScale;
            RestoreMaterial(state);
            tracked.Remove(button);
        }

        private void OnDisable()
        {
            touchButton = null;
            finished.Clear();
            finished.AddRange(tracked.Keys);
            foreach (var button in finished) Release(button, tracked[button]);
        }
    }
}
