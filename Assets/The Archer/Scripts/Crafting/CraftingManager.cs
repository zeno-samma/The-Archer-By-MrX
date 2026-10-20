using System;
using MrX.Armory;
using MrX.Save;
using UnityEngine;

namespace MrX.Crafting
{
    public class CraftingManager : MonoBehaviour
    {
        public CraftingDatabase Database { get; private set; }
        public EquipmentCrafting Service { get; private set; }
        public event Action Changed;
        private ISaveManager saves;
        private bool dirty;
        private float nextSave;
        public void Initialize(CraftingDatabase database, ArmoryDatabase armoryDatabase, ArmorySave armory, ISaveManager persistence, Action<string> unlocked)
        {
            Database = database; saves = persistence;
            Service = new EquipmentCrafting(armoryDatabase, armory, saves.GetSave<CraftingInventorySave>("Crafting Inventory"), saves, unlocked);
            Service.Changed += OnChanged; enabled = false;
        }
        private void OnChanged(bool saved)
        { dirty = !saved; enabled = dirty; Changed?.Invoke(); }
        private void LateUpdate() { if (dirty && Time.unscaledTime >= nextSave) FlushPending(); }
        public void FlushPending()
        { if (!dirty) return; dirty = false; enabled = false; nextSave = Time.unscaledTime + 2; saves.Save(false); }
        public void DropLoot(CraftingLootSource source, Vector3 position)
        {
            // Legacy entry point retained for callers/assets. Combat no longer
            // grants crafting resources; fragments come from the boss chest table.
        }
        private void OnDestroy() { if (Service != null) Service.Changed -= OnChanged; }
    }
}
