using UnityEngine;

namespace MrX.Projectile
{
    public class CactusSpikeEventsHandler : MonoBehaviour
    {
        protected CactusSpikeBehavior spike;

        protected virtual void Awake()
        {
            spike = GetComponentInParent<CactusSpikeBehavior>();
        }

        public virtual void OnHideEnded()
        {
            spike.OnHideAnimationEndedEventTriggered();
        }
    }
}