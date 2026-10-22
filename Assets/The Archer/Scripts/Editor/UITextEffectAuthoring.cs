using System;
using System.IO;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class UITextEffectAuthoring
    {
        public const string DemoPath = "Assets/The Archer/Prefabs/UI/Text Effects/UI Text Effects Demo.prefab";
        [MenuItem("Tools/MrX Studio/UI/Create Text Effects Demo Prefab")]
        public static void CreateDemo()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play first.");
            UITextEffectPreview.Stop();
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(DemoPath);
            if (existing != null) { Selection.activeObject = existing; return; }
            Directory.CreateDirectory(Path.GetDirectoryName(DemoPath)); AssetDatabase.Refresh();
            var translations = Resources.Load<MenuTranslations>("Menu Translations");
            var art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var root = new GameObject("UI Text Effects Demo", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.layer = 5;
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
                var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
                var panel = Rect(root.transform, "Demo Board", .02f,.025f,.98f,.975f);
                var background = panel.gameObject.AddComponent<Image>(); background.sprite = art.slotSprite; background.material = art.slotMaterial;
                background.type = Image.Type.Sliced; background.color = new Color(.86f,.94f,1); background.raycastTarget = false;
                Label(panel, "Heading", "MrX Studio · UI TEXT EFFECTS", translations.latinFont, 40,.03f,.925f,.97f,.99f);
                string[] titles = { "COUNT · ĐẾM SỐ", "BOUNCE · NẢY", "PULSE · NHỊP ĐẬP", "SHAKE · RUNG", "FADE · HIỆN DẦN", "TYPEWRITER · ĐÁNH MÁY", "WAVE · SÓNG", "RAINBOW · CẦU VỒNG", "POP · BẬT LÊN", "SLIDE · TRƯỢT", "FLOAT · LƠ LỬNG", "COLOR PULSE · ĐỔI MÀU" };
                for (int i = 0; i < titles.Length; i++)
                {
                    float left = .02f + i % 3 * .325f, bottom = .69f - i / 3 * .222f;
                    var tile = Rect(panel, ((UITextEffectStyle)i).ToString(), left,bottom,left+.31f,bottom+.205f);
                    var image = tile.gameObject.AddComponent<Image>(); image.sprite = art.slotSprite; image.material = art.slotMaterial;
                    image.type = Image.Type.Sliced; image.color = new Color(.96f,.99f,1); image.raycastTarget = false;
                    Label(tile,"Effect Name",titles[i],translations.latinFont,24,.03f,.79f,.97f,.96f);
                    var text = Label(tile,"Animated Text", i == 0 ? "12,345" : i == 5 ? "<b>MrX</b> xin chào!" : "MrX Studio",translations.latinFont,50,.04f,.22f,.96f,.76f);
                    var fx = text.gameObject.AddComponent<UITextEffect>(); var so = new SerializedObject(fx);
                    so.FindProperty("effect").enumValueIndex = i; so.FindProperty("duration").floatValue = 2.4f;
                    so.FindProperty("loop").boolValue = true; so.FindProperty("loopPause").floatValue = .5f;
                    if (i == 6 || i == 7) so.FindProperty("perCharacter").boolValue = true;
                    if (i == 7) so.FindProperty("frequency").floatValue = .35f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Label(tile,"Hint","Chọn Animated Text → UI Text Effect → Preview",translations.latinFont,16,.03f,.035f,.97f,.18f);
                }
                Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, DemoPath);
            }
            finally { Object.DestroyImmediate(root); }
        }
        private static RectTransform Rect(Transform parent, string name, float l,float b,float r,float t)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = 5; go.transform.SetParent(parent,false);
            var rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(l,b); rect.anchorMax = new Vector2(r,t);
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static TextMeshProUGUI Label(Transform parent,string name,string text,TMP_FontAsset font,float size,float l,float b,float r,float t)
        {
            var label = Rect(parent,name,l,b,r,t).gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text; label.font = font; label.fontSize = label.fontSizeMax = size; label.fontSizeMin = size*.6f;
            label.enableAutoSizing = true; label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.12f,.26f,.56f); label.raycastTarget = false; return label;
        }
    }
}
