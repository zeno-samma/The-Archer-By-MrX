using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MrX.Crafting;
using MrX.Drop;
using MrX.Enemy;
using MrX.Pool;
using MrX.Rewards;
using MrX.Save;
using MrX.UI;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class LootInventoryVerification
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        public static string Verify() => RunSettlementVerification.Verify();
        public static string VerifyLiveBoss()
        {
            if (!Application.isPlaying) throw new Exception("Play required");
            var root = new GameObject("Isolated boss loot checks"); root.SetActive(false);
            var originalDrops = StageController.DropManager; var saveProp = typeof(GameController).GetProperty("SaveManager"); var originalSaves = saveProp.GetValue(null);
            var owner = GameController.ArmoryManager; var bossProp = typeof(ArmoryManager).GetProperty("BossChests"); var originalBoss = bossProp.GetValue(owner);
            try
            {
                var saves = new MemorySaves(); saveProp.SetValue(null, saves);
                var delivery = new Delivery(); var service = new BossChestService(null, delivery, saves); bossProp.SetValue(owner, service);
                var prototype = new GameObject("Boss chest prototype"); prototype.transform.SetParent(root.transform); prototype.SetActive(false); prototype.AddComponent<BossChestDropBehavior>();
                var drops = root.AddComponent<DropManager>();
                var data = new DropData(DropType.BossChest, prototype, 2);
                typeof(DropManager).GetField("bossChestData", Fields).SetValue(drops, data);
                var pool = new PoolComponent<DropBehavior>("Test chest pool", prototype, 2, root.transform);
                ((Dictionary<DropType, PoolComponent<DropBehavior>>)typeof(DropManager).GetField("dropPools", Fields).GetValue(drops)).Add(DropType.BossChest, pool);
                StageController.RegisterDropManager(drops);
                var enemyRoot = new GameObject("Isolated enemy"); enemyRoot.SetActive(false); enemyRoot.transform.SetParent(root.transform);
                var enemy = enemyRoot.AddComponent<BatEnemyBehavior>(); var enemyData = JsonUtility.FromJson<EnemyData>("{\"isBoss\":false,\"drops\":[{\"dropType\":4},{\"dropType\":5}]}");
                typeof(EnemyBehavior).GetProperty("Data").SetValue(enemy, enemyData); typeof(EnemyBehavior).GetProperty("UseDrop").SetValue(enemy, true);
                enemy.Drop(0);
                var list = (List<DropBehavior>)typeof(DropManager).GetField("dropList", Fields).GetValue(drops);
                if (list.Count != 0 || saves.GetSave<BossChestSave>("Boss Chests").receipts.Count != 0) throw new Exception("Small enemy generated rare loot");
                JsonUtility.FromJsonOverwrite("{\"isBoss\":true,\"drops\":[]}", enemyData); enemy.Drop(0); enemy.Drop(0);
                if (list.Count != 1 || saves.GetSave<BossChestSave>("Boss Chests").receipts.Count != 1) throw new Exception("Boss did not create exactly one chest");
                var chest = list[0]; chest.OnPickedUp(); chest.OnPickedUp();
                if (delivery.Total != 0 || !saves.GetSave<BossChestSave>("Boss Chests").receipts[0].collected) throw new Exception("Invalid boss reward delivery");
                int before = delivery.Total; service.RecoverPending(); if (delivery.Total != before || list.Count != 0) throw new Exception("Boss reward duplicated");
                typeof(EnemyBehavior).GetField("bossChestDropped", Fields).SetValue(enemy, false); enemy.Drop(0);
                if (list.Count != 0) throw new Exception("Resume spawned claimed boss chest");
                return "Live loot: small enemy blocked; boss creates one pooled chest; repeated pickup and replay cannot duplicate reward.";
            }
            finally { StageController.RegisterDropManager(originalDrops); saveProp.SetValue(null, originalSaves); bossProp.SetValue(owner, originalBoss); UnityEngine.Object.DestroyImmediate(root); }
        }
        public static string VerifyLiveInventory()
        {
            if (string.IsNullOrEmpty(UnityEditor.SessionState.GetString(SerializationHelper.VerificationDirectoryKey, ""))) throw new Exception("An isolated VerificationSession is required before granting test stacks");
            var ui = UnityEngine.Object.FindAnyObjectByType<ArmoryDashboard>(); if (!Application.isPlaying || !ui) throw new Exception("Open Armory first");
            var service = GameController.ArmoryManager.Crafting.Service; var item = service.Recipes[0];
            service.AddLoot(new CraftingLoot { isFragment = true, itemId = item.Id, amount = 12 }); service.AddLoot(new CraftingLoot { isFragment = true, itemId = item.Id, amount = 18 });
            foreach (var material in GameController.ArmoryManager.Crafting.Database.materials) service.AddLoot(new CraftingLoot { material = material.material, amount = 5 });
            typeof(ArmoryDashboard).GetMethod("Refresh", Fields).Invoke(ui, null);
            var stacks = (IList)typeof(ArmoryDashboard).GetField("visibleStacks", Fields).GetValue(ui);
            if (stacks.Count != 4) throw new Exception("Expected one fragment stack and three material stacks");
            foreach (var cell in (IList)typeof(ArmoryDashboard).GetField("cells", Fields).GetValue(ui))
            {
                var t = cell.GetType(); var icon = (UnityEngine.UI.Image)t.GetField("icon").GetValue(cell); var label = (TMPro.TMP_Text)t.GetField("label").GetValue(cell);
                if (t.GetField("item").GetValue(cell) == null && t.GetField("stack").GetValue(cell) == null && icon.gameObject.activeInHierarchy)
                    if (icon.sprite.name != "ui_inventory_empty" || label.gameObject.activeSelf || icon.color != Color.white) throw new Exception("Empty slot art not bound correctly");
            }
            return "Live inventory: 12+18 fragments in one stack; three material stacks; empty slots use full-color supplied art with no weapon overlay.";
        }
        private sealed class Delivery : ILuckyRewardDelivery
        {
            public int Total; public bool Valid = true;
            public bool Resolve(LuckyPrize p, int amount, int cost, out LuckyGrant grant) { grant = new LuckyGrant { kind = p.kind, amount = amount }; return Valid; }
            public void Grant(LuckyGrant g) { Total += g.amount; } public Sprite Icon(LuckyGrant g) => null;
        }
        private sealed class MemorySaves : ISaveManager
        {
            public Dictionary<string, ISave> entries = new Dictionary<string, ISave>(); public int Calls;
            public T GetSave<T>(string key) where T : ISave, new() { if (!entries.ContainsKey(key)) entries[key] = new T(); return (T)entries[key]; }
            public T GetSave<T>(int key) where T : ISave, new() => GetSave<T>(key.ToString()); public void Save(bool multithreading = false) { Calls++; }
        }
    }
}

