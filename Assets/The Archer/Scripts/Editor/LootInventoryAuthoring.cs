using System.Collections.Generic;
using System.Linq;
using MrX.Armory;
using MrX.Crafting;
using MrX.Drop;
using MrX.Enemy;
using MrX.Rewards;
using MrX.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class LootInventoryAuthoring
    {
        public const string Folder = "Assets/The Archer/Scriptables/Boss Chest Rewards";
        public const string EmptyPath = "Assets/The Archer/Sprites/UI/Armory/ui_inventory_empty.png";
        [MenuItem("Tools/MrX Studio/Loot/Install Loot and Inventory Assets")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new System.Exception("Exit Play first.");
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var config = Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath);
            var materials = new[] { CraftingMaterial.Iron, CraftingMaterial.Wood, CraftingMaterial.Leather };
            config.materials = new List<CraftingMaterialData>();
            foreach (var m in materials) config.materials.Add(new CraftingMaterialData { material = m, title = m.ToString(), icon = Import("Assets/The Archer/Sprites/UI/Items/ui_" + m.ToString().ToLowerInvariant() + ".png") });
            config.enemy.chance = config.boss.chance = config.chest.chance = 0;
            var armory = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory;
            foreach (var item in armory.Items)
            {
                var recipe = item.Crafting; if (recipe == null || !recipe.enabled || recipe.materials.Any(m => m != null && m.material == CraftingMaterial.Wood)) continue;
                int wood = Mathf.Max(1, recipe.Cost(CraftingMaterial.Crystal) + recipe.Cost(CraftingMaterial.Scroll));
                recipe.materials = new List<MaterialCost> { new MaterialCost { material = CraftingMaterial.Iron, amount = Mathf.Max(1, recipe.Cost(CraftingMaterial.Iron)) },
                    new MaterialCost { material = CraftingMaterial.Wood, amount = wood }, new MaterialCost { material = CraftingMaterial.Leather, amount = Mathf.Max(1, recipe.Cost(CraftingMaterial.Leather)) } };
                EditorUtility.SetDirty(item);
            }
            config.defaultBossRewards = Table("Default Boss Chest", null, armory);
            var enemies = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>("Assets/The Archer/Scriptables/Enemies Database.asset");
            var enemyObject = new SerializedObject(enemies); var records = enemyObject.FindProperty("enemies");
            for (int i = 0; i < enemies.EnemiesCount; i++)
            {
                var enemy = enemies.GetEnemy(i); if (!enemy.IsBoss) continue;
                if (!enemy.BossChestRewards) records.GetArrayElementAtIndex(i).FindPropertyRelative("bossChestRewards").objectReferenceValue = Table(enemy.EnemyType.ToString(), enemy.Drops, armory);
            }
            enemyObject.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(enemies);
            foreach (var guid in AssetDatabase.FindAssets("t:StageData"))
            {
                var stage = AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(guid)); var so = new SerializedObject(stage);
                var rooms = so.FindProperty("rooms");
                for (int r = 0; r < stage.RoomsCount; r++)
                {
                    var room = stage.GetRoom(r); var waves = rooms.GetArrayElementAtIndex(r).FindPropertyRelative("waves");
                    for (int w = 0; w < room.Waves.Length; w++)
                    {
                        var spawns = waves.GetArrayElementAtIndex(w).FindPropertyRelative("enemySpawns");
                        for (int i = 0; i < room.Waves[w].EnemySpawns.Count; i++)
                        {
                            var spawn = room.Waves[w].EnemySpawns[i]; var enemy = enemies.GetEnemy(spawn.EnemyType);
                            if (enemy == null || !enemy.IsBoss || spawn.OverrideData == null) continue;
                            var overrides = spawn.OverrideData.DropOverrides;
                            if (overrides == null || !overrides.Any(d => d.DropType == DropType.Item)) continue;
                            var field = spawns.GetArrayElementAtIndex(i).FindPropertyRelative("overrideData").FindPropertyRelative("bossChestRewards");
                            if (!field.objectReferenceValue) field.objectReferenceValue = Table(stage.name + " Room " + r + " Wave " + w + " Boss " + i, overrides, armory);
                        }
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(stage);
            }
            const string chestPath = "Assets/The Archer/Prefabs/Drop/Boss Loot Chest.prefab";
            config.bossChestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(chestPath);
            if (!config.bossChestPrefab)
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/The Archer/Prefabs/Drop/Item.prefab");
                try
                {
                    var old = root.GetComponent<ItemDropBehavior>(); var target = root.AddComponent<BossChestDropBehavior>();
                    var from = new SerializedObject(old); var to = new SerializedObject(target); var p = from.GetIterator();
                    while (p.NextVisible(true)) if (p.depth == 0 && !p.name.StartsWith("m_") && to.FindProperty(p.propertyPath) != null) to.CopyFromSerializedProperty(p);
                    to.FindProperty("lifetime").floatValue = 0; to.ApplyModifiedPropertiesWithoutUndo(); Object.DestroyImmediate(old);
                    config.bossChestPrefab = PrefabUtility.SaveAsPrefabAsset(root, chestPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            EditorUtility.SetDirty(config);
            var art = Resources.Load<ArmoryDashboardDatabase>(ArmoryDashboardDatabase.ResourcePath); art.emptySlotIcon = Import(EmptyPath); EditorUtility.SetDirty(art);
            EditArmory(art); EditForge(config); EditLucky(config);
            StagePresentationAuthoring.RefreshWheel();
            AssetDatabase.SaveAssets(); Selection.activeObject = config.defaultBossRewards;
        }
        private static BossChestRewards Table(string name, List<EnemyDropData> old, ArmoryDatabase armory)
        {
            string path = Folder + "/" + name + ".asset"; var table = AssetDatabase.LoadAssetAtPath<BossChestRewards>(path); if (table) return table;
            table = ScriptableObject.CreateInstance<BossChestRewards>();
            var weights = new Dictionary<ItemData, float>();
            if (old != null) foreach (var d in old) if (d.DropType == DropType.Item && d.ItemData != null && d.GetChance(1) > 0) weights[d.ItemData] = d.GetChance(1);
            if (weights.Count == 0) foreach (var item in armory.Items) weights[item] = 1;
            foreach (var pair in weights)
            {
                table.prizes.Add(new LuckyPrize { id = "equipment-" + pair.Key.Id, kind = LuckyRewardKind.Equipment, item = pair.Key, weight = pair.Value * .7f });
                if (pair.Key.Crafting != null && pair.Key.Crafting.enabled)
                    table.prizes.Add(new LuckyPrize { id = "fragments-" + pair.Key.Id, kind = LuckyRewardKind.Fragments, item = pair.Key, minimumAmount = 1, maximumAmount = 3, weight = pair.Value * .3f });
            }
            AssetDatabase.CreateAsset(table, path); return table;
        }
        private static void EditArmory(ArmoryDashboardDatabase art)
        {
            string path = MenuPrefabAuthoring.Folder + "/Armory Page.prefab"; var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name != "Item or Empty Icon") continue;
                    var image = t.GetComponent<UnityEngine.UI.Image>(); image.sprite = art.emptySlotIcon; image.color = Color.white;
                    var rect = image.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                    t.parent.GetComponent<UnityEngine.UI.Image>().enabled = false;
                    var label = t.parent.Find("Label"); if (label) label.gameObject.SetActive(false);
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void EditForge(CraftingDatabase config)
        {
            string path = MenuPrefabAuthoring.Folder + "/Forge Page.prefab"; var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var so = new SerializedObject(root.GetComponent<ForgePageBehavior>()); var views = so.FindProperty("materialViews");
                for (int i = views.arraySize - 1; i >= 3; i--)
                { var view = views.GetArrayElementAtIndex(i); var icon = (UnityEngine.UI.Image)view.FindPropertyRelative("icon").objectReferenceValue; Object.DestroyImmediate(icon.transform.parent.gameObject); views.DeleteArrayElementAtIndex(i); }
                for (int i = 0; i < 3; i++)
                {
                    var view = views.GetArrayElementAtIndex(i); var material = config.materials[i]; view.FindPropertyRelative("material").intValue = (int)material.material;
                    var icon = (UnityEngine.UI.Image)view.FindPropertyRelative("icon").objectReferenceValue; icon.sprite = material.icon;
                    ((TMP_Text)view.FindPropertyRelative("title").objectReferenceValue).text = material.title;
                    var tile = (RectTransform)icon.transform.parent; tile.anchorMin = new Vector2(.05f + i * .31f, .12f); tile.anchorMax = new Vector2(.33f + i * .31f, .73f); tile.offsetMin = tile.offsetMax = Vector2.zero;
                }
                so.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void EditLucky(CraftingDatabase config)
        {
            foreach (string name in new[] { "Lucky Wheel", "Gem Chest" })
            {
                var table = AssetDatabase.LoadAssetAtPath<LuckyRewardTable>(LuckyRewardsAuthoring.DataFolder + "/" + name + ".asset");
                var seen = new Dictionary<CraftingMaterial, LuckyPrize>();
                foreach (var prize in table.prizes)
                {
                    if (prize.kind == LuckyRewardKind.Equipment || prize.kind == LuckyRewardKind.Fragments)
                    {
                        prize.material = prize.kind == LuckyRewardKind.Equipment ? CraftingMaterial.Leather : CraftingMaterial.Iron;
                        prize.kind = LuckyRewardKind.Material; prize.item = null;
                    }
                    if (prize.kind != LuckyRewardKind.Material) continue;
                    if (prize.material == CraftingMaterial.Crystal || prize.material == CraftingMaterial.Scroll) prize.material = CraftingMaterial.Wood;
                    prize.icon = config.Material(prize.material)?.icon; prize.title = prize.material.ToString();
                    if (!prize.enabled) continue;
                    if (seen.TryGetValue(prize.material, out var first)) { first.weight += prize.weight; prize.enabled = false; }
                    else seen.Add(prize.material, prize);
                }
                EditorUtility.SetDirty(table);
                string path = LuckyRewardsAuthoring.Folder + "/" + name + " Popup.prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var rows = root.GetComponentsInChildren<LuckyPrizeView>(true); int index = 0;
                    if (table.IsValid(out var total)) foreach (var prize in table.prizes)
                    {
                        if (!prize.enabled || prize.weight <= 0 || index >= rows.Length) continue;
                        var row = rows[index++]; row.gameObject.SetActive(true); row.icon.sprite = prize.icon;
                        row.title.text = prize.title; row.amount.text = "x " + prize.minimumAmount + (prize.maximumAmount == prize.minimumAmount ? "" : " - " + prize.maximumAmount);
                        row.odds.text = (prize.weight / total * 100).ToString("0.##") + "%";
                    }
                    for (; index < rows.Length; index++) rows[index].gameObject.SetActive(false);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        private static Sprite Import(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); if (importer == null) throw new System.Exception("Missing image: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.maxTextureSize = 512; importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
