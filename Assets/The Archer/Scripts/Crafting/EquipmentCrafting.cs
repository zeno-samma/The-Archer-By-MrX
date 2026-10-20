using System;
using System.Collections.Generic;
using MrX.Armory;
using MrX.Save;

namespace MrX.Crafting
{
    public struct CraftQuote
    { public ItemData item; public ItemLevel output; public CraftResult result; public long fragments; public int requiredFragments; }
    public sealed class EquipmentCrafting
    {
        public CraftingInventorySave Inventory { get; }
        public IReadOnlyList<ItemData> Recipes => recipes;
        public event Action<bool> Changed;
        private readonly List<ItemData> recipes = new List<ItemData>();
        private readonly List<ItemData> fragmentTargets = new List<ItemData>();
        private readonly Dictionary<string, ItemData> byId = new Dictionary<string, ItemData>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> fragmentKeys = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly ArmorySave armory;
        private readonly ISaveManager persistence;
        private readonly Action<string> onUnlocked;
        private bool crafting;
        public EquipmentCrafting(ArmoryDatabase database, ArmorySave save, CraftingInventorySave inventory, ISaveManager saves, Action<string> unlocked = null)
        {
            armory = save; Inventory = inventory; persistence = saves; onUnlocked = unlocked;
            foreach (var item in database.Items)
                if (item != null && !string.IsNullOrEmpty(item.Id) && item.Crafting != null && item.Crafting.enabled && !byId.ContainsKey(item.Id))
                {
                    byId.Add(item.Id, item); fragmentKeys.Add(item.Id, CraftingInventorySave.FragmentKey(item.Id)); recipes.Add(item);
                    if (item.Crafting.IsValid() && item.ItemLevels != null && item.ItemLevels.Count > 0 && item.ItemLevels[0] != null && item.ItemLevels[0].Stats != null && armory.GetItem(item.Id) == null) fragmentTargets.Add(item);
                }
        }
        public bool IsOwned(string id) => armory.GetItem(id) != null;
        public CraftQuote GetQuote(string id)
        {
            var quote = new CraftQuote { result = CraftResult.InvalidRecipe };
            if (string.IsNullOrEmpty(id) || !byId.TryGetValue(id, out quote.item)) return quote;
            var recipe = quote.item.Crafting;
            quote.requiredFragments = recipe.requiredFragments; quote.fragments = Inventory.Count(fragmentKeys[id]);
            if (!recipe.IsValid() || quote.item.ItemLevels == null || quote.item.ItemLevels.Count == 0 || quote.item.ItemLevels[0] == null || quote.item.ItemLevels[0].Stats == null) return quote;
            quote.output = quote.item.ItemLevels[0];
            if (IsOwned(id)) { quote.result = CraftResult.AlreadyOwned; return quote; }
            if (!armory.HasBagSpace) { quote.result = CraftResult.InventoryFull; return quote; }
            if (quote.fragments < recipe.requiredFragments) { quote.result = CraftResult.MissingFragments; return quote; }
            foreach (var cost in recipe.materials)
                if (Inventory.Count(CraftingInventorySave.MaterialKey(cost.material)) < cost.amount) { quote.result = CraftResult.MissingMaterials; return quote; }
            quote.result = CraftResult.Ready; return quote;
        }
        public CraftResult TryCraft(string id)
        {
            if (crafting) return CraftResult.Busy;
            var quote = GetQuote(id); if (quote.result != CraftResult.Ready) return quote.result;
            crafting = true; bool committed = false;
            try
            {
                // No callbacks until costs and ownership are both committed.
                Inventory.SpendValidated(fragmentKeys[id], quote.requiredFragments);
                foreach (var cost in quote.item.Crafting.materials) Inventory.SpendValidated(CraftingInventorySave.MaterialKey(cost.material), cost.amount);
                armory.AddItem(id); committed = true;
                RemoveFragmentTarget(id); onUnlocked?.Invoke(id);
                return CraftResult.Success;
            }
            finally
            {
                try { if (committed) { try { persistence.Save(false); } finally { Changed?.Invoke(true); } } }
                finally { crafting = false; }
            }
        }
        public void OnItemUnlocked(string id)
        {
            RemoveFragmentTarget(id);
            Changed?.Invoke(false);
        }
        private void RemoveFragmentTarget(string id)
        {
            for (var i = fragmentTargets.Count - 1; i >= 0; i--) if (fragmentTargets[i].Id == id) fragmentTargets.RemoveAt(i);
        }
        public bool AddLoot(CraftingLoot loot)
        {
            if (loot.amount <= 0) return false;
            string key;
            if (loot.isFragment) { if (loot.itemId == null || !fragmentKeys.TryGetValue(loot.itemId, out key)) return false; }
            else { key = CraftingInventorySave.MaterialKey(loot.material); if (key == null) return false; }
            if (!Inventory.Add(key, loot.amount)) return false;
            Changed?.Invoke(false); return true;
        }
        // Random inputs are passed in so tests can verify boundaries without
        // modifying Unity's global random sequence or any player state.
        public bool TryRoll(CraftingLootSettings settings, float chanceRoll, float kindRoll, int selection, int amount, out CraftingLoot loot)
        {
            loot = default;
            if (settings == null || settings.chance <= 0 || (settings.chance < 100 && chanceRoll >= settings.chance) || settings.minimumAmount < 1 || settings.maximumAmount > 100000 || settings.maximumAmount < settings.minimumAmount) return false;
            loot.amount = Math.Max(settings.minimumAmount, Math.Min(settings.maximumAmount, amount));
            if (kindRoll < settings.fragmentProbability && fragmentTargets.Count > 0)
            { loot.isFragment = true; loot.itemId = fragmentTargets[(int)((uint)selection % (uint)fragmentTargets.Count)].Id; }
            else loot.material = (CraftingMaterial)((uint)selection % 4u);
            return true;
        }
    }
}
