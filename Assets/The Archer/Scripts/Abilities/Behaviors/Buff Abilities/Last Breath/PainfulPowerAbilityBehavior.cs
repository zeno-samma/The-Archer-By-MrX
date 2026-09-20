using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class PainfulPowerAbilityBehavior : AbilityBehavior<PainfulPowerAbilityData, PainfulPowerAbilityLevel>
    {
        [SerializeField] protected ParticleSystem painfulPowerParticle;

        protected StatMultiplier attackDamageMultiplier = 1;

        protected IEasingCoroutine delayEasingCoroutine;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.onTakenDamage += OnPlayerTakenDamage;
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
        }

        protected virtual void OnPlayerTakenDamage(float damage)
        {
            if (damage <= 0) return;

            if(delayEasingCoroutine != null)
            {
                delayEasingCoroutine.Stop();
            } else
            {
                StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
                if(painfulPowerParticle != null) painfulPowerParticle.Play();
            }

            delayEasingCoroutine = EasingManager.DoAfter(AbilityLevel.AbilityDuration, OnAbilityDurationEnded);
        }

        protected virtual void OnAbilityDurationEnded()
        {
            delayEasingCoroutine = null;

            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);

            if (painfulPowerParticle != null) painfulPowerParticle.Stop();
        }

        public override void Clear()
        {
            if(delayEasingCoroutine != null)
            {
                delayEasingCoroutine.Stop();

                OnAbilityDurationEnded();
            }

            StageController.Player.onTakenDamage -= OnPlayerTakenDamage;

            base.Clear();
        }
    }
}