using System;
using System.Linq;
using MrX.UI;
using MrX.UI.Armory;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    // Authoring only. All graphics and both orientation profiles are saved in prefabs.
    public static partial class RepMenuAuthoring
    {
        const string Root = "Assets/The Archer/";
        const string Rep = Root + "Sprites/UI/Rep/";
        static Transform root;
        static ResponsiveMenuLayout layout;
        static readonly Color Ink = new Color(.035f,.10f,.25f);
        static readonly Color Navy = new Color(.035f,.10f,.23f);
        static readonly Color Cream = new Color(1,.96f,.86f);
        static string[] Assets = {
            "Button/Button01_Dark", "Button/Button01_Blue", "Button/Button01_Red", "Button/Button02_Yellow", "Button/Button_Square03_Navy", "Button/Button_Square05_Blue", "Button/Button_Circle147_Navy", "Button/Menu_TopBtn_Focus",
            "Frame/StageFrame_n_Blue", "Frame/BannerFrame03_Single_Navy", "Frame/CardFrame07_Focus", "Frame/ItemFrame01_f", "Frame/ItemFrame01_Single_Purple", "Frame/ItemFrame01_Single_Green", "Frame/ItemFrame01_Single_Blue", "Frame/ItemFrame01_Single_Yellow", "Frame/ItemFrame01_Single_Red", "Frame/ItemFrame02_White1", "Frame/ItemFrame03_Single_Navy", "Frame/CardFrame04_White1", "Frame/ProfileFrame01_Border",
            "Slider/Slider_Level01_Bg", "Slider/Slider_Level01_Fill_Green",
            "UI_Etc/ResourceBar_Bg", "UI_Etc/ResourceBar_Icon_Energy", "UI_Etc/ResourceBar_Icon_Gold", "UI_Etc/ResourceBar_Icon_Gem", "UI_Etc/ResourceBar_Btn_Single_Green", "UI_Etc/ResourceBar_Btn_Single_Yellow", "UI_Etc/ResourceBar_Btn_Icon_Add", "UI_Etc/Alert_Dot_Bg",
            "Icon_ItemIcons/Original/Icon_Map", "Icon_ItemIcons/Original/Icon_Shield", "Icon_ItemIcons/Original/Icon_Hammer", "Icon_ItemIcons/Original/Icon_Quest", "Icon_ItemIcons/Original/Icon_Trophy", "Icon_ItemIcons/Original/Icon_GearWheels", "Icon_ItemIcons/Original/Icon_Heart", "Icon_ItemIcons/Original/Icon_Star", "Icon_ItemIcons/Original/Icon_Sword", "Icon_ItemIcons/Original/Icon_Boots", "Icon_ItemIcons/Original/Icon_Lock"
        };

        [MenuItem("Tools/MrX Studio/UI/Apply Approved Rep Menu")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            foreach (var name in Assets) Import(name);
            var stageOne=AssetDatabase.LoadAssetAtPath<MrX.StageData>(Root+"Scriptables/Stages/Stage Data 001.asset");
            if(stageOne!=null){var so=new SerializedObject(stageOne);so.FindProperty("stageImage").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Main Menu/ui_background_main.png");so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(stageOne);}
            ConfigureInventoryArt();
            var translations=Resources.Load<MrX.Localization.MenuTranslations>("Menu Translations");
            if(!translations.entries.Any(e=>e.english=="FREE")){translations.entries.Add(new MrX.Localization.TranslationEntry{english="FREE",vietnamese="MIỄN PHÍ",spanish="GRATIS",portuguese="GRÁTIS",french="GRATUIT"});EditorUtility.SetDirty(translations);}
            Edit(Root+"Prefabs/UI/Armory/Item Card.prefab", ItemCard);
            Edit(Root+"Prefabs/UI/Menu Pages/Menu Dock.prefab", Dock);
            Edit(Root+"Prefabs/UI/Stage/Stage Page.prefab", Stage);
            Edit(Root+"Prefabs/UI/Menu Pages/Armory Page.prefab", Armory);
            // Existing scene instances carry older layout overrides: update only presentation targets.
            foreach (var page in Object.FindObjectsByType<MainMenuStagesPageBehavior>(FindObjectsInactive.Include)) ApplyScene(page.gameObject, Stage);
            foreach (var page in Object.FindObjectsByType<MainMenuArmoryPageBehavior>(FindObjectsInactive.Include)) ApplyScene(page.gameObject, Armory);
            foreach (var dock in Object.FindObjectsByType<MainMenuDockBehavior>(FindObjectsInactive.Include)) ApplyScene(dock.gameObject, Dock);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
        }
        static void ApplyScene(GameObject go, Action action)
        {
            if (!go.scene.IsValid() || string.IsNullOrEmpty(go.scene.path)) return;
            SetRoot(go); action();
            foreach (var c in go.GetComponentsInChildren<Component>(true)) if (c != null) { EditorUtility.SetDirty(c); if (PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c); }
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
        static void Edit(string path, Action action)
        {
            var go=PrefabUtility.LoadPrefabContents(path);
            try {SetRoot(go); action(); PrefabUtility.SaveAsPrefabAsset(go,path);}
            finally {PrefabUtility.UnloadPrefabContents(go);}
        }
        static void SetRoot(GameObject go) {root=go.transform; layout=go.GetComponent<ResponsiveMenuLayout>();}
        static void Import(string name)
        {
            var path=Rep+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null) throw new Exception("Missing Rep asset: "+path);
            if(importer.textureType==TextureImporterType.Sprite && AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Any()) return;
            var tex=new Texture2D(2,2); tex.LoadImage(System.IO.File.ReadAllBytes(path));
            var pixels=tex.GetPixels32(); int x0=tex.width,y0=tex.height,x1=0,y1=0;
            for(int y=0;y<tex.height;y++) for(int x=0;x<tex.width;x++) if(pixels[y*tex.width+x].a>24) {x0=Math.Min(x0,x);x1=Math.Max(x1,x);y0=Math.Min(y0,y);y1=Math.Max(y1,y);}
            var rect=new Rect(Math.Max(0,x0-2),Math.Max(0,y0-2),Math.Min(tex.width-x0+2,x1-x0+5),Math.Min(tex.height-y0+2,y1-y0+5));
            bool icon=name.Contains("Icon")||name.Contains("Alert_Dot");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.spritePixelsPerUnit=100;
            importer.maxTextureSize=icon?512:2048;importer.textureCompression=icon?TextureImporterCompression.Compressed:TextureImporterCompression.Uncompressed;
#pragma warning disable 618
            importer.spritesheet=new[]{new SpriteMetaData{name=System.IO.Path.GetFileName(name),rect=rect,pivot=Vector2.one*.5f,alignment=0,border=icon?Vector4.zero:SurfaceBorder(name,rect)}};
#pragma warning restore 618
            importer.SaveAndReimport(); Object.DestroyImmediate(tex);
        }
        static Sprite S(string name) {return AssetDatabase.LoadAllAssetsAtPath(Rep+name+".png").OfType<Sprite>().First();}
        static Transform Find(string path) {return root.Find(path);}
        static RectTransform Node(Transform parent,string name)
        {
            var t=parent.Find(name);if(t!=null)return (RectTransform)t;
            var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);return (RectTransform)go.transform;
        }
        static Image Image(Transform t,string sprite,Color? color=null)
        {
            var image=t.GetComponent<Image>();if(image==null)image=t.gameObject.AddComponent<Image>();
            image.enabled=true;image.sprite=S(sprite);image.color=color??(sprite=="Button/Button01_Dark"?new Color(.42f,.64f,1):Color.white);image.material=null;
            image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=3;image.preserveAspect=false;image.raycastTarget=t.GetComponent<Button>()!=null;
            if(layout!=null){var style=layout.images.Find(v=>v.target==image);if(style==null){style=new ResponsiveMenuLayout.ImageStyle{target=image};layout.images.Add(style);}style.landscape=3;style.portrait=6;}
            var shadow=t.GetComponent<Shadow>();if(shadow!=null)shadow.enabled=false;
            return image;
        }
        static void Skin(string path,string sprite,Color? color=null) {var t=Find(path);if(t!=null)Image(t,sprite,color);}
        static void Panel(string path)
        {
            var t=Find(path);Image(t,"Frame/CardFrame07_Focus");
            var fill=Node(t,"Rep Cream Fill");Image(fill,"Frame/CardFrame04_White1",Cream);StaticBox(fill,.018f,.025f,.982f,.975f);fill.SetAsFirstSibling();
        }
        static void Icon(Transform t,string sprite) {var im=Image(t,sprite);im.type=UnityEngine.UI.Image.Type.Simple;im.preserveAspect=true;im.raycastTarget=false;}
        static void StaticBox(RectTransform r,float x,float y,float z,float w) {r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(z,w);r.pivot=Vector2.one*.5f;r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;}
        static ResponsiveMenuLayout.Placement Box(float x,float y,float z,float w) {return new ResponsiveMenuLayout.Placement{min=new Vector2(x,y),max=new Vector2(z,w),pivot=Vector2.one*.5f,scale=Vector3.one};}
        static void Rect(string path,float x,float y,float z,float w,float px,float py,float pz,float pw) {var t=Find(path) as RectTransform;if(t!=null)Rect(t,x,y,z,w,px,py,pz,pw);}
        static void Rect(RectTransform t,float x,float y,float z,float w,float px,float py,float pz,float pw)
        {
            var value=layout.rectangles.Find(v=>v.target==t);if(value==null){value=new ResponsiveMenuLayout.RectStyle{target=t};layout.rectangles.Add(value);}
            value.landscape=Box(x,y,z,w);value.portrait=Box(px,py,pz,pw);value.landscape.Apply(t);
        }
        static void Point(string path,float x,float y,float dx,float dy,float width,float height,float scale,float px,float py,float ps)
        {
            var t=Find(path) as RectTransform;if(t==null)return;
            var v=layout.rectangles.Find(a=>a.target==t);if(v==null){v=new ResponsiveMenuLayout.RectStyle{target=t};layout.rectangles.Add(v);}
            v.landscape=Box(x,y,x,y);v.landscape.offsetMin=new Vector2(dx-width/2,dy-height/2);v.landscape.offsetMax=new Vector2(dx+width/2,dy+height/2);v.landscape.scale=Vector3.one*scale;
            v.portrait=Box(px,py,px,py);v.portrait.offsetMin=v.landscape.offsetMin;v.portrait.offsetMax=v.landscape.offsetMax;v.portrait.scale=Vector3.one*ps;v.landscape.Apply(t);
        }
        static void Font(TMP_Text text,float l,float p,Color? color=null)
        {
            if(text==null)return;
            if(text.font!=null)text.fontSharedMaterial=text.font.material;
            text.color=color??Ink;text.fontStyle=FontStyles.Bold;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            text.textWrappingMode=TextWrappingModes.NoWrap;
            var v=layout.texts.Find(a=>a.target==text);if(v==null){v=new ResponsiveMenuLayout.TextStyle{target=text};layout.texts.Add(v);}v.landscape=l;v.portrait=p;v.autoSize=true;
        }
        static void Font(string path,float l,float p,Color? color=null) {var t=Find(path);if(t!=null)Font(t.GetComponent<TMP_Text>(),l,p,color);}
        static TMP_Text Text(Transform parent,string name,string value,float size,Color color)
        {
            var t=Node(parent,name);var text=t.GetComponent<TMP_Text>();if(text==null)text=t.gameObject.AddComponent<TextMeshProUGUI>();
            if(text.font==null)text.font=root.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(v=>v!=text&&v.font!=null)?.font;
            text.text=value;Font(text,size,size*.65f,color);return text;
        }
        static void Hide(string path)
        {
            var t=Find(path);if(t==null)return;t.gameObject.SetActive(false);
            foreach(var p in root.GetComponentsInChildren<ResponsiveMenuLayout>(true)) {var v=p.visibility.Find(a=>a.target==t.gameObject);if(v!=null)v.landscape=v.portrait=false;}
        }
        static void Finish()
        {
            foreach(var image in root.GetComponentsInChildren<Image>(true))if(image.name=="Rep Cream Fill")image.transform.SetAsFirstSibling();
            layout.rectangles.RemoveAll(v=>v.target==null);layout.texts.RemoveAll(v=>v.target==null);layout.grids.RemoveAll(v=>v.target==null);
            RepairSurfaceTree(root.gameObject);
            layout.Apply(false);EditorUtility.SetDirty(layout);
        }
        static void Header()
        {
            var profile=root.GetComponentInChildren<HeroExperienceBar>(true);
            if(profile!=null)
            {
                profile.transform.SetParent(root,false);profile.name="Player Profile";profile.gameObject.SetActive(true);
                Rect("Player Profile",.02f,.885f,.38f,.98f,.025f,.90f,.60f,.98f);Image(profile.transform,"Button/Button01_Dark");
                var so=new SerializedObject(profile);
                so.FindProperty("useFaceCrop").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();
                var portrait=(Image)so.FindProperty("portrait").objectReferenceValue;
                var mask=Node(profile.transform,"Portrait Mask");var mi=Image(mask,"Frame/CardFrame04_White1");mi.color=Navy;mi.raycastTarget=false;
                if(mask.GetComponent<RectMask2D>()==null)mask.gameObject.AddComponent<RectMask2D>();
                Rect(mask,.015f,-.12f,.175f,1.12f,.015f,-.03f,.27f,1.03f);
                if(portrait.transform.parent!=mask)
                {
                    var face=Node(mask,"Face");var replacement=face.GetComponent<Image>();if(replacement==null)replacement=face.gameObject.AddComponent<Image>();replacement.sprite=portrait.sprite;replacement.raycastTarget=false;
                    portrait.gameObject.SetActive(false);portrait=replacement;so.FindProperty("portrait").objectReferenceValue=portrait;so.ApplyModifiedPropertiesWithoutUndo();
                }
                StaticBox(portrait.rectTransform,-.38f,-1.15f,1.38f,1.57f);portrait.preserveAspect=true;
                var border=Node(profile.transform,"Portrait Border");Image(border,"Frame/ProfileFrame01_Border");Rect(border,.005f,-.15f,.185f,1.15f,.005f,-.06f,.28f,1.06f);
                var title=(TMP_Text)so.FindProperty("heroName").objectReferenceValue;Rect(title.rectTransform,.21f,.49f,.97f,.94f,.31f,.51f,.98f,.96f);Font(title,56,35,Color.white);title.alignment=TextAlignmentOptions.Left;
                var fill=(Image)so.FindProperty("fill").objectReferenceValue;var track=fill.transform.parent;Image(track,"Slider/Slider_Level01_Bg");Rect((RectTransform)track,.22f,.15f,.97f,.43f,.31f,.12f,.97f,.42f);
                fill.sprite=S("Slider/Slider_Level01_Fill_Green");fill.color=Color.white;fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;StaticBox(fill.rectTransform,.015f,.13f,.985f,.87f);
                var amount=(TMP_Text)so.FindProperty("experience").objectReferenceValue;StaticBox(amount.rectTransform,0,-.04f,1,1.04f);Font(amount,30,22,Color.white);
            }
            Hide("Hero Panel/Info_Ui");
            Rect("Menu Economy",0,0,1,1,0,0,1,1);
            string[] wallets={"Energy","Gold","Gem"};
            for(int i=0;i<3;i++)
            {
                var path="Menu Economy/"+wallets[i]+" Wallet";float x=.555f+i*.13f;
                Rect(path,x,.918f,x+.124f,.971f,.025f+i*.30f,.847f,.31f+i*.30f,.887f);Skin(path,"Button/Button01_Dark");
                var icon=Find(path+"/Icon");if(icon!=null){Icon(icon,"UI_Etc/ResourceBar_Icon_"+wallets[i]);Rect((RectTransform)icon,-.05f,-.20f,.25f,1.20f,-.03f,-.15f,.22f,1.15f);}
                Rect(path+"/"+wallets[i]+" Amount",.25f,.05f,i==2?.96f:.77f,.95f,.22f,.05f,i==2?.96f:.77f,.95f);Font(path+"/"+wallets[i]+" Amount",48,30,Color.white);
                if(i<2){var buy=path+"/Buy "+wallets[i];Skin(buy,"UI_Etc/ResourceBar_Btn_Single_"+(i==0?"Green":"Yellow"));Rect(buy,.79f,-.04f,1.025f,1.04f,.79f,-.04f,1.025f,1.04f);Font(buy+"/Label",52,40,Color.white);}
            }
            Rect("Menu Economy/Energy Recovery",.555f,.895f,.679f,.917f,.025f,.825f,.31f,.847f);Font("Menu Economy/Energy Recovery",23,19,Color.white);
            Rect("Menu Economy/Run Energy Cost",.57f,.148f,.90f,.181f,.18f,.125f,.82f,.15f);Font("Menu Economy/Run Energy Cost",32,23);
            foreach(var name in new[]{"Settings Button","Settings"})
            {
                var settings=Find(name);if(settings==null)continue;Rect(name,.944f,.91f,.985f,.985f,.905f,.935f,.98f,.975f);Image(settings,"Button/Button_Square03_Navy");
                var gear=Node(settings,"Rep Gear");Icon(gear,"Icon_ItemIcons/Original/Icon_GearWheels");StaticBox(gear,.18f,.18f,.82f,.82f);
                foreach(var im in settings.GetComponentsInChildren<Image>(true))if(im.transform!=settings&&im.transform!=gear)im.enabled=false;
            }
        }
        static void Hero(bool armory)
        {
            Point("Hero Panel",.245f,.355f,0,0,100,100,1,.5f,.63f,armory?.49f:.50f);
            Point("Hero Panel/Hero/Hero Raw Image",.5f,.5f,0,460,1260,1260,1,.5f,.5f,1);
            Point("Hero Panel/Hero/Hero Stand Image",.5f,.5f,0,0,1120,310,1,.5f,.5f,1);
            Point("Hero Panel/Decor Image",.5f,.5f,0,510,1510,1510,1,.5f,.5f,1);
            var raw=Find("Hero Panel/Hero/Hero Raw Image").GetComponent<RawImage>();raw.color=Color.white;
            var fitter=raw.GetComponent<AspectRatioFitter>();if(fitter!=null){fitter.aspectMode=AspectRatioFitter.AspectMode.HeightControlsWidth;fitter.aspectRatio=1;}
            string[] slots={"Weapon","Helmet","Ring","Armor"};
            for(int i=0;i<4;i++)
            {
                string path="Hero Panel/Slots/"+slots[i]+" Slot";Point(path,.5f,.5f,i%2==0?-650:650,i<2?710:330,300,300,1,.5f,.5f,1);
                Skin(path,"Frame/ItemFrame01_f");var bg=Node(Find(path),"Navy Inset");Image(bg,"Frame/CardFrame04_White1",Navy);StaticBox(bg,.07f,.08f,.93f,.92f);bg.SetAsFirstSibling();
                var icon=Find(path+"/Icon Image");if(icon!=null){StaticBox((RectTransform)icon,.20f,.2f,.8f,.8f);icon.GetComponent<Image>().color=new Color(.55f,.65f,.8f);}
            }
            Skin("Hero Panel/Hero/Stats Background Image","Button/Button01_Dark");
            if(!armory)Point("Hero Panel/Hero/Stats Background Image",.5f,.5f,0,-5,650,95,1,.5f,.5f,1);
            if(!armory)foreach(var label in Find("Hero Panel/Hero/Stats Background Image").GetComponentsInChildren<TMP_Text>(true))Font(label,54,44,Color.white);
        }
        static void Stage()
        {
            Header();Hero(false);
            var pageStyle=new SerializedObject(root.GetComponent<MainMenuStagesPageBehavior>());pageStyle.FindProperty("playButtonEnabledSprite").objectReferenceValue=S("Button/Button02_Yellow");pageStyle.FindProperty("playButtonDisabledSprite").objectReferenceValue=S("Button/Button01_Dark");pageStyle.ApplyModifiedPropertiesWithoutUndo();
            Panel("Stage Showcase Card");Rect("Stage Showcase Card",.49f,.142f,.95f,.89f,.035f,.12f,.965f,.51f);Hide("Stage Showcase Card/Pastel Fill");
            var heading=Node(root,"Journey Heading");Image(heading,"Frame/BannerFrame03_Single_Navy");Rect(heading,.605f,.822f,.84f,.895f,.25f,.478f,.75f,.522f);
            var title=Text(heading,"Title","Journey",62,Color.white);StaticBox(title.rectTransform,.08f,.10f,.92f,.9f);
            Rect("Stage Panel",.515f,.535f,.925f,.805f,.07f,.32f,.93f,.475f);
            Rect("Stage Panel/Stage",0,0,1,1,0,0,1,1);Rect("Stage Panel/Stage/Stage Icon Mask",0,0,1,1,0,0,1,1);
            Skin("Stage Panel/Stage/Stage Icon Mask","Frame/CardFrame04_White1");
            var editorPreview=Find("Stage Panel/Stage/Stage Icon Mask/Editor Stage Preview");if(editorPreview!=null)editorPreview.GetComponent<Image>().sprite=root.GetComponent<MainMenuStagesPageBehavior>().StageDatabase.GetStageData(0).StageImage;
            Rect("Stage Panel/Stage Name",.20f,.012f,.80f,.22f,.18f,.01f,.82f,.24f);Skin("Stage Panel/Stage Name","Button/Button01_Dark");
            Rect("Stage Panel/Stage Name/Stage Number (TMP)",.02f,.57f,.98f,.96f,.02f,.55f,.98f,.96f);Font("Stage Panel/Stage Name/Stage Number (TMP)",28,22,Color.white);
            Rect("Stage Panel/Stage Name/Stage Name (TMP)",.02f,.03f,.98f,.60f,.02f,.02f,.98f,.60f);Font("Stage Panel/Stage Name/Stage Name (TMP)",45,28,Color.white);
            Rect("Stage Panel/Prev Stage Button",-.035f,.33f,.075f,.67f,-.035f,.3f,.075f,.7f);Rect("Stage Panel/Next Stage Button",.925f,.33f,1.035f,.67f,.925f,.3f,1.035f,.7f);
            // Keep the original directional arrow graphics and their gamepad shortcuts.
            var diff=Find("Stage Difficulty Selector");
            if(diff!=null)
            {
                var own=diff.GetComponent<ResponsiveMenuLayout>();if(own!=null)Object.DestroyImmediate(own);
                Rect("Stage Difficulty Selector",.52f,.45f,.92f,.52f,.07f,.27f,.93f,.315f);var im=diff.GetComponent<Image>();if(im!=null)im.enabled=false;
                var so=new SerializedObject(diff.GetComponent<StageDifficultySelector>());so.FindProperty("selectedFrame").objectReferenceValue=S("Button/Button01_Blue");so.FindProperty("idleFrame").objectReferenceValue=S("Button/Button01_Dark");so.FindProperty("compactLabels").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();
                for(int i=0;i<3;i++){string path="Stage Difficulty Selector/Difficulty "+i;Rect(path,i*.34f,.22f,i*.34f+.32f,1,i*.34f,.22f,i*.34f+.32f,1);Skin(path,i==0?"Frame/CardFrame07_Focus":"Button/Button01_Dark");Rect(path+"/Label",.04f,.10f,.96f,.9f,.04f,.1f,.96f,.9f);Font(path+"/Label",42,26,Color.white);}
                Rect("Stage Difficulty Selector/First Clear Reward",0,-.12f,1,.18f,0,-.12f,1,.18f);Font("Stage Difficulty Selector/First Clear Reward",23,18);
                foreach(var button in diff.GetComponentsInChildren<Button>(true)){var colors=button.colors;colors.disabledColor=new Color(.62f,.70f,.83f,1);button.colors=colors;}
            }
            Rect("Rewards",.52f,.29f,.92f,.425f,.08f,.193f,.92f,.265f);Skin("Rewards","Frame/CardFrame04_White1",Cream);
            Rect("Rewards/Text (TMP)",0,.75f,1,1,0,.75f,1,1);Font("Rewards/Text (TMP)",35,24);
            Rect("Rewards/Main",.12f,.05f,.88f,.74f,.08f,.04f,.92f,.74f);
            var rewards=Find("Rewards/Main");if(rewards!=null){var g=rewards.GetComponent<LayoutGroup>();if(g!=null)g.enabled=false;var im=rewards.GetComponent<Image>();if(im!=null)im.enabled=false;for(int i=0;i<rewards.childCount;i++){var child=rewards.GetChild(i);Rect((RectTransform)child,i*.2f,.02f,i*.2f+.18f,.98f,i*.2f,.02f,i*.2f+.18f,.98f);Image(child,"Button/Button_Square03_Navy");foreach(var icon in child.GetComponentsInChildren<Image>(true))if(icon.transform!=child){StaticBox(icon.rectTransform,.14f,.14f,.86f,.86f);icon.preserveAspect=true;}}}
            foreach(var parent in new[]{"Play Button Parent","Unlock Button Parent"})
            {
                Rect(parent,.565f,.19f,.895f,.282f,.16f,.145f,.84f,.192f);
                var button=Find(parent).GetComponentInChildren<Button>(true);Image(button.transform,"Button/Button02_Yellow");Rect((RectTransform)button.transform,0,0,1,1,0,0,1,1);
                foreach(var t in button.GetComponentsInChildren<TMP_Text>(true)){Font(t,80,42,new Color(.25f,.10f,0));StaticBox(t.rectTransform,.05f,.08f,.95f,.92f);}
                var line=button.transform.Find("Vertical Line");if(line!=null)line.gameObject.SetActive(false);
            }
            Rect("Info Button",.903f,.205f,.93f,.26f,.86f,.15f,.93f,.185f);Rect("Attempts Counter (TMP)",.58f,.15f,.89f,.18f,.20f,.121f,.80f,.142f);Font("Attempts Counter (TMP)",24,18);Hide("Button Shadow");
            string[] quick={"Lucky Wheel","Gem Chest"};
            for(int i=0;i<2;i++)
            {
                string path="Stage Lucky Rewards/"+quick[i]+" Shortcut";
                Rect(path,.12f+i*.147f,.16f,.26f+i*.147f,.315f,.12f+i*.40f,.52f,.48f+i*.40f,.60f);Panel(path);
                var t=Find(path);if(t==null)continue;foreach(var icon in t.GetComponentsInChildren<Image>(true))if(icon.transform!=t&&icon.name.Contains("Icon")){Rect(icon.rectTransform,.15f,.24f,.85f,.94f,.18f,.23f,.82f,.94f);icon.preserveAspect=true;}
                Rect(path+"/Label",.08f,.20f,.92f,.38f,.08f,.20f,.92f,.38f);Font(path+"/Label",27,21);
                var free=Node(t,"Daily Free Label");Image(free,"Button/Button01_Red");Rect(free,.20f,.015f,.80f,.22f,.20f,.015f,.80f,.22f);var freeText=Text(free,"Text","FREE",31,Color.white);StaticBox(freeText.rectTransform,.04f,.05f,.96f,.95f);
                var rewardsSo=new SerializedObject(Find("Stage Lucky Rewards").GetComponent<StageLuckyRewards>());rewardsSo.FindProperty(i==0?"wheelFreeLabel":"chestFreeLabel").objectReferenceValue=free.gameObject;rewardsSo.ApplyModifiedPropertiesWithoutUndo();
                Rect(path+"/Daily Free Reward Dot",.84f,.83f,.97f,.97f,.84f,.81f,.97f,.98f);Skin(path+"/Daily Free Reward Dot","UI_Etc/Alert_Dot_Bg");
            }
            Rect("Language Picker/Choose Language",.02f,.80f,.15f,.84f,.66f,.91f,.88f,.953f);Skin("Language Picker/Choose Language","Button/Button01_Dark");
            var language=Find("Language Picker/Choose Language");if(language!=null)foreach(var t in language.GetComponentsInChildren<TMP_Text>(true))Font(t,27,23,Color.white);
            Rect("Developer Tools/Developer Shortcut",.025f,.14f,.08f,.18f,.02f,.11f,.13f,.14f);Skin("Developer Tools/Developer Shortcut","Button/Button01_Dark");
            var dev=Find("Developer Tools/Developer Shortcut");if(dev!=null)foreach(var text in dev.GetComponentsInChildren<TMP_Text>(true))Font(text,30,23,Color.white);
            Finish();
        }
        static void Armory()
        {
            Header();Hero(true);
            var settings=Node(root,"Settings Shortcut");Image(settings,"Button/Button_Square03_Navy");if(settings.GetComponent<Button>()==null)settings.gameObject.AddComponent<Button>();if(settings.GetComponent<UIButtonAudio>()==null)settings.gameObject.AddComponent<UIButtonAudio>();if(settings.GetComponent<MenuSettingsShortcut>()==null)settings.gameObject.AddComponent<MenuSettingsShortcut>();settings.GetComponent<Image>().raycastTarget=true;settings.GetComponent<Button>().targetGraphic=settings.GetComponent<Image>();Rect(settings,.944f,.91f,.985f,.985f,.905f,.935f,.98f,.975f);var gear=Node(settings,"Gear");Icon(gear,"Icon_ItemIcons/Original/Icon_GearWheels");StaticBox(gear,.18f,.18f,.82f,.82f);
            Rect("Info Panel",.475f,.607f,.975f,.91f,.035f,.395f,.965f,.555f);Panel("Info Panel");Hide("Info Panel/Background");
            var banner=Node(Find("Info Panel"),"Heading Banner");Image(banner,"Frame/BannerFrame03_Single_Navy");StaticBox(banner,.24f,.79f,.72f,1.02f);banner.SetAsFirstSibling();
            Rect("Info Panel/Text (TMP)",.28f,.83f,.68f,.99f,.28f,.83f,.68f,.99f);Font("Info Panel/Text (TMP)",62,34,Color.white);
            Rect("Info Panel/Details",.77f,.82f,.95f,.97f,.77f,.82f,.95f,.97f);Skin("Info Panel/Details","Button/Button01_Blue");Font("Info Panel/Details/Label",31,22,Color.white);
            Rect("Info Panel/Main",.05f,.10f,.95f,.76f,.05f,.09f,.95f,.77f);var main=Find("Info Panel/Main");var grid=main.GetComponent<GridLayoutGroup>();if(grid!=null)grid.enabled=false;
            var mi=main.GetComponent<Image>();if(mi!=null)mi.enabled=false;
            string[] cards={"Bot","Bot (2)","Bot (1)","Bot (3)"};string[] icons={"Sword","Heart","Star","Boots"};
            for(int i=0;i<4;i++)
            {
                var path="Info Panel/Main/"+cards[i];float x=i%2*.51f,y=i<2?.53f:0;
                Rect(path,x,y,x+.49f,y+.47f,x,y,x+.49f,y+.47f);Skin(path,"Frame/CardFrame04_White1",new Color(1,.985f,.94f));
                var icon=Find(path+"/Image");Icon(icon,"Icon_ItemIcons/Original/Icon_"+icons[i]);Rect((RectTransform)icon,.04f,.14f,.26f,.87f,.04f,.14f,.26f,.87f);
                Rect(path+"/Text (TMP) (1)",.32f,.56f,.96f,.95f,.30f,.56f,.97f,.95f);Font(path+"/Text (TMP) (1)",36,25);Find(path+"/Text (TMP) (1)").GetComponent<TMP_Text>().alignment=TextAlignmentOptions.Left;
                Rect(path+"/Text (TMP) (2)",.32f,.08f,.65f,.60f,.30f,.08f,.66f,.60f);Font(path+"/Text (TMP) (2)",65,39);
                Rect(path+"/Text (TMP) (3)",.65f,.12f,.98f,.55f,.66f,.12f,.99f,.55f);Font(path+"/Text (TMP) (3)",43,28,new Color(.02f,.61f,.08f));
            }
            Rect("Armory Panel",.475f,.14f,.975f,.598f,.035f,.11f,.965f,.395f);Panel("Armory Panel");Hide("Armory Panel/Background Image");
            banner=Node(Find("Armory Panel"),"Heading Banner");Image(banner,"Frame/BannerFrame03_Single_Navy");StaticBox(banner,.06f,.855f,.45f,.995f);banner.SetAsFirstSibling();
            Rect("Armory Panel/Title",.08f,.87f,.43f,.98f,.08f,.87f,.43f,.98f);Font("Armory Panel/Title",56,34,Color.white);
            var capacityBacking=Node(Find("Armory Panel"),"Capacity Backdrop");Image(capacityBacking,"Button/Button01_Dark");Rect(capacityBacking,.51f,.87f,.68f,.98f,.49f,.87f,.67f,.98f);capacityBacking.SetSiblingIndex(Find("Armory Panel/Capacity").GetSiblingIndex());
            Rect("Armory Panel/Capacity",.51f,.87f,.68f,.98f,.49f,.87f,.67f,.98f);Font("Armory Panel/Capacity",40,27,Color.white);
            Rect("Armory Panel/Expand Bag",.68f,.87f,.735f,.98f,.68f,.87f,.75f,.98f);Skin("Armory Panel/Expand Bag","Button/Button_Square05_Blue");Font("Armory Panel/Expand Bag/Label",45,32,Color.white);
            Rect("Armory Panel/Filter",.76f,.87f,.94f,.98f,.77f,.87f,.94f,.98f);Skin("Armory Panel/Filter","Button/Button01_Dark");Font("Armory Panel/Filter/Label",36,24,Color.white);
            Rect("Armory Panel/Inventory Viewport",.045f,.065f,.95f,.84f,.045f,.055f,.95f,.84f);
            var scroll=Find("Armory Panel/Inventory Viewport").GetComponent<ScrollRect>();scroll.scrollSensitivity=160;scroll.movementType=ScrollRect.MovementType.Clamped;
            var sb=Node(Find("Armory Panel"),"Inventory Scrollbar");Image(sb,"Button/Button01_Dark");StaticBox(sb,.959f,.09f,.969f,.81f);
            var handle=Node(sb,"Handle");Image(handle,"Button/Button01_Blue");StaticBox(handle,0,0,1,1);
            var scrollbar=sb.GetComponent<Scrollbar>();if(scrollbar==null)scrollbar=sb.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=handle.GetComponent<Image>();scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;scroll.verticalNormalizedPosition=1;sb.GetComponent<Image>().raycastTarget=true;handle.GetComponent<Image>().raycastTarget=true;
            var cells=Find("Armory Panel/Inventory Viewport/Content");foreach(var label in cells.GetComponentsInChildren<TMP_Text>(true))Font(label,37,28,Color.white);
            foreach(var b in cells.GetComponentsInChildren<Button>(true)){Image(b.transform,"Frame/ItemFrame01_f");var inset=Node(b.transform,"Navy Inset");Image(inset,"Frame/CardFrame04_White1",Navy);StaticBox(inset,.08f,.08f,.92f,.92f);inset.SetAsFirstSibling();}
            Rect("Hero Selector",.055f,.14f,.44f,.325f,.12f,.56f,.88f,.62f);Panel("Hero Selector");
            banner=Node(Find("Hero Selector"),"Heading Banner");Image(banner,"Frame/BannerFrame03_Single_Navy");StaticBox(banner,.26f,.81f,.74f,1.03f);banner.SetAsFirstSibling();
            Rect("Hero Selector/Title",.28f,.84f,.72f,.99f,.28f,.84f,.72f,.99f);Font("Hero Selector/Title",40,25,Color.white);
            Rect("Hero Selector/Hero Roster Buttons",.08f,.05f,.92f,.80f,.06f,.05f,.94f,.79f);
            var roster=Find("Hero Selector/Hero Roster Buttons");var row=roster.GetComponent<HorizontalLayoutGroup>();row.spacing=55;row.padding=new RectOffset(35,35,0,0);row.childForceExpandWidth=true;row.childForceExpandHeight=true;
            foreach(Transform hero in roster)
            {
                Image(hero,"Frame/ItemFrame01_f");var portraitSource=hero.GetComponentsInChildren<Image>(true).FirstOrDefault(v=>v.name=="Portrait");
                if(portraitSource!=null)
                {
                    var mask=Node(hero,"Portrait Mask");Image(mask,"Frame/CardFrame04_White1",Navy);StaticBox(mask,.10f,.15f,.90f,.90f);
                    if(mask.GetComponent<RectMask2D>()==null)mask.gameObject.AddComponent<RectMask2D>();
                    var face=Node(mask,"Face");var faceImage=face.GetComponent<Image>();if(faceImage==null)faceImage=face.gameObject.AddComponent<Image>();
                    faceImage.sprite=portraitSource.sprite;faceImage.preserveAspect=true;faceImage.raycastTarget=false;portraitSource.gameObject.SetActive(false);
                    if(hero.name=="Hero Ranger")StaticBox(face,-.8f,-2.45f,1.8f,1.95f);else StaticBox(face,-.1f,-.45f,1.1f,1.37f);
                    mask.SetAsFirstSibling();
                }
                var label=hero.Find("Label").GetComponent<TMP_Text>();label.text=hero.name.Replace("Hero ","");StaticBox(label.rectTransform,.04f,.01f,.96f,.23f);Font(label,34,24,Color.white);label.transform.SetAsLastSibling();
                var locked=Node(hero,"Locked Badge");Icon(locked,"Icon_ItemIcons/Original/Icon_Lock");StaticBox(locked,.73f,.20f,.98f,.47f);
                var selected=Node(hero,"Selected Badge");Image(selected,"Button/Button_Circle147_Navy");StaticBox(selected,.76f,.70f,1.02f,.99f);var tick=Node(selected,"Check");var check=tick.GetComponent<Image>();if(check==null)check=tick.gameObject.AddComponent<Image>();check.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Armory/ui_icon_checkmark.png");check.preserveAspect=true;check.raycastTarget=false;StaticBox(tick,.19f,.19f,.81f,.81f);var oldTick=selected.Find("Tick");if(oldTick!=null)oldTick.gameObject.SetActive(false);selected.gameObject.SetActive(hero.name=="Hero Ranger");
            }
            Find("Armory Panel/Capacity").SetAsLastSibling();Finish();
        }
        static void ConfigureInventoryArt()
        {
            var art=Resources.Load<ArmoryDashboardDatabase>(ArmoryDashboardDatabase.ResourcePath);
            art.equipmentFrame=S("Frame/ItemFrame01_f");art.stackFrame=S("Button/Button_Square03_Navy");art.emptyFrame=S("Frame/ItemFrame02_White1");art.emptyPlus=S("UI_Etc/ResourceBar_Btn_Icon_Add");EditorUtility.SetDirty(art);
            art.rarityFrames=new[]{art.equipmentFrame,S("Frame/ItemFrame01_Single_Green"),S("Frame/ItemFrame01_Single_Blue"),S("Frame/ItemFrame01_Single_Purple"),S("Frame/ItemFrame01_Single_Yellow"),S("Frame/ItemFrame01_Single_Red")};
        }
        static void ItemCard()
        {
            var so=new SerializedObject(root.GetComponent<ItemCardBehavior>());so.FindProperty("levelPrefix").stringValue="Lv.";
            var frames=so.FindProperty("rarityDataList");var art=Resources.Load<ArmoryDashboardDatabase>(ArmoryDashboardDatabase.ResourcePath);for(int i=0;i<frames.arraySize;i++)frames.GetArrayElementAtIndex(i).FindPropertyRelative("backgroundSprite").objectReferenceValue=art.rarityFrames[Mathf.Clamp(frames.GetArrayElementAtIndex(i).FindPropertyRelative("rarityType").intValue,0,art.rarityFrames.Length-1)];
            var rarity=(Image)so.FindProperty("rarityImage").objectReferenceValue;rarity.type=UnityEngine.UI.Image.Type.Sliced;rarity.pixelsPerUnitMultiplier=5;rarity.color=Color.white;rarity.material=null;
            var inset=Node(rarity.transform,"Rep Card Inset");var insetImage=inset.GetComponent<Image>();if(insetImage==null)insetImage=inset.gameObject.AddComponent<Image>();insetImage.sprite=S("Frame/CardFrame04_White1");insetImage.type=UnityEngine.UI.Image.Type.Sliced;insetImage.pixelsPerUnitMultiplier=5;insetImage.color=Navy;insetImage.raycastTarget=false;StaticBox(inset,.075f,.075f,.925f,.925f);
            var background=(Image)so.FindProperty("rarityBackgroundImage").objectReferenceValue;background.sprite=S("Button/Button_Square03_Navy");background.type=UnityEngine.UI.Image.Type.Sliced;background.pixelsPerUnitMultiplier=5;
            var icon=(Image)so.FindProperty("iconImage").objectReferenceValue;StaticBox(icon.rectTransform,.12f,.17f,.88f,.91f);icon.preserveAspect=true;
            var level=(TMP_Text)so.FindProperty("levelText").objectReferenceValue;var levelParent=level.transform.parent;
            StaticBox((RectTransform)levelParent,.25f,.045f,.91f,.245f);var badge=levelParent.GetComponent<Image>();if(badge!=null){badge.sprite=S("Button/Button01_Dark");badge.type=UnityEngine.UI.Image.Type.Sliced;badge.pixelsPerUnitMultiplier=5;badge.color=Color.white;}
            StaticBox(level.rectTransform,.04f,0,.96f,1);level.fontSize=level.fontSizeMax=42;level.fontSizeMin=24;level.enableAutoSizing=true;level.color=Color.white;level.alignment=TextAlignmentOptions.Center;
            var type=(Image)so.FindProperty("itemTypeBackgroundImage").objectReferenceValue;type.gameObject.SetActive(false);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Dock()
        {
            if(layout==null)layout=root.gameObject.AddComponent<ResponsiveMenuLayout>();
            var grid=root.GetComponent<GridLayoutGroup>();if(grid!=null)Object.DestroyImmediate(grid);layout.grids.Clear();
            Rect((RectTransform)root,.005f,.005f,.995f,.13f,.015f,.012f,.985f,.095f);Image(root,"Button/Button01_Dark");
            var tabs=root.GetComponentsInChildren<MainMenuDockItemBehavior>(true).OrderBy(v=>(int)v.PageType).ToArray();
            // Preserve the navigation order already authored by the six dock slots.
            tabs=root.GetComponentsInChildren<MainMenuDockItemBehavior>(true).OrderBy(v=>v.transform.GetSiblingIndex()).ToArray();
            string[] icons={"Map","Shield",null,"Hammer","Quest","Trophy"};
            for(int i=0;i<tabs.Length;i++)
            {
                var tab=tabs[i];Rect((RectTransform)tab.transform,.035f+i*.157f,.05f,.185f+i*.157f,.95f,.01f+i*.165f,.04f,.17f+i*.165f,.96f);
                var so=new SerializedObject(tab);so.FindProperty("inactiveLabelAlpha").floatValue=1;
                var backdrop=Node(tab.transform,"Selected Backdrop");Image(backdrop,"Button/Menu_TopBtn_Focus");StaticBox(backdrop,.02f,.01f,.98f,.99f);backdrop.SetAsFirstSibling();backdrop.GetComponent<Image>().enabled=i==0;
                so.FindProperty("selectedBackdrop").objectReferenceValue=backdrop.GetComponent<Image>();
                so.FindProperty("selectedIconSize").vector2Value=new Vector2(145,145);so.FindProperty("unselectedIconSize").vector2Value=new Vector2(140,140);
                so.FindProperty("selectedIconPosition").vector2Value=new Vector2(0,35);so.FindProperty("unselectedIconPosition").vector2Value=new Vector2(0,35);
                var icon=(Image)so.FindProperty("iconImage").objectReferenceValue;if(i<icons.Length&&icons[i]!=null)Icon(icon.transform,"Icon_ItemIcons/Original/Icon_"+icons[i]);
                var text=(TMP_Text)so.FindProperty("pageNameText").objectReferenceValue;Font(text,43,23,new Color(.78f,.92f,1));Rect(text.rectTransform,0,.01f,1,.30f,0,.01f,1,.30f);
                Point(Relative(icon.transform),.5f,.5f,0,35,140,140,1,.5f,.5f,.57f);
                so.ApplyModifiedPropertiesWithoutUndo();
                // The whole authored tile receives pointer input, independently of decorative icons.
                var hit=tab.GetComponent<Image>();if(hit==null)hit=tab.gameObject.AddComponent<Image>();
                hit.sprite=null;hit.color=Color.clear;hit.raycastTarget=true;
                var button=(Button)so.FindProperty("button").objectReferenceValue;button.targetGraphic=hit;
                if(i>0){var divider=Node(tab.transform,"Divider");var di=divider.GetComponent<Image>();if(di==null)di=divider.gameObject.AddComponent<Image>();di.sprite=null;di.color=new Color(.15f,.43f,.69f);di.raycastTarget=false;StaticBox(divider,-.025f,.20f,-.021f,.80f);}
            }
            foreach(var im in root.GetComponentsInChildren<Image>(true))if(im.name.IndexOf("Reward Dot",StringComparison.OrdinalIgnoreCase)>=0) {im.sprite=S("UI_Etc/Alert_Dot_Bg");im.color=Color.white;im.preserveAspect=true;}
            Finish();
        }
        static string Relative(Transform t) {return AnimationUtility.CalculateTransformPath(t,root);}
    }
}
