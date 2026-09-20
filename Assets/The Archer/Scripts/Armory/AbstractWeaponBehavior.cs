using OctoberStudio.StatusEffects;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Weapon
{
    public abstract class AbstractWeaponBehavior : MonoBehaviour
    {
        public WeaponData WeaponData { get; protected set; }
        public bool IsDetached { get; set; }
        public bool IsRightHandWeapon => WeaponData.IsRightHandWeapon;

        public abstract Vector3 DetachedPosition { get; }
        public abstract Vector3 DetachedRotation { get; }
        public abstract Vector3 DetachedScale { get; }

        protected List<StatusEffect> appliedEffects = new List<StatusEffect>();

        public virtual void Init(WeaponData weaponData)
        {
            WeaponData = weaponData;
        }

        public virtual void ApplyStatusEffect(StatusEffect statusEffect)
        {
            appliedEffects.Add(statusEffect);
        }

        public virtual void RemoveStatusEffect(StatusEffect statusEffect)
        {
            appliedEffects.Remove(statusEffect);
        }

        public abstract void SetAnimationLength(float length);
        public abstract void Shoot(Transform target, Vector3 direction);
        public abstract void OnShootEventFired();

        public abstract void OnAttachedToHero();
        public abstract void OnDetachedFromHero();
    }
}
