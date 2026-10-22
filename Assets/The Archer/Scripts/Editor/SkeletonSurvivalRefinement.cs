using System;
using System.Linq;
using MrX.Enemy;
using MrX.Projectile;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;
using Object=UnityEngine.Object;
namespace MrX.EditorTools
{
    public static class SkeletonSurvivalRefinement
    {
        const string Root="Assets/The Archer/";
        const string MatPath=Root+"Materials/Enemies/Skeletons/";
        [MenuItem("Tools/MrX Studio/Stage/Apply Skeleton Boss Combat Revision")]
        public static void Combat()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
            var arrow=Projectile(false);var bolt=Projectile(true);
            foreach(SkeletonBossRank rank in Enum.GetValues(typeof(SkeletonBossRank)))
            {
                var path=Root+"Prefabs/Enemies/Bosses/Skeletons/Skeleton "+rank+" Boss.prefab";var go=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var so=new SerializedObject(go.GetComponent<SkeletonBossBehavior>());
                    Set(so,"shotsBeforeSkill",rank==SkeletonBossRank.Mage?3:6);
                    Set(so,"shotRecovery",rank==SkeletonBossRank.Mage?.22f:.16f);Set(so,"shotWarning",.12f);
                    Set(so,"attackPlaybackSpeed",rank==SkeletonBossRank.Rogue?3.2f:rank==SkeletonBossRank.Mage?2.2f:2.4f);
                    Set(so,"gapCloseMinDistance",rank==SkeletonBossRank.Minion?3.8f:4.3f);Set(so,"gapCloseCooldown",3.2f);Set(so,"gapCloseWarning",.48f);
                    Set(so,"chargeDistance",rank==SkeletonBossRank.Minion?8f:10f);Set(so,"chargeDuration",rank==SkeletonBossRank.Minion?.32f:.42f);
                    Set(so,"attackRange",rank==SkeletonBossRank.Minion?3.2f:rank==SkeletonBossRank.Warrior?3.8f:40);
                    if(rank==SkeletonBossRank.Rogue||rank==SkeletonBossRank.Mage)so.FindProperty("projectilePrefab").objectReferenceValue=rank==SkeletonBossRank.Mage?bolt:arrow;
                    so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(go,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(go);}
                var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(Root+"Animations/Enemies/Skeletons/Skeleton "+rank+".controller");
                if(!ctrl.parameters.Any(p=>p.name=="Attack Cadence"))ctrl.AddParameter("Attack Cadence",AnimatorControllerParameterType.Float);
                var sm=ctrl.layers[0].stateMachine;var attack=sm.states.First(s=>s.state.name=="Attack").state;attack.speedParameter="Attack Cadence";attack.speedParameterActive=true;
                if(rank==SkeletonBossRank.Mage)
                {
                    var slow=attack.motion;
                    string clipPath=Root+"Animations/Enemies/Skeletons/Skeleton Mage Bolt Attack.anim";
                    var quick=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if(quick==null)
                    {
                        quick=Object.Instantiate(AssetDatabase.LoadAllAssetsAtPath(Root+"Models/Heroes/Ranger/Animation_Pack/CombatRanged.fbx").OfType<AnimationClip>().First(c=>c.name=="Ranged_Magic_Shoot"));quick.name="Skeleton Mage Bolt Attack";
                        var settings=AnimationUtility.GetAnimationClipSettings(quick);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(quick,settings);
                        AnimationUtility.SetAnimationEvents(quick,new[]{new AnimationEvent{functionName="OnAttack",time=quick.length*.5f},new AnimationEvent{functionName="OnAttackEnded",time=quick.length*.95f}});AssetDatabase.CreateAsset(quick,clipPath);
                    }
                    var skill=sm.states.FirstOrDefault(s=>s.state.name=="Magic Skill").state;
                    if(skill==null){skill=sm.AddState("Magic Skill");skill.motion=slow;skill.speedParameter="Attack Cadence";skill.speedParameterActive=true;var exit=skill.AddTransition(sm.states.First(s=>s.state.name=="Locomotion").state);exit.hasExitTime=true;exit.exitTime=.98f;exit.duration=.05f;var entry=sm.AddAnyStateTransition(skill);entry.hasExitTime=false;entry.canTransitionToSelf=false;entry.duration=.05f;entry.AddCondition(AnimatorConditionMode.If,0,"Attack");entry.AddCondition(AnimatorConditionMode.Equals,1,"Attack Type");}
                    foreach(var t in sm.anyStateTransitions)if(t.destinationState==attack&&!t.conditions.Any(c=>c.parameter=="Attack Type"))t.AddCondition(AnimatorConditionMode.Equals,0,"Attack Type");
                    attack.motion=quick;
                }
                EditorUtility.SetDirty(ctrl);
            }
            AssetDatabase.SaveAssets();
        }
        static void Set(SerializedObject so,string name,float value){so.FindProperty(name).floatValue=value;}
        static void Set(SerializedObject so,string name,int value){so.FindProperty(name).intValue=value;}
        static Material ColorMat(string name,Color color)
        {
            var path=MatPath+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat!=null)return mat;
            mat=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name=name};mat.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(mat,path);return mat;
        }
        static GameObject Projectile(bool magic)
        {
            string path=Root+"Prefabs/Projectiles/Enemies/Skeleton "+(magic?"Arcane Bolt":"Crossbow Bolt")+".prefab";var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset!=null)return asset;
            var go=PrefabUtility.LoadPrefabContents(Root+"Prefabs/Projectiles/Enemies/Wasp Projectile.prefab");try
            {
                go.name=magic?"Skeleton Arcane Bolt":"Skeleton Crossbow Bolt";var so=new SerializedObject(go.GetComponent<SimpleProjectile>());so.FindProperty("trailsToDisableOnHit").ClearArray();
                while(go.transform.childCount>0)Object.DestroyImmediate(go.transform.GetChild(0).gameObject);
                so.FindProperty("speed").floatValue=magic?10:15;so.FindProperty("lifetime").floatValue=3.2f;
                if(!magic)
                {
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Heroes/Boss/Weapons/Skeleton_Arrow.fbx"),go.transform);
                    var rs=model.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);model.transform.localScale*=.9f/Mathf.Max(b.size.x,b.size.y,b.size.z);
                    foreach(var r in rs){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                }
                else
                {
                    var meshPath=Root+"Animations/Enemies/Skeletons/Arcane Octahedron.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(mesh==null){mesh=new Mesh{name="Arcane Octahedron"};mesh.vertices=new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};mesh.triangles=new[]{0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,meshPath);}
                    for(int i=0;i<2;i++){var core=new GameObject(i==0?"Arcane Aura":"Bright Core",typeof(MeshFilter),typeof(MeshRenderer));core.transform.SetParent(go.transform,false);core.transform.localScale=Vector3.one*(i==0?.45f:.24f);core.transform.localRotation=Quaternion.Euler(i*30,i*45,0);core.GetComponent<MeshFilter>().sharedMesh=mesh;var r=core.GetComponent<MeshRenderer>();r.sharedMaterial=ColorMat(i==0?"Arcane Violet":"Arcane Core",i==0?new Color(.5f,.1f,1):new Color(.6f,1,1));r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                }
                so.ApplyModifiedPropertiesWithoutUndo();return PrefabUtility.SaveAsPrefabAsset(go,path);
            }finally{PrefabUtility.UnloadPrefabContents(go);}
        }
        [MenuItem("Tools/MrX Studio/Stage/Rebuild Stage 5 Optimized Necropolis")]
        public static void Arena()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
            string path=Root+"Prefabs/Prop/Halloween/Stage 5 Bone Courtyard.prefab";var go=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old=go.transform.Find("Bone Courtyard Decor");if(old!=null)Object.DestroyImmediate(old.gameObject);old=go.transform.Find("Necropolis Architecture");if(old!=null)Object.DestroyImmediate(old.gameObject);
                foreach(var r in go.transform.Find("Prop/Outside Grass").GetComponentsInChildren<Renderer>())r.enabled=false;
                foreach(var t in go.GetComponentsInChildren<Transform>(true))if(t.name.StartsWith("Clouds"))t.gameObject.SetActive(false);
                var decor=new GameObject("Necropolis Architecture");decor.transform.SetParent(go.transform,false);
                var ground=ColorMat("Necropolis Outer Ground",new Color(.12f,.18f,.21f));var stone=ColorMat("Necropolis Dark Stone",new Color(.22f,.28f,.32f));var trim=ColorMat("Necropolis Pale Trim",new Color(.36f,.43f,.49f));
                Block(decor.transform,"Burial Plateau",new Vector3(0,-.12f,0),new Vector3(70,.12f,75),ground);
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<6;i++)Block(decor.transform,"Low Stone Border",new Vector3(side*7.5f,.18f,-10+i*4),new Vector3(.7f,.35f,3.3f),stone);
                    for(int i=0;i<3;i++)Prop(decor.transform,"pillar",new Vector3(side*8.3f,0,-10+i*10),new Vector3(0,side*90,0),2.7f);
                    for(int i=0;i<3;i++)Prop(decor.transform,i==1?"grave_B":"gravemarker_A",new Vector3(side*(i==1?10.5f:10),0,-7+i*7),new Vector3(0,side*-35,0),i==1?1.1f:1.25f);
                    Prop(decor.transform,"lantern_standing",new Vector3(side*8.4f,0,-5),Vector3.zero,1.5f);Prop(decor.transform,"lantern_standing",new Vector3(side*8.4f,0,7),Vector3.zero,1.5f);
                    Prop(decor.transform,"tree_dead_small",new Vector3(side*13,0,side*6),new Vector3(0,side*30,0),3.4f);
                    var lamp=new GameObject("Warm Crypt Accent",typeof(Light));lamp.transform.SetParent(decor.transform,false);lamp.transform.localPosition=new Vector3(side*8,2,3);var light=lamp.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.55f,.24f);light.intensity=.8f;light.range=7;light.shadows=LightShadows.None;
                }
                Prop(decor.transform,"crypt",new Vector3(0,0,16),Vector3.zero,5);Prop(decor.transform,"arch",new Vector3(0,0,-16),Vector3.zero,3.3f);
                Prop(decor.transform,"coffin",new Vector3(-11,0,10),new Vector3(0,25,0),.7f);Prop(decor.transform,"shrine",new Vector3(11,0,10),new Vector3(0,-30,0),1.5f);
                // Simple authored pavement accents frame the combat floor without adding navigation obstacles.
                for(int i=0;i<5;i++)foreach(int side in new[]{-1,1})Block(decor.transform,"Pavement Trim",new Vector3(side*6.6f,.07f,-10+i*5),new Vector3(.22f,.025f,3),trim);
                var tile=ColorMat("Necropolis Outer Paving",new Color(.17f,.22f,.26f));
                for(int x=-2;x<=2;x++)for(int z=-3;z<=3;z++)
                    Block(decor.transform,"Courtyard Paving",new Vector3(x*8,-.045f,z*8),new Vector3(7.88f,.025f,7.88f),(x+z)%2==0?tile:ground);
                foreach(int side in new[]{-1,1})
                {
                    Block(decor.transform,"Burial Gallery Wall",new Vector3(side*16,1.3f,0),new Vector3(.6f,2.6f,29),stone);
                    Block(decor.transform,"Gallery Cornice",new Vector3(side*16,2.65f,0),new Vector3(.9f,.15f,29),trim);
                    foreach(int row in new[]{-1,1})
                    {
                        Block(decor.transform,"Raised Burial Platform",new Vector3(side*11.4f,-.04f,row*7),new Vector3(6,.12f,7.5f),stone);
                        Block(decor.transform,"Gallery Step",new Vector3(side*8.7f,.01f,row*7),new Vector3(.45f,.08f,6),trim);
                    }
                    Prop(decor.transform,"arch",new Vector3(side*11.7f,0,side*7),new Vector3(0,side*-90,0),3.7f);
                    Prop(decor.transform,"coffin",new Vector3(side*11.7f,.03f,side*7),new Vector3(0,side*90,0),.65f);
                }
                foreach(var r in decor.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                PrefabUtility.SaveAsPrefabAsset(go,path);AssetDatabase.SaveAssets();
            }finally{PrefabUtility.UnloadPrefabContents(go);}
        }
        static void Block(Transform parent,string name,Vector3 pos,Vector3 size,Material mat){var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=pos;obj.transform.localScale=size;Object.DestroyImmediate(obj.GetComponent<Collider>());obj.GetComponent<Renderer>().sharedMaterial=mat;}
        static void Prop(Transform parent,string name,Vector3 pos,Vector3 rot,float height)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Prop/Halloween/"+name+".prefab"),parent);obj.transform.localPosition=pos;obj.transform.localRotation=Quaternion.Euler(rot);
            var renderers=obj.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);obj.transform.localScale*=height/Mathf.Max(.01f,b.size.y);
            foreach(var c in obj.GetComponentsInChildren<Collider>())c.enabled=false;foreach(var n in obj.GetComponentsInChildren<NavMeshObstacle>())n.enabled=false;
        }
    }
}

