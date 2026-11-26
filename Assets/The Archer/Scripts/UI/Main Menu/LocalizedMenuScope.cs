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

        private Binding Track(TMP_Text label)
        {
            if (bindings.TryGetValue(label, out var binding)) return binding;
            if (label.GetComponentInParent<LocalizedMenuScope>(true) != this) return null;
            var input = label.GetComponentInParent<TMP_InputField>(true);
            if (input != null && input.textComponent == label) return null;

            binding = new Binding
            {
                Font = label.font,
                Material = label.fontSharedMaterial,
                Source = label.text
            };
            bindings.Add(label, binding);
            return binding;
        }

        private void OnTextChanged(Object changed)
        {
            if (!(changed is TMP_Text label) || label == null) return;
            var binding = Track(label);
            if (binding != null && label.text != binding.Displayed)
                Apply(label, binding);
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
                if (translatedFont == null && binding.Material != null)
                    label.fontSharedMaterial = binding.Material;
            }

            if (label.text != translated) label.text = translated;
        }
    }
}
