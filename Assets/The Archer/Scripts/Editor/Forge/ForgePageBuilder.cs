using MrX.Armory;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class ForgePageBuilder
    {
        public const string Folder = "Assets/The Archer/Resources";
        public const string PrefabPath = MenuPrefabAuthoring.Folder + "/Forge Page.prefab";
        [MenuItem("Tools/MrX Studio/Forge/Create Database If Missing")]
        public static void CreateDatabase()
        {
            var path = Folder + "/Forge Page Database.asset";
            var database = AssetDatabase.LoadAssetAtPath<ForgePageDatabase>(path);
            if (database != null) { Selection.activeObject = database; return; }
            database = ScriptableObject.CreateInstance<ForgePageDatabase>();
            database.armory = AssetDatabase.LoadAssetAtPath<ArmoryDatabase>("Assets/The Archer/Scriptables/Armory/Armory Database.asset");
            database.slotSprite = Sprite("Armory/ui_slot.png"); database.forgeIcon = Sprite("Main Menu/ui_4.png");
            database.coinIcon = Sprite("Generic/ui_coin.png"); database.settingsIcon = Sprite("Main Menu/ui_settings.png");
            database.circleSprite = Sprite("Generic/ui_circle.png"); database.forgeButtonSprite = Sprite("Generic/ui_button_yellow.png"); database.disabledButtonSprite = Sprite("Generic/ui_button_gray.png");
            database.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/The Archer/Fonts/NeverMind/Bold Plain.asset");
            var material = new Material(Shader.Find("UI/Default")) { name = "Forge Pastel Slot" };
            material.color = new Color(1.07f, 1.13f, 1.30f, 1);
            AssetDatabase.CreateAsset(material, Folder + "/Forge Pastel Slot.mat"); database.slotMaterial = material;
            AssetDatabase.CreateAsset(database, path); AssetDatabase.SaveAssets(); Selection.activeObject = database;
        }
        [MenuItem("Tools/MrX Studio/Forge/Build Editable Page Prefab")]
        public static void BuildPrefab()
        {
            var database = AssetDatabase.LoadAssetAtPath<ForgePageDatabase>(Folder + "/Forge Page Database.asset");
            if (database == null || database.armory == null || database.slotSprite == null || database.font == null)
            { Debug.LogError("Forge: missing page database, armory, background or font."); return; }
            var page = ForgePageBehavior.Create(null, database);
            try { PrefabUtility.SaveAsPrefabAsset(page.gameObject, PrefabPath); AssetDatabase.SaveAssets(); Debug.Log("Forge editable prefab saved: " + PrefabPath); }
            finally { Object.DestroyImmediate(page.gameObject); }
        }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/UI/" + path);
    }
}
