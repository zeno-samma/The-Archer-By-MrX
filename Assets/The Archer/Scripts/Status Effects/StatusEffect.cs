using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.StatusEffects
{
    public abstract class StatusEffect: MonoBehaviour
    {
        public abstract StatusEffectType Type { get; }

        public float Duration { get; set; } = 1f;
        public float ArrowDamageMultiplier { get; set; } = 1f;
        public float EffectDamageMultiplier { get; set; } = 1f;
        public float DamageInterval { get; set; } = 1f;

        protected List<StatusEffectTargetData> targets;

        public StatusEffect()
        {
            targets = new List<StatusEffectTargetData>();
        }

        public virtual void ApplyToTarget(IProjectileTarget target, float damage)
        {
            var damageInterval = EffectDamageMultiplier == 0 ? Duration + 1 : DamageInterval;

            var targetData = new StatusEffectTargetData(target, target.GetStatusEffectDurationMultiplier(Type) * Duration, damageInterval, damage * EffectDamageMultiplier);
            targets.Add(targetData);

            target.SubscribeOnDefeat(OnTargetDefeated);
        }

        protected virtual void Update()
        {
            if (StageController.Room.AliveEnemiesCount == 0)
            {
                if(targets.Count > 0)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        var targetData = targets[i];

                        targetData.Target.RemoveStatusEffect(Type);
                        targetData.Target.UnsubscribeOnDefeat(OnTargetDefeated);
                    }

                    targets.Clear();
                }

                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                var targetData = targets[i];

                if(Time.time >= targetData.EndTime)
                {
                    targets.RemoveAt(i);
                    i--;

                    targetData.Target.RemoveStatusEffect(Type);
                    targetData.Target.UnsubscribeOnDefeat(OnTargetDefeated);

                    continue;
                } 

                if(Time.time >= targetData.NextDamageTime)
                {
                    targetData.Target.TakeDamage(targetData.DamagePerTick, DamageHelper.GetDamageTypeFromStatusEffect(Type));
                    targetData.Tick();
                }
            }
        }

        protected StatusEffectTargetData GetData(IProjectileTarget target)
        {
            for(int i = 0; i < targets.Count; i++)
            {
                if (targets[i].Target == target) return targets[i];
            }

            return null;
        }

        protected virtual void OnTargetDefeated(IDefeatable target)
        {
            var targetData = GetData(target as IProjectileTarget);

            targetData.Target.RemoveStatusEffect(Type);
            targetData.Target.UnsubscribeOnDefeat(OnTargetDefeated);

            targets.Remove(targetData);
        }

        public virtual void Clear()
        {
            for(int i = 0; i < targets.Count; i++)
            {
                var targetData = targets[i];
                targetData.Target.RemoveStatusEffect(Type);
                targetData.Target.UnsubscribeOnDefeat(OnTargetDefeated);
            }
            targets.Clear();
        }

        protected virtual void OnDisable()
        {
            Clear();
        }

        protected static WorldSpaceTextType GetDamageTypeFromStatusEffect(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Ignite: return WorldSpaceTextType.BurningDamage;
                case StatusEffectType.Freeze: return WorldSpaceTextType.FreezingDamage;
                case StatusEffectType.Poison: return WorldSpaceTextType.PoisonDamage;
                case StatusEffectType.Shock: return WorldSpaceTextType.ShockDamage;
                default: return WorldSpaceTextType.PhysicalDamage;
            }
        }

        protected class StatusEffectTargetData
        {
            public IProjectileTarget Target { get; protected set; }
            public float EndTime { get; protected set; }
            public float NextDamageTime { get; protected set; }
            public float DamagePerTick { get; protected set; }

            protected float damageInterval;

            public StatusEffectTargetData(IProjectileTarget target, float duration, float damageInterval, float damagePerTick)
            {
                Target = target;
                EndTime = Time.time + duration;
                NextDamageTime = Time.time + damageInterval;

                DamagePerTick = damagePerTick;

                this.damageInterval = damageInterval;
            }

            public void Tick()
            {
                NextDamageTime += damageInterval;
            }
        }
    }
}