using System;
using System.IO;
using System.Linq;
using MrX.Currency;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class MenuEconomyAuthoring
    {
        public const string PrefabPath = "Assets/The Archer/Prefabs/UI/Menu Pages/Menu Economy.prefab";
        private static ForgePageDatabase art;
        private static TMP_FontAsset font;
        public static readonly string[] Pages = {
            "Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab",
            "Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab",
            "Assets/The Archer/Prefabs/UI/Menu Pages/Upgrades Page.prefab",
            "Assets/The Archer/Prefabs/UI/Menu Pages/Forge Page.prefab",
            "Assets/The Archer/Prefabs/UI/Menu Pages/Daily Quests Page.prefab",
            "Assets/The Archer/Prefabs/UI/Menu Pages/Achievements Page.prefab" };

        [MenuItem("Tools/MrX Studio/Economy/Install Energy and Exchange Prefab")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required.");
            ImportTranslations();
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            font = Resources.Load<MenuTranslations>("Menu Translations").latinFont;
            var settings = Resources.Load<MenuEconomySettings>(MenuEconomySettings.ResourcePath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MenuEconomySettings>();
                settings.energyIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/UI/Main Menu/Speed.png");
                AssetDatabase.CreateAsset(settings, "Assets/The Archer/Resources/Menu Economy Settings.asset");
            }
            var currencies = AssetDatabase.LoadAssetAtPath<CurrenciesDatabase>("Assets/The Archer/Scriptables/Currencies Database.asset");
            var serialized = new SerializedObject(currencies); var list = serialized.FindProperty("currencies");
            if (currencies.GetCurrency("energy") == null)
            {
                int i = list.arraySize; list.InsertArrayElementAtIndex(i); var entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("id").stringValue = "energy"; entry.FindPropertyRelative("name").stringValue = "Energy";
                entry.FindPropertyRelative("icon").objectReferenceValue = settings.energyIcon;
                serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(currencies);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) prefab = Build(settings, currencies);
            foreach (var path in Pages)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    // Keep old references valid for page scripts, but replace their visible wallets.
                    foreach (var child in new[] {"Gold", "Gem", "Gold Wallet", "Gem Wallet"})
                    { var old = root.transform.Find(child); if (old != null) old.gameObject.SetActive(false); }
                    if (root.transform.Find("Top Bar/Gold Wallet") != null) root.transform.Find("Top Bar/Gold Wallet").gameObject.SetActive(false);
                    if (root.transform.Find("Top Bar/Gem Wallet") != null) root.transform.Find("Top Bar/Gem Wallet").gameObject.SetActive(false);
                    var settingsButton = root.transform.Find("Top Bar/Settings") as RectTransform;
                    if (settingsButton != null)
                    {
                        settingsButton.SetParent(root.transform,false); settingsButton.anchorMin=new Vector2(.955f,.925f); settingsButton.anchorMax=new Vector2(.99f,.98f);
                        settingsButton.offsetMin=settingsButton.offsetMax=Vector2.zero;
                    }
                    if (root.transform.Find("Menu Economy") == null)
                    {
                        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                        var rect = (RectTransform)instance.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
                    }
                    if (root.GetComponent<LocalizedMenuScope>() == null) root.AddComponent<LocalizedMenuScope>();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/MrX Studio/Localization/Import Additional Menu Translations")]
        public static void ImportTranslations()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required.");
            var table = Resources.Load<MenuTranslations>("Menu Translations");
            foreach (string line in File.ReadAllLines("Assets/The Archer/Resources/Menu Translation Additions.txt"))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var cells = line.Split('|').Select(s => s.Replace("\\n", "\n")).ToArray();
                if (cells.Length != 5) throw new Exception("Expected five language columns: " + line);
                // Existing Inspector edits remain authoritative on subsequent imports.
                if (table.entries.Any(e => string.Equals(e.english, cells[0], StringComparison.OrdinalIgnoreCase))) continue;
                table.entries.Add(new TranslationEntry { english=cells[0], vietnamese=cells[1], spanish=cells[2], portuguese=cells[3], french=cells[4] });
            }
            table.Rebuild();
            var latin = table.latinFont;
            string chars = string.Concat(table.entries.SelectMany(e => new[] {e.english,e.vietnamese,e.spanish,e.portuguese,e.french}));
            string needed = new string(chars.Distinct().Where(c => !char.IsControl(c) && !latin.HasCharacter(c)).ToArray());
            if (needed.Length > 0)
            {
                var fontObject = new SerializedObject(latin);
                fontObject.FindProperty("m_SourceFontFile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
                fontObject.ApplyModifiedPropertiesWithoutUndo();
                latin.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                string missing;
                bool complete = latin.TryAddCharacters(needed, out missing);
                latin.atlasPopulationMode = AtlasPopulationMode.Static;
                if (!complete && !string.IsNullOrEmpty(missing)) throw new Exception("Missing font glyphs: " + missing);
            }
            EditorUtility.SetDirty(latin); EditorUtility.SetDirty(table); AssetDatabase.SaveAssets();
        }

        private static GameObject Build(MenuEconomySettings settings, CurrenciesDatabase currencies)
        {
            var root = Rect(null, "Menu Economy", 0,0,1,1);
            var view = root.gameObject.AddComponent<MenuEconomyPanel>(); var so = new SerializedObject(view);
            var energy = Panel(root,"Energy Wallet",.60f,.925f,.735f,.98f);
            Icon(energy,settings.energyIcon,.02f,.12f,.23f,.88f);
            Set(so,"energyAmount",Text(energy,"Energy Amount","30 / 30",.23f,.1f,.79f,.90f,27));
            Set(so,"energyPlus",Button(energy,"Buy Energy","+",.80f,.08f,.98f,.92f));
            Set(so,"recoveryLabel",Text(root,"Energy Recovery","FULL",.60f,.900f,.735f,.922f,17));
            Set(so,"runCost",Text(root,"Run Energy Cost","RUN COST: 5 ENERGY",.60f,.87f,.935f,.899f,20));
            var gold = Panel(root,"Gold Wallet",.745f,.925f,.85f,.98f);
            Icon(gold,currencies.GetCurrency("gold").Icon,.02f,.12f,.23f,.88f);
            Set(so,"goldAmount",Text(gold,"Gold Amount","0",.23f,.1f,.77f,.9f,27));
            Set(so,"goldPlus",Button(gold,"Buy Gold","+",.78f,.08f,.98f,.92f));
            var gem = Panel(root,"Gem Wallet",.86f,.925f,.94f,.98f);
            Icon(gem,currencies.GetCurrency("gem").Icon,.02f,.12f,.28f,.88f);
            Set(so,"gemAmount",Text(gem,"Gem Amount","0",.29f,.1f,.96f,.9f,27));
            var shade = Rect(root,"Exchange Modal",0,0,1,1);
            shade.gameObject.AddComponent<Image>().color = new Color(.03f,.07f,.15f,.8f);
            var canvas = shade.gameObject.AddComponent<Canvas>(); canvas.overrideSorting=true; canvas.sortingOrder=500;
            shade.gameObject.AddComponent<GraphicRaycaster>();
            var dialog = Panel(shade,"Dialog",.28f,.27f,.72f,.75f);
            Set(so,"offerTitle",Text(dialog,"Title","BUY ENERGY",.08f,.77f,.84f,.94f,39));
            var close=Button(dialog,"Close","X",.87f,.81f,.97f,.95f); Set(so,"closeButton",close);
            Set(so,"offerAmount",Text(dialog,"Offer Amount","+10 ENERGY",.08f,.52f,.92f,.73f,44));
            Set(so,"offerPrice",Text(dialog,"Gem Price","20 GEMS",.08f,.36f,.92f,.51f,29));
            var buy=Button(dialog,"Confirm Purchase","BUY",.28f,.16f,.72f,.34f); Set(so,"buyButton",buy);
            Set(so,"feedback",Text(dialog,"Feedback","",.04f,.015f,.96f,.15f,21));
            buy.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=close,selectOnDown=close};
            close.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=buy,selectOnDown=buy};
            Set(so,"modal",shade.gameObject); so.ApplyModifiedPropertiesWithoutUndo(); shade.gameObject.SetActive(false);
            var result=PrefabUtility.SaveAsPrefabAsset(root.gameObject,PrefabPath); Object.DestroyImmediate(root.gameObject);return result;
        }
        private static void Set(SerializedObject so,string name,Object value){so.FindProperty(name).objectReferenceValue=value;}
        private static RectTransform Rect(Transform parent,string name,float x,float y,float r,float t)
        {var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(r,t);rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;}
        private static RectTransform Panel(Transform parent,string name,float x,float y,float r,float t)
        {var rect=Rect(parent,name,x,y,r,t);var image=rect.gameObject.AddComponent<Image>();image.sprite=art.slotSprite;image.material=art.slotMaterial;image.type=Image.Type.Sliced;image.color=new Color(.92f,.98f,1);return rect;}
        private static TMP_Text Text(Transform parent,string name,string text,float x,float y,float r,float t,float size)
        {var rect=Rect(parent,name,x,y,r,t);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.fontSize=label.fontSizeMax=size;label.fontSizeMin=size*.55f;label.enableAutoSizing=true;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.13f,.24f,.55f);label.raycastTarget=false;return label;}
        private static Button Button(Transform parent,string name,string text,float x,float y,float r,float t)
        {var rect=Panel(parent,name,x,y,r,t);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=rect.GetComponent<Image>();Text(rect,"Label",text,.03f,.03f,.97f,.97f,35);return button;}
        private static void Icon(Transform parent,Sprite sprite,float x,float y,float r,float t)
        {var image=Rect(parent,"Icon",x,y,r,t).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;}
    }
}
