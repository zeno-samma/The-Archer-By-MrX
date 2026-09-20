using OctoberStudio.Easing;
using OctoberStudio.StatusEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Projectile
{
    public abstract class AbstractProjectile : MonoBehaviour
    {
        public float Lifetime { get; set; }
        protected float Damage { get; set; }

        public bool IsPiercingEnabled { get; set; }
        public bool IgnoresObstacles { get; set; }

        /// <summary>
        /// Used for magnetic projectiles, like arrows. If set, projectile will follow this target until it hits it or lifetime ends.
        /// </summary>
        public Transform Target { get; set; }

        public AnimationCurve DamageWithDistanceMultiplier { get; set; }

        public event UnityAction<AbstractProjectile> onProjectileHidden;
        public event UnityAction<float> onDamageDealtToTarget;

        public abstract void SetBounceData(int maxBounceCount, float bounceDamageMultiplier);
        public abstract void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier);
        protected abstract float GetDamageDistanceMultiplier();

        public abstract void Launch(float damage);

        protected List<StatusEffect> appliedStatusEffects = new List<StatusEffect>();

        protected void InvokeOnProjectileHiddenEvent()
        {
            onProjectileHidden?.Invoke(this);
        }

        protected void InvokeOnDamageDealtToTargetEvent(float damage)
        {
            onDamageDealtToTarget?.Invoke(damage);
        }

        public virtual void ApplyStatusEffect(StatusEffect effect)
        {
            appliedStatusEffects.Add(effect);
        }

        public virtual void ApplyStatusEffects(List<StatusEffect> effects)
        {
            appliedStatusEffects.AddRange(effects);
        }

        /// <summary>
        /// Use this method for hiding projectile on hit or lifetime end. Can have delayed calls 
        /// </summary>
        public virtual void Hide()
        {
            onProjectileHidden?.Invoke(this);
            appliedStatusEffects.Clear();
        }

        /// <summary>
        /// Use this method for abrupt disabling of projectile, like exiting to the menu. Should not have any delayed calls
        /// </summary>
        public virtual void Clear()
        {
            appliedStatusEffects.Clear();
        }

        public virtual float GetDamage()
        {
            var damage = Damage;

            for (int i = 0; i < appliedStatusEffects.Count; i++)
            {
                damage *= appliedStatusEffects[i].ArrowDamageMultiplier;
            }

            damage *= GetDamageDistanceMultiplier();

            return damage;
        }

        protected virtual DamageType GetDamageType()
        {
            if (appliedStatusEffects.Count == 0) return DamageType.Physical;
            return DamageHelper.GetDamageTypeFromStatusEffect(appliedStatusEffects[^1].Type);
        }

        protected virtual void OnEnable()
        {
            if (StageController.Room == null) return;

            if (StageController.Room.AliveEnemiesCount > 0)
            {
                StageController.Room.onAllEnemiesDefeated -= OnAllEnemiesDefeated;
                StageController.Room.onAllEnemiesDefeated += OnAllEnemiesDefeated;
            }
            else
            {
                EasingManager.DoNextFrame(() =>
                {
                    if (gameObject.activeSelf) Hide();
                });
            }
        }

        protected virtual void OnDisable()
        {
            if (StageController.Room != null)
            {
                StageController.Room.onAllEnemiesDefeated -= OnAllEnemiesDefeated;
            }
        }

        protected virtual void OnAllEnemiesDefeated()
        {
            Hide();
        }
    }
}