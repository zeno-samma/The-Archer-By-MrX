using System.Collections;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class MeteorProjectileBehavior : AbstractProjectile
    {
        [SerializeField] protected float colliderEnableDelay = 0.3f;
        [SerializeField] protected float colliderDisableDelay = 0.1f;
        [SerializeField] protected float meteorHideDelay = 0.6f;

        protected Collider trigger;
        protected Coroutine damageCoroutine;

        protected virtual void Awake()
        {
            trigger = GetComponent<Collider>();
        }

        public override void Launch(float damage)
        {
            Damage = damage;

            damageCoroutine = StartCoroutine(MeteorCoroutine());
        }

        protected IEnumerator MeteorCoroutine()
        {
            yield return new WaitForSeconds(colliderEnableDelay);

            trigger.enabled = true;

            yield return new WaitForSeconds(colliderDisableDelay);

            trigger.enabled = false;

            yield return new WaitForSeconds(meteorHideDelay);

            damageCoroutine = null;
            Hide();
        }

        protected virtual void EnableCollider()
        {
            trigger.enabled = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                target.TakeDamage(GetDamage(), GetDamageType());
                target.ShowHitEffect(transform.forward, false);

                if (target.IsAlive)
                {
                    for (int i = 0; i < appliedStatusEffects.Count; i++)
                    {
                        appliedStatusEffects[i].ApplyToTarget(target, GetDamage());
                    }
                }
            }
        }

        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier) { }

        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier) { }

        protected override float GetDamageDistanceMultiplier() { return 1; }

        public override void Hide()
        {
            base.Hide();

            trigger.enabled = false;
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);

            gameObject.SetActive(false);
        }

        public override void Clear()
        {
            base.Clear();

            trigger.enabled = false;
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);

            gameObject.SetActive(false);
        }
    }
}