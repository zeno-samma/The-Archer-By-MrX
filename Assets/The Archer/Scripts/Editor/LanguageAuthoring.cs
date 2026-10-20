using System.Collections.Generic;
using System.Linq;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class LanguageAuthoring
    {
        private static ForgePageDatabase art;
        private const string Data="Assets/The Archer/Resources/Menu Translations.asset";
        public const string Picker="Assets/The Archer/Prefabs/UI/Stage/Language Picker.prefab";
        public static void Install()
        {
            if(Application.isPlaying)throw new System.Exception("Edit Mode required");
            art=Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var table=AssetDatabase.LoadAssetAtPath<MenuTranslations>(Data);
            if(!table){table=ScriptableObject.CreateInstance<MenuTranslations>();AssetDatabase.CreateAsset(table,Data);Seed(table);}
            if(!table.latinFont)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
                var font=TMP_FontAsset.CreateFontAsset(source,64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic);
                AssetDatabase.CreateAsset(font,"Assets/The Archer/Resources/Menu Latin Font.asset");
                var chars=string.Concat(table.entries.SelectMany(e=>new[]{e.english,e.vietnamese,e.spanish,e.portuguese,e.french}))+string.Concat(MenuLanguage.Names)+"LANGUAGE · EN VI ES PT FR0123456789";
                if(!font.TryAddCharacters(chars,out string missing))throw new System.Exception("Missing Latin glyphs: "+missing);
                font.atlasPopulationMode=AtlasPopulationMode.Static;
                foreach(var texture in font.atlasTextures)AssetDatabase.AddObjectToAsset(texture,font);AssetDatabase.AddObjectToAsset(font.material,font);EditorUtility.SetDirty(font);table.latinFont=font;
            }
            EditorUtility.SetDirty(table);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Picker);
            if(!prefab)
            {
                var root=Rect(null,"Language Picker",0,0,1,1);var ui=root.gameObject.AddComponent<LanguagePicker>();
                var open=Button(root,"Choose Language","LANGUAGE · EN",.04f,.89f,.20f,.96f,table.latinFont);
                var shade=Rect(root,"Language Modal",0,0,1,1);var c=shade.gameObject.AddComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=210;shade.gameObject.AddComponent<GraphicRaycaster>();shade.gameObject.AddComponent<Image>().color=new Color(.025f,.065f,.16f,.8f);
                var body=Panel(shade,"Dialog",.30f,.16f,.70f,.85f);Text(body,"Title","LANGUAGE",.06f,.84f,.84f,.97f,table.latinFont,64);
                var close=Button(body,"Close","X",.86f,.86f,.97f,.96f,table.latinFont);
                var buttons=new List<Button>();
                for(int i=0;i<5;i++)buttons.Add(Button(body,MenuLanguage.Codes[i],MenuLanguage.Names[i],.08f,.67f-i*.13f,.92f,.78f-i*.13f,table.latinFont));
                for(int i=0;i<5;i++)buttons[i].navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=i==0?close:buttons[i-1],selectOnDown=i==4?close:buttons[i+1]};
                close.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=buttons[0],selectOnUp=buttons[4]};
                var so=new SerializedObject(ui);Set(so,"openButton",open);Set(so,"closeButton",close);Set(so,"panel",shade.gameObject);Set(so,"currentLabel",open.GetComponentInChildren<TMP_Text>());
                var choices=so.FindProperty("choices");choices.arraySize=5;for(int i=0;i<5;i++)choices.GetArrayElementAtIndex(i).objectReferenceValue=buttons[i];so.ApplyModifiedPropertiesWithoutUndo();
                shade.gameObject.SetActive(false);prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Picker);Object.DestroyImmediate(root.gameObject);
            }
            foreach(var path in new[]{"Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Upgrades Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Forge Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Daily Quests Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Achievements Page.prefab","Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if(!root.GetComponent<LocalizedMenuScope>())root.AddComponent<LocalizedMenuScope>();
                    if(path.Contains("Stage Page")&&!root.transform.Find("Language Picker")){var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root.transform);var r=(RectTransform)instance.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();
        }
        private static void Seed(MenuTranslations t)
        {
            string[] lines={
                "START|BẮT ĐẦU|JUGAR|JOGAR|JOUER","Possible Rewards|Phần thưởng có thể nhận|Recompensas posibles|Recompensas possíveis|Récompenses possibles","Stage {0}|Màn {0}|Fase {0}|Fase {0}|Niveau {0}","Attempts Made: {0}|Số lần thử: {0}|Intentos: {0}|Tentativas: {0}|Tentatives : {0}","Attempts Made {0}|Số lần thử {0}|Intentos {0}|Tentativas {0}|Tentatives {0}",
                "LUCKY WHEEL|VÒNG QUAY MAY MẮN|RULETA DE LA SUERTE|RODA DA SORTE|ROUE DE LA CHANCE","GEM CHEST|RƯƠNG ĐÁ QUÝ|COFRE DE GEMAS|BAÚ DE GEMAS|COFFRE DE GEMMES","LANGUAGE|NGÔN NGỮ|IDIOMA|IDIOMA|LANGUE","LANGUAGE · {0}|NGÔN NGỮ · {0}|IDIOMA · {0}|IDIOMA · {0}|LANGUE · {0}",
                "HOME|TRANG CHỦ|INICIO|INÍCIO|ACCUEIL","JOURNEY|HÀNH TRÌNH|AVENTURA|JORNADA|AVENTURE","ARMORY|TRANG BỊ|ARSENAL|ARSENAL|ARSENAL","HEROES|ANH HÙNG|HÉROES|HERÓIS|HÉROS","STAGE|MÀN CHƠI|FASES|FASES|NIVEAUX","UPGRADES|NÂNG CẤP|MEJORAS|MELHORIAS|AMÉLIORATIONS","UPGRADE|NÂNG CẤP|MEJORAR|MELHORAR|AMÉLIORER","FORGE|RÈN ĐÚC|FORJA|FORJA|FORGE","QUESTS|NHIỆM VỤ|MISIONES|MISSÕES|QUÊTES","ACHIEVEMENTS|THÀNH TỰU|LOGROS|CONQUISTAS|SUCCÈS","DAILY QUESTS|NHIỆM VỤ HẰNG NGÀY|MISIONES DIARIAS|MISSÕES DIÁRIAS|QUÊTES QUOTIDIENNES",
                "Gold|Vàng|Oro|Ouro|Or","Gems|Đá quý|Gemas|Gemas|Gemmes","Iron|Sắt|Hierro|Ferro|Fer","Wood|Gỗ|Madera|Madeira|Bois","Leather|Da|Cuero|Couro|Cuir","CONTINUE|TIẾP TỤC|CONTINUAR|CONTINUAR|CONTINUER","REWARD RECEIVED|ĐÃ NHẬN THƯỞNG|RECOMPENSA RECIBIDA|RECOMPENSA RECEBIDA|RÉCOMPENSE REÇUE","Reward|Phần thưởng|Recompensa|Recompensa|Récompense","Not enough gems.|Không đủ đá quý.|No hay gemas suficientes.|Gemas insuficientes.|Gemmes insuffisantes.",
                "{0} GEM\nSPIN|{0} ĐÁ QUÝ\nQUAY|{0} GEMAS\nGIRAR|{0} GEMAS\nGIRAR|{0} GEMMES\nTOURNER","{0} GEM\nOPEN CHEST|{0} ĐÁ QUÝ\nMỞ RƯƠNG|{0} GEMAS\nABRIR COFRE|{0} GEMAS\nABRIR BAÚ|{0} GEMMES\nOUVRIR","One reward per purchase. Chances shown on the right.|Mỗi lượt nhận một phần thưởng. Tỉ lệ ở bên phải.|Una recompensa por compra. Probabilidades a la derecha.|Uma recompensa por compra. Chances à direita.|Une récompense par achat. Chances à droite.","Revealing your reward...|Đang mở phần thưởng...|Revelando recompensa...|Revelando recompensa...|Révélation en cours...",
                "Secret Chambers|Mật thất|Cámaras secretas|Câmaras secretas|Chambres secrètes","Ancient Temple|Đền cổ|Templo antiguo|Templo antigo|Temple antique","Secrets Pyramid|Kim tự tháp bí ẩn|Pirámide secreta|Pirâmide secreta|Pyramide secrète","Forgotten Tombs|Lăng mộ lãng quên|Tumbas olvidadas|Túmulos esquecidos|Tombeaux oubliés","Ranger  Lv.{0}|Cung thủ  Cấp {0}|Explorador  Nv.{0}|Patrulheiro  Nv.{0}|Rôdeur  Niv.{0}",
                "STATS|CHỈ SỐ|ESTADÍSTICAS|ATRIBUTOS|STATISTIQUES","INVENTORY|TÚI ĐỒ|INVENTARIO|INVENTÁRIO|INVENTAIRE","DETAILS  >|CHI TIẾT  >|DETALLES  >|DETALHES  >|DÉTAILS  >","ALL|TẤT CẢ|TODO|TODOS|TOUT","SWITCH HERO|ĐỔI ANH HÙNG|CAMBIAR HÉROE|TROCAR HERÓI|CHANGER DE HÉROS","EQUIPMENT FORGE|RÈN TRANG BỊ|FORJA DE EQUIPO|FORJA DE EQUIPAMENTO|FORGE D'ÉQUIPEMENT","MATERIALS|NGUYÊN LIỆU|MATERIALES|MATERIAIS|MATÉRIAUX","CLAIM|NHẬN|RECLAMAR|RESGATAR|RÉCLAMER","CLAIMED|ĐÃ NHẬN|RECIBIDO|RECEBIDO|REÇU","GO|ĐI ĐẾN|IR|IR|ALLER","OWNED|ĐÃ SỞ HỮU|EN PROPIEDAD|ADQUIRIDO|POSSÉDÉ"
            };
            foreach(var line in lines){var p=line.Split('|');t.entries.Add(new TranslationEntry{english=p[0],vietnamese=p[1],spanish=p[2],portuguese=p[3],french=p[4]});}
        }
        private static void Set(SerializedObject so,string name,Object v){so.FindProperty(name).objectReferenceValue=v;}
        private static RectTransform Rect(Transform parent,string name,float x,float y,float r,float t){var g=new GameObject(name,typeof(RectTransform));g.layer=5;g.transform.SetParent(parent,false);var rect=(RectTransform)g.transform;rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(r,t);rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;}
        private static RectTransform Panel(Transform parent,string name,float x,float y,float r,float t){var rect=Rect(parent,name,x,y,r,t);var i=rect.gameObject.AddComponent<Image>();i.sprite=art.slotSprite;i.material=art.slotMaterial;i.type=Image.Type.Sliced;i.color=new Color(.90f,.96f,1);return rect;}
        private static TMP_Text Text(Transform parent,string name,string text,float x,float y,float r,float t,TMP_FontAsset font,float size){var rect=Rect(parent,name,x,y,r,t);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.fontSize=label.fontSizeMax=size;label.fontSizeMin=size*.55f;label.enableAutoSizing=true;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.12f,.23f,.56f);label.raycastTarget=false;return label;}
        private static Button Button(Transform parent,string name,string text,float x,float y,float r,float t,TMP_FontAsset font){var rect=Panel(parent,name,x,y,r,t);var b=rect.gameObject.AddComponent<Button>();b.targetGraphic=rect.GetComponent<Image>();Text(rect,"Label",text,.04f,.05f,.96f,.95f,font,52);return b;}
    }
}
