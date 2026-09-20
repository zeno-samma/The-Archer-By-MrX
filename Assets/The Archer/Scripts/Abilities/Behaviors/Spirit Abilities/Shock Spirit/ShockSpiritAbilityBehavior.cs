using OctoberStudio.Easing;
using OctoberStudio.StatusEffects;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class ShockSpiritAbilityBehavior : AbilityBehavior<ShockSpiritAbilityData, ShockSpiritAbilityLevel>
    {
        [SerializeField] protected SpiritBehavior spirit;

        protected ShockStatusEffect effect;

        protected StatMultiplier damageMultiplier = 1;
        protected StatMultiplier attackDelayMultiplier = 1;

        protected IEasingCoroutine multipliersCoroutine;

        public override void Init(AbilityData data, int levelId)
        {
            effect = gameObject.AddComponent<ShockStatusEffect>();
            spirit.ApplyEffect(effect);

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

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = 1;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
            effect.ShockProjectileDamageMultiplier = AbilityLevel.ShockSpreadDamageMultiplier;
            effect.ShockSpreadRadius = AbilityLevel.ShockSpreadRadius;

            effect.SetProjectilePrefab(AbilityLevel.ShockSpreadProjectulePrefab);

            damageMultiplier.Value = AbilityLevel.SpiritDamageMultiplier;
            attackDelayMultiplier.Value = AbilityLevel.SpiritAttackDelay;
        }

        public override void Clear()
        {
            spirit.DamageStat.RemoveMultiplier(damageMultiplier);
            spirit.AttackDelayStat.RemoveMultiplier(attackDelayMultiplier);

            multipliersCoroutine.StopIfExists();

            if(spirit != null && spirit.gameObject != null) Destroy(spirit.gameObject);

            base.Clear();
        }
    }
}