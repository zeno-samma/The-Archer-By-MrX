using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MrX.Save;
using TMPro;
using UnityEngine;

namespace MrX.Localization
{
    [Serializable] public sealed class LanguageSave : ISave { public string code="en"; public void Flush(){} }
    [Serializable] public sealed class TranslationEntry
    { public string english; [TextArea] public string vietnamese, spanish, portuguese, french;
        public string Get(string code) { return code=="vi"?vietnamese:code=="es"?spanish:code=="pt"?portuguese:code=="fr"?french:english; } }
    [CreateAssetMenu(menuName="MrX Studio/Localization/Menu Translations")]
    public sealed class MenuTranslations : ScriptableObject
    {
        public TMP_FontAsset latinFont;
        public List<TranslationEntry> entries=new List<TranslationEntry>();
        private Dictionary<string,TranslationEntry> exact;
        private readonly List<KeyValuePair<Regex,TranslationEntry>> formats=new List<KeyValuePair<Regex,TranslationEntry>>();
        public void Rebuild()
        {
            exact=new Dictionary<string,TranslationEntry>(StringComparer.OrdinalIgnoreCase);formats.Clear();
            foreach(var entry in entries)
            {
                if(entry==null||string.IsNullOrEmpty(entry.english))continue;
                string key = Normalize(entry.english);
                if (Regex.IsMatch(key, @"\{\d+\}"))
                {
                    string pattern = Regex.Escape(key);
                    foreach (Match token in Regex.Matches(key, @"\{\d+\}")) pattern = pattern.Replace(Regex.Escape(token.Value), "(.+?)");
                    formats.Add(new KeyValuePair<Regex,TranslationEntry>(new Regex("^"+pattern+"$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant),entry));
                }
                else exact[key]=entry;
            }
            formats.Sort((a,b) => Regex.Replace(b.Value.english, @"\{\d+\}", "").Length.CompareTo(Regex.Replace(a.Value.english, @"\{\d+\}", "").Length));
        }
        public string Translate(string source,string code)
        {
            if(string.IsNullOrEmpty(source)||code=="en")return source;if(exact==null)Rebuild();
            var key = Normalize(source);
            if(exact.TryGetValue(key,out var e))return string.IsNullOrEmpty(e.Get(code))?source:e.Get(code);
            foreach(var pair in formats)
            {
                var m=pair.Key.Match(key);if(!m.Success)continue;
                var t=pair.Value.Get(code);if(string.IsNullOrEmpty(t))return source;
                var tokens=Regex.Matches(Normalize(pair.Value.english), @"\{\d+\}");
                for(int i=0;i<tokens.Count;i++)t=t.Replace(tokens[i].Value,m.Groups[i+1].Value);
                return t;
            }
            return source;
        }
        private static string Normalize(string text) { return Regex.Replace(text, @"\s+", " ").Trim(); }
        private void OnValidate() { exact = null; }
    }
    public static class MenuLanguage
    {
        public static readonly string[] Codes={"en","vi","es","pt","fr"};
        public static readonly string[] Names={"English","Tiếng Việt","Español","Português","Français"};
        public static event Action Changed;
        public static MenuTranslations Table {get;private set;}
        public static string Code {get;private set;}="en";
        private static LanguageSave save; private static ISaveManager persistence;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset(){Changed=null;Table=null;Code="en";save=null;persistence=null;}
        public static void Initialize(ISaveManager saves=null,MenuTranslations table=null)
        {
            if(save!=null)return;persistence=saves??GameController.SaveManager;if(persistence==null)return;
            Table=table??Resources.Load<MenuTranslations>("Menu Translations");save=persistence.GetSave<LanguageSave>("Language Settings");
            Code=Array.IndexOf(Codes,save.code)>=0?save.code:"en";
        }
        public static bool Select(string code)
        {
            Initialize();if(save==null||Array.IndexOf(Codes,code)<0||Code==code)return false;
            Code=save.code=code;persistence.Save(false);Changed?.Invoke();return true;
        }
        public static string Text(string value){return Table!=null?Table.Translate(value,Code):value;}
    }
}
