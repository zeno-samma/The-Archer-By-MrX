using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Projectile;
using UnityEngine;

namespace OctoberStudio.StatusEffects
{
    public class ShockStatusEffect : StatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Shock;

        public float ShockSpreadRadius { get; set; }
        public float ShockProjectileDamageMultiplier { get; set; }
        public GameObject ProjectilePrefab { get; protected set; }
        protected PoolComponent<ShockSpreadProjectileBehavior> projectilePool;

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
                    var colliders = Physics.OverlapSphere(target.Position, ShockSpreadRadius, 1 << target.Layer);

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        var additionalTarget = colliders[i].GetComponent<IProjectileTarget>();
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