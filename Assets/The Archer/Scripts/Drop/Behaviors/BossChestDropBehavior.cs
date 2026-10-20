using MrX.Rewards;
using UnityEngine;

namespace MrX.Drop
{
    public sealed class BossChestDropBehavior : DropBehavior
    {
        private BossChestService service;
        private BossChestReceipt receipt;
        private bool picked;
        public LuckyGrant Reward => receipt?.grant;
        public void Configure(BossChestService owner, BossChestReceipt reward, DropData data)
        { service = owner; receipt = reward; picked = false; Init(data, 0); }
        public override void OnPickedUp()
        {
            if (picked || service == null || receipt == null) return;
            if (receipt.claimed || receipt.collected || !service.Claim(receipt)) return;
            picked = true; base.OnPickedUp(); gameObject.SetActive(false);
        }
        protected override void OnDisable()
        { base.OnDisable(); service = null; receipt = null; picked = false; }
    }
}
