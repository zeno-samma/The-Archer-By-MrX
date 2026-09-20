using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class CactusSpikeBehavior : AbstractProjectile
    {
        public static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] protected Collider trigger;
        [SerializeField] protected ParticleSystem spawnParticle;

        [Space]
        [SerializeField] protected GameObject spikeObject;
        [SerializeField] protected Animator spikeAnimator;
        [SerializeField] protected float spikeLifetime;

        [Space]
        [SerializeField] protected Transform warningTransform;
        [SerializeField] protected float warningSpawnDuration = 0.1f;
        [SerializeField] protected float warningStayDuration = 0.5f;

        [Space]
        [SerializeField] protected AudioData spikeSpawnSound;

        public override void Launch(float damage)
        {
            Damage = damage;

            StartCoroutine(LifetimeCoroutine());
            if (spawnParticle != null) spawnParticle.Play();
        }

        protected virtual IEnumerator LifetimeCoroutine()
        {
            trigger.enabled = false;

            warningTransform.gameObject.SetActive(true);
            warningTransform.localScale = Vector3.zero;

            yield return warningTransform.DoLocalScale(Vector3.one, warningSpawnDuration).SetEasing(EasingType.SineOut);

            yield return new WaitForSeconds(warningStayDuration);

            trigger.enabled = true;

            GameController.AudioManager.PlayAudio(spikeSpawnSound);

            warningTransform.DoLocalScale(Vector3.zero, warningSpawnDuration).SetEasing(EasingType.SineIn);
            spikeObject.SetActive(true);

            yield return new WaitForSeconds(spikeLifetime);

            Hide();
        }

        public virtual void OnHideAnimationEndedEventTriggered()
        {
            gameObject.SetActive(false);
            spikeObject.SetActive(false);

            trigger.enabled = false;
        }

        public override void Hide()
        {
            base.Hide();

            spikeAnimator.SetTrigger(HIDE_TRIGGER);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if(StageController.Room.AliveEnemiesCount == 0) return;

            var target = other.GetComponent<IProjectileTarget>();

            if(target != null)
            {
                target.TakeDamage(Damage, DamageType.Physical);
                target.ShowHitEffect(Vector3.zero, false);
            }
        }

        protected override float GetDamageDistanceMultiplier()
        {
            return 1f; // Cactus spikes deal full damage at all distances
        }

        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier)
        {
            return; // Cactus spikes do not bounce
        }

        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier)
        {
            return; // Cactus spikes do not ricochet;
        }

        public override void Clear()
        {
            base.Clear();

            StopAllCoroutines();

            warningTransform.gameObject.SetActive(false);
            spikeObject.SetActive(false);

            trigger.enabled = false;
            gameObject.SetActive(false);
        }
    }
}