using UnityEngine;

namespace OctoberStudio.Player
{
    public class HeroEventsHandler : MonoBehaviour
    {
        protected IHeroBehavior Hero { get; private set; }

        public virtual void Awake()
        {
            Hero = GetComponentInParent<IHeroBehavior>();
        }

        public virtual void Shoot()
        {
            Hero.OnShootAnimationEventFired();
        }

        public virtual void OnDefeatAnimationEnded()
        {
            StageController.OnHeroDied();
        }

        public virtual void OnReviveAnimationEnded()
        {
            Hero.OnReviveAnimationEndedEventFired();
        }

        public virtual void Step()
        {
            Hero.OnStepEventFired();
        }
    }
}