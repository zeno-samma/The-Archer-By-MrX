using UnityEngine;

namespace OctoberStudio.Abilities
{
    public interface IOrbsAbility
    {
        void SetAngle(float angle);
        void SetPosition(Vector3 position);
        void SetGlobalOrbsDamageMultiplier(float damage);
        void ResetTrails();
    }
}