using OctoberStudio.StatusEffects;

namespace OctoberStudio.Abilities
{
    public class IgniteAbilityBehavior : AbilityBehavior<IgniteAbilityData, IgniteAbilityLevel>
    {
        protected IgniteStatusEffect effect;

        protected override void SetData(IgniteAbilityData data)
        {
            base.SetData(data);

            effect = gameObject.AddComponent<IgniteStatusEffect>();

            StageController.Player.SetProjectileEffect(effect);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = AbilityLevel.ArrowDamageMultiplier;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
        }

        public override void Clear()
        {
            StageController.Player.RemoveProjectileEffect(effect);
            effect.Clear();
            base.Clear();
        }
    }
}