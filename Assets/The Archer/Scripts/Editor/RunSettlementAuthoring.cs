using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class RunSettlementAuthoring
    {
        public const string LootCard = "Assets/The Archer/Prefabs/UI/Game/Run Loot Reward Card.prefab";
        public const string GemWallet = "Assets/The Archer/Prefabs/UI/Menu Pages/Gem Wallet.prefab";
        public static void Install()
        {
            if (Application.isPlaying) throw new System.Exception("Edit Mode required");
            var art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(LootCard))
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/The Archer/Prefabs/UI/Game/Small Item Reward Card.prefab");
                try
                {
                    var so = new SerializedObject(root.GetComponent<SmallItemRewardCard>());
                    var label = Object.Instantiate((TMP_Text)so.FindProperty("amountText").objectReferenceValue, root.transform);
                    label.name = "Reward Name"; Place(label.rectTransform, -.1f, -.30f, 1.1f, -.02f);
                    label.fontSize = label.fontSizeMax = 24; label.fontSizeMin = 16; label.enableAutoSizing = true; label.text = "Equipment / Fragments";
                    label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
                    so.FindProperty("rewardName").objectReferenceValue = label; so.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, LootCard);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var lootRoot = PrefabUtility.LoadPrefabContents(LootCard);
            try
            {
                var so = new SerializedObject(lootRoot.GetComponent<SmallItemRewardCard>());
                var icon = (Image)so.FindProperty("iconImage").objectReferenceValue; icon.rectTransform.sizeDelta = Vector2.one * 140;
                var count = (TMP_Text)so.FindProperty("amountText").objectReferenceValue;
                count.rectTransform.anchoredPosition = new Vector2(0,-113); count.rectTransform.sizeDelta = new Vector2(190,44); count.fontSize = 32;
                var title = (TMP_Text)so.FindProperty("rewardName").objectReferenceValue;
                Place(title.rectTransform,-.12f,-.52f,1.12f,-.18f);title.fontSize=title.fontSizeMax=32;title.fontSizeMin=24;
                PrefabUtility.SaveAsPrefabAsset(lootRoot,LootCard);
            }
            finally { PrefabUtility.UnloadPrefabContents(lootRoot); }
            var card = AssetDatabase.LoadAssetAtPath<GameObject>(LootCard);
            foreach (var name in new[] { "Stage Complete", "Stage Failed" })
            {
                string path = "Assets/The Archer/Prefabs/UI/Game/" + name + ".prefab"; var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var component = (Component)root.GetComponent<StageCompleteScreen>() ?? root.GetComponent<StageFailedScreen>();
                    var so = new SerializedObject(component); so.FindProperty("itemRewardCardPrefab").objectReferenceValue = card;
                    var grid = (GridLayoutGroup)so.FindProperty("rewardsGrid").objectReferenceValue;
                    grid.cellSize = new Vector2(190,190); grid.spacing = new Vector2(40,115); grid.padding.bottom = 110;
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; so.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var wallet = AssetDatabase.LoadAssetAtPath<GameObject>(GemWallet);
            if (!wallet)
            {
                var root = new GameObject("Gem Wallet", typeof(RectTransform), typeof(Image), typeof(MenuWalletView));
                root.layer = 5;
                var panel = root.GetComponent<Image>(); panel.sprite = art.slotSprite; panel.material = art.slotMaterial; panel.type = Image.Type.Sliced; panel.color = Color.white; panel.raycastTarget = false;
                var iconRoot = new GameObject("Gem Icon", typeof(RectTransform), typeof(Image)); iconRoot.layer = 5; iconRoot.transform.SetParent(root.transform, false);
                var icon = iconRoot.GetComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false; Place(icon.rectTransform, .05f, .12f, .28f, .88f);
                var currencies = AssetDatabase.LoadAssetAtPath<MrX.Currency.CurrenciesDatabase>("Assets/The Archer/Scriptables/Currencies Database.asset");
                for(int i=0;i<currencies.Count;i++)if(currencies.GetCurrency(i).ID=="gem")icon.sprite=currencies.GetCurrency(i).Icon;
                var labelRoot = new GameObject("Gem Amount", typeof(RectTransform), typeof(TextMeshProUGUI)); labelRoot.layer = 5; labelRoot.transform.SetParent(root.transform, false);
                var label = labelRoot.GetComponent<TextMeshProUGUI>(); label.font = art.font; label.fontSize = label.fontSizeMax = 54; label.fontSizeMin = 28; label.enableAutoSizing = true;
                label.color = new Color(.12f,.23f,.56f); label.alignment = TextAlignmentOptions.MidlineLeft; label.text = "0"; label.raycastTarget = false; Place(label.rectTransform,.32f,.1f,.96f,.9f);
                var so = new SerializedObject(root.GetComponent<MenuWalletView>()); so.FindProperty("icon").objectReferenceValue=icon;so.FindProperty("amount").objectReferenceValue=label;so.ApplyModifiedPropertiesWithoutUndo();
                wallet=PrefabUtility.SaveAsPrefabAsset(root,GemWallet);Object.DestroyImmediate(root);
            }
            foreach (var name in new[] { "Armory", "Upgrades", "Forge", "Daily Quests", "Achievements" })
            {
                string path = "Assets/The Archer/Prefabs/UI/Menu Pages/" + name + " Page.prefab"; var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform parent = name=="Armory"||name=="Upgrades" ? root.transform : root.transform.Find("Top Bar");
                    var existing=parent.Find("Gem Wallet");var view=existing?existing.gameObject:(GameObject)PrefabUtility.InstantiatePrefab(wallet,parent);
                    var rect=(RectTransform)view.transform;
                    if(name=="Armory") { rect.anchorMin=rect.anchorMax=Vector2.one; rect.pivot=Vector2.one*.5f; rect.anchoredPosition=new Vector2(-500,-111.68f);rect.sizeDelta=new Vector2(264.37f,91.7f); }
                    else if(name=="Upgrades")Place(rect,.665f,.90f,.78f,.97f);
                    else Place(rect,.52f,.14f,.70f,.86f);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(rect); PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }
        private static void Place(RectTransform rect,float x,float y,float r,float t)
        {rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(r,t);rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
