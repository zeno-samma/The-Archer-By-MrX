using System.Collections.Generic;
using MrX.Localization;
using TMPro;
using UnityEngine;

namespace MrX.UI
{
    public sealed class LocalizedMenuScope : MonoBehaviour
    {
        private sealed class Binding
        {
            public TMP_FontAsset Font;
            public Material Material;
            public string Source;
            public string Displayed;
        }

        private readonly Dictionary<TMP_Text, Binding> bindings = new Dictionary<TMP_Text, Binding>();
        private readonly List<TMP_Text> removedLabels = new List<TMP_Text>();
        private bool started;

        private void Start()
        {
            started = true;
            Connect();
        }

        private void OnEnable()
        {
            if (started) Connect();
        }

        private void Connect()
        {
            MenuLanguage.Initialize();
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                Track(label);

            MenuLanguage.Changed += Refresh;
            Canvas.preWillRenderCanvases += ApplyChanges;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            Refresh();
        }

        private void OnDisable()
        {
            MenuLanguage.Changed -= Refresh;
            Canvas.preWillRenderCanvases -= ApplyChanges;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void Track(TMP_Text label)
        {
            if (bindings.ContainsKey(label) || label.GetComponentInParent<LocalizedMenuScope>(true) != this) return;
            var input = label.GetComponentInParent<TMP_InputField>(true);
            if (input != null && input.textComponent == label) return;

            bindings.Add(label, new Binding
            {
                Font = label.font,
                Material = label.fontSharedMaterial,
                Source = label.text
            });
        }

        private void OnTextChanged(Object changed)
        {
            // TMP sends this event during canvas rebuild. Register new labels here
            // and update their text and font together before the next rebuild.
            if (changed is TMP_Text label && label != null)
                Track(label);
        }

        private void Refresh()
        {
            foreach (var entry in bindings)
                if (entry.Key != null) Apply(entry.Key, entry.Value);
        }

        private void ApplyChanges()
        {
            foreach (var entry in bindings)
            {
                if (entry.Key == null)
                    removedLabels.Add(entry.Key);
                else if (entry.Key.text != entry.Value.Displayed)
                    Apply(entry.Key, entry.Value);
            }

            foreach (var label in removedLabels)
                bindings.Remove(label);
            removedLabels.Clear();
        }

        private static void Apply(TMP_Text label, Binding binding)
        {
            if (label.text != binding.Displayed) binding.Source = label.text;
            string translated = MenuLanguage.Text(binding.Source);
            binding.Displayed = translated;

            var translatedFont = translated != binding.Source ? MenuLanguage.Table?.latinFont : null;
            var font = translatedFont != null ? translatedFont : binding.Font;
            if (font != null && label.font != font)
            {
                label.font = font;
                // A preset for a different atlas would render the right text with the wrong glyphs.
                var preset = translatedFont == null ? binding.Material : null;
                var presetAtlas = preset != null ? preset.GetTexture(ShaderUtilities.ID_MainTex) : null;
                var defaultAtlas = font.material != null ? font.material.GetTexture(ShaderUtilities.ID_MainTex) : null;
                label.fontSharedMaterial = presetAtlas != null &&
                    (presetAtlas == font.atlasTexture || presetAtlas == defaultAtlas) ? preset : font.material;
            }

            if (label.text != translated) label.text = translated;
        }
    }
}
