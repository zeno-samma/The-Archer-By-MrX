using OctoberStudio.StatusEffects;

namespace OctoberStudio.Abilities
{
    public class FreezeAbilityBehavior : AbilityBehavior<FreezeAbilityData, FreezeAbilityLevel>
    {
        protected FreezeStatusEffect effect;

        protected override void SetData(FreezeAbilityData data)
        {
            base.SetData(data);

            effect = gameObject.AddComponent<FreezeStatusEffect>();

            StageController.Player.SetProjectileEffect(effect);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = AbilityLevel.ArrowDamageMultiplier;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
            effect.TargetSpeedMultiplier = AbilityLevel.TargetSpeedMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.RemoveProjectileEffect(effect);
            effect.Clear();

            base.Clear();
        }
    }
}