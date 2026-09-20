using UnityEngine;

namespace OctoberStudio.Weapon
{
    public class WeaponEventsHandler : MonoBehaviour
    {
        protected AbstractWeaponBehavior weapon;

        protected virtual void Awake()
        {
            weapon = GetComponentInParent<AbstractWeaponBehavior>();
        }

        public virtual void Shoot()
        {
            weapon.OnShootEventFired();
        }
    }
}