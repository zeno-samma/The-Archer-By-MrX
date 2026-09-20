using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class SpiritLordAbilityBehavior : AbilityBehavior<SpiritLordAbilityData, SpiritLordAbilityLevel>
    {
        [SerializeField] protected SpiritBehavior spirit;

        protected StatMultiplier damageMultiplier = 1;
        protected StatMultiplier attackDelayMultiplier = 1;

        protected IEasingCoroutine multipliersCoroutine;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            multipliersCoroutine = EasingManager.DoNextFrame(() =>
            {
                spirit.DamageStat.AddMultiplier(damageMultiplier);
                spirit.AttackDelayStat.AddMultiplier(attackDelayMultiplier);
            });

            spirit.transform.SetParent(null);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            damageMultiplier.Value = AbilityLevel.SpiritDamageMultiplier;
            attackDelayMultiplier.Value = AbilityLevel.SpiritAttackDelay;
        }

        public override void Clear()
        {
            spirit.DamageStat.RemoveMultiplier(damageMultiplier);
            spirit.AttackDelayStat.RemoveMultiplier(attackDelayMultiplier);

            multipliersCoroutine.StopIfExists();

            if (spirit != null && spirit.gameObject != null) Destroy(spirit.gameObject);

            base.Clear();
        }
    }
}