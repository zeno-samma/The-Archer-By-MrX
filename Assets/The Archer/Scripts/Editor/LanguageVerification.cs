using System;
using System.Linq;
using System.Text.RegularExpressions;
using MrX.Abilities;
using MrX.Localization;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class LanguageVerification
    {
        private static readonly Regex placeholder = new Regex(@"\{(\d+)(?::loc)?\}");

        public static string Verify()
        {
            var table = Resources.Load<MenuTranslations>("Menu Translations");
            Require(table != null && table.latinFont != null, "Missing translation table or font");
            Require(MenuLanguage.Codes.Contains("en") && MenuLanguage.Codes.Contains("vi"), "English and Vietnamese must be selectable");

            foreach (var entry in table.entries)
            {
                Require(!string.IsNullOrWhiteSpace(entry.english) && !string.IsNullOrWhiteSpace(entry.vietnamese),
                    "Missing English or Vietnamese: " + entry.english);
                var sourceArguments = Arguments(entry.english);
                foreach (string code in MenuLanguage.Codes.Where(code => code != "en"))
                {
                    string translated = entry.Get(code);
                    if (!string.IsNullOrWhiteSpace(translated))
                        Require(sourceArguments.SequenceEqual(Arguments(translated)), "Wrong placeholders: " + entry.english + " (" + code + ")");
                }
            }

            foreach (char character in table.entries.SelectMany(entry => entry.vietnamese).Distinct())
                Require(char.IsControl(character) || table.latinFont.HasCharacter(character), "Vietnamese font is missing: " + character);

            Require(table.Translate("START", "vi") == "BẮT ĐẦU", "Static menu label");
            Require(table.Translate("Stage 3", "vi") == "Màn 3", "Dynamic menu label");
            Require(table.Translate("Intermediate\nLocked", "vi") == "Trung bình\nĐã khóa", "Multiline difficulty label");
            Require(table.Translate("Clear Secrets Pyramid on Hard to unlock.", "vi") == "Vượt Kim tự tháp bí ẩn ở mức Khó để mở khóa.", "Localized stage name inside unlock message");
            Require(table.Translate("1984", "vi") == "1984", "Numeric values remain intact");
            Require(table.Translate("Unknown Item Name", "vi") == "Unknown Item Name", "Unknown contents use English");
            foreach (string code in MenuLanguage.Codes.Where(code => code != "en" && code != "vi"))
            {
                var untranslated = table.entries.FirstOrDefault(entry =>
                    string.IsNullOrWhiteSpace(entry.Get(code)) && !placeholder.IsMatch(entry.english));
                if (untranslated != null)
                    Require(table.Translate(untranslated.english, code) == untranslated.english, "Incomplete locales use English");
            }
            Require(table.Translate("Intermediate\nLocked", "en") == "Intermediate\nLocked", "English rendering preserves source layout");

            var stages = AssetDatabase.LoadAssetAtPath<StageDatabase>("Assets/The Archer/Scriptables/Stages/Stage Database.asset");
            foreach (var stage in stages.Stages)
            {
                VerifyVietnamese(table, stage.StageName);
                VerifyVietnamese(table, stage.StageObjective);
            }

            var abilities = AssetDatabase.LoadAssetAtPath<AbilitiesDatabase>("Assets/The Archer/Scriptables/Abilities/Abilities Database.asset");
            for (int i = 0; i < abilities.AbilitiesCount; i++)
            {
                var ability = abilities.GetAbility(i);
                VerifyVietnamese(table, ability.Title);
                VerifyVietnamese(table, ability.Description);
            }

            return $"Verified {table.entries.Count} translations, {abilities.AbilitiesCount} abilities and {stages.StagesAmount} stages.";
        }

        private static string[] Arguments(string value) =>
            placeholder.Matches(value).Cast<Match>().Select(token => token.Groups[1].Value).Distinct().OrderBy(value => value).ToArray();

        private static void VerifyVietnamese(MenuTranslations table, string source)
        {
            Require(!string.IsNullOrWhiteSpace(source) && table.Translate(source, "vi") != source, "Missing Vietnamese: " + source);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
