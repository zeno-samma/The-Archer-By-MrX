using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MrX.Save;
using TMPro;
using UnityEngine;

namespace MrX.Localization
{
    [Serializable]
    public sealed class LanguageSave : ISave
    {
        public string code = "en";
        public void Flush() { }
    }

    [Serializable]
    public sealed class TranslationEntry
    {
        public string english;
        [TextArea] public string vietnamese, spanish, portuguese, french;

        public string Get(string code)
        {
            switch (code)
            {
                case "vi": return vietnamese;
                case "es": return spanish;
                case "pt": return portuguese;
                case "fr": return french;
                default: return english;
            }
        }
    }

    [CreateAssetMenu(menuName = "MrX Studio/Localization/Menu Translations")]
    public sealed class MenuTranslations : ScriptableObject
    {
        public TMP_FontAsset latinFont;
        public List<TranslationEntry> entries = new List<TranslationEntry>();

        private sealed class Format
        {
            public Regex Pattern;
            public TranslationEntry Entry;
            public int LiteralLength;
            public HashSet<string> LocalizedArguments;
        }

        private static readonly Regex placeholder = new Regex(@"\{(\d+)(:loc)?\}");
        private static readonly Regex whitespace = new Regex(@"\s+");
        private Dictionary<string, TranslationEntry> exact;
        private readonly List<Format> formats = new List<Format>();

        public void Rebuild()
        {
            exact = new Dictionary<string, TranslationEntry>(StringComparer.OrdinalIgnoreCase);
            formats.Clear();
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.english)) continue;
                string key = Normalize(entry.english);
                var tokens = placeholder.Matches(key);
                if (tokens.Count == 0)
                {
                    exact[key] = entry;
                    continue;
                }

                var pattern = new StringBuilder("^");
                var arguments = new HashSet<string>();
                var localized = new HashSet<string>();
                int offset = 0;
                int literalLength = key.Length;
                foreach (Match token in tokens)
                {
                    string index = token.Groups[1].Value;
                    pattern.Append(Regex.Escape(key.Substring(offset, token.Index - offset)));
                    pattern.Append(arguments.Add(index) ? $"(?<arg{index}>.+?)" : $@"\k<arg{index}>");
                    if (token.Groups[2].Success) localized.Add(index);
                    offset = token.Index + token.Length;
                    literalLength -= token.Length;
                }

                pattern.Append(Regex.Escape(key.Substring(offset))).Append('$');
                formats.Add(new Format
                {
                    Pattern = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                    Entry = entry,
                    LiteralLength = literalLength,
                    LocalizedArguments = localized
                });
            }

            formats.Sort((a, b) => b.LiteralLength.CompareTo(a.LiteralLength));
        }

        public string Translate(string source, string code)
        {
            if (string.IsNullOrEmpty(source) || code == "en" || !ContainsWords(source)) return source;
            if (exact == null) Rebuild();
            string key = Normalize(source);

            if (exact.TryGetValue(key, out var entry))
                return GetTranslation(entry, source, code);

            foreach (var format in formats)
            {
                var match = format.Pattern.Match(key);
                if (!match.Success) continue;

                string translated = format.Entry.Get(code);
                if (string.IsNullOrWhiteSpace(translated)) return source;
                return placeholder.Replace(translated, token =>
                {
                    string index = token.Groups[1].Value;
                    var value = match.Groups["arg" + index];
                    if (!value.Success) return token.Value;
                    if (format.LocalizedArguments.Contains(index) && exact.TryGetValue(Normalize(value.Value), out var argument))
                        return GetTranslation(argument, value.Value, code);
                    return value.Value;
                });
            }

            if (source.IndexOf('\n') < 0) return source;
            string[] lines = source.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                lines[i] = Translate(lines[i], code);
            return string.Join("\n", lines);
        }

        private static string GetTranslation(TranslationEntry entry, string fallback, string code)
        {
            string translated = entry.Get(code);
            return string.IsNullOrWhiteSpace(translated) ? fallback : translated;
        }

        private static bool ContainsWords(string text)
        {
            foreach (char character in text)
                if (char.IsLetter(character)) return true;
            return false;
        }

        private static string Normalize(string text) => whitespace.Replace(text.Normalize(), " ").Trim();
        private void OnValidate() => exact = null;
    }

    public static class MenuLanguage
    {
        public static readonly string[] Codes = { "en", "vi", "es", "pt", "fr" };
        public static readonly string[] Names = { "English", "Tiếng Việt", "Español", "Português", "Français" };
        public static event Action Changed;
        public static MenuTranslations Table { get; private set; }
        public static string Code { get; private set; } = "en";

        private static LanguageSave save;
        private static ISaveManager persistence;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Changed = null;
            Table = null;
            Code = "en";
            save = null;
            persistence = null;
        }

        public static void Initialize(ISaveManager saves = null, MenuTranslations table = null)
        {
            if (save != null) return;
            persistence = saves ?? GameController.SaveManager;
            if (persistence == null) return;

            Table = table ?? Resources.Load<MenuTranslations>("Menu Translations");
            save = persistence.GetSave<LanguageSave>("Language Settings");
            Code = Array.IndexOf(Codes, save.code) >= 0 ? save.code : "en";
        }

        public static bool Select(string code)
        {
            Initialize();
            if (save == null || Array.IndexOf(Codes, code) < 0 || Code == code) return false;

            Code = save.code = code;
            persistence.Save(false);
            Changed?.Invoke();
            return true;
        }

        public static string Text(string value) => Table != null ? Table.Translate(value, Code) : value;
    }
}
