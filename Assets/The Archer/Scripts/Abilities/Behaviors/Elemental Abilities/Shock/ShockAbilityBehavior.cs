using OctoberStudio.StatusEffects;

namespace OctoberStudio.Abilities
{
    public class ShockAbilityBehavior : AbilityBehavior<ShockAbilityData, ShockAbilityLevel>
    {
        protected ShockStatusEffect effect;

        protected override void SetData(ShockAbilityData data)
        {
            base.SetData(data);

            effect = gameObject.AddComponent<ShockStatusEffect>();

            StageController.Player.SetProjectileEffect(effect);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = AbilityLevel.ArrowDamageMultiplier;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
            effect.ShockSpreadRadius = AbilityLevel.ShockSpreadRadius;
            effect.ShockProjectileDamageMultiplier = AbilityLevel.ShockSpreadDamageMultiplier;

            effect.SetProjectilePrefab(AbilityLevel.ShockSpreadProjectulePrefab);
        }

        public override void Clear()
        {
            StageController.Player.RemoveProjectileEffect(effect);

            base.Clear();
        }
    }
}