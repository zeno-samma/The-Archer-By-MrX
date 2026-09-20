using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class IceCrystalEventsHandler : MonoBehaviour
    {
        protected IceCrystalBehavior iceCrystal;

        protected virtual void Awake()
        {
            iceCrystal = GetComponentInParent<IceCrystalBehavior>();
        }

        public virtual void OnSpawnEnded()
        {
            iceCrystal.OnSpawnEndedEventFired();
        }

        public virtual void OnChargeEnded()
        {
            iceCrystal.OnChargeEndedEventFired();
        }
    }
}