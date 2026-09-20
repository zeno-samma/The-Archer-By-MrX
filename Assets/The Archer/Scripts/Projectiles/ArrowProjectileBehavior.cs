using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class ArrowProjectileBehavior : SimpleProjectile
    {
        [Header("Trails")]
        [SerializeField] GameObject defaultTrail;
        [SerializeField] GameObject critTrail;

        [Space]
        [SerializeField] GameObject burningTrail;
        [SerializeField] GameObject freezeTrail;
        [SerializeField] GameObject poisonTrail;
        [SerializeField] GameObject shockTrail;

        protected Dictionary<StatusEffectType, GameObject> effectTrails = new Dictionary<StatusEffectType, GameObject>();

        protected override void Awake()
        {
            base.Awake();

            effectTrails = new Dictionary<StatusEffectType, GameObject>()
            {
                { StatusEffectType.Ignite, burningTrail},
                { StatusEffectType.Freeze, freezeTrail},
                { StatusEffectType.Poison, poisonTrail},
                { StatusEffectType.Shock, shockTrail},
            };
        }

        public override void ResetOverrides()
        {
            base.ResetOverrides();

            foreach (var effectType in effectTrails.Keys)
            {
                effectTrails[effectType].SetActive(false);
            }

            defaultTrail.SetActive(false);
            critTrail.SetActive(false);
        }

        public override void Launch(float damage)
        {
            base.Launch(damage);

            if (IsCriticalHit)
            {
                foreach (var effectType in effectTrails.Keys)
                {
                    effectTrails[effectType].SetActive(false);
                }

                defaultTrail.SetActive(false);
                critTrail.SetActive(true);
            }
            else if (appliedStatusEffects == null || appliedStatusEffects.Count == 0)
            {
                defaultTrail.SetActive(true);
                critTrail.SetActive(false);
            }
        }

        public override void ApplyStatusEffect(StatusEffect effect)
        {
            base.ApplyStatusEffect(effect);

            defaultTrail.SetActive(false);
            critTrail.SetActive(false);

            foreach (var effectType in effectTrails.Keys)
            {
                effectTrails[effectType].SetActive(effect.Type == effectType);
            }
        }

        public override void ApplyStatusEffects(List<StatusEffect> effects)
        {
            base.ApplyStatusEffects(effects);

            defaultTrail.SetActive(false);
            critTrail.SetActive(false);

            foreach (var effectType in effectTrails.Keys)
            {
                effectTrails[effectType].SetActive(effects[^1].Type == effectType);
            }
        }

        public override void Hide()
        {
            base.Hide();

            defaultTrail.SetActive(true);
            critTrail.SetActive(false);

            foreach (var effectType in effectTrails.Keys)
            {
                effectTrails[effectType].SetActive(false);
            }
        }
    }

}