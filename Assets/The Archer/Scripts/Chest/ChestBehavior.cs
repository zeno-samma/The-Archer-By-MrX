using MrX.Audio;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MrX
{
    public abstract class ChestBehavior : MonoBehaviour
    {
        public static readonly int OPEN_HASH = Animator.StringToHash("Open");
        public static readonly int HIDE_HASH = Animator.StringToHash("Hide");

        [SerializeField] protected ChestType chestType;

        [Space]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Collider trigger;

        [Space]
        [SerializeField] protected float openDelay = 0.5f;

        [Header("Particles")]
        [SerializeField] protected ParticleSystem spawnParticle;
        [SerializeField] protected ParticleSystem openParticle;
        [SerializeField] protected ParticleSystem idleParticle;

        [Space]
        [SerializeField] protected AudioData chestSpawnSound;

        public UnityAction<ChestBehavior> onHidden;
        private bool achievementOpenRecorded;

        protected virtual void Awake()
        {
            trigger.enabled = false;
        }

        public virtual void Spawn(ChestSpawnData chestSpawnData)
        {
            achievementOpenRecorded = false;
            chestSpawnData.TransformData.ApplyTo(transform);
        }

        public virtual void OnPlaySpawnSoundEventFired()
        {
            GameController.AudioManager.PlayAudio(chestSpawnSound);
        }

        public virtual void OnPlaySpawnParticleEventFired()
        {
            if(spawnParticle != null)
            {
                spawnParticle.Play();
            }
        }

        public virtual void OnPlayOpenParticleEventFired()
        {
            if(openParticle != null)
            {
                openParticle.Play();
            }
        }

        public virtual void OnStopIdleAnimationEventFired()
        {
            if(idleParticle != null)
            {
                idleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public virtual void OnSpawnAnimationEndedEventFired()
        {
            trigger.enabled = true;
            if(idleParticle != null)
            {
                idleParticle.Play();
            }
        }

        public virtual void OnHideAnimationEndedEventFired()
        {
            gameObject.SetActive(false);

            onHidden?.Invoke(this);
        }

        public virtual void OnOpenAnimationEndedEventFired()
        {
            if (achievementOpenRecorded) return;
            achievementOpenRecorded = true;
            GameController.AchievementsManager?.Record(Achievements.AchievementMetric.ChestsOpened);
            StartCoroutine(OpenCoroutine());
        }

        protected virtual IEnumerator OpenCoroutine()
        {
            yield return new WaitForSeconds(openDelay);

            while(StageController.GameScreen.HasAnyOpenedPage())
            {
                yield return null;
            }

            OpenChestUI();
        }

        protected abstract void OpenChestUI();

        public virtual void Hide()
        {
            animator.SetTrigger(HIDE_HASH);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            animator.SetTrigger(OPEN_HASH);
            trigger.enabled = false;
        }
    }
}
