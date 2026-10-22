using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MrX.Abilities;
using MrX.Armory;
using MrX.Player;
using MrX.Weapon;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    /// <summary>Editor-only content authoring. Existing generated assets remain authoritative on repeat installs.</summary>
    public static class HeroRosterAuthoring
    {
        private const string Root = "Assets/The Archer/";
        private const string HeroFolder = Root + "Scriptables/Armory/Heroes/Adventurers";
        private const string WeaponFolder = Root + "Scriptables/Armory/Items/Adventurers";
        private const string AnimFolder = Root + "Animations/Heroes/Adventurers";
        private const string HeroPrefabs = Root + "Prefabs/Player/Heroes/Adventurers";
        private const string WeaponPrefabs = Root + "Prefabs/Weapons/Adventurers";
        private const string Icons = Root + "Sprites/UI/Adventurers";
        private const string Materials = Root + "Materials/Heroes/Adventurers";
        private static ArmoryDatabase database;
        private static HeroData ranger;
        private static readonly Dictionary<string, WeaponAnimationsSet> animationSets = new Dictionary<string, WeaponAnimationsSet>();
        private sealed class WeaponSpec
        {
            public string name, model, motion, idle, offhand;
            public HeroClasses classes;
            public bool melee, left;
            public float size, damage, reach;
            public WeaponSpec(string n,string m,HeroClasses c,string a,float s,float d,bool close=false,string shield=null,bool l=false,string pose="Idle_A",float r=2.6f)
            {name=n;model=m;classes=c;motion=a;size=s;damage=d;melee=close;offhand=shield;left=l;idle=pose;reach=r;}
        }
        private static readonly WeaponSpec[] Weapons = {
            new WeaponSpec("Ranger Longbow","Ranger/bow_withString",HeroClasses.Ranger,"Ranged_Bow_Release",1.2f,16,false,null,true,"Ranged_Bow_Idle"),
            new WeaponSpec("Mage Wand","Mage/wand",HeroClasses.Mage,"Ranged_Magic_Spellcasting",.85f,15),
            new WeaponSpec("Mage Staff","Mage/staff",HeroClasses.Mage,"Ranged_Magic_Shoot",1.8f,20),
            new WeaponSpec("Arcane Spellbook","Mage/spellbook_open",HeroClasses.Mage,"Ranged_Magic_Spellcasting_Long",.65f,18,false,null,true),
            new WeaponSpec("Barbarian Axe and Shield","Barbarian/axe_1handed",HeroClasses.Barbarian,"Melee_1H_Attack_Slice_Horizontal",1.1f,18,true,"Barbarian/shield_round_barbarian",false,"Idle_A",2.7f),
            new WeaponSpec("Barbarian Battle Axe","Barbarian/axe_2handed",HeroClasses.Barbarian,"Melee_2H_Attack_Chop",1.75f,26,true,null,false,"Melee_2H_Idle",3.1f),
            new WeaponSpec("Knight Sword and Shield","Knight/sword_1handed",HeroClasses.Knight,"Melee_1H_Attack_Slice_Diagonal",1.15f,16,true,"Knight/shield_round_color"),
            new WeaponSpec("Knight Greatsword","Knight/sword_2handed",HeroClasses.Knight,"Melee_2H_Attack_Slice",1.8f,22,true,null,false,"Melee_2H_Idle",3f),
            new WeaponSpec("Royal Greatsword","Knight/sword_2handed_color",HeroClasses.Knight,"Melee_2H_Attack_Chop",1.8f,25,true,null,false,"Melee_2H_Idle",3f),
            new WeaponSpec("Rogue Dagger","Rogue/dagger",HeroClasses.Rogue,"Melee_1H_Attack_Stab",.8f,12,true,null,false,"Idle_A",2.3f),
            new WeaponSpec("Rogue Hand Crossbow","Rogue/crossbow_1handed",HeroClasses.Rogue,"Ranged_1H_Shoot",1f,15,false,null,false,"Ranged_1H_Aiming"),
            new WeaponSpec("Rogue Heavy Crossbow","Rogue/crossbow_2handed",HeroClasses.Rogue,"Ranged_2H_Shoot",1.25f,21,false,null,false,"Ranged_2H_Aiming")
        };
        private static void Folder(string path) { if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path)); }
        private static void Set(SerializedObject so,string name,Object value) { so.FindProperty(name).objectReferenceValue=value; }
        private static AnimationClip Clip(string name)
        {
            foreach(var file in new[]{"CombatRanged","CombatMelee","General","MovementBasic"})
                foreach(var a in AssetDatabase.LoadAllAssetsAtPath(Root+"Models/Heroes/Ranger/Animation_Pack/"+file+".fbx"))
                    if(a is AnimationClip clip && clip.name==name)return clip;
            throw new InvalidOperationException("Missing animation: "+name);
        }
        [MenuItem("Tools/MrX Studio/Armory/Install Adventurer Roster")]
        public static void Install()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            foreach(var folder in new[]{HeroFolder,WeaponFolder,AnimFolder,HeroPrefabs,WeaponPrefabs,Icons,Materials})Folder(folder);
            database=AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(Root+"Scriptables/Armory/Armory Database.asset");
            ranger=database.Heroes.First(h=>h.Name=="Ranger"); animationSets.Clear();
            bool initializeLegacy = !database.Heroes.Any(h=>h!=null&&h.Name=="Mage");
            var weapons=new Dictionary<string,WeaponData>();foreach(var spec in Weapons)weapons.Add(spec.name,CreateWeapon(spec));
            // Prepare all supplied offhand shapes as editable cosmetic prefabs, without inventing a fifth inventory slot.
            foreach(var name in new[]{"shield_badge_color","shield_round","shield_spikes","shield_spikes_color","shield_square","shield_square_color"})Offhand("Knight/"+name);
            Offhand("Mage/spellbook_closed");Offhand("Barbarian/mug_full");Offhand("Barbarian/mug_empty");
            var heroes=new List<HeroData>{ranger};
            heroes.Add(CreateHero("Mage",HeroClass.Mage,weapons["Mage Staff"],false,85,35,1.3f,5));
            heroes.Add(CreateHero("Barbarian",HeroClass.Barbarian,weapons["Barbarian Battle Axe"],false,150,38,1.1f,4.7f));
            heroes.Add(CreateHero("Knight",HeroClass.Knight,weapons["Knight Sword and Shield"],true,160,32,1.2f,4.7f));
            heroes.Add(CreateHero("Rogue",HeroClass.Rogue,weapons["Rogue Hand Crossbow"],true,95,28,1.8f,5.8f));
            heroes.Add(CreateHero("Rogue_Hooded",HeroClass.Rogue,weapons["Rogue Dagger"],true,95,28,1.8f,5.8f));
            if(ranger.StarterWeapon==null){var heroObject=new SerializedObject(ranger);Set(heroObject,"starterWeapon",database.Items.OfType<WeaponData>().First(w=>w.ItemName=="Signature Bow"));heroObject.ApplyModifiedPropertiesWithoutUndo();}
            var so=new SerializedObject(database);var items=so.FindProperty("items");
            foreach(var w in weapons.Values)if(!database.Items.Contains(w)){int i=items.arraySize;items.InsertArrayElementAtIndex(i);items.GetArrayElementAtIndex(i).objectReferenceValue=w;}
            var list=so.FindProperty("heroes");foreach(var h in heroes)if(!database.Heroes.Contains(h)){int i=list.arraySize;list.InsertArrayElementAtIndex(i);list.GetArrayElementAtIndex(i).objectReferenceValue=h;}
            so.ApplyModifiedPropertiesWithoutUndo();
            // Legacy IDs/levels stay intact. The old wand remains usable by Ranger for existing profiles.
            foreach(var w in database.Items.OfType<WeaponData>())if(initializeLegacy&&!weapons.Values.Contains(w)){
                var ws=new SerializedObject(w);ws.FindProperty("allowedClasses").intValue=w.ItemName.Contains("Wand")?(int)(HeroClasses.Ranger|HeroClasses.Mage):(int)(HeroClasses.Ranger|HeroClasses.Rogue);ws.ApplyModifiedPropertiesWithoutUndo();
            }
            so.Update();var mappings=so.FindProperty("weaponAnimationSetData").FindPropertyRelative("weaponAnimationSets");
            foreach(var hero in heroes)foreach(var weapon in database.Items.OfType<WeaponData>())if(weapon.Supports(hero)){
                var spec=Weapons.FirstOrDefault(s=>s.name==weapon.ItemName);WeaponAnimationsSet set;
                if(spec!=null)set=animationSets[spec.name];
                else if(weapon.ItemName.Contains("Wand"))set=animationSets["Mage Wand"];
                else if(weapon.ItemName.Contains("Cross")||weapon.ItemName.Contains("Cross bow"))set=animationSets["Rogue Hand Crossbow"];
                else {set=database.Heroes.Count>0?database.GetWeaponAnimationsSet(ranger.Id,weapon.Id):null;}
                int index=-1;for(int i=0;i<mappings.arraySize;i++)if(mappings.GetArrayElementAtIndex(i).FindPropertyRelative("heroId").stringValue==hero.Id&&mappings.GetArrayElementAtIndex(i).FindPropertyRelative("weaponId").stringValue==weapon.Id){index=i;break;}
                // Keep the existing Ranger bow/crossbow mapping.
                if(index>=0&&!(initializeLegacy&&hero==ranger&&weapon.ItemName.Contains("Wand")&&spec==null))continue;
                if(index<0){index=mappings.arraySize;mappings.InsertArrayElementAtIndex(index);}var entry=mappings.GetArrayElementAtIndex(index);entry.FindPropertyRelative("heroId").stringValue=hero.Id;entry.FindPropertyRelative("weaponId").stringValue=weapon.Id;entry.FindPropertyRelative("animationsSet").objectReferenceValue=set;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach(var guid in AssetDatabase.FindAssets("t:AbilityData")){
                var ability=AssetDatabase.LoadAssetAtPath<AbilityData>(AssetDatabase.GUIDToAssetPath(guid));
                if(new[]{AbilityType.Weapon_BouncyShot,AbilityType.Weapon_PiercingStrike,AbilityType.Weapon_Ricochet,AbilityType.Weapon_SplitShot,AbilityType.Weapon_Bullseye,AbilityType.Weapon_Sniper}.Contains(ability.AbilityType)){
                    var a=new SerializedObject(ability);a.FindProperty("rangedWeaponsOnly").boolValue=true;a.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            AuthorRosterButtons(heroes);AssetDatabase.SaveAssets();
            Debug.Log("Adventurer roster ready: "+heroes.Count+" heroes, "+weapons.Count+" additional weapons. Existing assets preserved.");
        }
        private static HeroData CreateHero(string name,HeroClass heroClass,WeaponData starter,bool hidden,float hp,float damage,float speed,float movement)
        {
            string path=HeroFolder+"/"+name+".asset";var data=AssetDatabase.LoadAssetAtPath<HeroData>(path);if(data!=null)return data;
            var prefab=CreateHeroPrefab(name);data=Object.Instantiate(ranger);data.name=name;AssetDatabase.CreateAsset(data,path);data.RecalculateId();
            var so=new SerializedObject(data);so.FindProperty("heroName").stringValue=name.Replace('_',' ');so.FindProperty("heroClass").intValue=(int)heroClass;so.FindProperty("hideInRoster").boolValue=hidden;so.FindProperty("isDefaultHero").boolValue=false;so.FindProperty("isUnlockedByDefault").boolValue=!hidden;Set(so,"prefab",prefab);Set(so,"starterWeapon",starter);
            var levels=so.FindProperty("heroLevels");for(int i=0;i<levels.arraySize;i++){
                var level=levels.GetArrayElementAtIndex(i);if(i==0)level.FindPropertyRelative("price").FindPropertyRelative("amount").intValue=hidden?1500:0;
                var stats=level.FindPropertyRelative("stats");for(int j=0;j<stats.arraySize;j++){var stat=stats.GetArrayElementAtIndex(j);var type=(StatType)stat.FindPropertyRelative("statType").intValue;float value=stat.FindPropertyRelative("value").floatValue;float reference=ranger.HeroLevels[0].GetStatValue(type);float target=type==StatType.HP?hp:type==StatType.Damage?damage:type==StatType.AttackSpeed?speed:type==StatType.MovementSpeed?movement:type==StatType.DamageReduction?(heroClass==HeroClass.Knight?.8f:1):reference;stat.FindPropertyRelative("value").floatValue=reference==0?target:value/reference*target;}
            }
            so.ApplyModifiedPropertiesWithoutUndo();Set(so,"icon",Portrait(prefab,name,true));so.ApplyModifiedPropertiesWithoutUndo();return data;
        }
        private static GameObject CreateHeroPrefab(string name)
        {
            string path=HeroPrefabs+"/"+name+".prefab";var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset!=null)return asset;
            var root=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(ranger.Prefab));try{
                root.name=name;var animator=root.GetComponentInChildren<Animator>();var visual=animator.gameObject;
                if(PrefabUtility.IsPartOfPrefabInstance(visual))PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                var boneMap=visual.GetComponentsInChildren<Transform>(true).GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
                var old=visual.GetComponentsInChildren<SkinnedMeshRenderer>();var baseMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Heroes/Hero Archer.mat");
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Heroes/"+name+"/"+name+".fbx");
                var source=model.GetComponentInChildren<SkinnedMeshRenderer>();var material=AssetDatabase.LoadAssetAtPath<Material>(Materials+"/"+name+".mat");if(material==null){material=new Material(baseMaterial){name=name+" Adventurer"};if(source.sharedMaterial.mainTexture!=null)material.mainTexture=source.sharedMaterial.mainTexture;AssetDatabase.CreateAsset(material,Materials+"/"+name+".mat");}
                var renderers=new List<Renderer>();foreach(var smr in model.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
                    var go=new GameObject(smr.name);go.transform.SetParent(visual.transform,false);go.transform.localPosition=smr.transform.localPosition;go.transform.localRotation=smr.transform.localRotation;go.transform.localScale=smr.transform.localScale;
                    var skin=go.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=smr.sharedMesh;skin.sharedMaterials=smr.sharedMaterials.Select(m=>material).ToArray();skin.bones=smr.bones.Select(b=>boneMap[b.name]).ToArray();skin.rootBone=boneMap[smr.rootBone.name];skin.localBounds=smr.localBounds;renderers.Add(skin);
                }
                foreach(var renderer in old)Object.DestroyImmediate(renderer.gameObject);
                var handler=new SerializedObject(root.GetComponentInChildren<RenderersHandler>(true));var list=handler.FindProperty("renderers");list.arraySize=renderers.Count;for(int i=0;i<renderers.Count;i++)list.GetArrayElementAtIndex(i).objectReferenceValue=renderers[i];handler.ApplyModifiedPropertiesWithoutUndo();
                return PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        private static GameObject Model(Transform parent,string path,float size)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Weapons/"+path+".fbx");if(source==null)throw new InvalidOperationException(path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(source,parent);var renderers=go.GetComponentsInChildren<Renderer>(true);var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);go.transform.localScale*=size/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            go.transform.localRotation=Quaternion.Euler(0,0,180);foreach(var a in go.GetComponentsInChildren<Animator>(true))Object.DestroyImmediate(a);return go;
        }
        private static GameObject Offhand(string model)
        {
            Folder(WeaponPrefabs+"/Offhand");string path=WeaponPrefabs+"/Offhand/"+Path.GetFileName(model)+".prefab";var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset!=null)return asset;
            var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject(Path.GetFileName(model));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);try{Model(root.transform,model,.8f);return PrefabUtility.SaveAsPrefabAsset(root,path);}finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
        private static WeaponData CreateWeapon(WeaponSpec spec)
        {
            var set=CreateAnimations(spec);animationSets[spec.name]=set;
            string path=WeaponFolder+"/"+spec.name+".asset";var data=AssetDatabase.LoadAssetAtPath<WeaponData>(path);if(data!=null)return data;
            string prefabPath=WeaponPrefabs+"/"+spec.name+".prefab";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(prefab==null){var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject(spec.name);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);try{
                Model(root.transform,spec.model,spec.size);
                if(spec.melee)root.AddComponent<MeleeWeaponBehavior>();else{
                    var behavior=root.AddComponent<WeaponBehavior>();var so=new SerializedObject(behavior);var spawn=new GameObject("Projectile Spawn");spawn.transform.SetParent(root.transform,false);spawn.transform.localPosition=new Vector3(0,0,.25f);Set(so,"projectileSpawn",spawn.transform);
                    var old=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Weapons/"+(spec.classes==HeroClasses.Mage?"Wand":"Bow")+".prefab").GetComponent<WeaponBehavior>();var oldObject=new SerializedObject(old);Set(so,"projectilePrefab",oldObject.FindProperty("projectilePrefab").objectReferenceValue);Set(so,"shootingSound",oldObject.FindProperty("shootingSound").objectReferenceValue);so.FindProperty("animateInHands").boolValue=false;so.FindProperty("animateDetached").boolValue=false;so.FindProperty("detachedScale").vector3Value=Vector3.one;so.ApplyModifiedPropertiesWithoutUndo();
                }
                prefab=PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }finally{EditorSceneManager.ClosePreviewScene(scene);}}
            var template=database.Items.OfType<WeaponData>().First(w=>w.ItemName=="Signature Bow");data=Object.Instantiate(template);data.name=spec.name;AssetDatabase.CreateAsset(data,path);data.RecalculateId();var serialized=new SerializedObject(data);serialized.FindProperty("itemName").stringValue=spec.name;serialized.FindProperty("isDefaultItem").boolValue=false;serialized.FindProperty("isUnlockedByDefault").boolValue=false;serialized.FindProperty("allowedClasses").intValue=(int)spec.classes;serialized.FindProperty("isMelee").boolValue=spec.melee;serialized.FindProperty("attackReach").floatValue=spec.reach;serialized.FindProperty("isRightHandWeapon").boolValue=!spec.left;Set(serialized,"weaponPrefab",prefab);if(spec.offhand!=null)Set(serialized,"offhandVisualPrefab",Offhand(spec.offhand));
            var levels=serialized.FindProperty("itemLevels");float baseDamage=template.ItemLevels[0].GetStatValue(StatType.Damage);for(int i=0;i<levels.arraySize;i++){var stats=levels.GetArrayElementAtIndex(i).FindPropertyRelative("stats");for(int j=0;j<stats.arraySize;j++)if(stats.GetArrayElementAtIndex(j).FindPropertyRelative("statType").intValue==(int)StatType.Damage)stats.GetArrayElementAtIndex(j).FindPropertyRelative("value").floatValue*=spec.damage/Mathf.Max(1,baseDamage);}
            serialized.FindProperty("crafting").FindPropertyRelative("enabled").boolValue=true;serialized.FindProperty("crafting").FindPropertyRelative("requiredFragments").intValue=30;serialized.ApplyModifiedPropertiesWithoutUndo();Set(serialized,"icon",Portrait(prefab,spec.name,false));serialized.ApplyModifiedPropertiesWithoutUndo();return data;
        }
        private static WeaponAnimationsSet CreateAnimations(WeaponSpec spec)
        {
            string path=AnimFolder+"/"+spec.name+" Set.asset";var set=AssetDatabase.LoadAssetAtPath<WeaponAnimationsSet>(path);if(set!=null)return set;
            var attack=Object.Instantiate(Clip(spec.motion));attack.name=spec.name+" Attack";AnimationUtility.SetAnimationEvents(attack,new[]{new AnimationEvent{time=attack.length*(spec.melee?.48f:.52f),functionName="Shoot"}});var settings=AnimationUtility.GetAnimationClipSettings(attack);settings.loopTime=true;settings.loopBlend=false;AnimationUtility.SetAnimationClipSettings(attack,settings);AssetDatabase.CreateAsset(attack,AnimFolder+"/"+attack.name+".anim");
            var controller=new AnimatorOverrideController(ranger.Prefab.GetComponentInChildren<Animator>().runtimeAnimatorController);controller.name=spec.name;var overrides=new List<KeyValuePair<AnimationClip,AnimationClip>>();controller.GetOverrides(overrides);
            for(int i=0;i<overrides.Count;i++){var clip=overrides[i].Key;AnimationClip replace=clip.name=="Hero Shoot"?attack:clip.name=="Ranger_Idle"?Clip(spec.idle):clip.name=="Ranger_Bow_Walk"?Clip("Walking_A"):clip.name=="Ranger_Bow_Run"?Clip("Running_A"):overrides[i].Value;overrides[i]=new KeyValuePair<AnimationClip,AnimationClip>(clip,replace);}controller.ApplyOverrides(overrides);AssetDatabase.CreateAsset(controller,AnimFolder+"/"+spec.name+".overrideController");
            set=ScriptableObject.CreateInstance<WeaponAnimationsSet>();AssetDatabase.CreateAsset(set,path);var so=new SerializedObject(set);so.FindProperty("weaponAnimationsSetType").intValue=1;so.FindProperty("movementAnimationSpeed").floatValue=5;so.FindProperty("normalizeAttackSpeed").boolValue=true;so.FindProperty("attackAnimationLength").floatValue=attack.length;Set(so,"runtimeAnimatorController",controller);so.ApplyModifiedPropertiesWithoutUndo();return set;
        }
        [MenuItem("Tools/MrX Studio/Armory/Rebuild Adventurer Portraits")]
        public static void RebuildPortraits()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            var db=AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(Root+"Scriptables/Armory/Armory Database.asset");
            foreach(var hero in db.Heroes.Where(h=>h!=null&&AssetDatabase.GetAssetPath(h).StartsWith(HeroFolder)))Portrait(hero.Prefab,Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(hero)),true,true);
            foreach(var weapon in db.Items.OfType<WeaponData>().Where(w=>AssetDatabase.GetAssetPath(w).StartsWith(WeaponFolder)))Portrait(weapon.WeaponPrefab,weapon.ItemName,false,true);
        }
        private static Sprite Portrait(GameObject prefab,string name,bool head,bool force=false)
        {
            string path=Icons+"/"+name+".png";if(!force&&File.Exists(path))return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var scene=EditorSceneManager.NewPreviewScene();var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);GameObject cameraObject=null;RenderTexture rt=null;Texture2D texture=null;var previous=RenderTexture.active;
            try{
                if(head)Clip("Idle_A").SampleAnimation(instance.GetComponentInChildren<Animator>().gameObject,0);
                else{var bounds=instance.GetComponentsInChildren<Renderer>(true).Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});instance.transform.position-=bounds.center;instance.transform.rotation=Quaternion.Euler(-15,20,-20);}
                cameraObject=new GameObject("Portrait Camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.orthographicSize=head?.85f:1.15f;camera.transform.position=head?new Vector3(0,1.5f,4):new Vector3(0,0,4);camera.transform.LookAt(head?new Vector3(0,1.5f,0):Vector3.zero);camera.nearClipPlane=.1f;
                var lightObject=new GameObject("Portrait Light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,scene);var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(35,165,0);rt=RenderTexture.GetTemporary(512,512,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture=new Texture2D(512,512,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,512,512),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            }finally{RenderTexture.active=previous;if(rt!=null)RenderTexture.ReleaseTemporary(rt);if(texture!=null)Object.DestroyImmediate(texture);EditorSceneManager.ClosePreviewScene(scene);}
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void AuthorRosterButtons(List<HeroData> heroes)
        {
            var path=Root+"Prefabs/UI/Menu Pages/Armory Page.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{
                var dashboard=root.GetComponent<UI.ArmoryDashboard>();var so=new SerializedObject(dashboard);var buttons=so.FindProperty("heroButtons");var ids=so.FindProperty("heroIds");var original=((Button)buttons.GetArrayElementAtIndex(0).objectReferenceValue).gameObject;var parent=original.transform.parent;if(parent.name=="Hero Roster Buttons")parent=parent.parent;var template=Object.Instantiate(original,parent,false);template.SetActive(false);
                var holder=parent.Find("Hero Roster Buttons");if(holder==null){var go=new GameObject("Hero Roster Buttons",typeof(RectTransform),typeof(HorizontalLayoutGroup));go.layer=5;go.transform.SetParent(parent,false);holder=go.transform;var rect=(RectTransform)holder;rect.anchorMin=new Vector2(.04f,.05f);rect.anchorMax=new Vector2(.96f,.73f);rect.offsetMin=rect.offsetMax=Vector2.zero;var layout=go.GetComponent<HorizontalLayoutGroup>();layout.spacing=12;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=true;layout.childForceExpandWidth=true;}
                while(holder.childCount>0)Object.DestroyImmediate(holder.GetChild(0).gameObject);
                buttons.arraySize=heroes.Count;ids.arraySize=heroes.Count;for(int i=0;i<heroes.Count;i++){var go=Object.Instantiate(template,holder,false);go.name="Hero "+heroes[i].Name;go.SetActive(true);go.GetComponentInChildren<Image>(true);var portrait=go.transform.Find("Portrait").GetComponent<Image>();portrait.sprite=heroes[i].Icon;buttons.GetArrayElementAtIndex(i).objectReferenceValue=go.GetComponent<Button>();ids.GetArrayElementAtIndex(i).stringValue=heroes[i].Id;}
                if(original!=null&&original.transform.parent!=holder)original.SetActive(false);Object.DestroyImmediate(template);Set(so,"firstHero",buttons.GetArrayElementAtIndex(0).objectReferenceValue);so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
    }
}
