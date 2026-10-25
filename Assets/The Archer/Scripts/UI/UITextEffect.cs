using System;
using System.Globalization;
using MrX.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MrX.UI
{
    public enum UITextEffectStyle
    {
        CountNumber = 0, Bounce = 1, Pulse = 2, Shake = 3, FadeIn = 4, Typewriter = 5,
        Wave = 6, Rainbow = 7, Pop = 8, SlideIn = 9, Float = 10, ColorPulse = 11
    }
    public enum UITextNumberFormat { Number = 0, MinutesSeconds = 1 }

    [DisallowMultipleComponent, RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("MrX Studio/UI/UI Text Effect")]
    public sealed class UITextEffect : MonoBehaviour
    {
        [SerializeField] private UITextEffectStyle effect = UITextEffectStyle.Bounce;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Min(.05f)] private float duration = 1.2f;
        [SerializeField, Min(0)] private float delay;
        [SerializeField] private bool loop;
        [SerializeField, Min(0)] private float loopPause = .2f;
        [SerializeField] private EasingType easing = EasingType.CubicOut;
        [SerializeField, Range(10, 120)] private int updatesPerSecond = 30;
        [SerializeField, Min(0)] private float amplitude = 12;
        [SerializeField, Min(.05f)] private float frequency = 3;
        [SerializeField, Range(0, 1)] private float scaleAmount = .18f;
        [SerializeField] private bool perCharacter;
        [SerializeField, Range(0, 2)] private float characterPhase = .45f;
        [SerializeField] private Vector2 slideOffset = new Vector2(0, -35);
        [SerializeField] private Color accentColor = new Color(1, .65f, .15f);
        [SerializeField] private bool readNumberFromText = true;
        [SerializeField] private double numberFrom;
        [SerializeField] private double numberTo = 1000;
        [SerializeField] private UITextNumberFormat numberDisplay;
        [SerializeField] private string numberFormat = "#,0.##";
        [SerializeField] private string prefix = "", suffix = "";
        [SerializeField] private UnityEvent onComplete = new UnityEvent();

        private sealed class MeshCopy { public Vector3[] vertices; public Color32[] colors; }
        private MeshCopy[] copies;
        private TextMeshProUGUI label;
        private UITextEffectStyle activeEffect;
        private float clock, untilDraw;
        private double from, to;
        private int originalVisible, revealCount;
        private string originalText, writtenText;
        private string activeNumberFormat;
        private bool running, pending, captured, subscribed;
        private float progress;
        public bool IsPlaying => running || pending;
        public event Action Completed;
        public string Prefix {get=>prefix;set=>prefix=value??"";}
        public string Suffix {get=>suffix;set=>suffix=value??"";}
        public UITextEffectStyle Effect { get => effect; set { Stop(); effect = value; } }
        private bool MeshEffect => activeEffect != UITextEffectStyle.CountNumber && activeEffect != UITextEffectStyle.Typewriter;

        private void Awake() { Cache(); }
        private void OnEnable() { pending = Application.isPlaying && playOnEnable; }
        private bool Cache() { if (label == null) label = GetComponent<TextMeshProUGUI>(); return label != null; }
        private void LateUpdate()
        {
            if (pending) Play();
            if (running) Advance(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        public void Play() { Begin(effect, false, 0, 0); }
        public void PlayText(string text) { if (!Cache()) return; Stop(); label.text = text; Play(); }
        public void CountTo(float target) { PlayNumber(numberFrom, target); }
        public void PlayNumber(double start, double target)
        {
            if (!Finite(start) || !Finite(target)) return;
            Begin(UITextEffectStyle.CountNumber, true, start, target);
        }
        private void Begin(UITextEffectStyle style, bool explicitNumber, double start, double target)
        {
            if (!Cache() || !isActiveAndEnabled) return;
            bool repeatCount = running && activeEffect == UITextEffectStyle.CountNumber && label.text == writtenText;
            double previousTarget = to;
            Stop(); pending = false;
            originalText = label.text; originalVisible = label.maxVisibleCharacters; captured = true;
            activeEffect = style; clock = -Mathf.Max(0, delay); untilDraw = 0; progress = 0;
            activeNumberFormat = numberFormat;
            try { 0d.ToString(activeNumberFormat); } catch (FormatException) { activeNumberFormat = "#,0.##"; }
            from = explicitNumber ? start : (Finite(numberFrom) ? numberFrom : 0);
            to = explicitNumber ? target : (Finite(numberTo) ? numberTo : 0);
            if (!explicitNumber && activeEffect == UITextEffectStyle.CountNumber && readNumberFromText)
            {
                if (repeatCount) to = previousTarget;
                else if (TryReadNumber(originalText, out double parsed)) to = parsed;
            }
            running = true;
            if (MeshEffect)
            {
                label.OnPreRenderText += BeforeRender; subscribed = true;
            }
            label.ForceMeshUpdate();
            revealCount = Mathf.Min(label.textInfo.characterCount, Mathf.Max(0, originalVisible));
            Draw(0);
        }
        private static bool Finite(double number) => !double.IsNaN(number) && !double.IsInfinity(number);
        public static string FormatTime(double seconds)
        {
            if (!Finite(seconds)) return "--:--";
            long hundredths = (long)Math.Round(Math.Max(0, seconds) * 100);
            return (hundredths / 6000).ToString("00") + ":" + ((hundredths % 6000) / 100.0).ToString("00.00");
        }
        private bool TryReadNumber(string text, out double value)
        {
            text = text.Trim();
            if (!string.IsNullOrEmpty(prefix) && text.StartsWith(prefix, StringComparison.Ordinal)) text = text.Substring(prefix.Length);
            if (!string.IsNullOrEmpty(suffix) && text.EndsWith(suffix, StringComparison.Ordinal)) text = text.Substring(0, text.Length - suffix.Length);
            if (numberDisplay == UITextNumberFormat.MinutesSeconds)
            {
                var parts = text.Split(':'); value = 0;
                if (parts.Length != 2 || !double.TryParse(parts[0], NumberStyles.Number, CultureInfo.CurrentCulture, out double minutes)
                    || !double.TryParse(parts[1], NumberStyles.Number, CultureInfo.CurrentCulture, out double seconds)) return false;
                value = minutes * 60 + seconds; return Finite(value);
            }
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value) && Finite(value);
        }
        private void Advance(float delta)
        {
            if (!running) return;
            clock += Mathf.Max(0, delta);
            if (clock < 0) return;
            float length = Mathf.Max(.05f, duration);
            float cycle = length + Mathf.Max(0, loopPause);
            if (loop && clock >= cycle) clock %= cycle;
            untilDraw -= delta;
            if (untilDraw > 0 && clock < length) return;
            untilDraw = 1f / Mathf.Clamp(updatesPerSecond, 10, 120);
            progress = Mathf.Clamp01(clock / length);
            Draw(progress);
            if (!loop && clock >= length) Finish(true);
        }
        private void Draw(float t)
        {
            float eased = EasingFunctions.ApplyEasing(t, easing);
            if (activeEffect == UITextEffectStyle.CountNumber)
            {
                double value = t >= 1 ? to : from * (1 - Mathf.Clamp01(eased)) + to * Mathf.Clamp01(eased);
                string number;
                if (numberDisplay == UITextNumberFormat.MinutesSeconds) number = FormatTime(value);
                else number = value.ToString(activeNumberFormat);
                writtenText = prefix + number + suffix;
                if (label.text != writtenText) label.text = writtenText;
            }
            else if (activeEffect == UITextEffectStyle.Typewriter)
            {
                // TMP counts parsed characters, not rich-text tag bytes.
                if (label.havePropertiesChanged) label.ForceMeshUpdate();
                revealCount = Mathf.Min(label.textInfo.characterCount, Mathf.Max(0, originalVisible));
                label.maxVisibleCharacters = t >= 1 ? originalVisible : Mathf.FloorToInt(revealCount * Mathf.Clamp01(eased));
            }
            else
            {
                progress = t;
                if (label.havePropertiesChanged || copies == null) label.ForceMeshUpdate();
                if (copies != null) { ApplyMesh(label.textInfo); label.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32); }
            }
        }
        private void BeforeRender(TMP_TextInfo info)
        {
            if (!running || !MeshEffect) return;
            if (copies == null || copies.Length < info.meshInfo.Length) Array.Resize(ref copies, info.meshInfo.Length);
            for (int m = 0; m < info.meshInfo.Length; m++)
            {
                var mesh = info.meshInfo[m]; int count = mesh.vertices.Length;
                if (copies[m] == null) copies[m] = new MeshCopy();
                if (copies[m].vertices == null || copies[m].vertices.Length < count)
                { copies[m].vertices = new Vector3[count]; copies[m].colors = new Color32[count]; }
                Array.Copy(mesh.vertices, copies[m].vertices, count);
                Array.Copy(mesh.colors32, copies[m].colors, count);
            }
            ApplyMesh(info);
        }
        private void ApplyMesh(TMP_TextInfo info)
        {
            if (copies == null) return;
            float t = progress, phase = Mathf.Max(0, clock) * frequency * Mathf.PI * 2;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float eased = EasingFunctions.ApplyEasing(t, easing);
            var center = label.textBounds.center;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible) continue;
                int material = ch.materialReferenceIndex, vertex = ch.vertexIndex;
                if (material >= copies.Length || copies[material] == null || vertex + 3 >= copies[material].vertices.Length) continue;
                var copy = copies[material]; var mesh = info.meshInfo[material];
                var pivot = perCharacter ? (copy.vertices[vertex] + copy.vertices[vertex + 2]) * .5f : center;
                float p = phase + (perCharacter || activeEffect == UITextEffectStyle.Wave ? i * characterPhase : 0);
                Vector3 offset = Vector3.zero; float scale = 1, alpha = 1;
                switch (activeEffect)
                {
                    case UITextEffectStyle.Bounce: offset.y = Mathf.Abs(Mathf.Sin(p)) * amplitude * (1 - t); break;
                    case UITextEffectStyle.Pulse: scale += Mathf.Sin(p) * scaleAmount * envelope; break;
                    case UITextEffectStyle.Shake:
                        float seed = Mathf.Floor(Mathf.Max(0, clock) * frequency * 8);
                        float letter = perCharacter ? i : 0;
                        offset = new Vector3(Mathf.PerlinNoise(letter + 17, seed * .83f) * 2 - 1, Mathf.PerlinNoise(letter + 71, seed * .67f) * 2 - 1) * amplitude * envelope; break;
                    case UITextEffectStyle.FadeIn: alpha = Mathf.Clamp01(eased); break;
                    case UITextEffectStyle.Wave: offset.y = Mathf.Sin(p) * amplitude * envelope; break;
                    case UITextEffectStyle.Pop: scale = Mathf.Max(0, EasingFunctions.ApplyEasing(t, EasingType.BackOut)); break;
                    case UITextEffectStyle.SlideIn: offset = (Vector3)slideOffset * (1 - eased); break;
                    case UITextEffectStyle.Float: offset.y = Mathf.Sin(p) * amplitude * envelope; break;
                }
                for (int v = 0; v < 4; v++)
                {
                    int n = vertex + v;
                    mesh.vertices[n] = pivot + (copy.vertices[n] - pivot) * scale + offset;
                    Color color = copy.colors[n];
                    if (activeEffect == UITextEffectStyle.Rainbow)
                    {
                        var rainbow = Color.HSVToRGB(Mathf.Repeat(phase / (Mathf.PI * 2) + i * characterPhase * .1f, 1), .7f, 1);
                        color = Color.Lerp(color, new Color(rainbow.r, rainbow.g, rainbow.b, color.a), envelope);
                    }
                    else if (activeEffect == UITextEffectStyle.ColorPulse)
                        color = Color.Lerp(color, new Color(accentColor.r, accentColor.g, accentColor.b, color.a), (.5f + .5f * Mathf.Sin(p)) * envelope);
                    color.a *= alpha; mesh.colors32[n] = color;
                }
            }
        }
        public void Complete()
        {
            if (pending) Play();
            if (!running) { pending = false; return; }
            progress = 1; Draw(1); Finish(true);
        }
        public void Stop() { pending = false; Finish(false); }
        public void StopAndRestore()
        {
            Stop();
            if (captured && Cache()) { label.text = originalText; label.maxVisibleCharacters = originalVisible; label.ForceMeshUpdate(); }
        }
        private void Finish(bool completed)
        {
            bool wasRunning = running; running = false;
            if (!Cache()) return;
            if (subscribed) { label.OnPreRenderText -= BeforeRender; subscribed = false; }
            if (wasRunning && captured)
            {
                if (activeEffect == UITextEffectStyle.Typewriter) label.maxVisibleCharacters = originalVisible;
                if (MeshEffect) label.ForceMeshUpdate();
            }
            if (wasRunning && completed && Application.isPlaying) {onComplete.Invoke();Completed?.Invoke();}
        }
        private void OnDisable()
        {
            // Hidden pooled labels must not reinterpret an interrupted counter as a new target on reuse.
            if (running && activeEffect == UITextEffectStyle.CountNumber && label != null && label.text == writtenText) Draw(1);
            Stop();
        }
        private void OnDestroy() { if (label != null && subscribed) label.OnPreRenderText -= BeforeRender; }
#if UNITY_EDITOR
        public void EditorPreviewStep(float delta) { Advance(delta); }
        public void EditorPreviewRestore()
        {
            // Do not discard text the designer edited or undid during a preview.
            string edited = captured && Cache() && label.text != (activeEffect == UITextEffectStyle.CountNumber ? writtenText : originalText) ? label.text : null;
            StopAndRestore();
            if (edited != null && label != null) label.text = edited;
        }
#endif
    }
}
