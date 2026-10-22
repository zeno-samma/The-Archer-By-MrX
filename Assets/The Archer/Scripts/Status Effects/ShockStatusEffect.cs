using MrX.Extensions;
using MrX.Pool;
using MrX.Projectile;
using UnityEngine;

namespace MrX.StatusEffects
{
    public class ShockStatusEffect : StatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Shock;

        public float ShockSpreadRadius { get; set; }
        public float ShockProjectileDamageMultiplier { get; set; }
        public GameObject ProjectilePrefab { get; protected set; }
        protected PoolComponent<ShockSpreadProjectileBehavior> projectilePool;
        private Collider[] spreadTargets = new Collider[16];

        public ShockStatusEffect() : base()
        {

        }

        public void SetProjectilePrefab(GameObject prefab)
        {
            if (prefab == ProjectilePrefab) return;

            if(projectilePool != null)
            {
                projectilePool.Destroy();
            }

            ProjectilePrefab = prefab;
            projectilePool = new PoolComponent<ShockSpreadProjectileBehavior>(ProjectilePrefab, 5);
        }

        public override void ApplyToTarget(IProjectileTarget target, float damage)
        {
            if (target.ApplyStatusEffect(Type, ShockSpreadRadius))
            {
                base.ApplyToTarget(target, damage);

                if(projectilePool != null)
                {
                    int count;
                    // Grow only on saturation: a fixed buffer would silently lose targets in dense waves.
                    while ((count = Physics.OverlapSphereNonAlloc(target.Position, ShockSpreadRadius, spreadTargets, 1 << target.Layer)) == spreadTargets.Length)
                        System.Array.Resize(ref spreadTargets, spreadTargets.Length * 2);

                    for (int i = 0; i < count; i++)
                    {
                        var collider = spreadTargets[i];
                        spreadTargets[i] = null;
                        var additionalTarget = collider.GetComponent<IProjectileTarget>();
                        if (additionalTarget != null && additionalTarget != target)
                        {
                            var projectile = projectilePool.GetEntity();

                            projectile.transform.forward = target.Position.DirectionToXZ(additionalTarget.Position);
                            projectile.transform.position = target.Position + Vector3.up + projectile.transform.forward * 0.2f;

                            projectile.Target = additionalTarget.Transform;
                            projectile.TargetToIgnore = target;
                            projectile.Launch(damage * ShockProjectileDamageMultiplier);
                        }
                    }
                }
            }
        }
    }
}
