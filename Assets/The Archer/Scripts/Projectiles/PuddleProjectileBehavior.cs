using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class PuddleProjectileBehavior : SimpleProjectile
    {
        [SerializeField] float damageInterval = 1f;

        protected Dictionary<IProjectileTarget, float> targetsInside = new Dictionary<IProjectileTarget, float>();
        protected IEasingCoroutine scaleEasing;

        protected bool isHiding;

        public override void Launch(float damage)
        {
            base.Launch(damage);

            transform.localScale = Vector3.zero;
            scaleEasing = transform.DoLocalScale(Vector3.one, 0.1f).SetEasing(EasingType.SineOut);
        }

        public override void ResetOverrides()
        {
            base.ResetOverrides();
            isHiding = false;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                DamageTarget(target);

                targetsInside.Add(target, Time.time);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null && targetsInside.ContainsKey(target))
            {
                if (targetsInside[target] + damageInterval <= Time.time)
                {
                    targetsInside[target] = Time.time;

                    DamageTarget(target);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                targetsInside.Remove(target);
            }
        }

        protected virtual void DamageTarget(IProjectileTarget target)
        {
            target.TakeDamage(Damage, DamageType.Physical);
        }

        protected override void Update()
        {
            if (CheckLifetime() && !isHiding) Hide();
        }

        public override void Hide()
        {
            isHiding = true;

            scaleEasing?.StopIfExists();

            scaleEasing = transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.SineIn).SetOnFinish(() =>
            {
                targetsInside.Clear();

                base.Hide();
            });
        }
    }
}