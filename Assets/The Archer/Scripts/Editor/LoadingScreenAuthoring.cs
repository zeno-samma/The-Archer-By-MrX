using System;
using System.Linq;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.EditorTools
{
 public static class LoadingScreenAuthoring
 {
  const string Root="Assets/The Archer/";
  public const string PrefabPath=Root+"Prefabs/UI/Loading/Loading Screen.prefab";
  static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 size,Vector2 pos)
  {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.sizeDelta=size;r.anchoredPosition=pos;return r;}
  static Sprite Import(string path,bool sliced)
  {
   var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=2048;t.spritePixelsPerUnit=100;
   t.GetSourceTextureWidthAndHeight(out int w,out int h);t.spriteBorder=sliced?new Vector4(w*.15f,h*.45f,w*.15f,h*.45f):Vector4.zero;t.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
  }
  static Image Image(RectTransform rect,Sprite sprite,Color color,bool slice=false)
  {var im=rect.gameObject.AddComponent<Image>();im.sprite=sprite;im.color=color;im.raycastTarget=false;im.type=slice?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;if(slice)im.pixelsPerUnitMultiplier=6;return im;}
  static TMP_Text Label(string name,Transform parent,string text,float y,float size,TMP_FontAsset font)
  {var r=Rect(name,parent,new Vector2(0,.12f),new Vector2(1,.12f),new Vector2(-40,70),new Vector2(0,y));var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.overflowMode=TextOverflowModes.Truncate;t.enableAutoSizing=true;t.fontSizeMin=24;t.fontSizeMax=size;t.color=Color.white;var shadow=r.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,.04f,.1f,.9f);shadow.effectDistance=new Vector2(2,-3);return t;}
  [MenuItem("Tools/MrX Studio/UI/Install Loading Screen")]
  public static void Install()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   var bg=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/UI/Main Menu/ui_background_main.png");
   var cursor=Import(Root+"Sprites/UI/Rep/Icon_ItemIcons/Original/Icon_Booster.png",false);
   var track=Import(Root+"Sprites/UI/Rep/Slider/Slider_Basic02_Bg.png",true);
   var fill=Import(Root+"Sprites/UI/Rep/Slider/Slider_Basic02_Fill_Yellow.png",true);
   var table=Resources.Load<MenuTranslations>("Menu Translations");
   if(!table.entries.Any(e=>e.english=="Loading data..."))table.entries.Add(new TranslationEntry{english="Loading data...",vietnamese="Đang tải dữ liệu...",spanish="Cargando datos...",portuguese="Carregando dados...",french="Chargement des données..."});table.Rebuild();EditorUtility.SetDirty(table);
   var go=new GameObject("Loading Screen",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
   try {
    var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
    var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
    var back=Rect("Background",go.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);Image(back,bg,Color.white);var aspect=back.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=bg.rect.width/bg.rect.height;
    Image(Rect("Bottom Shade",go.transform,Vector2.zero,new Vector2(1,.30f),Vector2.zero,Vector2.zero),null,new Color(.015f,.045f,.12f,.75f));
    var safe=Rect("Safe Content",go.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
    var bar=Rect("Progress Bar",safe,new Vector2(.1f,.12f),new Vector2(.9f,.12f),new Vector2(0,52),Vector2.zero);Image(bar,track,Color.white,true);
    var slider=bar.gameObject.AddComponent<Slider>();slider.interactable=false;slider.navigation=new Navigation{mode=Navigation.Mode.None};slider.transition=Selectable.Transition.None;
    var area=Rect("Fill Area",bar,Vector2.zero,Vector2.one,new Vector2(-14,-14),Vector2.zero);
    var fr=Rect("Fill",area,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);Image(fr,fill,Color.white,true);slider.fillRect=fr;
    var cr=Rect("Progress Cursor",area,new Vector2(.68f,.5f),new Vector2(.68f,.5f),new Vector2(112,112),Vector2.zero);Image(cr,cursor,Color.white).preserveAspect=true;
    var status=Label("Loading Status",safe,"Loading data...",78,44,table.latinFont);
    var percent=Label("Percentage",safe,"68%",-66,42,table.latinFont);
    var presenter=go.AddComponent<LoadingScreenPresenter>();var so=new SerializedObject(presenter);so.FindProperty("progressBar").objectReferenceValue=slider;so.FindProperty("progressCursor").objectReferenceValue=cr;so.FindProperty("statusText").objectReferenceValue=status;so.FindProperty("percentageText").objectReferenceValue=percent;so.FindProperty("safeContent").objectReferenceValue=safe;so.ApplyModifiedPropertiesWithoutUndo();presenter.ShowProgress(.68f);
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PrefabPath));AssetDatabase.Refresh();PrefabUtility.SaveAsPrefabAsset(go,PrefabPath);
   } finally {UnityEngine.Object.DestroyImmediate(go);}
   var active=EditorSceneManager.GetActiveScene();string path=Root+"Scenes/Loading Screen.unity";var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   foreach(var root in scene.GetRootGameObjects())if(root.GetComponent<Canvas>()!=null)UnityEngine.Object.DestroyImmediate(root);
   PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(opened)EditorSceneManager.CloseScene(scene,true);if(active.IsValid())EditorSceneManager.SetActiveScene(active);AssetDatabase.SaveAssets();Debug.Log("Loading Screen prefab and scene installed.");
  }
 }
}
