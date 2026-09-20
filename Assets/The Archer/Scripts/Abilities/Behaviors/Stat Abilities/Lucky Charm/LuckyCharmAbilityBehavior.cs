using OctoberStudio.Drop;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class LuckyCharmAbilityBehavior : AbilityBehavior<LuckyCharmAbilityData, LuckyCharmAbilityLevel>
    {
        protected StatMultiplier maxHPMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);

            HealDropBehavior.onHealPickedUp += OnHealPickedUp;
        }

        protected virtual void OnHealPickedUp(HealDropBehavior healDrop)
        {
            if (Random.value <= AbilityLevel.Chance)
            {
                maxHPMultiplier.Value *= AbilityLevel.MaxHPMultiplier;
            }
        }

        public override void Clear()
        {
            StageController.Player.Stats.MaxHPStat.RemoveMultiplier(maxHPMultiplier);
            HealDropBehavior.onHealPickedUp -= OnHealPickedUp;

            base.Clear();
        }
    }
}