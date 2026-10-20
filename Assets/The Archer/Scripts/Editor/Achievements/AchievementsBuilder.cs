using MrX.Achievements;
using MrX.Armory;
using MrX.UI;
using MrX.Upgrades;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class AchievementsBuilder
    {
        private const string Folder = "Assets/The Archer/Resources";
        private const string Art = "Assets/The Archer/Sprites/UI/";
        public const string PrefabPath = MenuPrefabAuthoring.Folder + "/Achievements Page.prefab";

        [MenuItem("Tools/MrX Studio/Achievements/Create Database If Missing")]
        public static void CreateDatabase()
        {
            var path = Folder + "/Achievements Database.asset";
            var database = AssetDatabase.LoadAssetAtPath<AchievementsDatabase>(path);
            if (database != null) { Selection.activeObject = database; return; }
            database = ScriptableObject.CreateInstance<AchievementsDatabase>();
            database.slotSprite = Sprite("Armory/ui_slot.png");
            database.slotMaterial = GetSlotMaterial();
            database.trophySprite = Sprite("Main Menu/ui_6.png");
            database.coinSprite = Sprite("Generic/ui_coin.png");
            database.circleSprite = Sprite("Generic/ui_circle.png");
            database.settingsSprite = Sprite("Main Menu/ui_settings.png");
            database.claimButtonSprite = Sprite("Generic/ui_button_yellow.png");
            database.disabledButtonSprite = Sprite("Generic/ui_button_gray.png");
            database.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/The Archer/Fonts/NeverMind/Bold Plain.asset");
            database.stages = AssetDatabase.LoadAssetAtPath<StageDatabase>("Assets/The Archer/Scriptables/Stages/Stage Database.asset");
            database.armory = AssetDatabase.LoadAssetAtPath<ArmoryDatabase>("Assets/The Archer/Scriptables/Armory/Armory Database.asset");
            database.upgrades = AssetDatabase.LoadAssetAtPath<UpgradesDatabase>("Assets/The Archer/Scriptables/Upgrades/Upgrades Database.asset");
            foreach (var category in database.categories)
                category.icon = Sprite(category.category == AchievementCategory.Adventure ? "Main Menu/ui_icons_map.png" :
                    category.category == AchievementCategory.Battle ? "Main Menu/ui_1.png" :
                    category.category == AchievementCategory.Collection ? "Main Menu/ui_2.png" : "Main Menu/ui_3.png");
            foreach (var definition in database.achievements)
                definition.icon = DefaultIcon(definition.metric);
            AssetDatabase.CreateAsset(database, path); AssetDatabase.SaveAssets(); Selection.activeObject = database;
            Debug.Log("Achievements: created 48 definitions in four categories. Existing databases are never overwritten.");
        }

        [MenuItem("Tools/MrX Studio/Achievements/Build Editable Page Prefab")]
        public static void BuildPrefab()
        {
            var database = AssetDatabase.LoadAssetAtPath<AchievementsDatabase>(Folder + "/Achievements Database.asset");
            if (database == null || !database.IsValid(out _))
            { Debug.LogError("Achievements: configure a valid database before building the page."); return; }
            var page = AchievementsPageBehavior.Create(null, database);
            try
            {
                PrefabUtility.SaveAsPrefabAsset(page.gameObject, PrefabPath); AssetDatabase.SaveAssets();
                Debug.Log("Achievements: editable prefab saved at " + PrefabPath);
            }
            finally { Object.DestroyImmediate(page.gameObject); }
        }

        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + path);

        public static Material GetSlotMaterial()
        {
            const string path = Folder + "/Achievements Slot.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            // Compensate for the warm source sprite through one shared UI/Default
            // material. Keep the original sprite, nine-slicing, masking and alpha.
            material = new Material(Shader.Find("UI/Default")) { name = "Achievements Slot" };
            material.color = new Color(1.10f, 1.15f, 1.35f, 1);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static Sprite DefaultIcon(AchievementMetric metric)
        {
            switch (metric)
            {
                case AchievementMetric.StagesCleared: return Sprite("Main Menu/ui_5.png");
                case AchievementMetric.UniqueStages: return Sprite("Main Menu/ui_icons_map.png");
                case AchievementMetric.RoomsCompleted: return Sprite("Main Menu/ui_icons_map.png");
                case AchievementMetric.ChestsOpened: return Sprite("Game/Gameplay/ui_icons_chest_good.png");
                case AchievementMetric.ItemsCollected: return Sprite("Items/ui_bow_1.png");
                case AchievementMetric.UniqueItems: return Sprite("Items/ui_helmet_1.png");
                case AchievementMetric.HeroUpgrades: return Sprite("Main Menu/ui_2.png");
                case AchievementMetric.ItemUpgrades: return Sprite("Main Menu/ui_4.png");
                case AchievementMetric.PermanentUpgrades: return Sprite("Main Menu/ui_3.png");
                case AchievementMetric.BossesDefeated: return Sprite("Upgrades/ui_upg_power.png");
                default: return Sprite("Main Menu/ui_1.png");
            }
        }
    }
}
