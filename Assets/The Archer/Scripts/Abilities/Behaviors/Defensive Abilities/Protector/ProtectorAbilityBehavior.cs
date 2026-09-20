using OctoberStudio.Easing;

namespace OctoberStudio.Abilities
{
    public class ProtectorAbilityBehavior : AbilityBehavior<ProtectorAbilityData, ProtectorAbilityLevel>
    {
        protected StatMultiplier maxHPMultiplier = 1;
        protected IEasingCoroutine waitCoroutine;

        protected bool isInvincibilityAllowed = true;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.MaxHPStat.AddMultiplier(maxHPMultiplier);
            StageController.Player.onTakenDamage += OnPlayerTakenDamage;
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            maxHPMultiplier.Value = AbilityLevel.MaxHPMultiplier;
        }

        protected virtual void OnPlayerTakenDamage(float damage)
        {
            if(damage <= 0) return;

            if (!isInvincibilityAllowed) return;

            StageController.Player.MakeInvincible(AbilityLevel.InvincibilityDuration, false);

            isInvincibilityAllowed = false;
            waitCoroutine = EasingManager.DoAfter(AbilityLevel.InvincibilityDuration + AbilityLevel.InvincibilityCooldown, () => isInvincibilityAllowed = true);
        }

        public override void Clear()
        {
            StageController.Player.Stats.MaxHPStat.RemoveMultiplier(maxHPMultiplier);
            StageController.Player.onTakenDamage -= OnPlayerTakenDamage;

            if (waitCoroutine.ExistsAndActive())
            {
                waitCoroutine.Stop();
            }

            base.Clear();
        }
    }
}