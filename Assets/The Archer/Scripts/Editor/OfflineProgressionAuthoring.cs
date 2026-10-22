using System;
using System.IO;
using System.Reflection;
using MrX.Achievements;
using MrX.Currency;
using MrX.Development;
using MrX.Localization;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class OfflineProgressionAuthoring
    {
        private const string Folder = "Assets/The Archer/Prefabs/UI/Offline Progression";
        private static ForgePageDatabase art;
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(.12f, .23f, .5f);
        [MenuItem("Tools/MrX Studio/Offline/Install Feature Switches Difficulty and Run Summary")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play first.");
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            font = Resources.Load<MenuTranslations>("Menu Translations").latinFont;
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var features = Resources.Load<GameFeatureSettings>(GameFeatureSettings.ResourcePath);
            if (features == null) { features = ScriptableObject.CreateInstance<GameFeatureSettings>(); AssetDatabase.CreateAsset(features, "Assets/The Archer/Resources/Game Feature Settings.asset"); }
            var developer = AssetDatabase.LoadAssetAtPath<DeveloperToolsSettings>("Assets/The Archer/Scriptables/Developer Tools Settings.asset");
            developer.features = features; EditorUtility.SetDirty(developer);
            var achievement = Resources.Load<AchievementsDatabase>(AchievementsDatabase.ResourcePath);
            foreach (var stage in achievement.stages.Stages)
                for (int i = 0; i < 3; i++)
                {
                    var mode = (StageDifficulty)i; string id = StageDifficultySelector.AchievementId(stage.StageId, mode);
                    if (achievement.achievements.Exists(a => a.id == id)) continue;
                    string name = i == 0 ? "Normal" : i == 1 ? "Hard" : "Nightmare";
                    achievement.achievements.Add(new AchievementDefinition { id = id, metric = AchievementMetric.StageDifficultyClear,
                        stageId = stage.StageId, difficulty = mode, category = AchievementCategory.Adventure, target = 1,
                        title = stage.StageName + " · " + name, description = "Clear this stage on " + name + " difficulty.",
                        reward = new Price("gem", i == 0 ? 20 : i == 1 ? 50 : 100), icon = achievement.trophySprite });
                    // Persist the default tuning explicitly so every stage exposes three Inspector entries.
                    var so = new SerializedObject(stage); var levels = so.FindProperty("difficulties");
                    if (levels.arraySize != 3)
                    {
                        levels.arraySize = 3;
                        for (int d = 0; d < 3; d++) { var item = levels.GetArrayElementAtIndex(d); item.FindPropertyRelative("enemyHp").floatValue = d == 0 ? 1 : d == 1 ? 1.5f : 2; item.FindPropertyRelative("enemyDamage").floatValue = d == 0 ? 1 : d == 1 ? 1.25f : 1.5f; }
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(stage);
                }
            EditorUtility.SetDirty(achievement); Translations();
            var selector = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Stage Difficulty Selector.prefab") ?? BuildSelector();
            const string stagePath = "Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab";
            var stageRoot = PrefabUtility.LoadPrefabContents(stagePath);
            try
            {
                if (!stageRoot.GetComponentInChildren<StageDifficultySelector>(true)) PrefabUtility.InstantiatePrefab(selector, stageRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(stageRoot, stagePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(stageRoot); }
            var sceneStage = Object.FindAnyObjectByType<MainMenuStagesPageBehavior>();
            if (sceneStage != null && !sceneStage.GetComponentInChildren<StageDifficultySelector>(true))
            { PrefabUtility.InstantiatePrefab(selector, sceneStage.transform); EditorSceneManager.MarkSceneDirty(sceneStage.gameObject.scene); }
            const string economyPath = "Assets/The Archer/Prefabs/UI/Menu Pages/Menu Economy.prefab";
            var economyRoot = PrefabUtility.LoadPrefabContents(economyPath);
            try
            {
                var so = new SerializedObject(economyRoot.GetComponent<MenuEconomyPanel>());
                var label = (TMP_Text)so.FindProperty("energyAmount").objectReferenceValue;
                so.FindProperty("energyGroup").objectReferenceValue = label.transform.parent.gameObject; so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(economyRoot, economyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(economyRoot); }
            var summary = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Run Summary.prefab") ?? BuildSummary();
            foreach (var name in new[] { "Stage Complete", "Stage Failed" })
            {
                string path = "Assets/The Archer/Prefabs/UI/Game/" + name + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (!root.GetComponentInChildren<RunSummaryPresenter>(true)) PrefabUtility.InstantiatePrefab(summary, root.transform);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            if (sceneStage != null && sceneStage.gameObject.scene.isDirty) EditorSceneManager.SaveScene(sceneStage.gameObject.scene);
            Selection.activeObject = developer;
            Debug.Log("Offline progression installed. Energy defaults OFF; online switches are reserved only.");
        }
        private static GameObject BuildSelector()
        {
            var root = Rect(null, "Stage Difficulty Selector", .025f, .39f, .18f, .61f); Skin(root.gameObject);
            var view = root.gameObject.AddComponent<StageDifficultySelector>();
            var choices = new Button[3]; var labels = new TMP_Text[3];
            var layout = root.gameObject.AddComponent<ResponsiveMenuLayout>();
            for (int i = 0; i < 3; i++)
            {
                var b = Button(root, "Difficulty " + i, i == 0 ? "Normal\nFirst clear" : i == 1 ? "Hard\nLocked" : "Nightmare\nLocked", .05f, .31f + (2 - i) * .22f, .95f, .51f + (2 - i) * .22f);
                choices[i] = b; labels[i] = b.GetComponentInChildren<TMP_Text>();
                layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle { target = (RectTransform)b.transform, landscape = ResponsiveMenuLayout.Placement.Capture((RectTransform)b.transform), portrait = Box(.02f + i * .325f, .34f, .32f + i * .325f, .96f) });
                layout.texts.Add(new ResponsiveMenuLayout.TextStyle { target = labels[i], landscape = 34, portrait = 28, autoSize = true });
            }
            var info = Text(root, "First Clear Reward", "First-clear reward: 20 gems · Claim in Achievements", 25, .06f, .02f, .94f, .29f);
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle { target = root, landscape = ResponsiveMenuLayout.Placement.Capture(root), portrait = Box(.055f, .455f, .945f, .535f) });
            Set(view, "choices", choices); Set(view, "labels", labels); Set(view, "info", info);
            root.gameObject.AddComponent<LocalizedMenuScope>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Stage Difficulty Selector.prefab"); Object.DestroyImmediate(root.gameObject); return prefab;
        }
        private static GameObject BuildSummary()
        {
            var root = Rect(null, "Run Summary", 0, 0, 1, 1);
            var presenter = root.gameObject.AddComponent<RunSummaryPresenter>();
            var layout = root.gameObject.AddComponent<ResponsiveMenuLayout>();
            var open = Button(root, "Open Statistics", "Run statistics", .72f, .81f, .95f, .88f);
            var details = Rect(root, "Statistics Panel", .04f, .08f, .96f, .92f); Skin(details.gameObject).raycastTarget = true;
            var title = Text(details, "Title", "RUN SUMMARY", 55, .035f, .87f, .8f, .97f);
            layout.texts.Add(new ResponsiveMenuLayout.TextStyle { target = title, landscape = 55, portrait = 32, autoSize = true });
            var close = Button(details, "Close Statistics", "Close", .82f, .88f, .97f, .97f);
            close.navigation = new Navigation { mode = Navigation.Mode.None };
            var metrics = ScrollText(details, "Combat Statistics", .035f, .07f, .49f, .85f);
            var build = ScrollText(details, "Build and Abilities", .51f, .07f, .965f, .85f);
            var metricsRect = (RectTransform)metrics.transform.parent.parent.parent;
            var buildRect = (RectTransform)build.transform.parent.parent.parent;
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle { target = metricsRect, landscape = ResponsiveMenuLayout.Placement.Capture(metricsRect), portrait = Box(.035f, .48f, .965f, .85f) });
            layout.rectangles.Add(new ResponsiveMenuLayout.RectStyle { target = buildRect, landscape = ResponsiveMenuLayout.Placement.Capture(buildRect), portrait = Box(.035f, .04f, .965f, .46f) });
            Set(presenter, "details", details.gameObject); Set(presenter, "openButton", open); Set(presenter, "closeButton", close);
            Set(presenter, "title", title); Set(presenter, "metrics", metrics); Set(presenter, "build", build);
            root.gameObject.AddComponent<LocalizedMenuScope>();
            details.gameObject.SetActive(false);
            ConfigureSummaryRecords(root.gameObject);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/Run Summary.prefab"); Object.DestroyImmediate(root.gameObject); return prefab;
        }
        [MenuItem("Tools/MrX Studio/Offline/Upgrade Run Summary Records")]
        public static void UpgradeSummaryRecords()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play first.");
            art = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            font = Resources.Load<MenuTranslations>("Menu Translations").latinFont;
            string path = Folder + "/Run Summary.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try { ConfigureSummaryRecords(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Translations(); AssetDatabase.SaveAssets();
        }
        private static void ConfigureSummaryRecords(GameObject root)
        {
            var details = root.transform.Find("Statistics Panel");
            if (details.Find("Record Highlights") != null) return;
            var strip = Rect(details, "Record Highlights", .035f, .68f, .965f, .85f);
            var view = strip.gameObject.AddComponent<RunRecordHighlights>();
            for (int i = 0; i < 2; i++)
            {
                var card = Rect(strip, i == 0 ? "Time Record Card" : "DPS Record Card", i == 0 ? 0 : .51f, 0, i == 0 ? .49f : 1, 1);
                Skin(card.gameObject).color = i == 0 ? new Color(.84f,.94f,1) : new Color(.88f,1,.92f);
                var heading = Text(card, "Heading", i == 0 ? "CLEAR TIME" : "AVERAGE DPS", 30, .03f,.76f,.97f,.96f);
                var number = Text(card, "Value", "0", 76, .04f,.3f,.96f,.78f);
                number.color = new Color(.1f,.3f,.7f);
                var best = Text(card, "Previous Best", "Personal best", 24, .03f,.02f,.97f,.19f);
                var badge = Text(card, "New Record Badge", "NEW RECORD!", 25, .53f,.78f,.97f,.99f);
                badge.text = "";
                Set(view, i == 0 ? "timeTitle" : "dpsTitle", heading); Set(view, i == 0 ? "timeValue" : "dpsValue", number);
                Set(view, i == 0 ? "timeBest" : "dpsBest", best); Set(view, i == 0 ? "timeBadge" : "dpsBadge", badge);
                if (i == 1) Set(view, "damageTotal", Text(card, "Total Damage", "Total damage", 24, .03f,.19f,.97f,.33f));
                // Reserve a separate line for the record badge instead of overlapping the heading.
                badge.rectTransform.anchorMin = new Vector2(.03f,.63f); badge.rectTransform.anchorMax = new Vector2(.97f,.77f);
                number.rectTransform.anchorMax = new Vector2(.96f,.64f);
            }
            Set(root.GetComponent<RunSummaryPresenter>(), "highlights", view);
            var layout = root.GetComponent<ResponsiveMenuLayout>();
            var metrics = (RectTransform)details.Find("Combat Statistics");
            var build = (RectTransform)details.Find("Build and Abilities");
            Box(.035f,.07f,.49f,.65f).Apply(metrics); Box(.51f,.07f,.965f,.65f).Apply(build);
            var metricsStyle = layout.rectangles.Find(s => s.target == metrics);
            var buildStyle = layout.rectangles.Find(s => s.target == build);
            metricsStyle.landscape = ResponsiveMenuLayout.Placement.Capture(metrics); metricsStyle.portrait = Box(.035f,.36f,.965f,.65f);
            buildStyle.landscape = ResponsiveMenuLayout.Placement.Capture(build); buildStyle.portrait = Box(.035f,.04f,.965f,.34f);
        }
        private static TMP_Text ScrollText(Transform parent, string name, float x, float y, float r, float t)
        {
            var root = Rect(parent, name, x, y, r, t); Skin(root.gameObject).raycastTarget = true;
            var scroll = root.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.scrollSensitivity = 65;
            var viewport = Rect(root, "Viewport", .035f, .035f, .965f, .965f); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect(viewport, "Content", 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1);
            var group = content.gameObject.AddComponent<VerticalLayoutGroup>(); group.childControlHeight = group.childControlWidth = true; group.childForceExpandHeight = false; group.childForceExpandWidth = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var text = Text(content, "Text", "Statistics will appear after the run.", 38, 0, 0, 1, 1);
            text.enableAutoSizing = false; text.alignment = TextAlignmentOptions.TopLeft; text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow;
            scroll.viewport = viewport; scroll.content = content; return text;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float r, float t)
        {
            var root = new GameObject(name, typeof(RectTransform)); root.layer = 5; root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform; Box(x, y, r, t).Apply(rect); return rect;
        }
        private static ResponsiveMenuLayout.Placement Box(float x, float y, float r, float t) => new ResponsiveMenuLayout.Placement { min = new Vector2(x, y), max = new Vector2(r, t), pivot = Vector2.one * .5f };
        private static Image Skin(GameObject root)
        { var image = root.AddComponent<Image>(); image.sprite = art.slotSprite; image.material = art.slotMaterial; image.type = Image.Type.Sliced; image.color = new Color(.89f, .96f, 1); image.raycastTarget = false; return image; }
        private static TMP_Text Text(Transform parent, string name, string value, float size, float x, float y, float r, float t)
        { var text = Rect(parent, name, x, y, r, t).gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = text.fontSizeMax = size; text.fontSizeMin = size * .65f; text.enableAutoSizing = true; text.color = Ink; text.text = value; text.raycastTarget = false; text.alignment = TextAlignmentOptions.Center; return text; }
        private static Button Button(Transform parent, string name, string value, float x, float y, float r, float t)
        { var root = Rect(parent, name, x, y, r, t); var image = Skin(root.gameObject); image.raycastTarget = true; var button = root.gameObject.AddComponent<Button>(); button.targetGraphic = image; Text(root, "Label", value, 38, .04f, .04f, .96f, .96f); return button; }
        private static void Set(Object target, string field, object value)
        { target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value); EditorUtility.SetDirty(target); }
        private static void Translations()
        {
            var table = Resources.Load<MenuTranslations>("Menu Translations");
            Add(table, "CLEAR TIME", "THỜI GIAN VƯỢT ẢI", "TIEMPO DE VICTORIA", "TEMPO DE VITÓRIA", "TEMPS DE VICTOIRE");
            Add(table, "TIME SURVIVED", "THỜI GIAN SINH TỒN", "TIEMPO SOBREVIVIDO", "TEMPO SOBREVIVIDO", "TEMPS SURVÉCU");
            Add(table, "AVERAGE DPS", "DPS TRUNG BÌNH", "DPS MEDIO", "DPS MÉDIO", "DPS MOYEN");
            Add(table, "NEW RECORD!", "KỶ LỤC MỚI!", "¡NUEVO RÉCORD!", "NOVO RECORDE!", "NOUVEAU RECORD !");
            Add(table, "Previous best clear: {0}", "Kỷ lục vượt ải trước: {0}", "Mejor victoria anterior: {0}", "Melhor vitória anterior: {0}", "Précédent meilleur temps : {0}");
            Add(table, "Previous best DPS: {0}", "Kỷ lục DPS trước: {0}", "Mejor DPS anterior: {0}", "Melhor DPS anterior: {0}", "Précédent meilleur DPS : {0}");
            Add(table, "Complete this stage to set a time record.", "Vượt ải để lập kỷ lục thời gian.", "Completa la fase para registrar tu tiempo.", "Conclua a fase para registrar seu tempo.", "Terminez ce niveau pour établir un record.");
            Add(table, "Your first DPS record", "Mốc DPS đầu tiên của bạn", "Tu primer récord de DPS", "Seu primeiro recorde de DPS", "Votre premier record de DPS");
            Add(table, "Your first clear record", "Kỷ lục vượt ải đầu tiên", "Tu primer récord de victoria", "Seu primeiro recorde de vitória", "Votre premier record de victoire");
            Add(table, "No previous DPS record", "Chưa có kỷ lục DPS trước đó", "Sin récord anterior de DPS", "Nenhum recorde anterior de DPS", "Aucun record de DPS précédent");
            Add(table, "Total damage: {0}", "Tổng sát thương: {0}", "Daño total: {0}", "Dano total: {0}", "Dégâts totaux : {0}");
            Add(table, "Close", "Đóng", "Cerrar", "Fechar", "Fermer");
            Add(table, "Victory", "Chiến thắng", "Victoria", "Vitória", "Victoire");
            Add(table, "Defeat", "Thất bại", "Derrota", "Derrota", "Défaite");
            Add(table, "Locked", "Chưa mở", "Bloqueado", "Bloqueado", "Verrouillé");
            Add(table, "Completed", "Đã vượt qua", "Completado", "Concluído", "Terminé");
            Add(table, "Reward claimed", "Đã nhận thưởng", "Recompensa recibida", "Recompensa resgatada", "Récompense récupérée");
            Add(table, "Reward ready: {0} gems · Claim in Achievements", "Có thể nhận {0} gem ở Thành tựu", "Recoge {0} gemas en Logros", "Resgate {0} gemas em Conquistas", "Récupérez {0} gemmes dans Succès");
            Add(table, "Normal", "Thường", "Normal", "Normal", "Normal");
            Add(table, "Hard", "Khó", "Difícil", "Difícil", "Difficile");
            Add(table, "Nightmare", "Ác mộng", "Pesadilla", "Pesadelo", "Cauchemar");
            Add(table, "First clear", "Lần đầu vượt ải", "Primera victoria", "Primeira vitória", "Première victoire");
            Add(table, "First-clear reward: {0} gems · Claim in Achievements", "Thưởng lần đầu: {0} gem · Nhận ở Thành tựu", "Primera victoria: {0} gemas · Reclamar en Logros", "Primeira vitória: {0} gemas · Resgatar em Conquistas", "Première victoire : {0} gemmes · À récupérer dans Succès");
            Add(table, "RUN SUMMARY", "THỐNG KÊ LƯỢT CHƠI", "RESUMEN DE PARTIDA", "RESUMO DA PARTIDA", "BILAN DE PARTIE");
            Add(table, "Run statistics", "Thống kê lượt chơi", "Estadísticas", "Estatísticas", "Statistiques");
            Add(table, "Time played", "Thời gian chơi", "Tiempo jugado", "Tempo jogado", "Temps joué");
            Add(table, "Enemies / Bosses defeated", "Quái / Boss đã hạ", "Enemigos / Jefes derrotados", "Inimigos / Chefes derrotados", "Ennemis / Boss vaincus");
            Add(table, "Damage dealt", "Sát thương gây ra", "Daño infligido", "Dano causado", "Dégâts infligés");
            Add(table, "Damage to bosses", "Sát thương lên boss", "Daño a jefes", "Dano aos chefes", "Dégâts aux boss");
            Add(table, "HP lost / restored", "HP mất / hồi phục", "Vida perdida / restaurada", "Vida perdida / restaurada", "PV perdus / récupérés");
            Add(table, "Hits taken", "Số lần trúng đòn", "Golpes recibidos", "Golpes recebidos", "Coups reçus");
            Add(table, "Critical hits / Hits landed", "Đòn chí mạng / Đòn trúng", "Críticos / Impactos", "Críticos / Acertos", "Critiques / Coups au but");
            Add(table, "Highest hit", "Sát thương đòn cao nhất", "Mayor golpe", "Maior golpe", "Coup le plus puissant");
            Add(table, "Damage by type", "Sát thương theo loại", "Daño por tipo", "Dano por tipo", "Dégâts par type");
            Add(table, "FINAL BUILD", "BUILD CUỐI LƯỢT", "CONFIGURACIÓN FINAL", "BUILD FINAL", "ÉQUIPEMENT FINAL");
            Add(table, "Includes equipment and permanent upgrades.", "Bao gồm trang bị và nâng cấp lâu dài.", "Incluye equipo y mejoras permanentes.", "Inclui equipamentos e melhorias permanentes.", "Inclut équipement et améliorations permanentes.");
            Add(table, "ATK / HP", "Công / Máu", "ATQ / Vida", "ATQ / Vida", "ATQ / PV");
            Add(table, "Attack speed / Critical chance", "Tốc đánh / Tỉ lệ chí mạng", "Velocidad / Crítico", "Velocidade / Crítico", "Vitesse / Critique");
            Add(table, "Abilities this run", "Kỹ năng trong lượt", "Habilidades de la partida", "Habilidades da partida", "Compétences de cette partie");
            Add(table, "Equipment", "Trang bị", "Equipo", "Equipamento", "Équipement");
            Add(table, "Permanent upgrades", "Nâng cấp lâu dài", "Mejoras permanentes", "Melhorias permanentes", "Améliorations permanentes");
            Add(table, "Physical", "Vật lý", "Físico", "Físico", "Physique");
            Add(table, "Burning", "Thiêu đốt", "Quemadura", "Queimadura", "Brûlure");
            Add(table, "Freezing", "Băng giá", "Hielo", "Gelo", "Gel");
            Add(table, "Poison", "Độc", "Veneno", "Veneno", "Poison");
            Add(table, "Shock", "Điện", "Electricidad", "Eletricidade", "Électricité");
            foreach (var difficulty in new[] { "Normal", "Hard", "Nightmare" })
            {
                string vi = difficulty == "Normal" ? "Thường" : difficulty == "Hard" ? "Khó" : "Ác mộng";
                string es = difficulty == "Normal" ? "Normal" : difficulty == "Hard" ? "Difícil" : "Pesadilla";
                string pt = difficulty == "Normal" ? "Normal" : difficulty == "Hard" ? "Difícil" : "Pesadelo";
                string fr = difficulty == "Normal" ? "Normal" : difficulty == "Hard" ? "Difficile" : "Cauchemar";
                Add(table, "Clear this stage on " + difficulty + " difficulty.", "Vượt ải này ở độ khó " + vi + ".", "Supera esta fase en dificultad " + es + ".", "Conclua esta fase na dificuldade " + pt + ".", "Terminez ce niveau en difficulté " + fr + ".");
                foreach (var stage in Resources.Load<AchievementsDatabase>(AchievementsDatabase.ResourcePath).stages.Stages)
                    Add(table, stage.StageName + " · " + difficulty, table.Translate(stage.StageName, "vi") + " · " + vi, table.Translate(stage.StageName, "es") + " · " + es, table.Translate(stage.StageName, "pt") + " · " + pt, table.Translate(stage.StageName, "fr") + " · " + fr);
            }
            table.Rebuild(); EditorUtility.SetDirty(table);
        }
        private static void Add(MenuTranslations table, string en, string vi, string es, string pt, string fr)
        { if (!table.entries.Exists(e => e.english == en)) table.entries.Add(new TranslationEntry { english = en, vietnamese = vi, spanish = es, portuguese = pt, french = fr }); }
    }
}
