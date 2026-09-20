using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class IcePuddleBehavior : PuddleProjectileBehavior
    {
        [SerializeField] float slowDownMultiplier = 0.3f;

        protected override void DamageTarget(IProjectileTarget target)
        {
            base.DamageTarget(target);

            target.SlowDown(slowDownMultiplier);
        }
    }
}