using System;
using System.Linq;
using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static partial class RepMenuAuthoring
    {
        // Cuts are beyond the complete corner ornaments, not a fraction of the shortest side.
        static Vector4 SurfaceBorder(string name, UnityEngine.Rect rect)
        {
            float x=.16f,y=.46f;
            if(name.Contains("CardFrame07")) {x=.23f;y=.34f;}
            else if(name.Contains("StageFrame")) {x=.27f;y=.44f;}
            else if(name.Contains("Banner")) {x=.24f;y=.44f;}
            else if(name.Contains("Frame")) {x=.16f;y=.22f;}
            else if(name.Contains("Square")) {x=.28f;y=.28f;}
            return new Vector4(rect.width*x,rect.height*y,rect.width*x,rect.height*y);
        }
        static void SurfaceDensity(Image image, ResponsiveMenuLayout profile)
        {
            if(image.sprite==null||image.type!=UnityEngine.UI.Image.Type.Sliced)return;
            string path=AssetDatabase.GetAssetPath(image.sprite);
            if(!path.StartsWith(Rep,StringComparison.Ordinal)||image.sprite.border==Vector4.zero)return;
            float l=3,p=6;
            if(path.Contains("/Frame/")){l=8;p=12;}
            if(image.name=="Rep Scrollbar"||image.GetComponent<Scrollbar>()!=null||image.GetComponentInParent<Scrollbar>()!=null)
            {
                image.sprite=S("Frame/CardFrame04_White1");
                image.color=image.name=="Handle"?new Color(.15f,.65f,1):new Color(.055f,.16f,.31f);
                l=12;p=16;
            }
            image.pixelsPerUnitMultiplier=l;
            if(profile!=null)
            {
                var style=profile.images.Find(v=>v.target==image);
                if(style==null){style=new ResponsiveMenuLayout.ImageStyle{target=image};profile.images.Add(style);}
                style.landscape=l;style.portrait=p;
            }
        }
        static void RepairSurfaceTree(GameObject go)
        {
            foreach(var image in go.GetComponentsInChildren<Image>(true))
                SurfaceDensity(image,image.GetComponentInParent<ResponsiveMenuLayout>(true));
            foreach(var dock in go.GetComponentsInChildren<MainMenuDockBehavior>(true))
            {
                var bg=dock.GetComponent<Image>();
                if(bg!=null){bg.sprite=S("Frame/CardFrame04_White1");bg.color=new Color(.035f,.10f,.23f);bg.type=UnityEngine.UI.Image.Type.Sliced;SurfaceDensity(bg,dock.GetComponent<ResponsiveMenuLayout>());}
                foreach(var item in dock.GetComponentsInChildren<MainMenuDockItemBehavior>(true))
                {
                    var back=item.transform.Find("Selected Backdrop");if(back==null)continue;
                    var im=back.GetComponent<Image>();im.sprite=S("Frame/CardFrame04_White1");im.color=new Color(.08f,.43f,.78f);im.type=UnityEngine.UI.Image.Type.Sliced;SurfaceDensity(im,dock.GetComponent<ResponsiveMenuLayout>());
                }
            }
        }
        [MenuItem("Tools/MrX Studio/UI/Repair Rep Sprite Scaling")]
        public static void RepairSpriteScaling()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            // Preserve rect, name, spriteID and fileID. Only update imported surface borders/quality.
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Rep}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                if(sprites.Length==0||sprites.All(s=>s.border==Vector4.zero))continue;
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
#pragma warning disable 618
                var rects=importer.spritesheet;
                for(int i=0;i<rects.Length;i++){var r=rects[i];r.border=SurfaceBorder(path,r.rect);rects[i]=r;}
                importer.spritesheet=rects;
#pragma warning restore 618
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=2048;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{Root+"Prefabs/UI"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(!asset.GetComponentsInChildren<Image>(true).Any(i=>i.sprite!=null&&AssetDatabase.GetAssetPath(i.sprite).StartsWith(Rep,StringComparison.Ordinal)))continue;
                var go=PrefabUtility.LoadPrefabContents(path);
                try{RepairSurfaceTree(go);PrefabUtility.SaveAsPrefabAsset(go,path);}finally{PrefabUtility.UnloadPrefabContents(go);}
            }
            var scene=EditorSceneManager.GetActiveScene();
            foreach(var go in scene.GetRootGameObjects())
            {
                RepairSurfaceTree(go);
                foreach(var image in go.GetComponentsInChildren<Image>(true))
                {if(PrefabUtility.IsPartOfPrefabInstance(image))PrefabUtility.RecordPrefabInstancePropertyModifications(image);}
                foreach(var profile in go.GetComponentsInChildren<ResponsiveMenuLayout>(true))
                {EditorUtility.SetDirty(profile);if(PrefabUtility.IsPartOfPrefabInstance(profile))PrefabUtility.RecordPrefabInstancePropertyModifications(profile);profile.Apply(false);}
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
