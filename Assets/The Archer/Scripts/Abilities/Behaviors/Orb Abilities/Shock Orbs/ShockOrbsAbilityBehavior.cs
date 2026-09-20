using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class ShockOrbsAbilityBehavior : AbilityBehavior<ShockOrbsAbilityData, ShockOrbsAbilityLevel>, IOrbsAbility
    {
        [SerializeField] protected Transform axisTransform;

        [SerializeField] protected List<OrbProjectileBehavior> orbs;

        protected ShockStatusEffect effect;

        protected float globalDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            effect = gameObject.AddComponent<ShockStatusEffect>();

            base.Init(data, levelId);

            StageController.Player.OrbsManager.RegisterOrbs(this);

            foreach (var orb in orbs)
            {
                orb.ApplyStatusEffect(effect);
            }
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            foreach (var orb in orbs)
            {
                orb.Launch(StageController.Player.Damage * AbilityLevel.OrbDamageMultiplier * globalDamageMultiplier);
            }

            effect.Duration = AbilityLevel.EffectDuration;
            effect.EffectDamageMultiplier = AbilityLevel.EffectDamageMultiplier;
            effect.ArrowDamageMultiplier = 1;
            effect.DamageInterval = AbilityLevel.EffectDealsDamageOverTime ? AbilityLevel.EffectDamageInterval : AbilityLevel.EffectDuration + 1;
            effect.ShockSpreadRadius = AbilityLevel.ShockSpreadRadius;
            effect.ShockProjectileDamageMultiplier = AbilityLevel.ShockSpreadDamageMultiplier;

            effect.SetProjectilePrefab(AbilityLevel.ShockSpreadProjectulePrefab);
        }

        public virtual void SetGlobalOrbsDamageMultiplier(float damageMultiplier)
        {
            globalDamageMultiplier = damageMultiplier;

            foreach (var orb in orbs)
            {
                orb.Launch(StageController.Player.Damage * AbilityLevel.OrbDamageMultiplier * globalDamageMultiplier);
            }
        }

        public virtual void SetAngle(float angle)
        {
            axisTransform.eulerAngles = Vector3.up * angle;
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public virtual void ResetTrails()
        {
            for (int i = 0; i < orbs.Count; i++)
            {
                orbs[i].ClearTrails();
            }
        }

        public override void Clear()
        {
            StageController.Player.OrbsManager.RemoveOrbs(this);

            base.Clear();
        }
    }
}