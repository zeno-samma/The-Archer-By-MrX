using System.Collections.Generic;
using System.Reflection;
using MrX.UI;
using MrX.Upgrades;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    /// <summary>One-time Editor authoring. The saved scene/prefabs are the source of truth thereafter.</summary>
    public static class MenuPrefabAuthoring
    {
        public const string Folder = "Assets/The Archer/Prefabs/UI/Menu Pages";
        private static ForgePageDatabase art;
        private static readonly Color Ink = new Color(.12f, .23f, .56f);
        private const float TextScale = 3840f / 1672f;
        private static void Set(object obj, string field, object value)
        {
            for (var type = obj.GetType(); type != null; type = type.BaseType)
            { var info = type.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly); if (info != null) { info.SetValue(obj, value); EditorUtility.SetDirty((Object)obj); return; } }
            throw new System.Exception("Missing field: " + field);
        }
        private static object Get(object obj, string field)
        {
            for (var type = obj.GetType(); type != null; type = type.BaseType)
            { var info = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly); if (info != null) return info.GetValue(obj); }
            throw new System.Exception(field);
        }
        [MenuItem("Tools/MrX Studio/UI/Install Authored Menu Prefabs")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode before authoring.");
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var screen = Object.FindAnyObjectByType<MainMenuScreenBehavior>();
            if (screen == null) throw new System.Exception("Open Main Menu scene first.");
            var scaler = screen.GetComponentInParent<CanvasScaler>();
            if (scaler != null) { scaler.matchWidthOrHeight = 1; EditorUtility.SetDirty(scaler); Canvas.ForceUpdateCanvases(); }
            var pages = (List<MainMenuPageBehavior>)Get(screen, "pages");
            InstallPage<ForgePageBehavior>(screen, pages, Folder + "/Forge Page.prefab", MainMenuPageType.Forge);
            InstallPage<AchievementsPageBehavior>(screen, pages, Folder + "/Achievements Page.prefab", MainMenuPageType.Achievements);
            InstallPage<DailyQuestsPageBehavior>(screen, pages, Folder + "/Daily Quests Page.prefab", MainMenuPageType.DailyQuests);
            var upgrades = pages.Find(p => p.PageType == MainMenuPageType.Upgrades);
            if (!(upgrades is UpgradeDashboardPage))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Upgrades Page.prefab");
                if (prefab == null) prefab = BuildUpgrades();
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, screen.transform);
                Stretch((RectTransform)instance.transform); pages.Remove(upgrades); pages.Add(instance.GetComponent<UpgradeDashboardPage>());
                if (upgrades != null) Undo.DestroyObjectImmediate(upgrades.gameObject);
            }
            var armory = pages.Find(p => p.PageType == MainMenuPageType.Armory) as MainMenuArmoryPageBehavior;
            if (armory != null && Get(armory, "dashboard") == null)
            {
                var dashboard = Undo.AddComponent<ArmoryDashboard>(armory.gameObject);
                var hero = (MrX.UI.Armory.ArmoryHeroPanel)Get(armory, "heroPanel");
                dashboard.Build((RectTransform)armory.transform, (RectTransform)hero.transform, Resources.Load<ArmoryDashboardDatabase>(ArmoryDashboardDatabase.ResourcePath));
                Set(armory, "dashboard", dashboard);
                Set(hero, "showCompactStats", false);
                ((RectTransform)Get(hero, "statsPanelRect")).gameObject.SetActive(false);
                ((Component)Get(armory, "tabsPanel")).gameObject.SetActive(false);
                ((Component)Get(armory, "itemsGrid")).gameObject.SetActive(false);
                ((Component)Get(armory, "heroesGrid")).gameObject.SetActive(false);
                var prefab = PrefabUtility.SaveAsPrefabAsset(armory.gameObject, Folder + "/Armory Page.prefab");
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, screen.transform); Stretch((RectTransform)instance.transform);
                pages.Remove(armory); pages.Add(instance.GetComponent<MainMenuArmoryPageBehavior>()); Undo.DestroyObjectImmediate(armory.gameObject);
            }
            var dock = (MainMenuDockBehavior)Get(screen, "dockBehavior");
            dock.RegisterDailyQuests(Resources.Load<MrX.Quests.DailyQuestsDatabase>(MrX.Quests.DailyQuestsDatabase.ResourcePath).questIcon);
            dock.RegisterAchievements(Resources.Load<MrX.Achievements.AchievementsDatabase>(MrX.Achievements.AchievementsDatabase.ResourcePath).trophySprite);
            dock.RegisterForge(art.forgeIcon);
            EditorUtility.SetDirty(dock); EditorUtility.SetDirty(screen);
            // Give dock a reusable prefab as well; its serialized slots are present before Play.
            PrefabUtility.SaveAsPrefabAssetAndConnect(dock.gameObject, Folder + "/Menu Dock.prefab", InteractionMode.AutomatedAction);
            dock.transform.SetAsLastSibling();
            EditorSceneManager.MarkSceneDirty(screen.gameObject.scene); AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(screen.gameObject.scene); Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Upgrades Page.prefab");
        }
        private static void InstallPage<T>(MainMenuScreenBehavior screen, List<MainMenuPageBehavior> pages, string path, MainMenuPageType type) where T : MainMenuPageBehavior
        {
            if (pages.Exists(p => p.PageType == type)) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new System.Exception("Missing authored prefab: " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, screen.transform); Stretch((RectTransform)instance.transform);
            // Bake typography once. Inspector changes then remain untouched in Play Mode.
            foreach (var label in instance.GetComponentsInChildren<TMP_Text>(true))
            { label.fontSize *= TextScale; label.fontSizeMax *= TextScale; label.fontSizeMin *= TextScale; }
            foreach (var element in instance.GetComponentsInChildren<LayoutElement>(true)) if (element.preferredHeight > 0) element.preferredHeight *= TextScale;
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            pages.Add(instance.GetComponent<T>());
        }
        private static GameObject BuildUpgrades()
        {
            var root = Rect(null, "Upgrades Page", 0, 0, 1, 1); root.sizeDelta = new Vector2(3840, 2160);
            var page = root.gameObject.AddComponent<UpgradeDashboardPage>();
            var canvas = root.gameObject.AddComponent<Canvas>(); root.gameObject.AddComponent<GraphicRaycaster>(); var group = root.gameObject.AddComponent<CanvasGroup>();
            Set(page, "pageType", MainMenuPageType.Upgrades); Set(page, "pageRect", root); Set(page, "canvas", canvas); Set(page, "canvasGroup", group);
            Text(root, "Page Heading", "PERMANENT UPGRADES", 26, .12f, .89f, .72f, .98f);
            var wallet = Panel(root, "Gold Wallet", .79f, .9f, .9f, .97f);
            Icon(wallet, "Coin", art.coinIcon, .05f, .1f, .26f, .9f);
            Set(page, "walletLabel", Text(wallet, "Gold Amount", "0", 24, .3f, .1f, .95f, .9f));
            var frame = Panel(root, "Main Frame", .11f, .15f, .9f, .88f);
            var detail = Panel(frame, "Selected Upgrade", .01f, .31f, .335f, .985f);
            Set(page, "selectedIcon", Icon(detail, "Upgrade Icon", art.forgeIcon, .32f, .56f, .68f, .94f));
            Set(page, "selectedTitle", Text(detail, "Upgrade Title", "Arrow Damage", 30, .04f, .43f, .96f, .57f, true));
            Set(page, "selectedLevel", Text(detail, "Level Preview", "Lv. 1  >  Lv. 2", 23, .08f, .34f, .92f, .44f, true));
            Set(page, "selectedDescription", Text(detail, "Description", "Increases arrow damage", 20, .06f, .18f, .94f, .34f, true));
            Set(page, "selectedEffect", Text(detail, "Effect Preview", "+0%  >  +10%", 23, .04f, .12f, .96f, .21f, true));
            var buy = Button(detail, "Upgrade Button", "UPGRADE", .08f, .015f, .92f, .125f, true);
            Set(page, "upgradeButton", buy); Set(page, "selectedCost", buy.GetComponentInChildren<TMP_Text>());
            var stats = Panel(frame, "Player Stats Preview", .01f, .01f, .335f, .3f);
            Text(stats, "Heading", "PLAYER STATS", 24, .05f, .78f, .95f, .99f);
            var current = new TMP_Text[5]; var next = new TMP_Text[5]; var statNames = new[] { "ATK", "HP", "CRIT", "SPEED", "ASPD" };
            for (int i = 0; i < 5; i++)
            {
                float y = .63f - i * .14f;
                Text(stats, statNames[i] + " Label", statNames[i], 19, .06f, y, .3f, y + .18f);
                current[i] = Text(stats, statNames[i] + " Current", "0", 21, .31f, y, .53f, y + .18f);
                next[i] = Text(stats, statNames[i] + " Preview", "> 0 (+0)", 18, .54f, y, .98f, y + .18f); next[i].color = new Color(.08f, .59f, .23f);
            }
            Set(page, "currentStats", current); Set(page, "nextStats", next);
            var list = Panel(frame, "Upgrade List", .345f, .01f, .99f, .985f);
            var viewport = Rect(list, "Viewport", .012f, .015f, .988f, .985f); viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = list.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 32;
            var content = Rect(viewport, "Content", 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1); scroll.content = content;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 6; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var database = AssetDatabase.LoadAssetAtPath<UpgradesDatabase>("Assets/The Archer/Scriptables/Upgrades/Upgrades Database.asset");
            var rows = new List<UpgradeDashboardPage.Row>();
            for (int i = 0; i < database.UpgradesCount; i++)
            {
                var data = database.GetUpgrade(i); var rect = Panel(content, "Upgrade - " + data.Title, 0, 0, 1, 1);
                rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 238;
                var select = rect.gameObject.AddComponent<Button>(); select.targetGraphic = rect.GetComponent<Image>(); select.targetGraphic.raycastTarget = true;
                var row = new UpgradeDashboardPage.Row { data = data, select = select, background = rect.GetComponent<Image>() };
                row.icon = Icon(rect, "Icon", data.Icon, .02f, .09f, .13f, .91f);
                row.title = Text(rect, "Title", data.Title, 23, .15f, .62f, .6f, .96f);
                row.description = Text(rect, "Description", data.Description, 16, .15f, .31f, .61f, .65f);
                row.level = Text(rect, "Level", "Lv. 1", 17, .15f, .04f, .24f, .32f);
                row.values = Text(rect, "Values", "+0% > +10%", 19, .59f, .3f, .77f, .79f, true);
                row.buy = Button(rect, "Buy", "100", .78f, .2f, .98f, .8f, false); row.buyImage = row.buy.image; row.price = row.buy.GetComponentInChildren<TMP_Text>();
                Icon(row.buy.transform, "Coin", art.coinIcon, .05f, .2f, .28f, .8f); Place(row.price.rectTransform, .3f, .05f, .96f, .95f);
                row.segments = new Image[6];
                for (int j = 0; j < 6; j++) row.segments[j] = Panel(rect, "Progress " + (j + 1), .26f + j * .045f, .1f, .3f + j * .045f, .23f).GetComponent<Image>();
                select.gameObject.AddComponent<AchievementRowFocus>().Initialize(scroll, rect);
                row.buy.gameObject.AddComponent<AchievementRowFocus>().Initialize(scroll, rect);
                rows.Add(row);
            }
            Set(page, "rows", rows); Set(page, "blueButton", Sprite("Generic/ui_button_blue.png")); Set(page, "yellowButton", art.forgeButtonSprite); Set(page, "grayButton", art.disabledButtonSprite);
            Set(page, "feedback", Text(root, "Feedback", "Select an upgrade to preview its effect before buying.", 19, .14f, .12f, .88f, .15f, true));
            canvas.enabled = false; group.alpha = 0; group.blocksRaycasts = group.interactable = false;
            var result = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Upgrades Page.prefab"); Object.DestroyImmediate(root.gameObject); return result;
        }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/UI/" + path);
        private static RectTransform Panel(Transform parent, string name, float x, float y, float r, float t)
        {
            var rect = Rect(parent, name, x, y, r, t); var image = rect.gameObject.AddComponent<Image>();
            image.sprite = art.slotSprite; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.5f; image.material = art.slotMaterial; image.color = new Color(.89f, .95f, 1); image.raycastTarget = false; return rect;
        }
        private static Image Icon(Transform parent, string name, Sprite sprite, float x, float y, float r, float t)
        { var image = Rect(parent, name, x, y, r, t).gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static TMP_Text Text(Transform parent, string name, string text, float size, float x, float y, float r, float t, bool center = false)
        {
            var label = Rect(parent, name, x, y, r, t).gameObject.AddComponent<TextMeshProUGUI>(); label.font = art.font; label.text = text; label.color = Ink;
            label.enableAutoSizing = true; label.fontSize = label.fontSizeMax = size * TextScale; label.fontSizeMin = size * TextScale * .65f;
            label.alignment = center ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft; label.raycastTarget = false; return label;
        }
        private static Button Button(Transform parent, string name, string text, float x, float y, float r, float t, bool gold)
        {
            var rect = Panel(parent, name, x, y, r, t); var image = rect.GetComponent<Image>(); image.sprite = gold ? art.forgeButtonSprite : Sprite("Generic/ui_button_blue.png"); image.material = null; image.color = Color.white; image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text(rect, "Label", text, 24, .06f, .04f, .94f, .96f, true).color = Color.white; return button;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float r, float t)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5; rect.SetParent(parent, false); Place(rect, x, y, r, t); return rect; }
        private static void Place(RectTransform rect, float x, float y, float r, float t)
        { rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(r, t); rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Stretch(RectTransform rect) { Place(rect, 0, 0, 1, 1); rect.localScale = Vector3.one; }
    }
}
