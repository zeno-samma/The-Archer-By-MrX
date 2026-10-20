using System;
using System.Linq;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class ResponsiveMenuAuthoring
    {
        private static ResponsiveMenuLayout profile;
        private static Transform root;
        [MenuItem("Tools/MrX Studio/UI/Install Portrait and Landscape Layouts")]
        public static void Install()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            MenuEconomyAuthoring.ImportTranslations();
            InstallPurchaseCounter();
            foreach(var path in MenuEconomyAuthoring.Pages)
            {
                var asset=PrefabUtility.LoadPrefabContents(path);root=asset.transform;
                try
                {
                    profile=asset.GetComponent<ResponsiveMenuLayout>();
                    if(profile==null)
                    {
                        profile=asset.AddComponent<ResponsiveMenuLayout>();
                        Typography();Common();
                        if(path.Contains("Stage Page"))Stage();else if(path.Contains("Armory Page"))Armory();else if(path.Contains("Upgrades Page"))Upgrades();else if(path.Contains("Daily Quests"))Quests();else if(path.Contains("Achievements"))Achievements();else Forge();
                    }
                    ScrollTargets(asset);
                    EditorUtility.SetDirty(profile);PrefabUtility.SaveAsPrefabAsset(asset,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(asset);}
            }
            InstallDock();InstallSurface();AssetDatabase.SaveAssets();
        }
        private static void Typography()
        {
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))profile.texts.Add(new ResponsiveMenuLayout.TextStyle{target=text,landscape=text.enableAutoSizing?text.fontSizeMax:text.fontSize,portrait=Mathf.Clamp(text.fontSize*.75f,28,42),autoSize=text.enableAutoSizing});
        }
        private static void Common()
        {
            Rect("Menu Economy/Energy Wallet",.03f,.94f,.36f,.985f);Rect("Menu Economy/Gold Wallet",.38f,.94f,.65f,.985f);Rect("Menu Economy/Gem Wallet",.67f,.94f,.89f,.985f);
            Rect("Menu Economy/Energy Recovery",.03f,.921f,.36f,.939f);Rect("Menu Economy/Run Energy Cost",.40f,.921f,.90f,.939f);
            Rect("Menu Economy/Exchange Modal/Dialog",.06f,.28f,.94f,.73f);
            Rect("Menu Economy/Exchange Modal/Dialog/Purchase Limit",.06f,.38f,.94f,.47f);
            Rect("Settings",.91f,.94f,.985f,.985f);Rect("Settings Button",.91f,.94f,.985f,.985f);
            Rect("Top Bar",.04f,.87f,.96f,.915f);Rect("Page Heading",.04f,.875f,.96f,.915f);
        }
        private static void Stage()
        {
            Point("Hero Panel",.5f,.64f,0,0,100,100,.72f);Point("Hero Panel/Info_Ui",.5f,.5f,0,610,800,100,1);
            Point("Stage Panel",.5f,.44f,0,0,100,100,.78f);
            Point("Rewards",.5f,.34f,0,0,1025,245,.8f);
            Point("Play Button Parent",.5f,.255f,0,0,650,140,1);Point("Unlock Button Parent",.5f,.255f,0,0,650,140,1);
            Point("Button Shadow",.5f,.245f,0,0,680,160,1);Point("Info Button",.86f,.255f,0,0,80,80,1);
            Point("Attempts Counter (TMP)",.5f,.212f,0,0,650,45,1);
            Rect("Stage Lucky Rewards/Lucky Wheel Shortcut",.10f,.105f,.44f,.20f);Rect("Stage Lucky Rewards/Gem Chest Shortcut",.56f,.105f,.90f,.20f);
            Rect("Language Picker/Choose Language",.03f,.865f,.39f,.91f);Rect("Language Picker/Language Modal/Dialog",.06f,.22f,.94f,.82f);
            Rect("Developer Tools/Developer Shortcut",.66f,.865f,.9f,.91f);Rect("Developer Tools/Developer Panel/Panel",.04f,.18f,.96f,.85f);
            foreach(var text in root.Find("Hero Panel/Info_Ui").GetComponentsInChildren<TMP_Text>(true))Font(text,48);
            foreach(var popup in root.GetComponentsInChildren<LuckyRewardsPopup>(true))
            {
                var so=new SerializedObject(popup);var panel=(RectTransform)popup.transform.Find("Panel");if(panel==null)continue;
                Rect(panel,.03f,.12f,.97f,.88f);
                Rect(panel.Find("Showcase") as RectTransform,.07f,.49f,.93f,.86f);
                var frame=panel.Find("Showcase/Wheel Frame") as RectTransform;if(frame!=null)Square(frame,.5f,.5f,650);
                var disc=so.FindProperty("wheelDisc");if(disc!=null&&disc.objectReferenceValue!=null)Rect((RectTransform)disc.objectReferenceValue,0,0,1,1);
                var rewards=so.FindProperty("rewardsScroll");if(rewards!=null&&rewards.objectReferenceValue!=null)Rect(((ScrollRect)rewards.objectReferenceValue).transform as RectTransform,.04f,.20f,.96f,.47f);
                Rect(panel.Find("Rewards Viewport") as RectTransform,.04f,.20f,.96f,.47f);
                Rect(panel.Find("Purchase") as RectTransform,.12f,.04f,.88f,.17f);
                Rect(panel.Find("Reward Heading") as RectTransform,.06f,.46f,.94f,.49f);Rect(panel.Find("Status") as RectTransform,.04f,.002f,.96f,.04f);
                Rect(panel.Find("Title") as RectTransform,.05f,.90f,.65f,.98f);Rect(panel.Find("Gem Wallet") as RectTransform,.68f,.90f,.87f,.98f);Rect(panel.Find("Close") as RectTransform,.90f,.91f,.98f,.98f);
                Rect(popup.transform.Find("Reward Result/Body") as RectTransform,.08f,.28f,.92f,.77f);
            }
        }
        private static void Armory()
        {
            Rect("Info Panel",.03f,.40f,.97f,.60f);Rect("Armory Panel",.03f,.11f,.97f,.39f);
            Point("Hero Panel",.5f,.70f,0,0,0,0,.55f);Rect("Hero Name and Level",.04f,.85f,.96f,.89f);
            Rect("Hero Selector",.12f,.61f,.88f,.68f);Rect("Armory Modal/Panel",.05f,.16f,.95f,.86f);
            var inventory=assetComponent<ArmoryDashboard>();var so=new SerializedObject(profile);so.FindProperty("inventory").objectReferenceValue=inventory;so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static T assetComponent<T>()where T:Component{return root.GetComponent<T>();}
        private static void Upgrades()
        {
            Rect("Main Frame",.03f,.11f,.97f,.87f);Rect("Feedback",.04f,.87f,.96f,.89f);
            Rect("Main Frame/Selected Upgrade",.02f,.68f,.98f,.99f);Rect("Main Frame/Player Stats Preview",.02f,.47f,.98f,.67f);Rect("Main Frame/Upgrade List",.02f,.01f,.98f,.46f);
            var page=assetComponent<UpgradeDashboardPage>();var so=new SerializedObject(page);
            Rect(Ref<Image>(so,"selectedIcon").rectTransform,.03f,.40f,.27f,.94f);Rect(Ref<TMP_Text>(so,"selectedTitle").rectTransform,.30f,.73f,.97f,.96f);
            Rect(Ref<TMP_Text>(so,"selectedDescription").rectTransform,.30f,.40f,.97f,.72f);Rect(Ref<TMP_Text>(so,"selectedLevel").rectTransform,.03f,.25f,.48f,.4f);
            Rect(Ref<TMP_Text>(so,"selectedEffect").rectTransform,.03f,.06f,.48f,.25f);Rect(Ref<Button>(so,"upgradeButton").transform as RectTransform,.53f,.06f,.97f,.35f);
            var rows=so.FindProperty("rows");for(int i=0;i<rows.arraySize;i++)
            {
                var row=rows.GetArrayElementAtIndex(i);var rect=Ref<Button>(row,"select").transform as RectTransform;Row(rect,240);
                Rect(Ref<Image>(row,"icon").rectTransform,.015f,.36f,.19f,.95f);Rect(Ref<TMP_Text>(row,"title").rectTransform,.22f,.66f,.98f,.95f);
                Rect(Ref<TMP_Text>(row,"description").rectTransform,.22f,.40f,.98f,.68f);Rect(Ref<TMP_Text>(row,"level").rectTransform,.22f,.24f,.55f,.40f);
                Rect(Ref<TMP_Text>(row,"values").rectTransform,.02f,.02f,.60f,.22f);Rect(Ref<Button>(row,"buy").transform as RectTransform,.69f,.03f,.97f,.30f);
                var segments=row.FindPropertyRelative("segments");for(int j=0;j<segments.arraySize;j++)Rect(((Image)segments.GetArrayElementAtIndex(j).objectReferenceValue).rectTransform,.24f+j*.048f,.17f,.28f+j*.048f,.23f);
            }
        }
        private static void Quests()
        {
            const string sheet="Quest Sheet - ui_slot sliced/";
            Rect(sheet.TrimEnd('/'),.03f,.11f,.97f,.87f);Rect(sheet+"Header",.03f,.80f,.97f,.98f);Rect(sheet+"Daily Milestones",.03f,.64f,.97f,.79f);Rect(sheet+"Quest List",.03f,.065f,.97f,.63f);
            Rect(sheet+"Header/Title Icon",.03f,.58f,.18f,.96f);Rect(sheet+"Header/Title",.20f,.56f,.98f,.95f);Rect(sheet+"Header/Subtitle",.05f,.30f,.95f,.55f);Rect(sheet+"Header/Reset Timer",.10f,.04f,.90f,.27f);
            foreach(var child in root.Find(sheet+"Quest List/Viewport/Content").Cast<Transform>())
            {
                var p=Path(child);Row(child as RectTransform,250);Rect(p+"/Icon Tile",.03f,.44f,.18f,.94f);Rect(p+"/Title",.21f,.65f,.96f,.95f);Rect(p+"/Description",.21f,.35f,.96f,.65f);
                Rect(p+"/Progress",.04f,.12f,.42f,.24f);Rect(p+"/Progress Label",.43f,.04f,.59f,.30f);Rect(p+"/Reward",.60f,.05f,.77f,.31f);Rect(p+"/Quest Action",.79f,.05f,.97f,.31f);
            }
        }
        private static void Achievements()
        {
            const string sheet="Achievements Sheet - ui_slot sliced/";
            Rect(sheet.TrimEnd('/'),.03f,.11f,.97f,.87f);Rect(sheet+"Header",.02f,.76f,.98f,.99f);Rect(sheet+"Body",.015f,.04f,.985f,.75f);
            Rect(sheet+"Header/Trophy Illustration",.02f,.42f,.22f,.98f);Rect(sheet+"Header/Title",.25f,.71f,.98f,.98f);Rect(sheet+"Header/Subtitle",.25f,.43f,.98f,.72f);Rect(sheet+"Header/Overall Completion",.03f,.02f,.97f,.41f);
            Rect(sheet+"Body/Categories",.02f,.83f,.98f,.99f);Rect(sheet+"Body/Achievement List",.02f,.03f,.98f,.81f);
            int i=0;foreach(Transform tab in root.Find(sheet+"Body/Categories")){Rect(tab as RectTransform,i*.25f+.005f,.02f,(i+1)*.25f-.005f,.98f);Rect(Path(tab)+"/Icon Tile",.04f,.40f,.28f,.90f);Rect(Path(tab)+"/Category Title",.31f,.42f,.95f,.94f);Rect(Path(tab)+"/Category Progress",.15f,.05f,.95f,.4f);i++;}
            foreach(Transform row in root.Find(sheet+"Body/Achievement List/Viewport/Content"))
            {
                string p=Path(row);Row(row as RectTransform,240);Rect(p+"/Icon Tile",.02f,.37f,.16f,.93f);Rect(p+"/Title",.18f,.67f,.97f,.94f);Rect(p+"/Description",.18f,.43f,.97f,.70f);
                Rect(p+"/Progress",.04f,.12f,.42f,.25f);Rect(p+"/Progress Label",.43f,.06f,.58f,.30f);Rect(p+"/Reward",.59f,.06f,.76f,.31f);Rect(p+"/Claim",.78f,.04f,.98f,.33f);
            }
        }
        private static void Forge()
        {
            Hide("Anvil Illustration");Hide("Forge Aura");Rect("Equipment Showcase",.38f,.72f,.62f,.86f);Rect("Previous Equipment",.08f,.76f,.24f,.83f);Rect("Next Equipment",.76f,.76f,.92f,.83f);
            Rect("Equipment Selection",.07f,.69f,.93f,.72f);Rect("Fragment Progress",.07f,.66f,.93f,.69f);Rect("Crafting Materials - ui_slot sliced",.03f,.49f,.97f,.65f);
            const string detail="Upgrade Details - ui_slot sliced/";Rect(detail.TrimEnd('/'),.03f,.11f,.97f,.48f);Rect(detail+"Selected Equipment",.03f,.77f,.18f,.98f);Rect(detail+"Item Name",.20f,.88f,.97f,.99f);Rect(detail+"Level Preview",.20f,.76f,.98f,.88f);
            Rect(detail+"Stat Comparison",.04f,.46f,.96f,.74f);Rect(detail+"Upgrade Result",.04f,.19f,.96f,.44f);Rect(detail+"Forge",.04f,.02f,.96f,.18f);
        }
        private static void InstallPurchaseCounter()
        {
            var path=MenuEconomyAuthoring.PrefabPath;var go=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var dialog=go.transform.Find("Exchange Modal/Dialog");var counter=dialog.Find("Purchase Limit");
                if(counter==null)
                {
                    var label=Object.Instantiate(dialog.Find("Gem Price").gameObject,dialog);label.name="Purchase Limit";var rect=(RectTransform)label.transform;rect.anchorMin=new Vector2(.06f,.37f);rect.anchorMax=new Vector2(.94f,.46f);rect.offsetMin=rect.offsetMax=Vector2.zero;
                    var text=label.GetComponent<TMP_Text>();text.fontSize=text.fontSizeMax=30;text.fontSizeMin=18;text.text="PURCHASES LEFT TODAY: 5 / 5";
                    var price=(RectTransform)dialog.Find("Gem Price");price.anchorMin=new Vector2(.08f,.46f);price.anchorMax=new Vector2(.92f,.54f);
                    var so=new SerializedObject(go.GetComponent<MenuEconomyPanel>());so.FindProperty("purchaseLimit").objectReferenceValue=text;so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(go,path);
            }finally{PrefabUtility.UnloadPrefabContents(go);}
        }
        private static void InstallDock()
        {
            const string path="Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab";var go=PrefabUtility.LoadPrefabContents(path);root=go.transform;
            try
            {
                profile=go.GetComponent<ResponsiveMenuLayout>();
                if(profile==null)
                {
                    profile=go.AddComponent<ResponsiveMenuLayout>();Typography();Rect((RectTransform)root,.02f,.015f,.98f,.09f);
                    foreach(var tab in go.GetComponentsInChildren<MainMenuDockItemBehavior>(true))
                    {
                        var rt=(RectTransform)tab.transform;var icon=rt.Find("Icon");if(icon!=null){var style=ResponsiveMenuLayout.Placement.Capture((RectTransform)icon);style.scale=Vector3.one*.62f;profile.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=(RectTransform)icon,landscape=ResponsiveMenuLayout.Placement.Capture((RectTransform)icon),portrait=style});}
                        var label=rt.Find("Text (TMP)") as RectTransform;if(label!=null){Rect(label,0,.02f,1,.32f);Font(label.GetComponent<TMP_Text>(),22);}
                    }
                    PrefabUtility.SaveAsPrefabAsset(go,path);
                }
                var grid=go.GetComponent<GridLayoutGroup>();
                if(profile.grids.Count==0)profile.grids.Add(new ResponsiveMenuLayout.GridStyle{target=grid,landscapeSize=new Vector2(300,180),landscapePadding=new RectOffset(),portraitSize=new Vector2(172.8f,150),portraitPadding=new RectOffset()});
                foreach(var style in profile.texts)
                {
                    style.landscape=40;style.portrait=22;style.autoSize=true;style.target.textWrappingMode=TextWrappingModes.NoWrap;
                    var rect=profile.rectangles.Find(r=>r.target==style.target.rectTransform);if(rect!=null)rect.landscape=new ResponsiveMenuLayout.Placement{min=Vector2.zero,max=new Vector2(1,.32f),pivot=Vector2.one*.5f,scale=Vector3.one};
                }
                var indicators=root.Find("Gamepad Indicators");if(indicators!=null){var element=indicators.GetComponent<LayoutElement>();if(element==null)element=indicators.gameObject.AddComponent<LayoutElement>();element.ignoreLayout=true;}
                PrefabUtility.SaveAsPrefabAsset(go,path);
            }finally{PrefabUtility.UnloadPrefabContents(go);}
        }
        private static void InstallSurface()
        {
            const string path="Assets/The Archer/Prefabs/UI/Menu Pages/Responsive Menu Surface.prefab";
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(asset==null)
            {
                var go=new GameObject("Responsive Menu Surface",typeof(RectTransform),typeof(ResponsiveMenuSurface));var rt=(RectTransform)go.transform;rt.sizeDelta=new Vector2(3840,2160);
                asset=PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);
            }
            var canvas=GameObject.Find("Main Menu Canvas");if(canvas==null)throw new Exception("Open Main Menu scene before installing responsive UI.");
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.referenceResolution=new Vector2(3840,2160);scaler.matchWidthOrHeight=.5f;
            var background=canvas.transform.Find("Background Image");var image=background.GetComponent<Image>();
            var aspect=background.GetComponent<AspectRatioFitter>();if(aspect==null)aspect=background.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=image.sprite.rect.width/image.sprite.rect.height;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            var surface=canvas.transform.Find("Responsive Menu Surface");
            if(surface==null)surface=((GameObject)PrefabUtility.InstantiatePrefab(asset,canvas.transform)).transform;
            var children=canvas.transform.Cast<Transform>().Where(c=>c!=surface&&c.name!="Background Image").ToArray();
            foreach(var child in children)
            {
                child.SetParent(surface,false);
                if(child.GetComponent<MainMenuPageBehavior>()!=null){var r=(RectTransform)child;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
            foreach(var module in Object.FindObjectsByType<InputSystemUIInputModule>(FindObjectsInactive.Include))
            {
                var refs=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(module.actionsAsset)).OfType<InputActionReference>();
                var wheel=refs.FirstOrDefault(a=>a.action!=null&&a.action.name=="ScrollWheel");
                if(wheel==null)throw new Exception("Missing imported UI/ScrollWheel action reference.");
                module.scrollWheel=wheel;EditorUtility.SetDirty(module);
            }
            EditorUtility.SetDirty(scaler);EditorSceneManager.MarkSceneDirty(canvas.scene);EditorSceneManager.SaveScene(canvas.scene);
        }
        private static void ScrollTargets(GameObject go)
        {
            foreach(var scroll in go.GetComponentsInChildren<ScrollRect>(true))
            {
                scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=90;
                var image=scroll.viewport.GetComponent<Image>();if(image==null)image=scroll.viewport.gameObject.AddComponent<Image>();image.color=Color.clear;image.raycastTarget=true;
            }
        }
        private static string Path(Transform tr){return AnimationUtility.CalculateTransformPath(tr,root);}
        private static T Ref<T>(SerializedObject so,string name)where T:Object{return (T)so.FindProperty(name).objectReferenceValue;}
        private static T Ref<T>(SerializedProperty so,string name)where T:Object{return (T)so.FindPropertyRelative(name).objectReferenceValue;}
        private static void Rect(string path,float x,float y,float r,float t){Rect(root.Find(path) as RectTransform,x,y,r,t);}
        private static void Rect(RectTransform target,float x,float y,float r,float t)
        {
            if(target==null)return;profile.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=target,landscape=ResponsiveMenuLayout.Placement.Capture(target),portrait=new ResponsiveMenuLayout.Placement{min=new Vector2(x,y),max=new Vector2(r,t),offsetMin=Vector2.zero,offsetMax=Vector2.zero,pivot=target.pivot,scale=Vector3.one}});
        }
        private static void Point(string path,float x,float y,float px,float py,float w,float h,float scale)
        {
            var target=root.Find(path) as RectTransform;if(target==null)return;
            profile.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=target,landscape=ResponsiveMenuLayout.Placement.Capture(target),portrait=new ResponsiveMenuLayout.Placement{min=new Vector2(x,y),max=new Vector2(x,y),pivot=Vector2.one*.5f,offsetMin=new Vector2(px-w/2,py-h/2),offsetMax=new Vector2(px+w/2,py+h/2),scale=Vector3.one*scale}});
        }
        private static void Square(RectTransform target,float x,float y,float size){Point(Path(target),x,y,0,0,size,size,1);}
        private static void Row(RectTransform target,float height){if(target==null)return;var element=target.GetComponent<LayoutElement>();if(element!=null)profile.rows.Add(new ResponsiveMenuLayout.RowStyle{target=element,landscape=element.preferredHeight,portrait=height});}
        private static void Font(TMP_Text label,float size){var style=profile.texts.Find(s=>s.target==label);if(style!=null)style.portrait=size;}
        private static void Hide(string path){var tr=root.Find(path);if(tr!=null)profile.visibility.Add(new ResponsiveMenuLayout.Visibility{target=tr.gameObject,landscape=tr.gameObject.activeSelf,portrait=false});}
    }
}
