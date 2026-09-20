using UnityEngine;

namespace OctoberStudio
{
    public class ChestEventsHandler : MonoBehaviour
    {
        protected ChestBehavior chest;

        protected virtual void Awake()
        {
            chest = GetComponentInParent<ChestBehavior>();
        }

        public virtual void PlaySpawnParticle()
        {
            chest.OnPlaySpawnParticleEventFired();
        }

        public virtual void PlaySpawnSound()
        {
            chest.OnPlaySpawnSoundEventFired();
        }

        public virtual void PlayOpenParticle()
        {
            chest.OnPlayOpenParticleEventFired();
        }

        public virtual void StopIdleParticle()
        {
            chest.OnStopIdleAnimationEventFired();
        }

        public virtual void SpawnAnimationEnded()
        {
            chest.OnSpawnAnimationEndedEventFired();
        }

        public virtual void HideAnimationEnded()
        {
            chest.OnHideAnimationEndedEventFired();
        }

        public virtual void OpenAnimationEnded()
        {
            chest.OnOpenAnimationEndedEventFired();
        }
    }
}