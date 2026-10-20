using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Armory;
using MrX.Crafting;
using MrX.Drop;
using MrX.Save;
using MrX.UI;
using MrX.Pool;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class CraftingVerification
    {
        private const BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance;
        [MenuItem("Tools/MrX Studio/Forge/Run Crafting Checks")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            int checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception("Crafting: " + message); checks++; };
            var actual = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var database = ScriptableObject.CreateInstance<ArmoryDatabase>();
            var a = UnityEngine.Object.Instantiate(actual.armory.Items[0]); var b = UnityEngine.Object.Instantiate(actual.armory.Items[0]);
            try
            {
                JsonUtility.FromJsonOverwrite("{\"guid\":\"test-a\",\"itemName\":\"Craft A\"}", a);
                JsonUtility.FromJsonOverwrite("{\"guid\":\"test-b\",\"itemName\":\"Craft B\"}", b);
                a.Crafting.enabled = b.Crafting.enabled = true;
                typeof(ArmoryDatabase).GetField("items", Fields).SetValue(database, new List<ItemData> { a, b });
                var saves = new MemorySaves(); var armory = saves.GetSave<ArmorySave>("Armory"); armory.Init();
                var legacy = armory.AddItem("legacy-equipped"); legacy.IsEquipped = true;
                JsonUtility.FromJsonOverwrite("{\"level\":2}", legacy);
                var inventory = saves.GetSave<CraftingInventorySave>("Crafting Inventory");
                EquipmentCrafting service = null;
                CraftResult reentrant = CraftResult.Success; bool coherent = false; int unlocks = 0;
                service = new EquipmentCrafting(database, armory, inventory, saves, id =>
                {
                    unlocks++; reentrant = service.TryCraft(id);
                    coherent = armory.GetItem(id) != null && inventory.Count(CraftingInventorySave.FragmentKey(id)) == 0;
                });
                check(service.GetQuote(null).result == CraftResult.InvalidRecipe && service.TryCraft("unknown") == CraftResult.InvalidRecipe, "invalid identities rejected");
                check(a.Crafting.requiredFragments == 30 && a.Crafting.IsValid(), "30 fragments and four valid material requirements");
                check(!service.AddLoot(new CraftingLoot { isFragment = true, itemId = "missing", amount = 1 }) && !service.AddLoot(new CraftingLoot { material = (CraftingMaterial)99, amount = 1 }), "unknown resources rejected");
                check(!service.AddLoot(new CraftingLoot { material = CraftingMaterial.Iron, amount = -1 }), "negative loot rejected");
                service.AddLoot(Fragment(a, 29)); service.AddLoot(Fragment(b, 30));
                check(service.GetQuote(a.Id).result == CraftResult.MissingFragments && service.TryCraft(a.Id) == CraftResult.MissingFragments, "29 fragments cannot craft");
                check(inventory.Count(CraftingInventorySave.FragmentKey(b.Id)) == 30, "fragment identities are independent");
                service.AddLoot(Fragment(a, 1));
                check(service.GetQuote(a.Id).result == CraftResult.MissingMaterials && armory.GetItem(a.Id) == null && saves.SaveCalls == 0, "30 fragments still require all materials and rejection is side-effect free");
                for (int i = 0; i < a.Crafting.materials.Count - 1; i++) service.AddLoot(Material(a.Crafting.materials[i].material, a.Crafting.materials[i].amount));
                check(service.TryCraft(a.Id) == CraftResult.MissingMaterials && inventory.Count(CraftingInventorySave.FragmentKey(a.Id)) == 30 && inventory.Count(CraftingInventorySave.MaterialKey(CraftingMaterial.Iron)) == 10, "missing fourth material consumes nothing");
                service.AddLoot(Material(CraftingMaterial.Leather, a.Crafting.Cost(CraftingMaterial.Leather)));
                check(service.GetQuote(a.Id).result == CraftResult.Ready && service.GetQuote(a.Id).output == a.ItemLevels[0], "output is existing ItemData level zero");
                check(service.TryCraft(a.Id) == CraftResult.Success && coherent && reentrant == CraftResult.Busy && unlocks == 1, "atomic unlock and callback reentrancy guard");
                check(armory.GetItem(a.Id).Level == 0 && !armory.GetItem(a.Id).IsEquipped && legacy.Level == 2 && legacy.IsEquipped && saves.SaveCalls == 1, "new level-one item does not replace loadout or old upgrade data");
                check(inventory.Count(CraftingInventorySave.FragmentKey(a.Id)) == 0 && inventory.Count(CraftingInventorySave.FragmentKey(b.Id)) == 30, "only selected equipment fragments consumed");
                foreach (CraftingMaterial material in Enum.GetValues(typeof(CraftingMaterial))) check(inventory.Count(CraftingInventorySave.MaterialKey(material)) == 0, "each material charged exactly once");
                Fill(service, a);
                check(service.TryCraft(a.Id) == CraftResult.AlreadyOwned && armory.GetItem(a.Id).Level == 0 && saves.SaveCalls == 1 && inventory.Count(CraftingInventorySave.FragmentKey(a.Id)) == 30, "owned recipe never consumes resources or grants a duplicate level");
                armory.Flush();
                var restoredArmory = JsonUtility.FromJson<ArmorySave>(JsonUtility.ToJson(armory)); restoredArmory.Init();
                var restoredInventory = JsonUtility.FromJson<CraftingInventorySave>(JsonUtility.ToJson(inventory));
                var restored = new EquipmentCrafting(database, restoredArmory, restoredInventory, new MemorySaves());
                check(restored.GetQuote(a.Id).result == CraftResult.AlreadyOwned && restoredInventory.Count(CraftingInventorySave.FragmentKey(b.Id)) == 30, "ownership and resource save roundtrip");
                var settings = new CraftingLootSettings { chance = 100, fragmentProbability = 1, minimumAmount = 2, maximumAmount = 4 };
                check(restored.TryRoll(settings, 100, 0, 0, 999, out var loot) && loot.isFragment && loot.itemId == b.Id && loot.amount == 4, "100 percent boundary, quantity clamp and owned targets excluded");
                settings.chance = 0; check(!restored.TryRoll(settings, 0, 0, 0, 1, out _), "disabled loot profile produces no drops");
                settings.chance = 12;
                check(restored.TryRoll(settings, 11.9f, 0, 0, 2, out _) && !restored.TryRoll(settings, 12, 0, 0, 2, out _), "chance boundary is deterministic");
                restoredArmory.AddItem(b.Id); restored.OnItemUnlocked(b.Id); settings.chance = 100;
                check(restored.TryRoll(settings, 0, 0, 0, 2, out loot) && !loot.isFragment, "all-owned accounts receive materials instead of unusable fragments");
                a.Crafting.materials[a.Crafting.materials.Count - 1].material = CraftingMaterial.Iron;
                check(!a.Crafting.IsValid() && service.GetQuote(a.Id).result == CraftResult.InvalidRecipe, "duplicate material IDs invalidate recipe");
                a.Crafting.materials[a.Crafting.materials.Count - 1].material = CraftingMaterial.Leather;
                var failureSave = new MemorySaves(); var failureArmory = failureSave.GetSave<ArmorySave>("Armory"); failureArmory.Init();
                var failure = new EquipmentCrafting(database, failureArmory, new CraftingInventorySave(), failureSave, id => { throw new InvalidOperationException("test listener"); });
                Fill(failure, a); try { failure.TryCraft(a.Id); } catch (InvalidOperationException) { }
                check(failureArmory.GetItem(a.Id) != null && failure.Inventory.Count(CraftingInventorySave.FragmentKey(a.Id)) == 0 && failureSave.SaveCalls == 1 && failure.TryCraft(a.Id) == CraftResult.AlreadyOwned, "throwing observer preserves complete transaction and releases lock");
                var overflow = new CraftingInventorySave(); overflow.Add("test", long.MaxValue); overflow.Add("test", 1);
                check(overflow.Count("test") == long.MaxValue && !overflow.Add(null, 10), "resource overflow saturates safely");
                var config = Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath);
                var fullBag = new ArmorySave(); fullBag.Init();
                for (int i = 0; i < 40; i++) fullBag.AddItem("bag-test-" + i);
                var fullInventory = new CraftingInventorySave(); var fullPersistence = new MemorySaves();
                var fullService = new EquipmentCrafting(database, fullBag, fullInventory, fullPersistence);
                Fill(fullService, b);
                check(fullService.TryCraft(b.Id) == CraftResult.InventoryFull && fullPersistence.SaveCalls == 0 && fullBag.GetItem(b.Id) == null, "full bag blocks crafting before unlock and persistence");
                check(fullInventory.Count(CraftingInventorySave.FragmentKey(b.Id)) == 30 && fullInventory.Count(CraftingInventorySave.MaterialKey(CraftingMaterial.Iron)) == b.Crafting.Cost(CraftingMaterial.Iron), "full bag consumes no fragments or materials");
                check(config != null && config.bossChestPrefab != null && config.bossChestPrefab.GetComponent<BossChestDropBehavior>() != null && config.materials.Count == 3, "boss chest prefab and three active material definitions registered");
                return "Crafting: " + checks + " isolated checks passed. Player data untouched.";
            }
            finally { UnityEngine.Object.DestroyImmediate(database); UnityEngine.Object.DestroyImmediate(a); UnityEngine.Object.DestroyImmediate(b); }
        }
        public static string VerifyLiveButtons()
        {
            var page = GameController.MainMenuScreenBehavior.CurrentPage as ForgePageBehavior;
            if (!Application.isPlaying || page == null || !GameController.MainMenuScreenBehavior.IsDockEnabled) throw new Exception("Open Forge first.");
            var data = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var type = typeof(ForgePageBehavior); var serviceField = type.GetField("service", Fields);
            var originalService = serviceField.GetValue(page); var originalId = type.GetField("selectedId", Fields).GetValue(page);
            var originalIndex = type.GetField("selectedIndex", Fields).GetValue(page);
            var pool = (List<ItemData>)type.GetField("owned", Fields).GetValue(page); var originalPool = new List<ItemData>(pool);
            var feedback = (TMPro.TMP_Text)type.GetField("feedback", Fields).GetValue(page); var originalFeedback = feedback.text;
            var root = new GameObject("Isolated Crafting Verification") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var saves = new MemorySaves(); var armory = saves.GetSave<ArmorySave>("Armory"); armory.Init();
                var manager = root.AddComponent<CraftingManager>(); manager.Initialize(Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath), data.armory, armory, saves, null);
                var service = manager.Service; var item = service.Recipes[0];
                Fill(service, item); pool.Clear(); pool.Add(item); serviceField.SetValue(page, service);
                type.GetField("selectedIndex", Fields).SetValue(page, 0); type.GetField("nextForgeTime", Fields).SetValue(page, 0f);
                type.GetMethod("BindSelected", Fields).Invoke(page, null); type.GetMethod("Refresh", Fields).Invoke(page, null);
                var button = page.transform.Find("Upgrade Details - ui_slot sliced/Forge").GetComponent<Button>();
                if (!button.interactable) throw new Exception("Ready recipe button disabled.");
                button.onClick.Invoke(); button.onClick.Invoke(); type.GetMethod("Refresh", Fields).Invoke(page, null);
                var label = button.GetComponentInChildren<TMPro.TMP_Text>();
                if (armory.GetItem(item.Id) == null || armory.GetItem(item.Id).Level != 0 || button.interactable || label.text != "OWNED" || service.Inventory.Count(CraftingInventorySave.FragmentKey(item.Id)) != 0) throw new Exception("Craft button failed one-time unlock.");
                var dropRoot = new GameObject("Isolated Pooled Crafting Drop"); dropRoot.SetActive(false);
                var drop = dropRoot.AddComponent<CraftingDropBehavior>();
                var dropData = new DropData(DropType.CraftingResource, dropRoot, 1);
                try
                {
                    dropRoot.SetActive(true); drop.Configure(Material(CraftingMaterial.Iron, 2), manager, dropData);
                    drop.OnPickedUp(); drop.OnPickedUp();
                    if (service.Inventory.Count(CraftingInventorySave.MaterialKey(CraftingMaterial.Iron)) != 2) throw new Exception("Pickup counted twice.");
                    dropRoot.SetActive(true); drop.Configure(Fragment(item, 3), manager, dropData); drop.OnPickedUp();
                    if (service.Inventory.Count(CraftingInventorySave.FragmentKey(item.Id)) != 3 || service.Inventory.Count(CraftingInventorySave.MaterialKey(CraftingMaterial.Iron)) != 2) throw new Exception("Pooled payload leaked across reuse.");
                }
                finally { UnityEngine.Object.DestroyImmediate(dropRoot); }
                return "Live crafting passed: actual FORGE onClick, one unlock, no duplicate level, OWNED lock, one-time pickup and pooled payload reset. Player data untouched.";
            }
            finally
            {
                pool.Clear(); pool.AddRange(originalPool); serviceField.SetValue(page, originalService);
                type.GetField("selectedId", Fields).SetValue(page, originalId); type.GetField("selectedIndex", Fields).SetValue(page, originalIndex);
                type.GetField("nextForgeTime", Fields).SetValue(page, 0f); type.GetMethod("BindSelected", Fields).Invoke(page, null);
                type.GetMethod("Refresh", Fields).Invoke(page, null); feedback.text = originalFeedback;
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
        private static CraftingLoot Fragment(ItemData item, int amount) => new CraftingLoot { isFragment = true, itemId = item.Id, amount = amount };
        public static string VerifyLiveLoot()
        {
            if (!Application.isPlaying) throw new Exception("Play Mode required.");
            var root = new GameObject("Isolated Crafting Loot Integration") { hideFlags = HideFlags.HideAndDontSave };
            var config = UnityEngine.Object.Instantiate(Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath));
            var armoryManager = GameController.ArmoryManager;
            var craftingProperty = typeof(ArmoryManager).GetProperty("Crafting"); var originalManager = craftingProperty.GetValue(armoryManager);
            var achievements = typeof(GameController).GetProperty("AchievementsManager"); var originalAchievements = achievements.GetValue(null);
            var originalDrops = StageController.DropManager;
            try
            {
                config.enemy.chance = config.boss.chance = config.chest.chance = 100;
                config.enemy.fragmentProbability = config.boss.fragmentProbability = config.chest.fragmentProbability = 1;
                config.enemy.minimumAmount = config.enemy.maximumAmount = 1;
                config.boss.minimumAmount = config.boss.maximumAmount = 2;
                config.chest.minimumAmount = config.chest.maximumAmount = 3;
                var saves = new MemorySaves(); var armory = saves.GetSave<ArmorySave>("Armory"); armory.Init();
                var data = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
                var manager = root.AddComponent<CraftingManager>(); manager.Initialize(config, data.armory, armory, saves, null);
                craftingProperty.SetValue(armoryManager, manager); achievements.SetValue(null, null);
                // Inactive manager avoids scene/bootstrap registration; its real
                // pool, DropCrafting, pickup and cleanup methods remain in use.
                var dropManagerObject = new GameObject("Isolated DropManager"); dropManagerObject.transform.SetParent(root.transform); dropManagerObject.SetActive(false);
                var drops = dropManagerObject.AddComponent<DropManager>();
                var prototype = new GameObject("Isolated Resource Prototype"); prototype.transform.SetParent(root.transform); prototype.SetActive(false);
                prototype.AddComponent<CraftingDropBehavior>();
                var resource = new DropData(DropType.CraftingResource, prototype, 2);
                typeof(DropManager).GetField("craftingDropData", Fields).SetValue(drops, resource);
                var pool = new PoolComponent<DropBehavior>("Isolated Resource Pool", prototype, 2, root.transform);
                var pools = (Dictionary<DropType, PoolComponent<DropBehavior>>)typeof(DropManager).GetField("dropPools", Fields).GetValue(drops);
                pools.Add(DropType.CraftingResource, pool); StageController.RegisterDropManager(drops);
                foreach (CraftingLootSource source in Enum.GetValues(typeof(CraftingLootSource))) manager.DropLoot(source, Vector3.zero);
                var list = (List<DropBehavior>)typeof(DropManager).GetField("dropList", Fields).GetValue(drops);
                if (list.Count != 0) throw new Exception("Legacy enemy/boss/chest resource profiles still spawn drops.");
                var picked = new List<DropBehavior>(list);
                foreach (var drop in picked) { drop.OnPickedUp(); drop.OnPickedUp(); }
                long fragments = 0;
                foreach (var item in manager.Service.Recipes) fragments += manager.Service.Inventory.Count(CraftingInventorySave.FragmentKey(item.Id));
                if (fragments != 0 || list.Count != 0) throw new Exception("Legacy profiles granted resources.");
                var chestObject = new GameObject("Isolated Chest Callback"); chestObject.transform.SetParent(root.transform); chestObject.SetActive(false);
                var collider = chestObject.AddComponent<SphereCollider>(); collider.isTrigger = true;
                var chest = chestObject.AddComponent<GoodChestBehavior>();
                typeof(ChestBehavior).GetField("trigger", Fields).SetValue(chest, collider); chestObject.SetActive(true);
                chest.Spawn(new ChestSpawnData(ChestType.Good, chestObject.transform));
                chest.OnOpenAnimationEndedEventFired(); chest.OnOpenAnimationEndedEventFired();
                if (list.Count != 0) throw new Exception("Ability chest still produces crafting loot.");
                chest.Spawn(new ChestSpawnData(ChestType.Good, chestObject.transform)); chest.OnOpenAnimationEndedEventFired();
                if (list.Count != 0) throw new Exception("Reused ability chest still produces crafting loot.");
                drops.HideAllDrop();
                if (list.Count != 0) throw new Exception("Drop cleanup left registered drops.");
                foreach (var drop in root.GetComponentsInChildren<CraftingDropBehavior>(true))
                    if (drop.gameObject.activeSelf) throw new Exception("Drop cleanup skipped a pooled object.");
                return "Live loot passed: legacy enemy/boss/ability-chest profiles disabled even at 100 percent chance; repeated and reused ability chests grant no resources.";
            }
            finally
            {
                craftingProperty.SetValue(armoryManager, originalManager); achievements.SetValue(null, originalAchievements);
                StageController.RegisterDropManager(originalDrops);
                UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(config);
            }
        }
        private static CraftingLoot Material(CraftingMaterial material, int amount) => new CraftingLoot { material = material, amount = amount };
        private static void Fill(EquipmentCrafting service, ItemData item)
        { service.AddLoot(Fragment(item, item.Crafting.requiredFragments)); foreach (var cost in item.Crafting.materials) service.AddLoot(Material(cost.material, cost.amount)); }
        private class MemorySaves : ISaveManager
        {
            private readonly Dictionary<int, ISave> entries = new Dictionary<int, ISave>(); public int SaveCalls;
            public T GetSave<T>(int id) where T : ISave, new() { if (!entries.ContainsKey(id)) entries[id] = new T(); return (T)entries[id]; }
            public T GetSave<T>(string id) where T : ISave, new() => GetSave<T>(id.GetHashCode());
            public void Save(bool multithreading = false) { SaveCalls++; }
        }
    }
}
