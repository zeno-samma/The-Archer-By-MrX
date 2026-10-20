using System;
using System.Collections.Generic;
using UnityEngine;

namespace MrX.Crafting
{
    public enum CraftingMaterial { Iron = 0, Crystal = 1, Scroll = 2, Leather = 3, Wood = 4 }
    public enum CraftingLootSource { Enemy = 0, Boss = 1, Chest = 2 }
    public enum CraftResult { Ready, Success, AlreadyOwned, MissingFragments, MissingMaterials, InvalidRecipe, Busy, InventoryFull }
    [Serializable] public class MaterialCost { public CraftingMaterial material; [Min(1)] public int amount = 1; }
    [Serializable] public class CraftingRecipe
    {
        public bool enabled;
        [Min(1)] public int requiredFragments = 30;
        public List<MaterialCost> materials = new List<MaterialCost>
        {
            new MaterialCost { material = CraftingMaterial.Iron, amount = 10 },
            new MaterialCost { material = CraftingMaterial.Wood, amount = 5 },
            new MaterialCost { material = CraftingMaterial.Leather, amount = 2 }
        };
        public int Cost(CraftingMaterial type)
        { if (materials != null) foreach (var cost in materials) if (cost != null && cost.material == type) return cost.amount; return 0; }
        public bool IsValid()
        {
            if (!enabled || requiredFragments < 1 || materials == null || materials.Count < 1) return false;
            var seen = new HashSet<CraftingMaterial>();
            foreach (var cost in materials)
            {
                if (cost == null || cost.amount < 1 || !Enum.IsDefined(typeof(CraftingMaterial), cost.material) || !seen.Add(cost.material)) return false;
            }
            return true;
        }
    }
    [Serializable] public class CraftingMaterialData { public CraftingMaterial material; public string title; public Sprite icon; }
    [Serializable] public class CraftingLootSettings
    {
        [Range(0, 100)] public float chance = 12;
        [Range(0, 1)] public float fragmentProbability = .5f;
        [Min(1)] public int minimumAmount = 1;
        [Min(1)] public int maximumAmount = 1;
    }
    public struct CraftingLoot
    { public bool isFragment; public string itemId; public CraftingMaterial material; public int amount; }

    [CreateAssetMenu(menuName = "MrX Studio/Crafting/Database", fileName = "Crafting Database")]
    public class CraftingDatabase : ScriptableObject
    {
        public const string ResourcePath = "Crafting Database";
        public GameObject dropPrefab;
        public GameObject bossChestPrefab;
        public Rewards.BossChestRewards defaultBossRewards;
        [Min(0)] public int initialPoolSize = 12;
        [HideInInspector] public CraftingLootSettings enemy = new CraftingLootSettings { chance = 0 };
        [HideInInspector] public CraftingLootSettings boss = new CraftingLootSettings { chance = 0 };
        [HideInInspector] public CraftingLootSettings chest = new CraftingLootSettings { chance = 0 };
        public List<CraftingMaterialData> materials = new List<CraftingMaterialData>
        {
            new CraftingMaterialData { material = CraftingMaterial.Iron, title = "Iron" },
            new CraftingMaterialData { material = CraftingMaterial.Wood, title = "Wood" },
            new CraftingMaterialData { material = CraftingMaterial.Leather, title = "Leather" }
        };
        public CraftingLootSettings Settings(CraftingLootSource source) => source == CraftingLootSource.Boss ? boss : source == CraftingLootSource.Chest ? chest : enemy;
        public CraftingMaterialData Material(CraftingMaterial type)
        { foreach (var data in materials) if (data != null && data.material == type) return data; return null; }
    }
}
