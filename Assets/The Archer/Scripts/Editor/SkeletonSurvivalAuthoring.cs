using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MrX.Enemy;
using MrX.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class SkeletonSurvivalAuthoring
    {
        const string Root="Assets/The Archer/";
        const string Folder=Root+"Prefabs/Enemies/Bosses/Skeletons/";
        const string Anim=Root+"Animations/Enemies/Skeletons/";
        const string Content=Root+"Scriptables/Stages/Stage Data 005.asset";
        static Material warning,body;
        [MenuItem("Tools/MrX Studio/Stage/Install Stage 5 Skeleton Survival")]
        public static void Install()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
            Ensure(Folder.TrimEnd('/'));Ensure(Anim.TrimEnd('/'));Ensure(Root+"Materials/Enemies/Skeletons");
            warning=Mat(Root+"Materials/Enemies/Skeletons/Warning.mat",new Color(1,.22f,.05f,.38f),true);
            body=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Enemies/Skeletons/Skeleton Body.mat");
            if(body==null){body=new Material(AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Heroes/Hero Archer.mat"));body.name="Skeleton Body";body.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/Characters/skeleton_texture.png");AssetDatabase.CreateAsset(body,Root+"Materials/Enemies/Skeletons/Skeleton Body.mat");}
            var db=AssetDatabase.LoadAssetAtPath<EnemiesDatabase>(Root+"Scriptables/Enemies Database.asset");
            var so=new SerializedObject(db);var list=so.FindProperty("enemies");
            for(int i=0;i<4;i++)
            {
                var rank=(SkeletonBossRank)i;var prefab=Boss(rank);
                int index=-1;for(int j=0;j<list.arraySize;j++)if(list.GetArrayElementAtIndex(j).FindPropertyRelative("enemyType").intValue==106+i){index=j;break;}
                if(index<0){index=list.arraySize;list.InsertArrayElementAtIndex(index);var entry=list.GetArrayElementAtIndex(index);entry.FindPropertyRelative("name").stringValue="Skeleton "+rank;entry.FindPropertyRelative("enemyType").intValue=106+i;entry.FindPropertyRelative("isBoss").boolValue=true;entry.FindPropertyRelative("prefab").objectReferenceValue=prefab;entry.FindPropertyRelative("drops").ClearArray();entry.FindPropertyRelative("bossChestRewards").objectReferenceValue=db.GetEnemy(EnemyType.Golem).BossChestRewards;}
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            BuildStage();AssetDatabase.SaveAssets();
        }
        static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;var parent=Path.GetDirectoryName(path).Replace('\\','/');Ensure(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));}
        static Material Mat(string path,Color color,bool transparent)
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat!=null)return mat;
            mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mat.SetColor("_BaseColor",color);
            if(transparent){mat.SetFloat("_Surface",1);mat.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.SetFloat("_Cull",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;}
            AssetDatabase.CreateAsset(mat,path);return mat;
        }
        static AnimationClip Source(string file,string name,bool boss=true)
        {
            string path=Root+"Models/Heroes/"+(boss?"Boss/Animation/Rig_Medium_":"Ranger/Animation_Pack/")+file+".fbx";
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>c.name==name);
        }
        static AnimationClip Copy(string name,AnimationClip source,bool loop,string evt=null,float eventTime=.5f)
        {
            string path=Anim+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(clip!=null)return clip;
            clip=Object.Instantiate(source);clip.name=name;
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;settings.loopBlend=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            var events=new List<AnimationEvent>();if(evt!=null)events.Add(new AnimationEvent{functionName=evt,time=clip.length*eventTime});
            if(name.Contains("Attack"))events.Add(new AnimationEvent{functionName="OnAttackEnded",time=clip.length*.95f});
            AnimationUtility.SetAnimationEvents(clip,events.ToArray());AssetDatabase.CreateAsset(clip,path);return clip;
        }
        static AnimatorController Controller(SkeletonBossRank rank)
        {
            string path=Anim+"Skeleton "+rank+".controller";var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(ctrl!=null)return ctrl;
            var idle=Copy("Skeleton Idle",Source("General","Idle_A"),true);
            var walk=Copy("Skeleton Run",Source("MovementBasic","Running_A"),true);
            var death=Copy("Skeleton Death",Source("General","Death_A"),false,"OnDefeatAnimationEnded",.96f);
            var spawn=Copy("Skeleton Spawn",Source("General","Spawn_Ground"),false,"OnSpawnEnded",.94f);
            string motion=rank==SkeletonBossRank.Rogue?"Ranged_2H_Shoot":rank==SkeletonBossRank.Mage?"Ranged_Magic_Spellcasting_Long":"Melee_1H_Attack_Slice_Horizontal";
            var attack=Copy("Skeleton "+rank+" Attack",Source(rank==SkeletonBossRank.Rogue||rank==SkeletonBossRank.Mage?"CombatRanged":"CombatMelee",motion,false),false,"OnAttack",.50f);
            ctrl=AnimatorController.CreateAnimatorControllerAtPath(path);
            ctrl.AddParameter("Movement Blend",AnimatorControllerParameterType.Float);ctrl.AddParameter("Attack",AnimatorControllerParameterType.Trigger);ctrl.AddParameter("Attack Type",AnimatorControllerParameterType.Int);ctrl.AddParameter("Defeat",AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Attack Cadence",AnimatorControllerParameterType.Float);
            var sm=ctrl.layers[0].stateMachine;
            var locomotion=sm.AddState("Locomotion");var blend=new BlendTree{name="Skeleton Locomotion",blendParameter="Movement Blend",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,ctrl);blend.AddChild(idle,0);blend.AddChild(walk,1);locomotion.motion=blend;
            var start=sm.AddState("Spawn");start.motion=spawn;sm.defaultState=start;Transition(start,locomotion,true);
            var strike=sm.AddState("Attack");strike.motion=attack;strike.speedParameter="Attack Cadence";strike.speedParameterActive=true;Transition(strike,locomotion,true);var enter=sm.AddAnyStateTransition(strike);enter.hasExitTime=false;enter.duration=.08f;enter.canTransitionToSelf=false;enter.AddCondition(AnimatorConditionMode.If,0,"Attack");
            var end=sm.AddState("Death");end.motion=death;var dead=sm.AddAnyStateTransition(end);dead.hasExitTime=false;dead.duration=.08f;dead.canTransitionToSelf=false;dead.AddCondition(AnimatorConditionMode.If,0,"Defeat");
            EditorUtility.SetDirty(ctrl);return ctrl;
        }
        static void Transition(AnimatorState a,AnimatorState b,bool exit){var t=a.AddTransition(b);t.hasExitTime=exit;t.exitTime=.98f;t.duration=.08f;}
        static GameObject Boss(SkeletonBossRank rank)
        {
            string path=Folder+"Skeleton "+rank+" Boss.prefab";var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing!=null)return existing;
            var root=PrefabUtility.LoadPrefabContents(Root+"Prefabs/Enemies/Simple Enemies/Candle Enemy.prefab");
            try
            {
                root.name="Skeleton "+rank+" Boss";
                var old=root.GetComponent<CandleEnemyBehavior>();var oldSo=new SerializedObject(old);var boss=root.AddComponent<SkeletonBossBehavior>();var so=new SerializedObject(boss);
                var it=oldSo.GetIterator();bool children=true;while(it.NextVisible(children)){children=false;if(it.name=="m_Script")continue;var prop=so.FindProperty(it.propertyPath);if(prop!=null)so.CopyFromSerializedProperty(it);}so.ApplyModifiedPropertiesWithoutUndo();
                var oldVisual=(GameObject)oldSo.FindProperty("graphicsObject").objectReferenceValue;Object.DestroyImmediate(old);
                var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Heroes/Boss/Skeleton_"+rank+".fbx"),root.transform);
                PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);visual.name="Skeleton "+rank+" Visual";
                visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one*(rank==SkeletonBossRank.Minion?1.25f:rank==SkeletonBossRank.Mage?1.65f:1.45f);
                var animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=Controller(rank);animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;visual.AddComponent<EnemyEventsHandler>();
                var all=visual.GetComponentsInChildren<Transform>(true);var right=all.First(t=>t.name=="handslot.r");var left=all.First(t=>t.name=="handslot.l");
                Weapon(right,rank==SkeletonBossRank.Minion?"Skeleton_Blade":rank==SkeletonBossRank.Warrior?"Skeleton_Axe":rank==SkeletonBossRank.Rogue?"Skeleton_Crossbow":"Skeleton_Staff",rank==SkeletonBossRank.Rogue?new Vector3(0,0,180):new Vector3(90,0,0));
                if(rank==SkeletonBossRank.Minion||rank==SkeletonBossRank.Warrior)Weapon(left,rank==SkeletonBossRank.Minion?"Skeleton_Shield_Small_A":"Skeleton_Shield_Large_A",new Vector3(0,0,180));
                var renderers=visual.GetComponentsInChildren<Renderer>(true);foreach(var r in renderers){if(r is SkinnedMeshRenderer)r.sharedMaterials=r.sharedMaterials.Select(m=>body).ToArray();}
                var handler=new SerializedObject(root.GetComponentInChildren<RenderersHandler>());var list=handler.FindProperty("renderers");var skins=visual.GetComponentsInChildren<SkinnedMeshRenderer>();list.arraySize=skins.Length;for(int j=0;j<skins.Length;j++)list.GetArrayElementAtIndex(j).objectReferenceValue=skins[j];handler.ApplyModifiedPropertiesWithoutUndo();
                so.Update();so.FindProperty("animator").objectReferenceValue=animator;so.FindProperty("graphicsObject").objectReferenceValue=visual;
                so.FindProperty("rank").intValue=(int)rank;so.FindProperty("useKnockback").boolValue=false;so.FindProperty("hitScale").vector3Value=Vector3.one;
                Float(so.FindProperty("baseHealth"),new[]{1800f,3000,4200,5500}[(int)rank]);Float(so.FindProperty("baseDamage"),new[]{12f,16,18,21}[(int)rank]);Float(so.FindProperty("collisionDamageMultiplier"),1);
                so.FindProperty("attackRange").floatValue=rank==SkeletonBossRank.Minion?3.4f:rank==SkeletonBossRank.Warrior?5:10;
                so.FindProperty("projectileCount").intValue=rank==SkeletonBossRank.Mage?14:5;so.FindProperty("recovery").floatValue=rank==SkeletonBossRank.Mage?1.9f:1.6f;
                so.FindProperty("projectilePrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Projectiles/Enemies/"+(rank==SkeletonBossRank.Mage?"Candle":"Wasp")+" Projectile.prefab");
                var origin=new GameObject("Projectile Origin");origin.transform.SetParent(root.transform,false);origin.transform.localPosition=new Vector3(0,1.1f,.8f);so.FindProperty("projectileOrigin").objectReferenceValue=origin.transform;
                var markers=new GameObject("Attack Telegraphs");markers.transform.SetParent(root.transform,false);
                so.FindProperty("circleMarker").objectReferenceValue=Marker(markers.transform,"Ground Shock",360,48);
                so.FindProperty("coneMarker").objectReferenceValue=Marker(markers.transform,"Cleave or Fan",rank==SkeletonBossRank.Rogue?55:100,24);
                so.FindProperty("lineMarker").objectReferenceValue=Line(markers.transform);
                var runes=so.FindProperty("runeMarkers");runes.arraySize=3;for(int j=0;j<3;j++)runes.GetArrayElementAtIndex(j).objectReferenceValue=Marker(markers.transform,"Rune "+(j+1),360,48);
                so.ApplyModifiedPropertiesWithoutUndo();Object.DestroyImmediate(oldVisual);
                var detector=root.GetComponentInChildren<PlayerDetector>();if(detector!=null)Object.DestroyImmediate(detector.gameObject);
                var agent=root.GetComponent<NavMeshAgent>();agent.speed=rank==SkeletonBossRank.Warrior?3.2f:rank==SkeletonBossRank.Rogue?4:2.6f;agent.radius=.55f;agent.height=2.7f;
                var col=root.GetComponent<CapsuleCollider>();col.radius=.7f;col.height=2.8f;col.center=new Vector3(0,1.4f,0);
                return PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        static void Float(SerializedProperty p,float v){p.FindPropertyRelative("propertyType").intValue=0;p.FindPropertyRelative("value").floatValue=v;}
        static void Weapon(Transform hand,string name,Vector3 rotation)
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Heroes/Boss/Weapons/"+name+".fbx");var go=(GameObject)PrefabUtility.InstantiatePrefab(model,hand);go.transform.localRotation=Quaternion.Euler(rotation);
            var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);go.transform.localScale*=name.Contains("Staff")?1.6f/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z):1f/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
        }
        static Transform Marker(Transform parent,string name,float degrees,int segments)
        {
            string path=Anim+"Warning "+degrees+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh{name="Warning "+degrees};var vertices=new Vector3[segments+2];var tri=new int[segments*3];for(int j=0;j<=segments;j++){float angle=(-degrees*.5f+degrees*j/segments)*Mathf.Deg2Rad;vertices[j+1]=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));}for(int j=0;j<segments;j++){tri[j*3]=0;tri[j*3+1]=j+1;tri[j*3+2]=j+2;}mesh.vertices=vertices;mesh.triangles=tri;mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);}
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=warning;go.SetActive(false);return go.transform;
        }
        static Transform Line(Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Charge Lane";go.transform.SetParent(parent,false);Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=warning;go.transform.localScale=new Vector3(1,.015f,1);go.SetActive(false);return go.transform;
        }
        static GameObject Arena()
        {
            string path=Root+"Prefabs/Prop/Halloween/Stage 5 Bone Courtyard.prefab";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab!=null)return prefab;
            var root=PrefabUtility.LoadPrefabContents(Root+"Prefabs/Prop/Temple/Temple Env.prefab");try
            {
                root.name="Stage 5 Bone Courtyard";var prop=root.transform.Find("Prop");var env=prop.Find("Environment");env.gameObject.SetActive(false);
                var floor=prop.Find("Play Field");var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Environment/Halloween Crypt Stone.mat");if(mat!=null)foreach(var r in floor.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mat).ToArray();
                var decor=new GameObject("Bone Courtyard Decor");decor.transform.SetParent(root.transform,false);
                for(int i=0;i<10;i++){float z=-10+i*2.2f;Decor(decor.transform,"gravestone",new Vector3(-8.5f,0,z),new Vector3(0,90,0));Decor(decor.transform,"coffin_decorated",new Vector3(8.5f,0,z),new Vector3(0,-90,0));}
                Decor(decor.transform,"crypt",new Vector3(0,0,16),Vector3.zero);Decor(decor.transform,"fence_gate",new Vector3(0,0,-15),Vector3.zero);
                return PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        static void Decor(Transform parent,string name,Vector3 pos,Vector3 rot){var p=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Prop/Halloween/"+name+".prefab");if(p==null)return;var go=(GameObject)PrefabUtility.InstantiatePrefab(p,parent);go.transform.localPosition=pos;go.transform.localRotation=Quaternion.Euler(rot);}
        static void BuildStage()
        {
            var stage=AssetDatabase.LoadAssetAtPath<StageData>(Content);var so=new SerializedObject(stage);var rooms=so.FindProperty("rooms");if(rooms.arraySize!=1||stage.Rooms[0].Waves.Length!=33)throw new InvalidOperationException("Stage 5 must have one room / 33 waves before installing.");
            var room=rooms.GetArrayElementAtIndex(0);var prefabs=room.FindPropertyRelative("prefabs");prefabs.arraySize=1;prefabs.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue=Arena();
            var waves=room.FindPropertyRelative("waves");var milestones=new[]{7,15,23,32};
            // Read existing boss overrides first. Preserve authored chest rates in the new sequence.
            var oldBosses=new List<EnemySpawnData>();for(int i=0;i<waves.arraySize;i++){var enemies=waves.GetArrayElementAtIndex(i).FindPropertyRelative("enemySpawns");for(int j=0;j<enemies.arraySize;j++){var e=enemies.GetArrayElementAtIndex(j);if(e.FindPropertyRelative("enemyType").intValue>=100)oldBosses.Add(new EnemySpawnData(e));}}
            bool installed=oldBosses.Any(e=>(int)e.EnemyType>=106);
            if(!installed)
            {
                for(int i=0;i<waves.arraySize;i++){var wave=waves.GetArrayElementAtIndex(i);wave.FindPropertyRelative("groupId").intValue=i+1;var enemies=wave.FindPropertyRelative("enemySpawns");for(int j=enemies.arraySize-1;j>=0;j--)if(enemies.GetArrayElementAtIndex(j).FindPropertyRelative("enemyType").intValue>=100)enemies.DeleteArrayElementAtIndex(j);if(enemies.arraySize==0&&!milestones.Contains(i)){enemies.arraySize=1;enemies.GetArrayElementAtIndex(0).FindPropertyRelative("enemyType").intValue=(int)EnemyType.Candle;}}
                for(int i=0;i<4;i++){var enemies=waves.GetArrayElementAtIndex(milestones[i]).FindPropertyRelative("enemySpawns");enemies.arraySize=1;if(oldBosses.Count>0)oldBosses[Mathf.Min(i,oldBosses.Count-1)].SaveToSerializedProperty(enemies.GetArrayElementAtIndex(0));var e=enemies.GetArrayElementAtIndex(0);e.FindPropertyRelative("enemyType").intValue=106+i;e.FindPropertyRelative("transformData").FindPropertyRelative("position").vector3Value=new Vector3(0,0,8);}
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var stageDb=AssetDatabase.FindAssets("t:StageDatabase").Select(g=>AssetDatabase.LoadAssetAtPath<StageDatabase>(AssetDatabase.GUIDToAssetPath(g))).First();var dbSo=new SerializedObject(stageDb);var stages=dbSo.FindProperty("stages");bool has=false;for(int i=0;i<stages.arraySize;i++)if(stages.GetArrayElementAtIndex(i).objectReferenceValue==stage)has=true;if(!has){stages.InsertArrayElementAtIndex(stages.arraySize);stages.GetArrayElementAtIndex(stages.arraySize-1).objectReferenceValue=stage;dbSo.ApplyModifiedPropertiesWithoutUndo();}
        }
    }
}
