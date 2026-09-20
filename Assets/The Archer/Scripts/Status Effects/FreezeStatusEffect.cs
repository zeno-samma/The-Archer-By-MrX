namespace OctoberStudio.StatusEffects
{
    public class FreezeStatusEffect : StatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Freeze;

        public float TargetSpeedMultiplier { get; set; }

        public FreezeStatusEffect(): base()
        {

        }

        public override void ApplyToTarget(IProjectileTarget target, float damage)
        {
            if(target.ApplyStatusEffect(Type, TargetSpeedMultiplier))
            {
                base.ApplyToTarget(target, damage);
            }
        }
    }
}