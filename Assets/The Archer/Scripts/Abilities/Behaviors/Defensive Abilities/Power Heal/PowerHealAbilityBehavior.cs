using OctoberStudio.Drop;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class PowerHealAbilityBehavior : AbilityBehavior<PowerHealAbilityData, PowerHealAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);

            HealDropBehavior.onHealPickedUp += OnHealPickedUp;
        }

        protected virtual void OnHealPickedUp(HealDropBehavior healDrop)
        {
            if (Random.value <= AbilityLevel.Chance)
            {
                attackDamageMultiplier.Value *= AbilityLevel.AttackDamageMultiplier;
            }
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            HealDropBehavior.onHealPickedUp -= OnHealPickedUp;

            base.Clear();
        }
    }
}