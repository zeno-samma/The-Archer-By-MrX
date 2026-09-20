using OctoberStudio.StatusEffects;
using UnityEngine;

namespace OctoberStudio
{
    public interface IProjectileTarget : IDefeatable
    {
        Transform Transform { get; }
        int Layer { get; }
        bool IsAlive { get; }

        Vector3 Position { get; }
        void TakeDamage(float damage, DamageType damageType, bool isCrit = false, bool disableSound = false);
        void ShowHitEffect(Vector3 direction, bool useknockBack);
        void SlowDown(float multiplier);

        bool ApplyStatusEffect(StatusEffectType type, float multiplier);
        void RemoveStatusEffect(StatusEffectType type);

        float GetStatusEffectDurationMultiplier(StatusEffectType type);
    }
}