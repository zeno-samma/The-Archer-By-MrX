using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class HoneyMineBehavior : MonoBehaviour
    {
        protected static readonly int CHARGE_TRIGGER = Animator.StringToHash("Charge");

        [SerializeField] protected Animator animator;
        [SerializeField] protected Collider trigger;
        [SerializeField] protected float autoExplosionDelay;

        [Space]
        [SerializeField] protected GameObject explosionParticlePrefab;
        [Space]
        [SerializeField] protected AudioData explosionSound;

        protected IEasingCoroutine autoExplodeCoroutine;

        protected List<IProjectileTarget> targets = new List<IProjectileTarget>();

        protected bool isCharging = false;

        public float Damage { get; set; }

        protected virtual void Awake()
        {
            if(explosionParticlePrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(explosionParticlePrefab);
            }
        }

        protected virtual void OnEnable()
        {
            animator.gameObject.SetActive(false);
            EasingManager.DoAfter(0.5f, () => animator.gameObject.SetActive(true));
            
        }

        public virtual void OnSpawnEndedEventFired()
        {
            trigger.enabled = true; 

            if(autoExplosionDelay >= 0)
            {
                autoExplodeCoroutine = EasingManager.DoAfter(autoExplosionDelay, Explode);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if(target != null)
            {
                targets.Add(target);
            }

            autoExplodeCoroutine.StopIfExists();
            Explode();
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                targets.Remove(target);
            }
        }

        public virtual void Charge()
        {
            isCharging = true;
            animator.SetTrigger(CHARGE_TRIGGER);
        }

        public virtual void OnChargeEndedEventFired()
        {
            Explode();
        }

        protected virtual void Explode()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var direction = (target.Position - transform.position).normalized;

                targets[i].TakeDamage(Damage, DamageType.Physical);
                targets[i].ShowHitEffect(Vector3.zero, false);
            }

            if(explosionParticlePrefab != null)
            {
                var explosion = StageController.ParticlesManager.GetParticle(explosionParticlePrefab);
                explosion.transform.position = transform.position;
                explosion.Play();
            }

            gameObject.SetActive(false);

            GameController.AudioManager.PlayAudio(explosionSound);
        }

        protected virtual void OnDisable()
        {
            isCharging = false;
            trigger.enabled = false;
            targets.Clear();
        }
    }
}