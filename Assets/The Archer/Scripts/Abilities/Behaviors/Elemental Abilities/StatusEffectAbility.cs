using MrX.StatusEffects;

namespace MrX.Abilities
{
    public interface StatusEffectAbility
    {
        StatusEffect CreateStatusEffect(IProjectileTarget target);
    }
}