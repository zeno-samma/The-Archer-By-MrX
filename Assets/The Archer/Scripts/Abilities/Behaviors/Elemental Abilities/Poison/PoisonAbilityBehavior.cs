using OctoberStudio.StatusEffects;

namespace OctoberStudio.Abilities
{
    public class PoisonAbilityBehavior : AbilityBehavior<PoisonAbilityData, PoisonAbilityLevel>
    {
        protected PoisonStatusEffect effect;

        protected override void SetData(PoisonAbilityData data)
        {
            base.SetData(data);

            effect = gameObject.AddComponent<PoisonStatusEffect>();

            StageController.Player.SetProjectileEffect(effect);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = AbilityLevel.ArrowDamageMultiplier;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
            effect.TargetDamageReductionMultiplier = AbilityLevel.TargetDamageReductionMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.RemoveProjectileEffect(effect);

            base.Clear();
        }
    }
}