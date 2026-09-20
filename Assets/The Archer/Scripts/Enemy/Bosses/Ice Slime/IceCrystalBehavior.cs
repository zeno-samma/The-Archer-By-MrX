using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class IceCrystalBehavior : AbstractProjectile
    {
        protected static readonly int CHARGE_TRIGGER = Animator.StringToHash("Charge");

        [SerializeField] protected Animator animator;
        [SerializeField] protected Collider trigger;

        [Space]
        [SerializeField] protected float lifetime;
        [SerializeField] protected float damageRadius;
        [SerializeField] protected Transform damageWarning;

        [Space]
        [SerializeField] protected GameObject onDeathParticlePrefab;
        [SerializeField] protected AudioData explosionSound;

        protected List<IProjectileTarget> targets = new List<IProjectileTarget>();

        protected bool isCharging = false;
        protected IEasingCoroutine scaleEasingCoroutine;

        protected float launchTime;

        protected virtual void Start()
        {
            if (onDeathParticlePrefab != null) StageController.ParticlesManager.RegisterParticle(onDeathParticlePrefab);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            trigger.enabled = false;
            damageWarning.localScale = Vector3.zero;
            damageWarning.gameObject.SetActive(false);

            isCharging = false;

            launchTime = Time.time;
        }

        public override void Launch(float damage)
        {
            Damage = damage;

            damageWarning.gameObject.SetActive(true);
            damageWarning.localScale = Vector3.zero;
            scaleEasingCoroutine = damageWarning.DoLocalScale(Vector3.one, 0.5f).SetEasing(EasingType.SineOut);
        }

        public virtual void OnSpawnEndedEventFired()
        {
            trigger.enabled = true;
        }

        public virtual void OnChargeEndedEventFired()
        {
            for(int i = 0; i < targets.Count; i++)
            {
                targets[i].TakeDamage(Damage, GetDamageType());
                targets[i].ShowHitEffect(Vector3.zero, false);
            }

            Hide();
        }

        protected virtual void Update()
        {
            if (CheckLifetime())
            {
                if (!isCharging)
                {
                    isCharging = true;
                    animator.SetTrigger(CHARGE_TRIGGER);
                }
            }
        }

        protected virtual bool CheckLifetime()
        {
            return launchTime + lifetime <= Time.time;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (StageController.Room.AliveEnemiesCount == 0) return;

            var target = other.GetComponent<IProjectileTarget>();

            if(target != null)
            {
                targets.Add(target);

                if (!isCharging)
                {
                    isCharging = true;
                    animator.SetTrigger(CHARGE_TRIGGER);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();

            if(target != null)
            {
                targets.Remove(target);
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (onDeathParticlePrefab != null)
            {
                var deathParticle = StageController.ParticlesManager.GetParticle(onDeathParticlePrefab);
                deathParticle.transform.position = transform.position;
                deathParticle.Play();
            }

            GameController.AudioManager.PlayAudio(explosionSound);

            gameObject.SetActive(false);
            damageWarning.gameObject.SetActive(false);

            targets.Clear();

            scaleEasingCoroutine.StopIfExists();
        }

        public override void Clear()
        {
            base.Clear();

            gameObject.SetActive(false);
            damageWarning.gameObject.SetActive(false);

            targets.Clear();

            scaleEasingCoroutine.StopIfExists();
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