using System;
using System.Linq;
using System.Collections.Generic;
using MrX.Armory;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class StageShowcaseAuthoring
    {
        const string Root = "Assets/The Archer/";
        static Transform root;
        static ResponsiveMenuLayout layout;
        [MenuItem("Tools/MrX Studio/Stage/Apply Hero Showcase Layout")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required");
            RepairAnimations(); RepairGrips();
            string path=Root+"Prefabs/UI/Stage/Stage Page.prefab";
            var go=PrefabUtility.LoadPrefabContents(path); root=go.transform;
            try
            {
                layout=go.GetComponent<ResponsiveMenuLayout>();
                // The prefab and its two authored layout profiles are the only source of placement.
                var card=root.Find("Stage Showcase Card");
                if(card==null){var obj=new GameObject("Stage Showcase Card",typeof(RectTransform),typeof(Image));obj.layer=5;obj.transform.SetParent(root,false);card=obj.transform;}
                var inner=card.Find("Pastel Fill");if(inner==null){var obj=new GameObject("Pastel Fill",typeof(RectTransform),typeof(Image));obj.layer=5;obj.transform.SetParent(card,false);inner=obj.transform;}var fill=inner.GetComponent<Image>();fill.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Armory/ui_slot.png");fill.type=Image.Type.Sliced;fill.color=new Color(.94f,.99f,1);fill.raycastTarget=false;var ir=(RectTransform)inner;ir.anchorMin=Vector2.zero;ir.anchorMax=Vector2.one;ir.offsetMin=Vector2.one*18;ir.offsetMax=-Vector2.one*18;
                var image=card.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Armory/ui_slot.png");image.type=Image.Type.Sliced;image.color=new Color(.55f,.82f,1);image.raycastTarget=false;card.SetSiblingIndex(0);
                Rect(card as RectTransform,.19f,.145f,.81f,.49f,.04f,.20f,.96f,.46f);
                Point("Hero Panel",.5f,.55f,0,0,100,100,1.15f,.5f,.59f,0,0,100,100,.85f);
                Point("Hero Panel/Hero/Hero Raw Image",.5f,.5f,0,260,780,780,1,.5f,.5f,0,245,720,720,1);
                Point("Hero Panel/Hero/Hero Stand Image",.5f,.5f,0,0,560,210,1,.5f,.5f,0,0,520,200,1);
                Point("Hero Panel/Decor Image",.5f,.5f,0,330,1080,1080,1,.5f,.5f,0,320,1000,1000,1);
                Point("Hero Panel/Hero/Stats Background Image",.5f,.5f,0,-65,317.48f,65,1,.5f,.5f,0,-65,317.48f,65,1);
                Point("Hero Panel/Info_Ui",.5f,.5f,0,790,800,100,1,.5f,.5f,0,760,800,100,1);
                Point("Hero Panel/Slots/Weapon Slot",.5f,.5f,-440,470,190,190,1,.5f,.5f,-400,440,180,180,1);
                Point("Hero Panel/Slots/Helmet Slot",.5f,.5f,440,470,190,190,1,.5f,.5f,400,440,180,180,1);
                Point("Hero Panel/Slots/Ring Slot",.5f,.5f,-440,205,190,190,1,.5f,.5f,-400,190,180,180,1);
                Point("Hero Panel/Slots/Armor Slot",.5f,.5f,440,205,190,190,1,.5f,.5f,400,190,180,180,1);
                Rect("Stage Panel",.205f,.26f,.795f,.48f,.06f,.295f,.94f,.45f);
                Rect("Stage Panel/Stage",.47f,.04f,.96f,.96f,.48f,.05f,.96f,.95f);
                Hide("Stage Panel/Stage/Decor Image");Hide("Stage Panel/Stage/Stage Icon Shadow");Hide("Stage Panel/Stage/Stage Icon Background");
                Rect("Stage Panel/Stage/Stage Icon Mask",0,0,1,1,0,0,1,1);
                var mask=root.Find("Stage Panel/Stage/Stage Icon Mask");var maskImage=mask.GetComponent<Image>();maskImage.sprite=null;maskImage.color=new Color(1,1,1,.99f);
                Rect("Stage Panel/Stage Name",.035f,.15f,.44f,.92f,.025f,.16f,.46f,.92f);
                Hide("Stage Panel/Stage Name/Stage Name Shadow");Hide("Stage Panel/Stage Name/Stage Name Background");
                Rect("Stage Panel/Stage Name/Stage Number (TMP)",.02f,.67f,.98f,.90f,.02f,.67f,.98f,.90f);
                Rect("Stage Panel/Stage Name/Stage Name (TMP)",.02f,.14f,.98f,.66f,.02f,.14f,.98f,.66f);
                Font("Stage Panel/Stage Name/Stage Name (TMP)",76,52);Font("Stage Panel/Stage Name/Stage Number (TMP)",44,32);
                Rect("Stage Panel/Prev Stage Button",-.055f,.30f,.035f,.70f,-.04f,.30f,.05f,.70f);root.Find("Stage Panel/Prev Stage Button").localScale=new Vector3(-1,1,1);UpdateScale("Stage Panel/Prev Stage Button",new Vector3(-1,1,1));
                Rect("Stage Panel/Next Stage Button",.965f,.30f,1.055f,.70f,.95f,.30f,1.04f,.70f);
                Rect("Stage Panel/Gamepad Indicators",0,.2f,1,.8f,0,.2f,1,.8f);
                Point("Stage Panel/Gamepad Indicators/Left Gamepad Shoulder Indicator",0,.5f,-80,0,70,70,1,0,.5f,-30,0,65,65,1);
                Point("Stage Panel/Gamepad Indicators/Right Gamepad Shoulder Indicator",1,.5f,80,0,70,70,1,1,.5f,30,0,65,65,1);
                var rewardsRoot=root.Find("Rewards");var rootGrid=rewardsRoot.GetComponent<GridLayoutGroup>();if(rootGrid!=null)Object.DestroyImmediate(rootGrid);var rootImage=rewardsRoot.GetComponent<Image>();if(rootImage!=null)rootImage.enabled=false;
                Rect("Rewards",.225f,.175f,.47f,.255f,.08f,.235f,.51f,.29f);
                Rect("Rewards/Text (TMP)",0,.66f,1,1,0,.67f,1,1);Font("Rewards/Text (TMP)",38,29);
                Rect("Rewards/Main",.03f,0,.97f,.66f,.03f,0,.97f,.66f);
                var rewards=root.Find("Rewards/Main");var grid=rewards.GetComponent<GridLayoutGroup>();if(grid!=null)Object.DestroyImmediate(grid);var backing=rewards.GetComponent<Image>();if(backing!=null)backing.enabled=false;for(int i=0;i<rewards.childCount;i++){var r=rewards.GetChild(i) as RectTransform;Rect(r,i*.20f,0,i*.20f+.18f,1,i*.20f,0,i*.20f+.18f,1);var im=r.GetComponent<Image>();if(im!=null){im.enabled=true;im.preserveAspect=true;}foreach(var childImage in r.GetComponentsInChildren<Image>(true)){if(childImage.transform==r)continue;Rect(childImage.rectTransform,.1f,.1f,.9f,.9f,.1f,.1f,.9f,.9f);childImage.preserveAspect=true;}}
                Rect("Play Button Parent",.50f,.18f,.755f,.255f,.55f,.235f,.91f,.29f);
                Rect("Unlock Button Parent",.50f,.18f,.755f,.255f,.55f,.235f,.91f,.29f);
                Hide("Button Shadow");Rect("Info Button",.765f,.185f,.792f,.24f,.90f,.205f,.95f,.23f);
                Rect("Attempts Counter (TMP)",.50f,.15f,.755f,.175f,.52f,.205f,.88f,.23f);Font("Attempts Counter (TMP)",30,23);
                Rect("Stage Lucky Rewards/Lucky Wheel Shortcut",.845f,.58f,.95f,.76f,.08f,.105f,.44f,.185f);
                Rect("Stage Lucky Rewards/Gem Chest Shortcut",.845f,.37f,.95f,.55f,.56f,.105f,.92f,.185f);
                layout.Apply(false);EditorUtility.SetDirty(layout);PrefabUtility.SaveAsPrefabAsset(go,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(go);}
            // The pool view fills the authored banner mask, retaining its original slide and lock behavior.
            var db=AssetDatabase.LoadAssetAtPath<GameObject>(path);var selector=db.GetComponentInChildren<MainMenuStageSelector>(true);var sp=new SerializedObject(selector);var icon=(GameObject)sp.FindProperty("selectablePrefab").objectReferenceValue;
            var content=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(icon));try{var rt=content.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;var bg=content.GetComponent<Image>();bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Armory/ui_slot.png");bg.type=Image.Type.Sliced;var iconSo=new SerializedObject(content.GetComponent<MainMenuStageIcon>());var im=(Image)iconSo.FindProperty("iconImage").objectReferenceValue;im.preserveAspect=true;var fitter=im.GetComponent<AspectRatioFitter>();if(fitter==null)fitter=im.gameObject.AddComponent<AspectRatioFitter>();fitter.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fitter.aspectRatio=1;iconSo.FindProperty("iconAspect").objectReferenceValue=fitter;iconSo.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(content,AssetDatabase.GetAssetPath(icon));}finally{PrefabUtility.UnloadPrefabContents(content);}
            var previewPath=Root+"Prefabs/UI/Armory/Hero Preview.prefab";
            var preview=PrefabUtility.LoadPrefabContents(previewPath);
            try {var camera=preview.GetComponentInChildren<Camera>();camera.orthographicSize=1.65f;var position=camera.transform.localPosition;position.y=1.15f;camera.transform.localPosition=position;PrefabUtility.SaveAsPrefabAsset(preview,previewPath);}
            finally {PrefabUtility.UnloadPrefabContents(preview);}
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/MrX Studio/Armory/Apply Mage Barbarian Gem Defaults")]
        public static void ConfigureHeroes()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required");
            foreach(var name in new[]{"Mage","Barbarian"}){var h=AssetDatabase.LoadAssetAtPath<HeroData>(Root+"Scriptables/Armory/Heroes/Adventurers/"+name+".asset");var so=new SerializedObject(h);so.FindProperty("isUnlockedByDefault").boolValue=false;var levels=so.FindProperty("heroLevels");var price=levels.GetArrayElementAtIndex(0).FindPropertyRelative("price");price.FindPropertyRelative("currencyId").stringValue="gem";price.FindPropertyRelative("amount").intValue=name=="Mage"?500:750;
                if(name=="Barbarian"){foreach(var pair in new[]{new Vector2(1,220),new Vector2(0,55)}){var first=levels.GetArrayElementAtIndex(0).FindPropertyRelative("stats");float old=0;for(int j=0;j<first.arraySize;j++)if(first.GetArrayElementAtIndex(j).FindPropertyRelative("statType").intValue==(int)pair.x)old=first.GetArrayElementAtIndex(j).FindPropertyRelative("value").floatValue;for(int i=0;i<levels.arraySize;i++){var stats=levels.GetArrayElementAtIndex(i).FindPropertyRelative("stats");for(int j=0;j<stats.arraySize;j++)if(stats.GetArrayElementAtIndex(j).FindPropertyRelative("statType").intValue==(int)pair.x)stats.GetArrayElementAtIndex(j).FindPropertyRelative("value").floatValue*=pair.y/Mathf.Max(1,old);}}}so.ApplyModifiedPropertiesWithoutUndo();}
        }
        static AnimationClip Loop(AnimationClip source)
        {
            string path=Root+"Animations/Heroes/Adventurers/Loop "+source.name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(clip==null){clip=Object.Instantiate(source);clip.name="Loop "+source.name;AnimationUtility.SetAnimationEvents(clip,new AnimationEvent[0]);AssetDatabase.CreateAsset(clip,path);}var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=true;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);return clip;
        }
        static void RepairAnimations()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:AnimatorOverrideController",new[]{Root+"Animations/Heroes/Adventurers"})){var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(AssetDatabase.GUIDToAssetPath(guid));var pairs=new List<KeyValuePair<AnimationClip,AnimationClip>>();ctrl.GetOverrides(pairs);for(int i=0;i<pairs.Count;i++){var p=pairs[i];if(p.Value!=null&&(p.Key.name=="Ranger_Idle"||p.Key.name=="Ranger_Bow_Walk"||p.Key.name=="Ranger_Bow_Run")){var src=p.Value;if(!src.name.StartsWith("Loop "))pairs[i]=new KeyValuePair<AnimationClip,AnimationClip>(p.Key,Loop(src));}}ctrl.ApplyOverrides(pairs);EditorUtility.SetDirty(ctrl);}
        }
        static void RepairGrips()
        {
            foreach(var name in new[]{"Mage Staff","Mage Wand","Barbarian Battle Axe","Barbarian Axe and Shield"}){string path=Root+"Prefabs/Weapons/Adventurers/"+name+".prefab";var go=PrefabUtility.LoadPrefabContents(path);try{var mesh=go.GetComponentInChildren<Renderer>().transform;var model=mesh;while(model.parent!=go.transform)model=model.parent;model.localRotation=name=="Barbarian Battle Axe"?Quaternion.Euler(346.65f,121.21f,0)*Quaternion.Euler(0,0,90):Quaternion.Euler(name=="Mage Staff"?70:90,0,0);model.localPosition=Vector3.zero;PrefabUtility.SaveAsPrefabAsset(go,path);}finally{PrefabUtility.UnloadPrefabContents(go);}}
        }
        static void Hide(string path){var t=root.Find(path);if(t==null)return;t.gameObject.SetActive(false);var v=layout.visibility.Find(x=>x.target==t.gameObject);if(v==null){v=new ResponsiveMenuLayout.Visibility{target=t.gameObject};layout.visibility.Add(v);}v.landscape=v.portrait=false;}
        static void Font(string path,float l,float p){var t=root.Find(path).GetComponent<TMP_Text>();t.color=new Color(.12f,.22f,.48f);t.enableAutoSizing=true;var v=layout.texts.Find(x=>x.target==t);if(v==null){v=new ResponsiveMenuLayout.TextStyle{target=t};layout.texts.Add(v);}v.landscape=l;v.portrait=p;v.autoSize=true;}
        static void UpdateScale(string path,Vector3 value){var s=layout.rectangles.Find(x=>x.target==root.Find(path));s.landscape.scale=s.portrait.scale=value;}
        static void Rect(string path,float x,float y,float z,float w,float a,float b,float c,float d){var t=root.Find(path) as RectTransform;if(t!=null)Rect(t,x,y,z,w,a,b,c,d);}
        static void Rect(RectTransform t,float x,float y,float z,float w,float a,float b,float c,float d){var s=layout.rectangles.Find(v=>v.target==t);if(s==null){s=new ResponsiveMenuLayout.RectStyle{target=t};layout.rectangles.Add(s);}s.landscape=Box(x,y,z,w);s.portrait=Box(a,b,c,d);s.landscape.Apply(t);}
        static ResponsiveMenuLayout.Placement Box(float x,float y,float z,float w){return new ResponsiveMenuLayout.Placement{min=new Vector2(x,y),max=new Vector2(z,w),pivot=Vector2.one*.5f,scale=Vector3.one};}
        static void Point(string path,float x,float y,float px,float py,float w,float h,float s,float a,float b,float ax,float ay,float aw,float ah,float ps){var t=root.Find(path) as RectTransform;if(t==null)return;var v=layout.rectangles.Find(k=>k.target==t);if(v==null){v=new ResponsiveMenuLayout.RectStyle{target=t};layout.rectangles.Add(v);}v.landscape=Box(x,y,x,y);v.landscape.offsetMin=new Vector2(px-w/2,py-h/2);v.landscape.offsetMax=new Vector2(px+w/2,py+h/2);v.landscape.scale=Vector3.one*s;v.portrait=Box(a,b,a,b);v.portrait.offsetMin=new Vector2(ax-aw/2,ay-ah/2);v.portrait.offsetMax=new Vector2(ax+aw/2,ay+ah/2);v.portrait.scale=Vector3.one*ps;v.landscape.Apply(t);}
    }
}




