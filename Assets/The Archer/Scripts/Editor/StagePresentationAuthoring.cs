using System.Reflection;
using MrX.Development;
using MrX.Rewards;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class StagePresentationAuthoring
    {
        public const string Folder = "Assets/The Archer/Prefabs/UI/Stage";
        public const string SettingsPath = "Assets/The Archer/Scriptables/Developer Tools Settings.asset";
        private static ForgePageDatabase art;
        private static readonly Color Ink = new Color(.12f, .23f, .56f);
        private static void Set(Object target, string field, object value)
        { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = (Object)value; so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target); }
        [MenuItem("Tools/MrX Studio/Stage/Install Developer Tools and Editor Preview")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode first.");
            var stage = Object.FindAnyObjectByType<MainMenuStagesPageBehavior>();
            if (!stage) throw new System.Exception("Open Main Menu scene first.");
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var settings = AssetDatabase.LoadAssetAtPath<DeveloperToolsSettings>(SettingsPath);
            if (!settings) { settings = ScriptableObject.CreateInstance<DeveloperToolsSettings>(); settings.armory = art.armory; AssetDatabase.CreateAsset(settings, SettingsPath); }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Developer Tools.prefab");
            if (!prefab) prefab = BuildTools(settings);
            if (!stage.GetComponentInChildren<DeveloperToolsPanel>(true))
            { var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, stage.transform); Place((RectTransform)go.transform, 0, 0, 1, 1); PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform); }
            AuthorStage(stage);
            RefreshWheel();
            EditorSceneManager.MarkSceneDirty(stage.gameObject.scene); AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(stage.gameObject.scene);
            Selection.activeObject = settings;
        }
        private static void AuthorStage(MainMenuStagesPageBehavior stage)
        {
            var hero = stage.transform.Find("Hero Panel");
            var info = hero.Find("Info_Ui");
            var panel = info.Find("Panel"); var grid = panel.GetComponent<GridLayoutGroup>(); if (grid) Object.DestroyImmediate(grid);
            var label = panel.GetComponentInChildren<TMP_Text>(true);
            panel.gameObject.SetActive(true); label.gameObject.SetActive(true);
            Place((RectTransform)panel, .2f, .18f, .95f, .82f);
            var panelImage = panel.GetComponent<Image>(); if (panelImage) panelImage.enabled = false;
            Place(label.rectTransform, 0, 0, 1, 1); label.text = art.armory.GetDefaultHero().Name + "  Lv.1";
            label.font = art.font; label.color = Ink; label.fontSize = 54; label.enableAutoSizing = true; label.fontSizeMin = 34; label.fontSizeMax = 54; label.alignment = TextAlignmentOptions.Center;
            var identity = hero.GetComponent<StageHeroIdentity>() ?? hero.gameObject.AddComponent<StageHeroIdentity>(); Set(identity, "heroLabel", label);
            var raw = hero.GetComponentInChildren<RawImage>(true);
            var fitter = raw.GetComponent<AspectRatioFitter>() ?? raw.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth; fitter.aspectRatio = (float)raw.texture.width / raw.texture.height;
            Set(hero.GetComponentInChildren<HeroPreviewBehavior>(true), "previewAspect", fitter);
            var selector = stage.GetComponentInChildren<MainMenuStageSelector>(true);
            var mask = selector.transform.Find("Stage/Stage Icon Mask");
            var preview = mask.Find("Editor Stage Preview");
            if (!preview) preview = Icon(mask, "Editor Stage Preview", stage.StageDatabase.GetStageData(0).StageImage, 0, 0, 1, 1).transform;
            preview.GetComponent<Image>().sprite = stage.StageDatabase.GetStageData(0).StageImage;
            Set(selector, "editorStagePreview", preview.gameObject);
            foreach (var text in selector.GetComponentsInChildren<TMP_Text>(true))
            { if (text.name == "Stage Number (TMP)") text.text = "Stage 1"; else if (text.name == "Stage Name (TMP)") text.text = stage.StageDatabase.GetStageData(0).StageName; }
            // These are nested instances inside the Stage page. Store their overrides
            // in a page variant instead of changing the shared Armory hero prefab.
            foreach (var component in hero.GetComponentsInChildren<Component>(true))
            { PrefabUtility.RecordPrefabInstancePropertyModifications(component); PrefabUtility.RecordPrefabInstancePropertyModifications(component.gameObject); }
            foreach (var component in selector.GetComponentsInChildren<Component>(true))
            { PrefabUtility.RecordPrefabInstancePropertyModifications(component); PrefabUtility.RecordPrefabInstancePropertyModifications(component.gameObject); }
            string path = Folder + "/Stage Page.prefab";
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(stage.gameObject) != path)
                PrefabUtility.SaveAsPrefabAssetAndConnect(stage.gameObject, path, InteractionMode.AutomatedAction);
        }
        public static void RefreshWheel()
        {
            var slicePath = LuckyRewardsAuthoring.Folder + "/Wheel Slice.prefab";
            var sliceRoot = PrefabUtility.LoadPrefabContents(slicePath);
            var sliceView = sliceRoot.GetComponent<LuckyWheelSlice>(); sliceView.icon.rectTransform.sizeDelta = Vector2.one * 180; sliceView.label.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(sliceRoot, slicePath); PrefabUtility.UnloadPrefabContents(sliceRoot);
            var path = LuckyRewardsAuthoring.Folder + "/Lucky Wheel Popup.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var table = AssetDatabase.LoadAssetAtPath<LuckyRewardTable>(LuckyRewardsAuthoring.DataFolder + "/Lucky Wheel.asset");
                var slices = root.GetComponentsInChildren<LuckyWheelSlice>(true);
                int count = table.prizes.FindAll(p => p != null && p.enabled && p.weight > 0).Count, i = 0;
                foreach (var prize in table.prizes)
                { if (prize == null || !prize.enabled || prize.weight <= 0) continue; if (i >= slices.Length) break; slices[i].Bind(i * 360f / count, 1f / count, prize.icon, "", i); i++; }
                for (; i < slices.Length; i++) slices[i].gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static GameObject BuildTools(DeveloperToolsSettings settings)
        {
            var root = Rect(null, "Developer Tools", .5f, .5f, .5f, .5f); root.sizeDelta = new Vector2(3840, 2160);
            var ui = root.gameObject.AddComponent<DeveloperToolsPanel>(); Set(ui, "settings", settings);
            var open = Button(root, "Developer Shortcut", "DEV", .06f, .24f, .16f, .31f); Set(ui, "shortcut", open.gameObject); Set(ui, "openButton", open);
            var modal = Rect(root, "Developer Panel", 0, 0, 1, 1); var canvas = modal.gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 220; modal.gameObject.AddComponent<GraphicRaycaster>();
            modal.gameObject.AddComponent<Image>().color = new Color(.02f, .07f, .2f, .78f);
            var body = Panel(modal, "Body", .24f, .17f, .76f, .88f);
            Text(body, "Title", "DEVELOPER TOOLS", 72, .05f, .86f, .85f, .97f);
            Set(ui, "closeButton", Button(body, "Close", "X", .87f, .87f, .97f, .97f));
            Text(body, "Subtitle", "Quick grants for testing", 42, .05f, .79f, .95f, .86f);
            var amount = Button(body, "Amount", "AMOUNT: 1,000   >", .06f, .65f, .94f, .76f); Set(ui, "amountButton", amount); Set(ui, "amountLabel", amount.GetComponentInChildren<TMP_Text>());
            Set(ui, "goldButton", Button(body, "Add Gold", "+ GOLD", .06f, .51f, .48f, .63f));
            Set(ui, "gemButton", Button(body, "Add Gems", "+ GEMS", .52f, .51f, .94f, .63f));
            var item = Panel(body, "Equipment", .06f, .19f, .94f, .48f);
            Text(item, "Equipment Heading", "EQUIPMENT", 38, .04f, .76f, .96f, .98f);
            Set(ui, "itemIcon", Icon(item, "Item Icon", settings.armory.Items[0].Icon, .15f, .12f, .35f, .77f));
            Set(ui, "itemLabel", Text(item, "Item Name", settings.armory.Items[0].ItemName + "\n1 / " + settings.armory.ItemsCount, 42, .39f, .37f, .87f, .76f));
            Set(ui, "previousItem", Button(item, "Previous Item", "<", .015f, .22f, .13f, .69f));
            Set(ui, "nextItem", Button(item, "Next Item", ">", .88f, .22f, .995f, .69f));
            Set(ui, "grantItem", Button(item, "Grant Item", "+ 1 ITEM", .4f, .05f, .85f, .34f));
            Set(ui, "status", Text(body, "Status", "Grants are saved to this player profile.", 37, .06f, .025f, .94f, .16f));
            Set(ui, "panel", modal.gameObject); modal.gameObject.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Developer Tools.prefab"); Object.DestroyImmediate(root.gameObject); return prefab;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float r, float t)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5; rect.SetParent(parent, false); Place(rect, x, y, r, t); return rect; }
        private static void Place(RectTransform rect, float x, float y, float r, float t)
        { rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(r, t); rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static RectTransform Panel(Transform parent, string name, float x, float y, float r, float t)
        { var rect = Rect(parent, name, x, y, r, t); var image = rect.gameObject.AddComponent<Image>(); image.sprite = art.slotSprite; image.type = Image.Type.Sliced; image.material = art.slotMaterial; image.color = new Color(.89f, .95f, 1); image.raycastTarget = false; return rect; }
        private static Image Icon(Transform parent, string name, Sprite sprite, float x, float y, float r, float t)
        { var image = Rect(parent, name, x, y, r, t).gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static TMP_Text Text(Transform parent, string name, string text, float size, float x, float y, float r, float t)
        { var label = Rect(parent, name, x, y, r, t).gameObject.AddComponent<TextMeshProUGUI>(); label.font = art.font; label.text = text; label.color = Ink; label.fontSize = label.fontSizeMax = size; label.fontSizeMin = size * .7f; label.enableAutoSizing = true; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; return label; }
        private static Button Button(Transform parent, string name, string text, float x, float y, float r, float t)
        { var rect = Panel(parent, name, x, y, r, t); var image = rect.GetComponent<Image>(); image.raycastTarget = true; var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; Text(rect, "Label", text, 48, .04f, .04f, .96f, .96f); return button; }
    }
}
