using OctoberStudio.StatusEffects;

namespace OctoberStudio.Abilities
{
    public interface StatusEffectAbility
    {
        StatusEffect CreateStatusEffect(IProjectileTarget target);
    }
}