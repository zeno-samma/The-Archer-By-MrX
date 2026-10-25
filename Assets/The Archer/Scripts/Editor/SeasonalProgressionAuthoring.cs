using System;
using System.IO;
using System.Linq;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace MrX.EditorTools
{
    public static partial class OfflineProgressionAuthoring
    {
        private const string SeasonalFolder="Assets/The Archer/Prefabs/UI/Run Progression";
        [MenuItem("Tools/MrX Studio/Offline/Install Seasons Hero EXP and Compact Results")]
        public static void InstallSeasonalProgression()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Exit Play first.");
            art=Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);font=Resources.Load<MenuTranslations>("Menu Translations").latinFont;
            Directory.CreateDirectory(SeasonalFolder);AssetDatabase.Refresh();
            var settings=Resources.Load<RunProgressionSettings>(RunProgressionSettings.ResourcePath);
            if(settings==null)
            {
                settings=ScriptableObject.CreateInstance<RunProgressionSettings>();
                settings.seasons=new[]{Season(RunSeason.Spring,"Spring Thunder","Chain lightning damage +{0}%",new Color(.86f,.88f,1)),Season(RunSeason.Summer,"Summer Sun","Burn duration +{0}%",new Color(1,.9f,.77f)),Season(RunSeason.Autumn,"Autumn Mist","Poison weakening effect +{0}%",new Color(.88f,1,.83f)),Season(RunSeason.Winter,"Winter Frost","Freeze duration +{0}%",new Color(.82f,.96f,1))};
                AssetDatabase.CreateAsset(settings,"Assets/The Archer/Resources/Run Progression Settings.asset");
                var stages=AssetDatabase.FindAssets("t:StageData").Select(g=>AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(s=>s.name).ToArray();
                for(int i=0;i<stages.Length;i++){var so=new SerializedObject(stages[i]);so.FindProperty("heroExperienceMultiplier").floatValue=1+i*.25f;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(stages[i]);}
            }
            var abilities=AssetDatabase.FindAssets("t:AbilityData");
            foreach(var guid in abilities)
            {
                var data=AssetDatabase.LoadAssetAtPath<MrX.Abilities.AbilityData>(AssetDatabase.GUIDToAssetPath(guid));if(data==null||data.SeasonAffinity!=RunSeason.None)continue;
                string name=data.AbilityType.ToString();int affinity=name.Contains("Shock")?0:name.Contains("Ignite")||name.Contains("Fire")||name.Contains("Meteor")?1:name.Contains("Poison")?2:name.Contains("Freeze")||name.Contains("Ice")?3:-1;
                if(affinity<0)continue;var so=new SerializedObject(data);so.FindProperty("seasonAffinity").intValue=affinity;so.ApplyModifiedPropertiesWithoutUndo();
            }
            var bar=AssetDatabase.LoadAssetAtPath<GameObject>(SeasonalFolder+"/Hero EXP Bar.prefab")??BuildExperienceBar();
            var reward=AssetDatabase.LoadAssetAtPath<GameObject>(SeasonalFolder+"/Result Reward Tile.prefab")??BuildResultTile();
            var compact=AssetDatabase.LoadAssetAtPath<GameObject>(SeasonalFolder+"/Compact Run Result.prefab")??BuildCompactResult(bar,reward);
            var weather=AssetDatabase.LoadAssetAtPath<GameObject>(SeasonalFolder+"/Run Season HUD.prefab")??BuildSeasonHud();
            foreach(var path in new[]{"Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab"})InstallHeroHeader(path,bar);
            foreach(var path in new[]{"Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab","Assets/The Archer/Prefabs/UI/Game/Stage Failed.prefab"})InstallResult(path,compact);
            var game=EditorSceneManager.OpenScene("Assets/The Archer/Scenes/Game.unity",OpenSceneMode.Additive);
            try{var screen=game.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<GameScreenBehavior>(true)).First();if(screen.GetComponentInChildren<RunSeasonView>(true)==null)PrefabUtility.InstantiatePrefab(weather,screen.transform);EditorSceneManager.SaveScene(game);}
            finally{EditorSceneManager.CloseScene(game,true);}
            // Existing scene page overrides may predate their prefab headers.
            foreach(var identity in Object.FindObjectsByType<StageHeroIdentity>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {var label=(TMP_Text)new SerializedObject(identity).FindProperty("heroLabel").objectReferenceValue;if(label!=null)AttachHeader(label,bar);}
            foreach(var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="Hero Name and Level").ToArray())AttachHeader(label,bar);
            var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(current.name=="Main Menu"&&current.isDirty)EditorSceneManager.SaveScene(current);
            SeasonalTranslations();AssetDatabase.SaveAssets();Selection.activeObject=settings;
        }
        private static SeasonDefinition Season(RunSeason id,string title,string description,Color tint)
        {
            string key=id==RunSeason.Spring?"Elemental_Shock":id==RunSeason.Summer?"Elemental_Ignite":id==RunSeason.Autumn?"Elemental_Poison":"Elemental_Freeze";
            Sprite icon=null;
            foreach(var guid in AssetDatabase.FindAssets("t:AbilityData")){var a=AssetDatabase.LoadAssetAtPath<MrX.Abilities.AbilityData>(AssetDatabase.GUIDToAssetPath(guid));if(a!=null&&a.AbilityType.ToString()==key){icon=a.Icon;break;}}
            return new SeasonDefinition{season=id,title=title,description=description,color=tint,icon=icon};
        }
        private static GameObject BuildExperienceBar()
        {
            var root=Rect(null,"Hero EXP Bar",0,0,1,1);Skin(root.gameObject).color=new Color(.83f,.93f,1);var view=root.gameObject.AddComponent<HeroExperienceBar>();
            var icon=Rect(root,"Hero Portrait",.025f,.1f,.19f,.9f).gameObject.AddComponent<Image>();icon.preserveAspect=true;
            var name=Text(root,"Hero Name and Level","Hero · Lv.1",25,.22f,.48f,.96f,.96f);
            var track=Rect(root,"EXP Track",.22f,.12f,.96f,.45f);Skin(track.gameObject).color=new Color(.23f,.32f,.5f);
            var fill=Rect(track,"EXP Fill",0,0,1,1).gameObject.AddComponent<Image>();fill.sprite=art.slotSprite;fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.color=new Color(.4f,.9f,.16f);fill.raycastTarget=false;
            var amount=Text(track,"EXP Amount","0 / 250 EXP",17,.02f,0,.98f,1);amount.color=Color.white;
            Set(view,"heroName",name);Set(view,"experience",amount);Set(view,"portrait",icon);Set(view,"fill",fill);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,SeasonalFolder+"/Hero EXP Bar.prefab");Object.DestroyImmediate(root.gameObject);return prefab;
        }
        private static void AttachHeader(TMP_Text label,GameObject prefab)
        {
            if(label.GetComponentInParent<HeroExperienceBar>()!=null)return;
            if(label.GetComponentInChildren<HeroExperienceBar>(true)!=null)return;
            label.enabled=false;var header=(GameObject)PrefabUtility.InstantiatePrefab(prefab,label.transform);var rect=(RectTransform)header.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(0,-18);rect.offsetMax=new Vector2(0,8);
            EditorUtility.SetDirty(label);
        }
        private static void InstallHeroHeader(string path,GameObject bar)
        {
            var root=PrefabUtility.LoadPrefabContents(path);try{
            var identity=root.GetComponentInChildren<StageHeroIdentity>(true);if(identity!=null){var label=(TMP_Text)new SerializedObject(identity).FindProperty("heroLabel").objectReferenceValue;if(label!=null)AttachHeader(label,bar);}
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="Hero Name and Level").ToArray())AttachHeader(text,bar);
            PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        private static GameObject BuildResultTile()
        {
            var root=Rect(null,"Result Reward Tile",0,0,1,1);Skin(root.gameObject);var view=root.gameObject.AddComponent<ResultRewardView>();
            var icon=Rect(root,"Icon",.14f,.37f,.86f,.95f).gameObject.AddComponent<Image>();icon.preserveAspect=true;icon.raycastTarget=false;
            Set(view,"icon",icon);Set(view,"title",Text(root,"Name","Reward",28,.02f,.2f,.98f,.38f));Set(view,"amount",Text(root,"Amount","× 1",36,.02f,.01f,.98f,.22f));
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,SeasonalFolder+"/Result Reward Tile.prefab");Object.DestroyImmediate(root.gameObject);return prefab;
        }
        private static GameObject BuildCompactResult(GameObject bar,GameObject reward)
        {
            var root=Rect(null,"Compact Run Result",0,0,1,1);var shade=Skin(root.gameObject);shade.color=new Color(.02f,.06f,.16f,.8f);shade.raycastTarget=true;
            var view=root.gameObject.AddComponent<CompactRunResult>();var layout=root.gameObject.AddComponent<ResponsiveMenuLayout>();Set(view,"layout",layout);
            var panel=Rect(root,"Result Panel",.27f,.035f,.73f,.965f);Skin(panel.gameObject);
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=panel,landscape=ResponsiveMenuLayout.Placement.Capture(panel),portrait=Box(.055f,.035f,.945f,.965f)});
            var ribbon=Rect(panel,"Outcome Ribbon",.015f,.89f,.985f,.99f);Skin(ribbon.gameObject).color=new Color(.25f,.55f,1);
            var heading=Text(ribbon,"Outcome","Victory",64,.03f,.05f,.97f,.95f);heading.color=Color.white;heading.fontStyle=FontStyles.Bold;Set(view,"title",heading);
            var timeCard=Rect(panel,"Time Record",.045f,.65f,.955f,.88f);Skin(timeCard.gameObject).color=new Color(.82f,.92f,1);
            var timer=Text(timeCard,"Run Time","00:00.00",140,.04f,.36f,.96f,.98f);var effect=timer.gameObject.AddComponent<UITextEffect>();var so=new SerializedObject(effect);so.FindProperty("effect").enumValueIndex=0;so.FindProperty("numberDisplay").enumValueIndex=1;so.FindProperty("playOnEnable").boolValue=false;so.FindProperty("duration").floatValue=2.4f;so.ApplyModifiedPropertiesWithoutUndo();Set(view,"timeEffect",effect);
            Set(view,"stage",Text(timeCard,"Stage","Stage",27,.04f,.17f,.96f,.36f));var record=Text(timeCard,"Record","Personal best",25,.03f,.005f,.97f,.18f);record.color=new Color(.15f,.53f,.1f);Set(view,"record",record);
            var recordFx=record.gameObject.AddComponent<UITextEffect>();var recordData=new SerializedObject(recordFx);recordData.FindProperty("effect").enumValueIndex=(int)UITextEffectStyle.Pop;recordData.FindProperty("playOnEnable").boolValue=false;recordData.ApplyModifiedPropertiesWithoutUndo();
            Set(view,"kills",Text(panel,"Kills","Enemies / Bosses defeated",30,.045f,.59f,.955f,.645f));
            var dps=Text(panel,"Highest DPS","Highest DPS",40,.045f,.49f,.955f,.585f);dps.fontStyle=FontStyles.Bold;Set(view,"dps",dps);
            var dpsFx=dps.gameObject.AddComponent<UITextEffect>();var dpsData=new SerializedObject(dpsFx);dpsData.FindProperty("effect").enumValueIndex=0;dpsData.FindProperty("playOnEnable").boolValue=false;dpsData.FindProperty("duration").floatValue=2.4f;dpsData.ApplyModifiedPropertiesWithoutUndo();Set(view,"dpsEffect",dpsFx);
            Set(view,"season",Text(panel,"Season","Season",24,.045f,.445f,.955f,.49f));
            var row=Rect(panel,"Rewards Scroll",.045f,.215f,.955f,.44f);Skin(row.gameObject).raycastTarget=true;var scroll=row.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=50;
            var viewport=Rect(row,"Viewport",.025f,.035f,.975f,.965f);viewport.gameObject.AddComponent<RectMask2D>();var content=Rect(viewport,"Reward Tiles",0,1,1,1);content.pivot=new Vector2(.5f,1);
            var grid=content.gameObject.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(220,230);grid.spacing=new Vector2(12,12);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;grid.childAlignment=TextAnchor.UpperCenter;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=viewport;scroll.content=content;Set(view,"rewards",content);Set(view,"rewardPrefab",reward.GetComponent<ResultRewardView>());
            Set(view,"experience",Text(panel,"EXP Gained","+0 EXP",25,.04f,.175f,.96f,.21f));
            var xp=(GameObject)PrefabUtility.InstantiatePrefab(bar,panel);Box(.045f,.075f,.955f,.17f).Apply((RectTransform)xp.transform);Set(view,"heroBar",xp.GetComponent<HeroExperienceBar>());
            var confirm=Button(panel,"Confirm","Confirm",.32f,.008f,.955f,.07f);confirm.image.color=new Color(1,.81f,.2f);Set(view,"confirm",confirm);
            Button(panel,"Statistics","Statistics",.045f,.008f,.29f,.07f);
            root.gameObject.AddComponent<LocalizedMenuScope>();var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,SeasonalFolder+"/Compact Run Result.prefab");Object.DestroyImmediate(root.gameObject);return prefab;
        }
        private static void InstallResult(string path,GameObject compact)
        {
            var root=PrefabUtility.LoadPrefabContents(path);try{
                var summary=root.GetComponentInChildren<RunSummaryPresenter>(true);
                var view=root.GetComponentInChildren<CompactRunResult>(true);
                if(view==null){foreach(Transform child in root.transform)if(child.GetComponent<RunSummaryPresenter>()==null)child.gameObject.SetActive(false);view=((GameObject)PrefabUtility.InstantiatePrefab(compact,root.transform)).GetComponent<CompactRunResult>();}
                Object controller=(Object)root.GetComponent<StageCompleteScreen>()??root.GetComponent<StageFailedScreen>();Set(controller,"compactResult",view);
                if(summary!=null)
                {
                    Set(view,"summary",summary);PrefabUtility.RecordPrefabInstancePropertyModifications(view);
                    var old=summary.transform.Find("Open Statistics");if(old!=null)old.gameObject.SetActive(false);
                    Set(summary,"openButton",view.transform.Find("Result Panel/Statistics").GetComponent<Button>());Set(summary,"highlights",null);
                    var highlights=summary.transform.Find("Statistics Panel/Record Highlights");if(highlights!=null)highlights.gameObject.SetActive(false);
                    var layout=summary.GetComponent<ResponsiveMenuLayout>();foreach(var style in layout.rectangles){if(style.target==null)continue;if(style.target.name=="Combat Statistics"){style.landscape=Box(.035f,.07f,.49f,.85f);style.portrait=Box(.035f,.48f,.965f,.85f);}if(style.target.name=="Build and Abilities"){style.landscape=Box(.51f,.07f,.965f,.85f);style.portrait=Box(.035f,.04f,.965f,.46f);}}
                    summary.transform.SetAsLastSibling();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        private static GameObject BuildSeasonHud()
        {
            var root=Rect(null,"Run Season HUD",0,0,1,1);var view=root.gameObject.AddComponent<RunSeasonView>();var layout=root.gameObject.AddComponent<ResponsiveMenuLayout>();
            var badge=Button(root,"Season Badge","Season",.68f,.91f,.97f,.97f);Set(view,"badge",badge);Set(view,"badgeText",badge.GetComponentInChildren<TMP_Text>());
            var info=Rect(root,"Season Information",.58f,.70f,.97f,.89f);var bg=Skin(info.gameObject);bg.raycastTarget=true;Set(view,"information",info.gameObject);Set(view,"background",bg);
            var icon=Rect(info,"Season Icon",.035f,.2f,.2f,.9f).gameObject.AddComponent<Image>();icon.preserveAspect=true;icon.raycastTarget=false;Set(view,"icon",icon);
            var title=Text(info,"Season Title","Season",33,.22f,.66f,.85f,.97f);Set(view,"title",title);var effect=title.gameObject.AddComponent<UITextEffect>();
            var effectData=new SerializedObject(effect);effectData.FindProperty("effect").enumValueIndex=(int)UITextEffectStyle.Pop;effectData.ApplyModifiedPropertiesWithoutUndo();
            Set(view,"description",Text(info,"Season Benefits","Benefits",25,.22f,.05f,.96f,.65f));Set(view,"close",Button(info,"Close","×",.87f,.72f,.98f,.97f));
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=(RectTransform)badge.transform,landscape=ResponsiveMenuLayout.Placement.Capture((RectTransform)badge.transform),portrait=Box(.62f,.76f,.96f,.82f)});
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle{target=info,landscape=ResponsiveMenuLayout.Placement.Capture(info),portrait=Box(.08f,.5f,.92f,.75f)});
            info.gameObject.SetActive(false);var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,SeasonalFolder+"/Run Season HUD.prefab");Object.DestroyImmediate(root.gameObject);return prefab;
        }
        private static void SeasonalTranslations()
        {
            var table=Resources.Load<MenuTranslations>("Menu Translations");
            Add(table,"Spring Thunder","Xuân · Mưa giông","Primavera · Tormenta","Primavera · Tempestade","Printemps · Orage");Add(table,"Summer Sun","Hạ · Nắng cháy","Verano · Sol","Verão · Sol","Été · Soleil");Add(table,"Autumn Mist","Thu · Sương độc","Otoño · Niebla","Outono · Névoa","Automne · Brume");Add(table,"Winter Frost","Đông · Băng giá","Invierno · Escarcha","Inverno · Geada","Hiver · Gel");
            Add(table,"Chain lightning damage +{0}%","Sát thương điện lan +{0}%","Daño de rayo en cadena +{0}%","Dano de raio em cadeia +{0}%","Dégâts de chaîne électrique +{0}%");Add(table,"Burn duration +{0}%","Thời gian thiêu đốt +{0}%","Duración de quemadura +{0}%","Duração da queimadura +{0}%","Durée de brûlure +{0}%");Add(table,"Poison weakening effect +{0}%","Hiệu quả suy yếu của độc +{0}%","Debilitamiento por veneno +{0}%","Enfraquecimento por veneno +{0}%","Affaiblissement du poison +{0}%");Add(table,"Freeze duration +{0}%","Thời gian đóng băng +{0}%","Duración de congelación +{0}%","Duração de congelamento +{0}%","Durée du gel +{0}%");
            Add(table,"Elemental damage","Sát thương nguyên tố","Daño elemental","Dano elemental","Dégâts élémentaires");Add(table,"No elemental season","Không có mùa nguyên tố","Sin temporada elemental","Sem estação elemental","Aucune saison élémentaire");
            Add(table,"Gain hero EXP by completing runs","Nhận EXP hero sau mỗi lượt chơi","Gana EXP de héroe en partidas","Ganhe EXP de herói nas partidas","Gagnez de l’EXP de héros en jouant");Add(table,"Hero EXP","EXP hero","EXP de héroe","EXP de herói","EXP de héros");
            Add(table,"Highest DPS","DPS cao nhất","DPS máximo","DPS máximo","DPS maximal");Add(table,"This run DPS","DPS lượt này","DPS de esta partida","DPS desta partida","DPS de cette partie");Add(table,"Fastest clear","Vượt ải nhanh nhất","Victoria más rápida","Vitória mais rápida","Victoire la plus rapide");Add(table,"Longest survival","Sinh tồn lâu nhất","Supervivencia más larga","Maior sobrevivência","Survie la plus longue");
            Add(table,"Statistics","Thống kê","Estadísticas","Estatísticas","Statistiques");Add(table,"Confirm","Xác nhận","Confirmar","Confirmar","Confirmer");EditorUtility.SetDirty(table);table.Rebuild();
        }
    }
}
