using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class PerfectStanceAbilityBehavior : AbilityBehavior<PerfectStanceAbilityData, PerfectStanceAbilityLevel>
    {
        protected StatMultiplier attackDamageMultiplier = 1;
        protected StatMultiplier attackSpeedMultiplier = 1;

        protected float lastTimeMoved;
        protected bool isParticleShown = false;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);

            lastTimeMoved = Time.time;
        }

        protected virtual void Update()
        {
            if (StageController.Player.MovedThisFrame)
            {
                lastTimeMoved = Time.time;

                attackDamageMultiplier.Value = 1;
                attackSpeedMultiplier.Value = 1;

                if (isParticleShown)
                {
                    isParticleShown = false;
                    StageController.Player.HidePerfectStance();
                }
            } else
            {
                var timeSinceLastMove = Time.time - lastTimeMoved;
                attackDamageMultiplier.Value = AbilityLevel.TimeDamageMultiplierCurve.Evaluate(timeSinceLastMove);
                attackSpeedMultiplier.Value = AbilityLevel.TimeAttackSpeedMultiplierCurve.Evaluate(timeSinceLastMove);

                if(!isParticleShown && timeSinceLastMove >= AbilityLevel.PerfectStanceParticleShowTime)
                {
                    isParticleShown = true;
                    StageController.Player.ShowPerfectStance();
                }
            }
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);

            base.Clear();
        }
    }
}