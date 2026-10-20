using System;
using System.Collections.Generic;
using MrX.Save;
using UnityEngine;

namespace MrX.Crafting
{
    [Serializable] public class CraftingStack { public string id; public long amount; }
    [Serializable] public class CraftingInventorySave : ISave
    {
        [SerializeField] private List<CraftingStack> stacks = new List<CraftingStack>();
        [NonSerialized] private Dictionary<string, CraftingStack> lookup;
        public static string FragmentKey(string itemId) => "fragment:" + itemId;
        public static string MaterialKey(CraftingMaterial material)
        {
            switch (material) { case CraftingMaterial.Iron: return "material:iron"; case CraftingMaterial.Crystal: return "material:crystal";
                case CraftingMaterial.Scroll: return "material:scroll"; case CraftingMaterial.Leather: return "material:leather"; case CraftingMaterial.Wood: return "material:wood"; default: return null; }
        }
        private void Initialize()
        {
            if (lookup != null) return;
            lookup = new Dictionary<string, CraftingStack>(StringComparer.Ordinal);
            if (stacks == null) stacks = new List<CraftingStack>();
            foreach (var stack in stacks)
            {
                if (stack == null || string.IsNullOrEmpty(stack.id)) continue;
                stack.amount = Math.Max(0, stack.amount);
                if (lookup.TryGetValue(stack.id, out var prior)) prior.amount = Math.Max(prior.amount, stack.amount);
                else lookup.Add(stack.id, stack);
            }
            stacks = new List<CraftingStack>(lookup.Values);
        }
        public long Count(string id) { Initialize(); return id != null && lookup.TryGetValue(id, out var stack) ? stack.amount : 0; }
        public bool Add(string id, long amount)
        {
            if (string.IsNullOrEmpty(id) || amount <= 0) return false;
            var before = Count(id); var after = before > long.MaxValue - amount ? long.MaxValue : before + amount;
            if (after == before) return false;
            if (!lookup.TryGetValue(id, out var stack)) { stack = new CraftingStack { id = id }; lookup.Add(id, stack); stacks.Add(stack); }
            stack.amount = after; return true;
        }
        internal void SpendValidated(string id, int amount) { lookup[id].amount -= amount; }
        public void Flush() { Initialize(); }
    }
}
