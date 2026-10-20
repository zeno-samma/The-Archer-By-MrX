using MrX.Armory;
using MrX.Crafting;
using MrX.Drop;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class CraftingBuilder
    {
        private const string Folder = "Assets/The Archer/Resources";
        private const string DropPath = Folder + "/Crafting Drop.prefab";
        [MenuItem("Tools/MrX Studio/Forge/Install Crafting Assets")]
        public static void Install()
        {
            var path = Folder + "/Crafting Database.asset";
            var database = AssetDatabase.LoadAssetAtPath<CraftingDatabase>(path);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CraftingDatabase>();
                database.materials[0].icon = Sprite("UI/Items/ui_iron.png");
                database.materials[1].icon = Sprite("UI/Items/ui_wood.png");
                database.materials[2].icon = Sprite("UI/Items/ui_leather.png");
                AssetDatabase.CreateAsset(database, path);
            }
            if (database.dropPrefab == null)
            {
                database.dropPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DropPath);
                if (database.dropPrefab == null) database.dropPrefab = BuildDrop();
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets(); Selection.activeObject = database;
        }

        // Explicit authoring action: preserves every original ItemLevel, price,
        // GUID, hero/weapon mapping and any recipe already enabled by the author.
        [MenuItem("Tools/MrX Studio/Forge/Enable Default Recipes For Registered Items")]
        public static void EnableRecipes()
        {
            var database = AssetDatabase.LoadAssetAtPath<ArmoryDatabase>("Assets/The Archer/Scriptables/Armory/Armory Database.asset");
            foreach (var item in database.Items)
            {
                if (item == null || item.Crafting != null && item.Crafting.enabled) continue;
                var serialized = new SerializedObject(item);
                var recipe = serialized.FindProperty("crafting");
                recipe.FindPropertyRelative("enabled").boolValue = true;
                recipe.FindPropertyRelative("requiredFragments").intValue = 30;
                var costs = recipe.FindPropertyRelative("materials"); costs.arraySize = 3;
                var values = new[] { 10, 5, 2 };
                var materials = new[] { CraftingMaterial.Iron, CraftingMaterial.Wood, CraftingMaterial.Leather };
                for (var i = 0; i < materials.Length; i++)
                {
                    var cost = costs.GetArrayElementAtIndex(i);
                    cost.FindPropertyRelative("material").intValue = (int)materials[i];
                    cost.FindPropertyRelative("amount").intValue = values[i];
                }
                serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();
        }
        private static GameObject BuildDrop()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/The Archer/Prefabs/Drop/XP Gem.prefab");
            try
            {
                root.name = "Crafting Resource Drop";
                var source = root.GetComponent<XPDropBehavior>(); var target = root.AddComponent<CraftingDropBehavior>();
                var sourceData = new SerializedObject(source); var targetData = new SerializedObject(target);
                var property = sourceData.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.depth != 0 || property.name.StartsWith("m_")) continue;
                    if (targetData.FindProperty(property.propertyPath) != null) targetData.CopyFromSerializedProperty(property);
                }
                targetData.ApplyModifiedPropertiesWithoutUndo();
                Object.DestroyImmediate(source);
                var collider = root.GetComponent<SphereCollider>(); collider.isTrigger = true; collider.radius = .35f;
                targetData.Update(); targetData.FindProperty("trigger").objectReferenceValue = collider;
                targetData.FindProperty("lifetime").floatValue = 0;
                targetData.ApplyModifiedPropertiesWithoutUndo();
                var materialPath = Folder + "/Crafting Drop.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Crafting Drop" };
                    material.SetColor("_BaseColor", new Color(.58f, .35f, 1));
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true)) renderer.sharedMaterial = material;
                return PrefabUtility.SaveAsPrefabAsset(root, DropPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/" + path);
    }
}
