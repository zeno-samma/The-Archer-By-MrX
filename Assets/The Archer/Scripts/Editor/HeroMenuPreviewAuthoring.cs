using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MrX.Armory;
using MrX.UI;
using MrX.Weapon;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace MrX.EditorTools
{
    [CustomEditor(typeof(HeroPreviewBehavior),true)]
    public sealed class HeroPreviewBehaviorEditor:Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Editor Hero/Weapon chỉ dùng xem trước, không đổi save. Chỉnh RectTransform của Hero Raw Image trực tiếp; khung hình không còn bị AspectRatioFitter khóa chiều rộng. Chỉnh camera trong Hero Preview Prefab rồi Refresh để xem góc mới.",MessageType.Info);
            using(new EditorGUI.DisabledScope(Application.isPlaying))
                if(GUILayout.Button("Refresh Editor Preview"))HeroMenuPreviewAuthoring.Refresh((HeroPreviewBehavior)target);
            if(GUILayout.Button("Open Preview Camera Prefab"))
            {
                var rig=serializedObject.FindProperty("heroPreviewPrefab").objectReferenceValue;
                if(rig!=null)AssetDatabase.OpenAsset(rig);
            }
        }
    }
    public static class HeroMenuPreviewAuthoring
    {
        const string Root="Assets/The Archer/";
        [MenuItem("Tools/MrX Studio/UI/Refresh Menu Editor Previews")]
        public static void RefreshScene()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode before editing previews.");
            foreach(var p in Object.FindObjectsByType<HeroPreviewBehavior>(FindObjectsInactive.Include))Refresh(p);
        }
        [MenuItem("CONTEXT/HeroPreviewBehavior/Refresh Editor Preview")]
        static void RefreshContext(MenuCommand command){Refresh((HeroPreviewBehavior)command.context);}
        public static void Refresh(HeroPreviewBehavior view)
        {
            if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode before editing previews.");
            var so=new SerializedObject(view);
            var db=AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(Root+"Scriptables/Armory/Armory Database.asset");
            var hero=so.FindProperty("editorHero").objectReferenceValue as HeroData ?? db.GetDefaultHero();
            var weapon=so.FindProperty("editorWeapon").objectReferenceValue as WeaponData ?? hero.StarterWeapon ?? db.Items.OfType<WeaponData>().FirstOrDefault(w=>w.IsDefaultItem&&w.Supports(hero));
            if(weapon==null||!weapon.Supports(hero))throw new InvalidOperationException("Choose a preview weapon compatible with this hero.");
            var raw=(RawImage)so.FindProperty("rawImage").objectReferenceValue;
            var rig=(GameObject)so.FindProperty("heroPreviewPrefab").objectReferenceValue;
            if(raw==null||rig==null)throw new InvalidOperationException("Preview RawImage and rig prefab are required.");
            int h=Mathf.Clamp(so.FindProperty("renderHeight").intValue,256,1024);
            int w=Mathf.Clamp(Mathf.RoundToInt(h*raw.rectTransform.rect.width/Mathf.Max(1,raw.rectTransform.rect.height)),64,2048);
            string folder=Root+"Sprites/UI/Editor Previews";Directory.CreateDirectory(folder);
            var owner=view.GetComponentInParent<MainMenuPageBehavior>();
            string previewName=(owner!=null?owner.name:view.name)+"-"+hero.name+"-"+weapon.name+"-"+w+"x"+h;
            foreach(char invalid in Path.GetInvalidFileNameChars())previewName=previewName.Replace(invalid,'_');
            string path=folder+"/"+previewName+".png";
            var preview=new PreviewRenderUtility();Texture2D png=null;AnimatorOverrideController overrides=null;
            try
            {
                var instance=preview.InstantiatePrefabInScene(rig);instance.transform.position=Vector3.zero;instance.transform.rotation=Quaternion.Euler(so.FindProperty("previewRotation").vector3Value);
                var model=preview.InstantiatePrefabInScene(hero.Prefab);model.transform.SetParent(instance.transform,false);
                var behavior=model.GetComponent<MrX.Player.HeroBehavior>();var hb=new SerializedObject(behavior);var animator=model.GetComponentInChildren<Animator>(true);
                var set=db.GetWeaponAnimationsSet(hero.Id,weapon.Id);
                var controller=set.WeaponAnimationsSetType==WeaponAnimationsSetType.Animator?set.RuntimeAnimatorController:animator.runtimeAnimatorController;
                if(set.WeaponAnimationsSetType==WeaponAnimationsSetType.Separate_Animations)
                {
                    overrides=new AnimatorOverrideController(controller);var clips=new List<KeyValuePair<AnimationClip,AnimationClip>>();overrides.GetOverrides(clips);
                    for(int i=0;i<clips.Count;i++){var replacement=set.GetAnimation(clips[i].Key.name);if(replacement!=null)clips[i]=new KeyValuePair<AnimationClip,AnimationClip>(clips[i].Key,replacement.AnimationClip);}overrides.ApplyOverrides(clips);controller=overrides;
                }
                animator.runtimeAnimatorController=controller;animator.fireEvents=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind();animator.Update(0);
                var idle=controller.animationClips.FirstOrDefault(c=>c.name.IndexOf("Idle",StringComparison.OrdinalIgnoreCase)>=0);
                if(idle!=null)idle.SampleAnimation(animator.gameObject,0);
                var hand=(Transform)hb.FindProperty(weapon.IsRightHandWeapon?"rightWeaponParent":"leftWeaponParent").objectReferenceValue;
                var weaponModel=preview.InstantiatePrefabInScene(weapon.WeaponPrefab);weaponModel.transform.SetParent(hand,false);
                if(weapon.OffhandVisualPrefab!=null){var off=preview.InstantiatePrefabInScene(weapon.OffhandVisualPrefab);off.transform.SetParent((Transform)hb.FindProperty(weapon.IsRightHandWeapon?"leftWeaponParent":"rightWeaponParent").objectReferenceValue,false);}
                foreach(var ps in instance.GetComponentsInChildren<ParticleSystem>(true))ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var sourceCamera=instance.GetComponentInChildren<Camera>(true);sourceCamera.enabled=false;
                var camera=preview.camera;camera.orthographic=sourceCamera.orthographic;camera.orthographicSize=sourceCamera.orthographicSize;camera.fieldOfView=sourceCamera.fieldOfView;camera.nearClipPlane=sourceCamera.nearClipPlane;camera.farClipPlane=sourceCamera.farClipPlane;camera.transform.SetPositionAndRotation(sourceCamera.transform.position,sourceCamera.transform.rotation);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.aspect=(float)w/h;
                preview.lights[0].intensity=so.FindProperty("editorKeyLightIntensity").floatValue;
                preview.lights[0].transform.rotation=Quaternion.Euler(so.FindProperty("editorKeyLightRotation").vector3Value);
                preview.lights[1].intensity=so.FindProperty("editorFillLightIntensity").floatValue;
                preview.lights[1].transform.rotation=Quaternion.Euler(so.FindProperty("editorFillLightRotation").vector3Value);
                preview.ambientColor=so.FindProperty("editorAmbientColor").colorValue;
                // EndStaticPreview returns an opaque RGB thumbnail in Unity 6. Read the
                // preview target as RGBA instead so the menu background stays visible.
                preview.BeginStaticPreview(new UnityEngine.Rect(0,0,w,h));
                preview.Render(true);
                var target=preview.camera.targetTexture;
                var previous=RenderTexture.active;
                // The Editor can render at a higher DPI than the requested logical size.
                try { RenderTexture.active=target;png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);png.ReadPixels(new UnityEngine.Rect(0,0,target.width,target.height),0,0);png.Apply(); }
                finally { RenderTexture.active=previous; }
                Object.DestroyImmediate(preview.EndStaticPreview());
                File.WriteAllBytes(path,png.EncodeToPNG());
            }
            finally{if(png!=null)Object.DestroyImmediate(png);if(overrides!=null)Object.DestroyImmediate(overrides);preview.Cleanup();}
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            Undo.RecordObject(raw,"Refresh hero editor preview");raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);raw.uvRect=new UnityEngine.Rect(0,0,1,1);Changed(raw);
            var page=view.GetComponentInParent<MainMenuPageBehavior>();if(page!=null)
            {
                // Bind representative level-one stats without starting managers or touching saves.
                foreach(var panel in page.GetComponentsInChildren<MrX.UI.Armory.ArmoryHeroPanel>(true))
                {
                    var p=new SerializedObject(panel);
                    foreach(var stat in new[]{StatType.Damage,StatType.HP})
                    {
                        float amount=hero.GetHeroLevel(0).GetStatValue(stat);
                        foreach(var item in db.Items.Where(i=>i.ItemType!=ItemType.Weapon&&i.IsDefaultItem))amount+=item.GetItemLevel(0).GetStatValue(stat);
                        amount+=weapon.GetItemLevel(0).GetStatValue(stat);
                        var indicator=p.FindProperty(stat==StatType.Damage?"damageLabel":"hpLabel").objectReferenceValue;
                        if(indicator==null)continue;
                        var label=(TMP_Text)new SerializedObject(indicator).FindProperty("label").objectReferenceValue;
                        Undo.RecordObject(label,"Preview hero stats");label.text=((int)amount).ToString();Changed(label);
                    }
                }
                foreach(var bar in page.GetComponentsInChildren<HeroExperienceBar>(true))
                {
                    var b=new SerializedObject(bar);var title=(TMP_Text)b.FindProperty("heroName").objectReferenceValue;var exp=(TMP_Text)b.FindProperty("experience").objectReferenceValue;var portrait=(Image)b.FindProperty("portrait").objectReferenceValue;var fill=(Image)b.FindProperty("fill").objectReferenceValue;
                    Undo.RecordObjects(new Object[]{title,exp,portrait,portrait.rectTransform},"Preview hero profile");title.text=hero.Name+" · Lv.1";exp.text="0 / "+Resources.Load<RunProgressionSettings>(RunProgressionSettings.ResourcePath).RequiredExperience(hero,0)+" EXP";portrait.sprite=hero.Icon;portrait.enabled=hero.Icon!=null;
                    if(b.FindProperty("useFaceCrop").boolValue&&hero.Icon!=null){var bounds=b.FindProperty(hero.Icon.rect.height>hero.Icon.rect.width*1.5f?"tallPortraitBounds":"squarePortraitBounds").vector4Value;portrait.rectTransform.anchorMin=new Vector2(bounds.x,bounds.y);portrait.rectTransform.anchorMax=new Vector2(bounds.z,bounds.w);portrait.rectTransform.offsetMin=portrait.rectTransform.offsetMax=Vector2.zero;}
                    Changed(title);Changed(exp);Changed(portrait);Changed(portrait.rectTransform);
                    if(fill!=null){Undo.RecordObject(fill,"Preview initial experience");fill.fillAmount=0;Changed(fill);}
                }
                var stage=page as MainMenuStagesPageBehavior;if(stage!=null)
                {
                    var selector=stage.GetComponentInChildren<MainMenuStageSelector>(true);var st=new SerializedObject(selector);var name=(TMP_Text)st.FindProperty("stageNameText").objectReferenceValue;var number=(TMP_Text)st.FindProperty("stageNumberText").objectReferenceValue;Undo.RecordObjects(new Object[]{name,number},"Preview stage labels");name.text=stage.StageDatabase.GetStageData(0).StageName;number.text="Stage 1";Changed(name);Changed(number);
                }
            }
            SceneView.RepaintAll();
        }
        static void Changed(Object o){EditorUtility.SetDirty(o);if(PrefabUtility.IsPartOfPrefabInstance(o))PrefabUtility.RecordPrefabInstancePropertyModifications(o);}
    }
}
