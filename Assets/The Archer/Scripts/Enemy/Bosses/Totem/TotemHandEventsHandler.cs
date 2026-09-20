using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class TotemHandEventsHandler : MonoBehaviour
    {
        protected TotemHandBehavior hand;

        protected virtual void Awake()
        {
            hand = GetComponentInParent<TotemHandBehavior>();
        }

        public virtual void OnHandSlammed()
        {
            hand.OnHandSlammedEventFired();
        }

        public virtual void OnAnimationEnded()
        {
            hand.Hide();
        }
    }
}