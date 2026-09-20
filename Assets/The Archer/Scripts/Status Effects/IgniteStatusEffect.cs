namespace OctoberStudio.StatusEffects
{
    public class IgniteStatusEffect : StatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Ignite;

        public IgniteStatusEffect() : base()
        {

        }

        public override void ApplyToTarget(IProjectileTarget target, float damage)
        {
            if (target.ApplyStatusEffect(Type, 0))
            {
                base.ApplyToTarget(target, damage);
            }
        }
    }
}