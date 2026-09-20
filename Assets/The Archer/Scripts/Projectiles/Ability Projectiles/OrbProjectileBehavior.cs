using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class OrbProjectileBehavior : AbstractProjectile
    {
        [SerializeField] protected TrailRenderer trail;
        [SerializeField] protected List<ParticleSystem> particles;

        public override void Launch(float damage)
        {
            Damage = damage;
        }

        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier)
        {

        }

        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier)
        {

        }

        public virtual void ClearTrails()
        {
            if (trail != null) trail.Clear();
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] != null) particles[i].Clear();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                float damage = GetDamage();

                target.TakeDamage(damage, GetDamageType());
                target.ShowHitEffect(Vector3.zero, false);

                InvokeOnDamageDealtToTargetEvent(damage);

                if (target.IsAlive)
                {
                    for (int i = 0; i < appliedStatusEffects.Count; i++)
                    {
                        appliedStatusEffects[i].ApplyToTarget(target, GetDamage());
                    }
                }
            }
        }

        public override void Hide()
        {
            base.Hide();

            gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override float GetDamageDistanceMultiplier()
        {
            return 1;
        }

        protected override void OnAllEnemiesDefeated()
        {

        }
    }
}