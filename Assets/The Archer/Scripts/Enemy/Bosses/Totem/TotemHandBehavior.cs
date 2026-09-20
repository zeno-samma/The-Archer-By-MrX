using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class TotemHandBehavior : AbstractProjectile
    {
        [SerializeField] protected Collider trigger;
        [SerializeField] protected GameObject slamParticlePrefab;

        [Space]
        [SerializeField] protected Transform warningTransform;
        [SerializeField] protected float warningSpawnDuration = 0.2f;
        [SerializeField] protected float warningStayDuration = 0.25f;

        [Space]
        [SerializeField] protected AudioData impactSound;

        protected virtual void Awake()
        {
            if (slamParticlePrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(slamParticlePrefab);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            trigger.enabled = false;
        }

        public override void Launch(float damage)
        {
            Damage = damage;
        }

        public virtual void OnHandSlammedEventFired()
        {
            trigger.enabled = true;

            if(slamParticlePrefab != null)
            {
                var particle = StageController.ParticlesManager.GetParticle(slamParticlePrefab);
                particle.transform.position = transform.position;
                particle.Play();
            }

            GameController.AudioManager.PlayAudio(impactSound);
        }

        protected virtual IEnumerator WarningCoroutine()
        {
            warningTransform.gameObject.SetActive(true);
            warningTransform.localScale = Vector3.zero;

            yield return warningTransform.DoLocalScale(Vector3.one, warningSpawnDuration).SetEasing(EasingType.SineOut);

            yield return new WaitForSeconds(warningStayDuration);

            warningTransform.DoLocalScale(Vector3.zero, warningSpawnDuration).SetEasing(EasingType.SineIn).SetOnFinish(() => warningTransform.gameObject.SetActive(false));
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (StageController.Room.AliveEnemiesCount == 0) return;

            var target = other.GetComponent<IProjectileTarget>();

            if (target != null)
            {
                target.TakeDamage(Damage, GetDamageType());
                target.ShowHitEffect(Vector3.zero, false);
            }
        }

        public override void Hide()
        {
            base.Hide();

            gameObject.SetActive(false);
        }

        public override void Clear()
        {
            Hide();
        }

        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier)
        {

        }

        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier)
        {
            
        }

        protected override float GetDamageDistanceMultiplier()
        {
            return 1;
        }
    }
}