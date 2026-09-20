using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class SimpleBouncingProjectile : SimpleProjectile
    {
        protected override void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                target.TakeDamage(Damage, GetDamageType());
                target.ShowHitEffect(Vector3.zero, false);

                Hide();
            }
            else
            {
                if (Physics.Raycast(transform.position - transform.forward, transform.forward, out var hit, 5f, 4096))
                {
                    transform.forward = Vector3.Reflect(transform.forward, hit.normal).SetY(0).normalized;
                }
                else
                {
                    Hide();
                }
            }
        }
    }
}