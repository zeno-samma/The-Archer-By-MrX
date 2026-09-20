using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class ShockSpreadProjectileBehavior : SimpleProjectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target == TargetToIgnore) return;

            if (target != null)
            {
                if (target.Transform == Target)
                {
                    target.TakeDamage(Damage, DamageType.Shock);
                    target.ShowHitEffect(transform.forward, false);

                    if (target.IsAlive)
                    {
                        for (int i = 0; i < appliedStatusEffects.Count; i++)
                        {
                            appliedStatusEffects[i].ApplyToTarget(target, GetDamage());
                        }
                    }

                    Hide();
                }
            }
            else
            {
                if (BouncesCount < MaxBouncesCount)
                {
                    if (Physics.Raycast(transform.position - transform.forward * 2, transform.forward, out var hit, 10f, 4096))
                    {
                        transform.forward = Vector3.Reflect(transform.forward, hit.normal).SetY(0).normalized;
                        BouncesCount++;

                        if (onDeathParticlePrefab != null)
                        {
                            var deathParticle = StageController.ParticlesManager.GetParticle(onDeathParticlePrefab);
                            deathParticle.transform.position = transform.position;
                            deathParticle.Play();
                        }
                    }
                    else
                    {
                        Hide();
                    }
                }
                else
                {
                    Hide();

                }
            }
        }
    }
}