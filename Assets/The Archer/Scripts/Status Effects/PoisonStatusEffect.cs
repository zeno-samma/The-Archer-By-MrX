namespace OctoberStudio.StatusEffects
{
    public class PoisonStatusEffect : StatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Poison;

        public float TargetDamageReductionMultiplier { get; set; }

        public PoisonStatusEffect() : base()
        {

        }

        public override void ApplyToTarget(IProjectileTarget target, float damage)
        {
            if (target.ApplyStatusEffect(Type, TargetDamageReductionMultiplier))
            {
                base.ApplyToTarget(target, damage);
            }
        }
    }
}