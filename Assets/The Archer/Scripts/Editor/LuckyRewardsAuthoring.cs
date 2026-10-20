using System.Collections.Generic;
using System.Reflection;
using MrX.Armory;
using MrX.Crafting;
using MrX.Currency;
using MrX.Rewards;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class LuckyRewardsAuthoring
    {
        public const string Folder = "Assets/The Archer/Prefabs/UI/Lucky Rewards";
        public const string DataFolder = "Assets/The Archer/Scriptables/Lucky Rewards";
        private static ForgePageDatabase art;
        private static Sprite wheelArt, chestArt, gem;
        private static readonly Color Ink = new Color(.12f, .23f, .56f);
        private static void Field(Object target, string field, object value)
        { target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value); EditorUtility.SetDirty(target); }
        [MenuItem("Tools/MrX Studio/Lucky Rewards/Install Missing Assets and Stage Shortcuts")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode before authoring UI.");
            StageUtility.GoToMainStage();
            System.IO.Directory.CreateDirectory(Folder); System.IO.Directory.CreateDirectory(DataFolder); AssetDatabase.Refresh();
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            wheelArt = Import("Assets/The Archer/Sprites/UI/Lucky Rewards/Lucky Wheel.png");
            chestArt = Import("Assets/The Archer/Sprites/UI/Lucky Rewards/Gem Chest.png"); gem = Sprite("Generic/gem.png");
            var currencies = AssetDatabase.LoadAssetAtPath<CurrenciesDatabase>("Assets/The Archer/Scriptables/Currencies Database.asset");
            if (currencies.GetCurrency("gem") == null)
            {
                var so = new SerializedObject(currencies); var list = so.FindProperty("currencies"); int i = list.arraySize; list.InsertArrayElementAtIndex(i);
                var entry = list.GetArrayElementAtIndex(i); entry.FindPropertyRelative("id").stringValue = "gem"; entry.FindPropertyRelative("name").stringValue = "Gem"; entry.FindPropertyRelative("icon").objectReferenceValue = gem;
                so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(currencies);
            }
            var wheel = Table("Lucky Wheel", true); var chest = Table("Gem Chest", false);
            var row = AssetDatabase.LoadAssetAtPath<LuckyPrizeView>(Folder + "/Prize Row.prefab") ?? BuildPrizeRow();
            var slice = AssetDatabase.LoadAssetAtPath<LuckyWheelSlice>(Folder + "/Wheel Slice.prefab") ?? BuildSlice();
            var wheelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Lucky Wheel Popup.prefab") ?? BuildPopup(wheel, true, row, slice);
            var chestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Gem Chest Popup.prefab") ?? BuildPopup(chest, false, row, slice);
            var shortcuts = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Stage Lucky Rewards.prefab");
            if (shortcuts == null)
            {
                var root = Root("Stage Lucky Rewards"); root.sizeDelta = new Vector2(3840, 2160); var hub = root.gameObject.AddComponent<StageLuckyRewards>();
                var spin = Button(root, "Lucky Wheel Shortcut", "LUCKY WHEEL", .82f, .54f, .94f, .75f);
                var open = Button(root, "Gem Chest Shortcut", "GEM CHEST", .82f, .3f, .94f, .51f);
                Icon(spin.transform, "Wheel Icon", wheelArt, .1f, .23f, .9f, .95f); Icon(open.transform, "Chest Icon", chestArt, .1f, .23f, .9f, .95f);
                Place(spin.GetComponentInChildren<TMP_Text>().rectTransform, .02f, .01f, .98f, .24f); Place(open.GetComponentInChildren<TMP_Text>().rectTransform, .02f, .01f, .98f, .24f);
                var wp = (GameObject)PrefabUtility.InstantiatePrefab(wheelPrefab, root); Stretch((RectTransform)wp.transform); wp.SetActive(false);
                var cp = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab, root); Stretch((RectTransform)cp.transform); cp.SetActive(false);
                Field(hub, "wheelButton", spin); Field(hub, "chestButton", open); Field(hub, "wheelPopup", wp.GetComponent<LuckyRewardsPopup>()); Field(hub, "chestPopup", cp.GetComponent<LuckyRewardsPopup>());
                shortcuts = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Stage Lucky Rewards.prefab"); Object.DestroyImmediate(root.gameObject);
            }
            var stage = Object.FindAnyObjectByType<MainMenuStagesPageBehavior>();
            if (stage == null) throw new System.Exception("Open Main Menu scene first.");
            if (stage.GetComponentInChildren<StageLuckyRewards>(true) == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(shortcuts, stage.transform); Stretch((RectTransform)instance.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            }
            var gemRoot = stage.transform.Find("Gem");
            if (gemRoot != null)
            {
                foreach (var indicator in gemRoot.GetComponentsInChildren<CurrencyScreenIncicatorBehavior>(true))
                { var so = new SerializedObject(indicator); so.FindProperty("currencyID").stringValue = "gem"; so.FindProperty("currency").FindPropertyRelative("value").stringValue = "gem"; var icon = (Image)so.FindProperty("icon").objectReferenceValue; icon.sprite = gem; EditorUtility.SetDirty(icon); so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            EditorSceneManager.MarkSceneDirty(stage.gameObject.scene); AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(stage.gameObject.scene);
            Selection.activeObject = wheel;
        }
        private static LuckyRewardTable Table(string name, bool spin)
        {
            string path = DataFolder + "/" + name + ".asset";
            var table = AssetDatabase.LoadAssetAtPath<LuckyRewardTable>(path); if (table) return table;
            table = ScriptableObject.CreateInstance<LuckyRewardTable>(); table.id = spin ? "lucky-wheel" : "gem-chest"; table.title = name.ToUpperInvariant(); table.price = new Price("gem", spin ? 10 : 30); table.animationSeconds = spin ? 4 : 1.8f;
            var crafting = Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath);
            if (spin)
            {
                table.prizes.Add(new LuckyPrize { id = "gold", kind = LuckyRewardKind.Currency, currencyId = "gold", title = "Gold", icon = art.coinIcon, weight = 35, minimumAmount = 100, maximumAmount = 100 });
                foreach (var material in crafting.materials) table.prizes.Add(new LuckyPrize { id = material.material.ToString().ToLowerInvariant(), kind = LuckyRewardKind.Material, material = material.material, icon = material.icon, title = material.title, weight = material.material == CraftingMaterial.Iron ? 20 : material.material == CraftingMaterial.Leather ? 10 : 15, minimumAmount = 3, maximumAmount = 5 });
                table.prizes.Add(new LuckyPrize { id = "gems", kind = LuckyRewardKind.Currency, currencyId = "gem", title = "Gems", icon = gem, weight = 5, minimumAmount = 20, maximumAmount = 20 });
            }
            else
            {
                table.prizes.Add(new LuckyPrize { id = "gold", kind = LuckyRewardKind.Currency, currencyId = "gold", title = "Gold", icon = art.coinIcon, weight = 25, minimumAmount = 400, maximumAmount = 600 });
                var fragmentItem = art.armory.Items.Find(item => !item.IsDefaultItem && !item.IsUnlockedByDefault && item.Crafting != null && item.Crafting.enabled) ?? art.armory.Items[0];
                table.prizes.Add(new LuckyPrize { id = "bow-fragments", kind = LuckyRewardKind.Fragments, item = fragmentItem, icon = fragmentItem.Icon, weight = 30, minimumAmount = 8, maximumAmount = 12 });
                var material = crafting.Material(CraftingMaterial.Crystal);
                table.prizes.Add(new LuckyPrize { id = "crystal", kind = LuckyRewardKind.Material, material = CraftingMaterial.Crystal, icon = material.icon, weight = 20, minimumAmount = 8, maximumAmount = 10 });
                table.prizes.Add(new LuckyPrize { id = "equipment", kind = LuckyRewardKind.Equipment, item = art.armory.Items[art.armory.Items.Count - 1], icon = art.armory.Items[art.armory.Items.Count - 1].Icon, weight = 10, maxLevelDuplicateGold = 500 });
                material = crafting.Material(CraftingMaterial.Scroll);
                table.prizes.Add(new LuckyPrize { id = "scroll", kind = LuckyRewardKind.Material, material = CraftingMaterial.Scroll, icon = material.icon, weight = 10, minimumAmount = 6, maximumAmount = 8 });
                table.prizes.Add(new LuckyPrize { id = "gems", kind = LuckyRewardKind.Currency, currencyId = "gem", title = "Gems", icon = gem, weight = 5, minimumAmount = 60, maximumAmount = 60 });
            }
            AssetDatabase.CreateAsset(table, path); return table;
        }
        private static LuckyPrizeView BuildPrizeRow()
        {
            var root = Root("Prize Row"); root.sizeDelta = new Vector2(850, 145); var view = root.gameObject.AddComponent<LuckyPrizeView>(); Skin(root.gameObject.AddComponent<Image>());
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 145;
            view.icon = Icon(root, "Icon", gem, .02f, .12f, .17f, .88f); view.title = Text(root, "Title", "Reward", 42, .2f, .42f, .78f, .94f);
            view.amount = Text(root, "Amount", "x 100", 34, .2f, .06f, .64f, .47f); view.odds = Text(root, "Chance", "25%", 38, .78f, .15f, .98f, .85f);
            var asset = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Prize Row.prefab"); Object.DestroyImmediate(root.gameObject); return asset.GetComponent<LuckyPrizeView>();
        }
        private static LuckyWheelSlice BuildSlice()
        {
            var root = Root("Wheel Slice"); root.sizeDelta = Vector2.one * 1000; var slice = root.gameObject.AddComponent<LuckyWheelSlice>();
            slice.sector = Icon(root, "Sector", art.circleSprite, 0, 0, 1, 1); slice.sector.preserveAspect = false; slice.sector.type = Image.Type.Filled; slice.sector.fillMethod = Image.FillMethod.Radial360; slice.sector.fillOrigin = (int)Image.Origin360.Top; slice.sector.fillClockwise = true;
            slice.icon = Icon(root, "Prize Icon", gem, .5f, .75f, .5f, .75f); slice.icon.rectTransform.sizeDelta = Vector2.one * 180;
            slice.label = Text(root, "Chance", "25%", 35, .5f, .9f, .5f, .9f); slice.label.rectTransform.sizeDelta = new Vector2(160, 60); slice.label.alignment = TextAlignmentOptions.Center;
            slice.label.gameObject.SetActive(false);
            var asset = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Wheel Slice.prefab"); Object.DestroyImmediate(root.gameObject); return asset.GetComponent<LuckyWheelSlice>();
        }
        private static GameObject BuildPopup(LuckyRewardTable table, bool spin, LuckyPrizeView rowPrefab, LuckyWheelSlice slicePrefab)
        {
            var root = Root(table.title + " Popup"); root.sizeDelta = new Vector2(3840, 2160); var popup = root.gameObject.AddComponent<LuckyRewardsPopup>();
            var canvas = root.gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 200; root.gameObject.AddComponent<GraphicRaycaster>();
            var shade = root.gameObject.AddComponent<Image>(); shade.color = new Color(.03f, .1f, .26f, .75f);
            var panel = Panel(root, "Panel", .12f, .14f, .88f, .9f);
            Field(popup, "table", table); Field(popup, "isWheel", spin); Field(popup, "prizePrefab", rowPrefab); Field(popup, "slicePrefab", slicePrefab);
            Field(popup, "heading", Text(panel, "Title", table.title, 80, .04f, .88f, .65f, .98f));
            var wallet = Panel(panel, "Gem Wallet", .7f, .89f, .9f, .97f); Icon(wallet, "Gem", gem, .04f, .05f, .25f, .95f); Field(popup, "walletText", Text(wallet, "Amount", "0", 48, .3f, .05f, .96f, .95f));
            var close = Button(panel, "Close", "X", .92f, .9f, .98f, .98f); Field(popup, "closeButton", close);
            var artArea = Rect(panel, "Showcase", .04f, .24f, .57f, .86f);
            RectTransform disc = null, illustration = null;
            var slices = new List<LuckyWheelSlice>(); var rows = new List<LuckyPrizeView>();
            if (spin)
            {
                var frame = Rect(artArea, "Wheel Frame", .06f, .02f, .94f, .98f); var aspect = frame.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = 1;
                Icon(frame, "Blue Rim", art.circleSprite, 0, 0, 1, 1).color = new Color(.2f, .53f, 1);
                disc = Rect(frame, "Rotating Disc", .04f, .04f, .96f, .96f);
                var pointer = Icon(frame, "Fixed Pointer", Sprite("Generic/ui_arrow.png"), .44f, .88f, .56f, 1.1f); pointer.rectTransform.localEulerAngles = new Vector3(0, 0, -90); pointer.color = new Color(.35f, .7f, 1);
                Icon(frame, "Center", art.circleSprite, .4f, .4f, .6f, .6f).color = new Color(.28f, .55f, 1);
            }
            else illustration = Icon(artArea, "Chest Illustration", chestArt, .04f, .02f, .96f, .98f).rectTransform;
            Field(popup, "wheelDisc", disc); Field(popup, "chestIllustration", illustration);
            Text(panel, "Reward Heading", "POSSIBLE REWARDS", 44, .61f, .8f, .96f, .88f);
            var viewport = Panel(panel, "Rewards Viewport", .6f, .17f, .97f, .79f); viewport.gameObject.AddComponent<RectMask2D>(); viewport.GetComponent<Image>().raycastTarget = true;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.horizontal = false; scroll.scrollSensitivity = 70; scroll.movementType = ScrollRect.MovementType.Clamped;
            var content = Rect(viewport, "Content", .02f, 1, .98f, 1); content.pivot = new Vector2(.5f, 1); scroll.content = content;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 12; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Field(popup, "prizeContent", content);
            table.IsValid(out var total); int index = 0;
            int activeCount = table.prizes.FindAll(p => p != null && p.enabled && p.weight > 0).Count;
            foreach (var prize in table.prizes)
            {
                if (prize == null || !prize.enabled || prize.weight <= 0) continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(rowPrefab.gameObject, content); var row = instance.GetComponent<LuckyPrizeView>(); row.title.text = !string.IsNullOrEmpty(prize.title) ? prize.title : prize.item ? prize.item.ItemName : prize.kind == LuckyRewardKind.Currency ? prize.currencyId : prize.material.ToString(); row.icon.sprite = prize.icon;
                row.amount.text = "x " + prize.minimumAmount + (prize.maximumAmount == prize.minimumAmount ? "" : " - " + prize.maximumAmount); row.odds.text = (prize.weight / total * 100).ToString("0.##") + "%"; rows.Add(row);
                if (spin) { var obj = (GameObject)PrefabUtility.InstantiatePrefab(slicePrefab.gameObject, disc); Stretch((RectTransform)obj.transform); var slice = obj.GetComponent<LuckyWheelSlice>(); float fraction = 1f / activeCount; slice.Bind(index * fraction * 360, fraction, prize.icon, "", index++); slices.Add(slice); }
            }
            Field(popup, "prizePool", rows); Field(popup, "slicePool", slices);
            var purchase = Button(panel, "Purchase", table.price.Amount + " GEM\n" + (spin ? "SPIN" : "OPEN CHEST"), .09f, .075f, .53f, .21f, true);
            Icon(purchase.transform, "Gem", gem, .04f, .15f, .18f, .85f); Place(purchase.GetComponentInChildren<TMP_Text>().rectTransform, .2f, .04f, .95f, .96f);
            Field(popup, "purchaseButton", purchase); Field(popup, "costText", purchase.GetComponentInChildren<TMP_Text>());
            Field(popup, "status", Text(panel, "Status", "Spend gems to receive one reward.", 36, .05f, .005f, .96f, .065f));
            var result = Rect(root, "Reward Result", 0, 0, 1, 1); var resultShade = result.gameObject.AddComponent<Image>(); resultShade.color = new Color(.02f, .08f, .22f, .8f);
            var resultBody = Panel(result, "Body", .3f, .25f, .7f, .78f); Text(resultBody, "Heading", "REWARD RECEIVED", 70, .05f, .8f, .95f, .97f, true);
            Field(popup, "resultIcon", Icon(resultBody, "Reward Icon", gem, .33f, .43f, .67f, .81f));
            Field(popup, "resultTitle", Text(resultBody, "Reward Title", "Reward", 58, .05f, .3f, .95f, .44f, true));
            Field(popup, "resultAmount", Text(resultBody, "Reward Amount", "x 100", 42, .05f, .15f, .95f, .31f, true));
            Field(popup, "continueButton", Button(resultBody, "Continue", "CONTINUE", .18f, .025f, .82f, .15f, true)); Field(popup, "resultPanel", result.gameObject); result.gameObject.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/" + (spin ? "Lucky Wheel" : "Gem Chest") + " Popup.prefab"); Object.DestroyImmediate(root.gameObject); return prefab;
        }
        private static Sprite Import(string path)
        { var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/UI/" + path);
        private static RectTransform Root(string name)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer = 5; rect.anchorMin = rect.anchorMax = Vector2.one * .5f; return rect; }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float r, float t)
        { var rect = Root(name); rect.SetParent(parent, false); Place(rect, x, y, r, t); return rect; }
        private static void Place(RectTransform rect, float x, float y, float r, float t)
        { rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(r, t); rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Stretch(RectTransform rect) { Place(rect, 0, 0, 1, 1); rect.localScale = Vector3.one; PrefabUtility.RecordPrefabInstancePropertyModifications(rect); }
        private static void Skin(Image image)
        { image.sprite = art.slotSprite; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.5f; image.material = art.slotMaterial; image.color = new Color(.89f, .95f, 1); image.raycastTarget = false; }
        private static RectTransform Panel(Transform parent, string name, float x, float y, float r, float t)
        { var rect = Rect(parent, name, x, y, r, t); Skin(rect.gameObject.AddComponent<Image>()); return rect; }
        private static Image Icon(Transform parent, string name, Sprite sprite, float x, float y, float r, float t)
        { var image = Rect(parent, name, x, y, r, t).gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static TMP_Text Text(Transform parent, string name, string text, float size, float x, float y, float r, float t, bool center = false)
        { var label = Rect(parent, name, x, y, r, t).gameObject.AddComponent<TextMeshProUGUI>(); label.font = art.font; label.text = text; label.color = Ink; label.fontSize = label.fontSizeMax = size; label.fontSizeMin = size * .65f; label.enableAutoSizing = true; label.raycastTarget = false; label.alignment = center ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft; return label; }
        private static Button Button(Transform parent, string name, string text, float x, float y, float r, float t, bool gold = false)
        { var rect = Panel(parent, name, x, y, r, t); var image = rect.GetComponent<Image>(); image.raycastTarget = true; if (gold) { image.sprite = art.forgeButtonSprite; image.material = null; image.color = Color.white; } var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; Text(rect, "Label", text, 48, .05f, .05f, .95f, .95f, true); return button; }
    }
}
