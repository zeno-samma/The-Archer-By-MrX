using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class RageAbilityBehavior : AbilityBehavior<RageAbilityData, RageAbilityLevel>
    {
        [SerializeField] protected ParticleSystem rageParticle;

        protected StatMultiplier attackDamageMultiplier = 1;
        protected StatMultiplier attackSpeedMultiplier = 1;

        protected bool isAbilityActive = false;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.onHPChanged += OnHPChanged;

            if (ShouldActivateAbility())
            {
                ActivateAbility();
            }
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
        }

        protected virtual void OnHPChanged()
        {
            if(ShouldActivateAbility())
            {
                ActivateAbility();
            } else if(ShouldDisableAbility())
            {
                DisableAbility();
            }
        }

        protected virtual bool ShouldActivateAbility()
        {
            var player = StageController.Player;
            return player.HP / player.Stats.MaxHPStat <= AbilityLevel.AbilityStartHpProportion && !isAbilityActive;
        }

        protected virtual void ActivateAbility()
        {
            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);

            if(rageParticle != null) rageParticle.Play();

            isAbilityActive = true;
        }

        protected virtual bool ShouldDisableAbility()
        {
            var player = StageController.Player;
            return player.HP / player.Stats.MaxHPStat > AbilityLevel.AbilityStartHpProportion && isAbilityActive;
        }

        protected virtual void DisableAbility()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);

            if (rageParticle != null) rageParticle.Stop();

            isAbilityActive = false;
        }

        public override void Clear()
        {
            if (isAbilityActive)
            {
                DisableAbility();
            }

            StageController.Player.onHPChanged -= OnHPChanged;

            base.Clear();
        }
    }
}