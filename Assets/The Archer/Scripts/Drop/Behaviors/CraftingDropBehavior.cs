using MrX.Crafting;
using MrX.Easing;
using UnityEngine;

namespace MrX.Drop
{
    public class CraftingDropBehavior : DropBehavior
    {
        private CraftingLoot payload;
        private CraftingManager owner;
        private bool consumed = true;
        private IEasingCoroutine pendingSpawn;
        public void Configure(CraftingLoot loot, CraftingManager manager, DropData data)
        {
            payload = loot; owner = manager; consumed = false;
            Init(data, 0);
        }
        public override void Init(DropData data, float delay)
        {
            pendingSpawn?.Stop(); dropData = data;
            if (trail != null) trail.Clear();
            transform.localScale = Vector3.zero;
            if (trigger != null) trigger.enabled = false;
            IsSpawning = true; IsFlyingToPlayer = false;
            pendingSpawn = EasingManager.DoAfter(delay, Spawn);
        }
        public override void OnPickedUp()
        {
            if (consumed || owner == null || owner.Service == null) return;
            consumed = true;
            try { owner.Service.AddLoot(payload); }
            finally
            {
                try { base.OnPickedUp(); }
                finally { gameObject.SetActive(false); }
            }
        }
        protected override void OnDisable()
        {
            pendingSpawn?.Stop(); pendingSpawn = null;
            base.OnDisable();
            owner = null; payload = default; consumed = true; IsSpawning = IsFlyingToPlayer = false;
        }
    }
}
