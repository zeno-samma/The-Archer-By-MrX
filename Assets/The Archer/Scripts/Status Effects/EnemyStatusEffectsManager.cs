using OctoberStudio.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.StatusEffects
{
    public class EnemyStatusEffectsManager : MonoBehaviour
    {
        protected Dictionary<EnemyType, List<StatusEffect>> enemyEffectsDictionary = new Dictionary<EnemyType, List<StatusEffect>>();

        protected virtual void Awake()
        {
            StageController.RegisterEnemyStatusEffectsManager(this);
        }

        public virtual T RegisterStatusEffect<T>(EnemyType enemyType, StatusEffectType effectType, float duration, float damageInterval, float damageMultiplier, float additionalMultiplier) where T: StatusEffect
        {
            if (!enemyEffectsDictionary.ContainsKey(enemyType))
            {
                enemyEffectsDictionary.Add(enemyType, new List<StatusEffect>());
            }
            var registeredEffects = enemyEffectsDictionary[enemyType];

            StatusEffect requiredEffect = null;
            for (int i = 0; i < registeredEffects.Count; i++)
            {
                var effect = registeredEffects[i];

                if(effect.Type == effectType)
                {
                    if(effect.Duration == duration &&
                        effect.DamageInterval == damageInterval &&
                        effect.EffectDamageMultiplier == damageMultiplier)
                    {
                        switch (effectType)
                        {
                            case StatusEffectType.Ignite:
                                requiredEffect = effect;
                                break;

                            case StatusEffectType.Freeze:
                                if(effect is FreezeStatusEffect freezingEffect && freezingEffect.TargetSpeedMultiplier == additionalMultiplier)
                                {
                                    requiredEffect = effect;
                                }
                                break;

                            case StatusEffectType.Poison:
                                if (effect is PoisonStatusEffect poisonEffect && poisonEffect.TargetDamageReductionMultiplier == additionalMultiplier)
                                {
                                    requiredEffect = effect;
                                }
                                break;
                            case StatusEffectType.Shock:
                                if (effect is ShockStatusEffect shockedEffect && shockedEffect.ShockProjectileDamageMultiplier == additionalMultiplier)
                                {
                                    requiredEffect = effect;
                                }
                                break;
                        }

                        if (requiredEffect != null) break;
                    }
                }
            }

            if (requiredEffect != null) return requiredEffect as T;

            var newBurningEffect = gameObject.AddComponent<T>();
            registeredEffects.Add(newBurningEffect);

            newBurningEffect.Duration = duration;
            newBurningEffect.DamageInterval = damageInterval;
            newBurningEffect.EffectDamageMultiplier = damageMultiplier;

            return newBurningEffect;
        }
    }
}