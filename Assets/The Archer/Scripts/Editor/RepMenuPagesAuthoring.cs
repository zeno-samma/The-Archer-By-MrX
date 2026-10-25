using System;
using System.Linq;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace MrX.EditorTools
{
    public static partial class RepMenuAuthoring
    {
        [MenuItem("Tools/MrX Studio/UI/Apply Rep Remaining Pages and Repair Dock")]
        public static void InstallRemainingPages()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            foreach(var name in Assets)Import(name);
            Import("Icon_ItemIcons/Original/Icon_Chest");
            ConfigurePageButtons();
            Edit(Root+"Prefabs/UI/Menu Pages/Menu Dock.prefab",Dock);
            var names=new[]{"Upgrades Page","Forge Page","Daily Quests Page","Achievements Page"};
            Action[] actions={RepUpgrades,RepForge,RepQuests,RepAchievements};
            for(int i=0;i<names.Length;i++)Edit(Root+"Prefabs/UI/Menu Pages/"+names[i]+".prefab",actions[i]);
            foreach(var page in Object.FindObjectsByType<MainMenuPageBehavior>(FindObjectsInactive.Include))
            {
                if(page is UpgradeDashboardPage)ApplyScene(page.gameObject,RepUpgrades);
                else if(page is ForgePageBehavior)ApplyScene(page.gameObject,RepForge);
                else if(page is DailyQuestsPageBehavior)ApplyScene(page.gameObject,RepQuests);
                else if(page is AchievementsPageBehavior)ApplyScene(page.gameObject,RepAchievements);
            }
            foreach(var dock in Object.FindObjectsByType<MainMenuDockBehavior>(FindObjectsInactive.Include))ApplyScene(dock.gameObject,Dock);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveOpenScenes();
        }
        static void ConfigurePageButtons()
        {
            var translations=Resources.Load<MrX.Localization.MenuTranslations>("Menu Translations");
            if(!translations.entries.Any(e=>e.english=="DAILY REWARDS")){translations.entries.Add(new MrX.Localization.TranslationEntry{english="DAILY REWARDS",vietnamese="PHẦN THƯỞNG MỖI NGÀY",spanish="RECOMPENSAS DIARIAS",portuguese="RECOMPENSAS DIÁRIAS",french="RÉCOMPENSES DU JOUR"});EditorUtility.SetDirty(translations);}
            foreach(var name in new[]{"Daily Quests Database","Achievements Database","Forge Page Database"})
            {
                var asset=Resources.Load<ScriptableObject>(name);if(asset==null)continue;var so=new SerializedObject(asset);
                foreach(var field in new[]{"claimButtonSprite","forgeButtonSprite"}){var p=so.FindProperty(field);if(p!=null)p.objectReferenceValue=S("Button/Button02_Yellow");}
                foreach(var field in new[]{"blueButtonSprite","disabledButtonSprite"}){var p=so.FindProperty(field);if(p!=null)p.objectReferenceValue=S(field=="blueButtonSprite"?"Button/Button01_Blue":"Button/Button01_Dark");}
                so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);
            }
        }
        static void CommonPage(string title)
        {
            Header();Hide("Page Heading");
            var top=Find("Top Bar");if(top!=null)foreach(var text in top.GetComponentsInChildren<TMP_Text>(true))if(text.transform.parent==top)Hide(Relative(text.transform));
            if(Find("Settings")==null&&Find("Settings Button")==null)
            {
                var settings=Node(root,"Settings");Image(settings,"Button/Button_Square03_Navy");var button=settings.gameObject.AddComponent<Button>();button.targetGraphic=settings.GetComponent<Image>();settings.GetComponent<Image>().raycastTarget=true;
                settings.gameObject.AddComponent<UIButtonAudio>();settings.gameObject.AddComponent<MenuSettingsShortcut>();
                Rect(settings,.944f,.91f,.985f,.985f,.905f,.935f,.98f,.975f);var gear=Node(settings,"Rep Gear");Icon(gear,"Icon_ItemIcons/Original/Icon_GearWheels");StaticBox(gear,.18f,.18f,.82f,.82f);
            }
            var heading=Node(root,"Rep Page Heading");Image(heading,"Frame/BannerFrame03_Single_Navy");Rect(heading,.035f,.91f,.43f,.985f,.035f,.915f,.82f,.98f);
            var label=Text(heading,"Title",title,65,Color.white);StaticBox(label.rectTransform,.06f,.1f,.94f,.9f);
            foreach(var old in root.GetComponentsInChildren<Outline>(true))old.enabled=false;
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))if(text.overflowMode==TextOverflowModes.Ellipsis)text.overflowMode=TextOverflowModes.Truncate;
            foreach(var b in root.GetComponentsInChildren<Button>(true))
            {
                if(b.targetGraphic==null&&b.GetComponent<Image>()!=null)b.targetGraphic=b.GetComponent<Image>();
                if(b.targetGraphic!=null)b.targetGraphic.raycastTarget=true;
                var colors=b.colors;colors.disabledColor=new Color(.65f,.70f,.8f,1);b.colors=colors;
            }
        }
        static void Heading(Transform owner,RectTransform label)
        {
            var banner=Node(owner,"Rep Heading Banner");Image(banner,"Frame/BannerFrame03_Single_Navy");StaticBox(banner,.18f,.79f,.82f,1.02f);banner.SetAsFirstSibling();
            Rect(label,.21f,.82f,.79f,.98f,.21f,.82f,.79f,.98f);Font(label.GetComponent<TMP_Text>(),49,32,Color.white);
        }
        static void RowHeight(RectTransform rect,float landscape,float portrait)
        {
            var element=rect.GetComponent<LayoutElement>();if(element==null)element=rect.gameObject.AddComponent<LayoutElement>();element.minHeight=0;element.flexibleHeight=0;
            var style=layout.rows.Find(s=>s.target==element);if(style==null){style=new ResponsiveMenuLayout.RowStyle{target=element};layout.rows.Add(style);}style.landscape=landscape;style.portrait=portrait;element.preferredHeight=landscape;
        }
        static void Scroll(string path)
        {
            var t=Find(path);if(t==null)return;var scroll=t.GetComponent<ScrollRect>();if(scroll==null)return;
            scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=150;scroll.movementType=ScrollRect.MovementType.Clamped;
            if(scroll.viewport!=null){var im=scroll.viewport.GetComponent<Image>();if(im!=null){im.color=Color.clear;im.raycastTarget=true;}}
            if(scroll.content!=null){var row=scroll.content.GetComponent<VerticalLayoutGroup>();if(row!=null){row.spacing=20;row.padding=new RectOffset(12,12,10,10);row.childControlHeight=true;row.childForceExpandHeight=false;}}
            var bar=Node(t,"Rep Scrollbar");Image(bar,"Button/Button01_Dark");StaticBox(bar,.985f,.035f,.997f,.965f);bar.GetComponent<Image>().raycastTarget=true;
            var handle=Node(bar,"Handle");Image(handle,"Button/Button01_Blue");StaticBox(handle,0,0,1,1);handle.GetComponent<Image>().raycastTarget=true;
            var scrollbar=bar.GetComponent<Scrollbar>();if(scrollbar==null)scrollbar=bar.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=handle.GetComponent<Image>();scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;scroll.verticalNormalizedPosition=1;
            if(scroll.viewport!=null)Rect(scroll.viewport,0,0,.975f,1,0,0,.975f,1);
        }
        static void Wrap(TMP_Text text) {text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Truncate;}
        static void RepUpgrades()
        {
            CommonPage("UPGRADES");
            Rect("Main Frame",.035f,.15f,.965f,.89f,.035f,.11f,.965f,.825f);Panel("Main Frame");
            Rect("Main Frame/Selected Upgrade",.018f,.34f,.313f,.975f,.025f,.66f,.975f,.98f);Panel("Main Frame/Selected Upgrade");
            Rect("Main Frame/Player Stats Preview",.018f,.025f,.313f,.325f,.025f,.445f,.975f,.645f);Panel("Main Frame/Player Stats Preview");
            Rect("Main Frame/Upgrade List",.33f,.025f,.98f,.975f,.025f,.015f,.975f,.43f);Skin("Main Frame/Upgrade List","Frame/CardFrame04_White1",Cream);
            var page=root.GetComponent<UpgradeDashboardPage>();var so=new SerializedObject(page);
            so.FindProperty("blueButton").objectReferenceValue=S("Button/Button01_Blue");so.FindProperty("yellowButton").objectReferenceValue=S("Button/Button02_Yellow");so.FindProperty("grayButton").objectReferenceValue=S("Button/Button01_Dark");
            var icon=(Image)so.FindProperty("selectedIcon").objectReferenceValue;icon.preserveAspect=true;
            Rect(icon.rectTransform,.27f,.62f,.73f,.94f,.025f,.35f,.28f,.94f);
            PlaceText(so,"selectedTitle",.05f,.49f,.95f,.61f,.31f,.73f,.97f,.96f,59,36);
            PlaceText(so,"selectedDescription",.08f,.31f,.92f,.48f,.31f,.36f,.97f,.72f,38,27);Wrap((TMP_Text)so.FindProperty("selectedDescription").objectReferenceValue);
            PlaceText(so,"selectedLevel",.10f,.245f,.90f,.31f,.035f,.22f,.48f,.35f,41,28);
            PlaceText(so,"selectedEffect",.10f,.18f,.90f,.245f,.035f,.04f,.48f,.20f,43,29);
            var upgrade=(Button)so.FindProperty("upgradeButton").objectReferenceValue;Image(upgrade.transform,"Button/Button02_Yellow");Rect((RectTransform)upgrade.transform,.06f,.035f,.94f,.17f,.53f,.045f,.96f,.31f);Font((TMP_Text)so.FindProperty("selectedCost").objectReferenceValue,47,31,Color.white);
            var stats=Find("Main Frame/Player Stats Preview");Heading(stats,(RectTransform)stats.Find("Heading"));
            string[] names={"ATK","HP","CRIT","SPEED","ASPD"};
            for(int i=0;i<5;i++)
            {
                float y=.69f-i*.135f;var prefix="Main Frame/Player Stats Preview/"+names[i];
                Rect(prefix+" Label",.07f,y,.30f,y+.13f,.05f,y,.30f,y+.13f);Font(prefix+" Label",33,26);
                Rect(prefix+" Current",.30f,y,.51f,y+.13f,.30f,y,.50f,y+.13f);Font(prefix+" Current",37,28);
                Rect(prefix+" Preview",.51f,y,.96f,y+.13f,.5f,y,.97f,y+.13f);Font(prefix+" Preview",35,26,new Color(.02f,.55f,.16f));
            }
            var rows=so.FindProperty("rows");
            for(int i=0;i<rows.arraySize;i++)
            {
                var r=rows.GetArrayElementAtIndex(i);var button=(Button)r.FindPropertyRelative("select").objectReferenceValue;var rt=(RectTransform)button.transform;Image(rt,"Frame/CardFrame04_White1");RowHeight(rt,230,255);
                var im=(Image)r.FindPropertyRelative("icon").objectReferenceValue;im.preserveAspect=true;Rect(im.rectTransform,.018f,.15f,.15f,.9f,.02f,.38f,.20f,.93f);
                PlaceText(r,"title",.175f,.64f,.53f,.95f,.23f,.67f,.98f,.96f,46,32,true);
                PlaceText(r,"description",.175f,.35f,.53f,.65f,.23f,.40f,.98f,.69f,33,25,true);Wrap((TMP_Text)r.FindPropertyRelative("description").objectReferenceValue);
                PlaceText(r,"level",.175f,.13f,.35f,.36f,.23f,.24f,.55f,.4f,32,26,true);
                PlaceText(r,"values",.55f,.20f,.795f,.80f,.03f,.025f,.63f,.23f,42,30,true);
                var buy=(Button)r.FindPropertyRelative("buy").objectReferenceValue;Image(buy.transform,"Button/Button01_Blue");Rect((RectTransform)buy.transform,.81f,.19f,.98f,.81f,.68f,.025f,.97f,.30f);Font((TMP_Text)r.FindPropertyRelative("price").objectReferenceValue,48,32,Color.white);
                var seg=r.FindPropertyRelative("segments");for(int j=0;j<seg.arraySize;j++){var s=(Image)seg.GetArrayElementAtIndex(j).objectReferenceValue;Image(s.transform,"Frame/CardFrame04_White1");Rect(s.rectTransform,.35f+j*.038f,.14f,.382f+j*.038f,.23f,.24f+j*.047f,.23f,.279f+j*.047f,.29f);}
            }
            so.ApplyModifiedPropertiesWithoutUndo();Scroll("Main Frame/Upgrade List");Rect("Feedback",.04f,.132f,.96f,.151f,.04f,.095f,.96f,.11f);Font("Feedback",27,19);
            Finish();
        }
        static void PlaceText(SerializedObject so,string key,float a,float b,float c,float d,float pa,float pb,float pc,float pd,float size,float ps)
        {var t=(TMP_Text)so.FindProperty(key).objectReferenceValue;Rect(t.rectTransform,a,b,c,d,pa,pb,pc,pd);Font(t,size,ps);}
        static void PlaceText(SerializedProperty so,string key,float a,float b,float c,float d,float pa,float pb,float pc,float pd,float size,float ps,bool relative)
        {var t=(TMP_Text)so.FindPropertyRelative(key).objectReferenceValue;Rect(t.rectTransform,a,b,c,d,pa,pb,pc,pd);Font(t,size,ps);}

        static void RepForge()
        {
            CommonPage("FORGE");Hide("Anvil Illustration");Hide("Forge Aura");
            var showcase=Node(root,"Rep Forge Showcase");Rect(showcase,.035f,.35f,.48f,.89f,.035f,.61f,.965f,.825f);Panel(Relative(showcase));showcase.SetAsFirstSibling();
            var heading=Node(showcase,"Rep Heading");Image(heading,"Frame/BannerFrame03_Single_Navy");StaticBox(heading,.13f,.86f,.87f,1.02f);var ht=Text(heading,"Title","EQUIPMENT RECIPES",49,Color.white);StaticBox(ht.rectTransform,.05f,.1f,.95f,.9f);
            Point("Equipment Showcase",.255f,.65f,0,0,630,630,1,.5f,.725f,.43f);Skin("Equipment Showcase","Frame/ItemFrame01_f");
            var inset=Node(Find("Equipment Showcase"),"Rep Navy Inset");Image(inset,"Frame/CardFrame04_White1",Navy);StaticBox(inset,.08f,.08f,.92f,.92f);inset.SetAsFirstSibling();
            Rect("Equipment Showcase/Equipment Icon",.12f,.13f,.88f,.92f,.12f,.13f,.88f,.92f);Find("Equipment Showcase/Equipment Icon").GetComponent<Image>().preserveAspect=true;
            Skin("Equipment Showcase/Enhancement Badge","Button/Button01_Dark");Font("Equipment Showcase/Enhancement Badge/Enhancement",40,30,Color.white);
            Rect("Equipment Showcase/Enhancement Badge",.62f,.025f,.96f,.22f,.62f,.025f,.96f,.22f);
            Rect("Previous Equipment",.075f,.61f,.12f,.69f,.07f,.685f,.19f,.73f);Rect("Next Equipment",.395f,.61f,.44f,.69f,.81f,.685f,.93f,.73f);
            foreach(var path in new[]{"Previous Equipment","Next Equipment"}){Skin(path,"Button/Button01_Blue");Font(path+"/Label",75,45,Color.white);}
            Rect("Equipment Selection",.09f,.405f,.42f,.455f,.13f,.635f,.87f,.66f);Font("Equipment Selection",38,26);
            Rect("Fragment Progress",.08f,.36f,.43f,.405f,.10f,.611f,.90f,.635f);Font("Fragment Progress",43,29);
            const string materials="Crafting Materials - ui_slot sliced";
            Rect(materials,.035f,.15f,.48f,.335f,.035f,.435f,.965f,.60f);Panel(materials);
            Heading(Find(materials),(RectTransform)Find(materials+"/Cost Heading"));
            var so=new SerializedObject(root.GetComponent<ForgePageBehavior>());var views=so.FindProperty("materialViews");
            for(int i=0;i<views.arraySize;i++)
            {
                var v=views.GetArrayElementAtIndex(i);var icon=(Image)v.FindPropertyRelative("icon").objectReferenceValue;var tile=(RectTransform)icon.transform.parent;
                Rect(tile,.07f+i*.30f,.07f,.33f+i*.30f,.76f,.07f+i*.30f,.07f,.33f+i*.30f,.76f);Image(tile,"Frame/CardFrame04_White1");
                Rect(icon.rectTransform,.20f,.28f,.80f,.94f,.20f,.28f,.80f,.94f);icon.preserveAspect=true;
                var name=(TMP_Text)v.FindPropertyRelative("title").objectReferenceValue;Rect(name.rectTransform,.02f,.17f,.98f,.36f,.02f,.17f,.98f,.36f);Font(name,27,23);
                var count=(TMP_Text)v.FindPropertyRelative("count").objectReferenceValue;Rect(count.rectTransform,.02f,.01f,.98f,.21f,.02f,.01f,.98f,.21f);Font(count,36,29);
            }
            const string detail="Upgrade Details - ui_slot sliced";
            Rect(detail,.50f,.15f,.965f,.89f,.035f,.11f,.965f,.425f);Panel(detail);
            Skin(detail+"/Selected Equipment","Frame/ItemFrame01_f");Rect(detail+"/Selected Equipment",.055f,.75f,.25f,.94f,.035f,.77f,.20f,.96f);
            Rect(detail+"/Item Name",.29f,.83f,.95f,.95f,.24f,.86f,.96f,.97f);Font(detail+"/Item Name",61,37);
            Rect(detail+"/Level Preview",.30f,.75f,.95f,.82f,.24f,.755f,.96f,.85f);Skin(detail+"/Level Preview","Button/Button01_Dark");Font(detail+"/Level Preview/Level Text",43,29,Color.white);
            Rect(detail+"/Stat Comparison",.055f,.435f,.945f,.72f,.04f,.44f,.96f,.74f);Skin(detail+"/Stat Comparison","Frame/CardFrame04_White1");
            foreach(var stat in Find(detail+"/Stat Comparison").GetComponentsInChildren<TMP_Text>(true))Font(stat,40,27,stat.name=="Next"||stat.name=="Arrow"?new Color(.02f,.55f,.16f):Ink);
            Rect(detail+"/Upgrade Result",.055f,.20f,.945f,.42f,.04f,.18f,.96f,.425f);Skin(detail+"/Upgrade Result","Frame/CardFrame04_White1",new Color(.9f,1,.9f));
            Font(detail+"/Upgrade Result/Result Heading",34,23);Font(detail+"/Upgrade Result/Result Summary",45,29,new Color(.02f,.55f,.16f));Font(detail+"/Upgrade Result/Status",34,25);Font(detail+"/Upgrade Result/Feedback",26,20);
            Rect(detail+"/Forge",.055f,.035f,.945f,.18f,.04f,.025f,.96f,.165f);Skin(detail+"/Forge","Button/Button02_Yellow");foreach(var t in Find(detail+"/Forge").GetComponentsInChildren<TMP_Text>(true))Font(t,60,37,Color.white);
            Finish();
        }
        static void RepQuests()
        {
            CommonPage("DAILY QUESTS");const string sheet="Quest Sheet - ui_slot sliced";
            Rect(sheet,.035f,.15f,.965f,.89f,.035f,.11f,.965f,.825f);Panel(sheet);
            Rect(sheet+"/Header",.025f,.80f,.975f,.975f,.025f,.78f,.975f,.98f);
            HeaderBanner(sheet+"/Header","Title",.15f,.40f,.73f,1.02f);
            Find(sheet+"/Header/Title").GetComponent<TMP_Text>().text="DAILY REWARDS";
            Rect(sheet+"/Header/Title",.20f,.52f,.68f,.95f,.23f,.56f,.98f,.96f);Font(sheet+"/Header/Title",65,41,Color.white);
            Rect(sheet+"/Header/Title Icon",.02f,.05f,.14f,.95f,.02f,.46f,.20f,.96f);Skin(sheet+"/Header/Title Icon","Button/Button_Square03_Navy");
            var questIcon=Find(sheet+"/Header/Title Icon/Quest Icon");if(questIcon!=null)Icon(questIcon,"Icon_ItemIcons/Original/Icon_Quest");
            Rect(sheet+"/Header/Subtitle",.17f,.08f,.71f,.42f,.05f,.28f,.95f,.55f);Font(sheet+"/Header/Subtitle",32,25);Wrap(Find(sheet+"/Header/Subtitle").GetComponent<TMP_Text>());
            Rect(sheet+"/Header/Reset Timer",.76f,.24f,.98f,.86f,.15f,.03f,.85f,.28f);Skin(sheet+"/Header/Reset Timer","Button/Button01_Dark");Font(sheet+"/Header/Reset Timer/Countdown",39,27,Color.white);
            Rect(sheet+"/Daily Milestones",.025f,.61f,.975f,.79f,.025f,.61f,.975f,.77f);Skin(sheet+"/Daily Milestones","Frame/CardFrame04_White1");
            Rect(sheet+"/Daily Milestones/Completion Counter",.015f,.11f,.16f,.89f,.015f,.12f,.19f,.88f);Skin(sheet+"/Daily Milestones/Completion Counter","Button/Button01_Dark");Font(sheet+"/Daily Milestones/Completion Counter/Completed Quests",30,22,Color.white);
            Skin(sheet+"/Daily Milestones/Milestone Progress","Slider/Slider_Level01_Bg");Skin(sheet+"/Daily Milestones/Milestone Progress/Fill","Slider/Slider_Level01_Fill_Green");
            Rect(sheet+"/Daily Milestones/Milestone Progress",.18f,.015f,.97f,.065f,.21f,.01f,.97f,.045f);
            var milestones=Find(sheet+"/Daily Milestones").Cast<Transform>().Where(t=>t.GetComponent<Button>()!=null).ToArray();
            for(int i=0;i<milestones.Length;i++)
            {
                var t=milestones[i];float step=.79f/milestones.Length;Rect((RectTransform)t,.19f+i*step,.03f,.19f+(i+1)*step-.02f,.97f,.22f+i*.75f/milestones.Length,.03f,.22f+(i+1)*.75f/milestones.Length-.02f,.97f);
                Image(t,"Frame/CardFrame04_White1");Font(Relative(t)+"/Label",29,21);var art=t.Find("Chest Art");if(art!=null){Rect((RectTransform)art,.13f,.30f,.87f,.97f,.10f,.35f,.9f,.98f);Icon(art,"Icon_ItemIcons/Original/Icon_Chest");}
            }
            Rect(sheet+"/Quest List",.025f,.05f,.975f,.60f,.025f,.055f,.975f,.60f);Scroll(sheet+"/Quest List");
            foreach(Transform row in Find(sheet+"/Quest List/Viewport/Content"))RewardRow(row,"Quest Action");
            Rect(sheet+"/Feedback",.05f,.005f,.95f,.045f,.05f,.005f,.95f,.05f);Font(sheet+"/Feedback",29,21);Finish();
        }
        static void HeaderBanner(string owner,string label,float x,float y,float z,float w)
        {var t=Node(Find(owner),"Rep Title Banner");Image(t,"Frame/BannerFrame03_Single_Navy");Rect(t,x,y,z,w,.21f,owner.Contains("Achievements")?.64f:.53f,.99f,1.01f);t.SetAsFirstSibling();}
        static void RewardRow(Transform row,string action)
        {
            var path=Relative(row);Image(row,"Frame/CardFrame04_White1");RowHeight((RectTransform)row,220,255);
            Rect(path+"/Icon Tile",.02f,.15f,.105f,.88f,.025f,.40f,.19f,.94f);Skin(path+"/Icon Tile","Button/Button_Square03_Navy");
            Rect(path+"/Title",.13f,.57f,.69f,.94f,.22f,.65f,.97f,.95f);Font(path+"/Title",43,31);var title=Find(path+"/Title").GetComponent<TMP_Text>();title.alignment=TextAlignmentOptions.Left;Wrap(title);
            Rect(path+"/Description",.13f,.28f,.69f,.60f,.22f,.34f,.97f,.66f);Font(path+"/Description",31,25);var desc=Find(path+"/Description").GetComponent<TMP_Text>();desc.alignment=TextAlignmentOptions.Left;Wrap(desc);
            Rect(path+"/Progress",.13f,.09f,.56f,.19f,.04f,.12f,.43f,.23f);Skin(path+"/Progress","Slider/Slider_Level01_Bg");Skin(path+"/Progress/Fill","Slider/Slider_Level01_Fill_Green");
            Rect(path+"/Progress Label",.57f,.03f,.69f,.26f,.44f,.05f,.60f,.28f);Font(path+"/Progress Label",30,24);
            Rect(path+"/Reward",.70f,.23f,.80f,.80f,.60f,.045f,.77f,.31f);
            var reward=Find(path+"/Reward");if(reward!=null)foreach(var text in reward.GetComponentsInChildren<TMP_Text>(true))Font(text,35,25);
            Rect(path+"/"+action,.82f,.24f,.98f,.79f,.79f,.04f,.97f,.31f);Skin(path+"/"+action,"Button/Button02_Yellow");
            var button=Find(path+"/"+action);if(button!=null)foreach(var text in button.GetComponentsInChildren<TMP_Text>(true))Font(text,39,26,Color.white);
            var dot=Find(path+"/Claimable Badge");if(dot!=null)Image(dot,"UI_Etc/Alert_Dot_Bg");
        }
        static void RepAchievements()
        {
            CommonPage("ACHIEVEMENTS");const string sheet="Achievements Sheet - ui_slot sliced";
            Rect(sheet,.035f,.15f,.965f,.89f,.035f,.11f,.965f,.825f);Panel(sheet);
            Rect(sheet+"/Header",.025f,.76f,.975f,.975f,.025f,.77f,.975f,.98f);
            HeaderBanner(sheet+"/Header","Title",.16f,.45f,.68f,1.01f);
            Find(sheet+"/Header/Title").GetComponent<TMP_Text>().text="YOUR JOURNEY";
            Icon(Find(sheet+"/Header/Trophy Illustration"),"Icon_ItemIcons/Original/Icon_Trophy");
            Rect(sheet+"/Header/Trophy Illustration",.02f,.08f,.15f,.98f,.02f,.43f,.20f,.98f);
            Rect(sheet+"/Header/Title",.20f,.58f,.64f,.95f,.24f,.66f,.98f,.97f);Font(sheet+"/Header/Title",66,42,Color.white);
            Rect(sheet+"/Header/Subtitle",.18f,.16f,.66f,.48f,.24f,.43f,.98f,.65f);Font(sheet+"/Header/Subtitle",34,25);Wrap(Find(sheet+"/Header/Subtitle").GetComponent<TMP_Text>());
            Rect(sheet+"/Header/Overall Completion",.71f,.08f,.98f,.95f,.03f,.025f,.97f,.40f);Skin(sheet+"/Header/Overall Completion","Frame/CardFrame04_White1");
            foreach(var t in Find(sheet+"/Header/Overall Completion").GetComponentsInChildren<TMP_Text>(true))Font(t,t.name=="Percentage"?66:35,t.name=="Percentage"?39:26);
            Skin(sheet+"/Header/Overall Completion/Overall Progress","Slider/Slider_Level01_Bg");Skin(sheet+"/Header/Overall Completion/Overall Progress/Fill","Slider/Slider_Level01_Fill_Green");
            Rect(sheet+"/Body",.02f,.025f,.98f,.75f,.015f,.025f,.985f,.76f);var bodyImage=Find(sheet+"/Body").GetComponent<Image>();if(bodyImage!=null)bodyImage.enabled=false;
            Rect(sheet+"/Body/Categories",.005f,.05f,.205f,.97f,.015f,.855f,.985f,.99f);
            int i=0;foreach(Transform tab in Find(sheet+"/Body/Categories"))
            {
                Rect((RectTransform)tab,.02f,.77f-i*.25f,.98f,.99f-i*.25f,i*.25f+.005f,.02f,(i+1)*.25f-.005f,.98f);Image(tab,"Frame/CardFrame04_White1");
                var p=Relative(tab);Rect(p+"/Icon Tile",.04f,.40f,.29f,.91f,.04f,.40f,.29f,.91f);
                Rect(p+"/Category Title",.32f,.42f,.97f,.94f,.31f,.42f,.96f,.94f);Font(p+"/Category Title",40,25);
                Rect(p+"/Category Progress",.14f,.07f,.96f,.39f,.12f,.04f,.96f,.40f);Font(p+"/Category Progress",32,22);i++;
            }
            Rect(sheet+"/Body/Achievement List",.225f,.045f,.99f,.98f,.015f,.025f,.985f,.84f);Scroll(sheet+"/Body/Achievement List");
            foreach(Transform row in Find(sheet+"/Body/Achievement List/Viewport/Content"))RewardRow(row,"Claim");
            Rect(sheet+"/Body/Feedback",.23f,.002f,.99f,.04f,.03f,.002f,.97f,.025f);Font(sheet+"/Body/Feedback",27,20);Finish();
        }
    }
}
