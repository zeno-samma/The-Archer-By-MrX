using OctoberStudio.Easing;

namespace OctoberStudio.Abilities
{
    public class SafetyBubbleAbilityBehavior : AbilityBehavior<SafetyBubbleAbilityData, SafetyBubbleAbilityLevel>
    {
        protected int hitsReceived = 0;
        protected bool IsActive => invincibilityData != null;

        protected InvincibilityData invincibilityData;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            hitsReceived = 0;

            StageController.DoAfterRoomLoaded(() =>
            {
                StageController.Room.onRoomStarted += OnWaveStarted;
                StageController.Player.onTakenDamage += OnPlayerTakeDamage;
            });

            invincibilityData = StageController.Player.MakeInvincible(-1, true);
        }

        protected virtual void OnWaveStarted()
        {
            if(!IsActive)
            {
                invincibilityData = StageController.Player.MakeInvincible(-1, true);
            }

            hitsReceived = 0;
        }

        protected virtual void OnPlayerTakeDamage(float damage)
        {
            if (!IsActive) return;

            hitsReceived++;
            if (hitsReceived >= AbilityLevel.HitsToDisableProtection)
            {
                EasingManager.DoNextFrame(RemoveInvincibility);
            }
        }

        protected virtual void RemoveInvincibility()
        {
            if (IsActive)
            {
                StageController.Player.RemoveInvincibility(invincibilityData);
                invincibilityData = null;
            }
        }

        public override void Clear()
        {
            StageController.Room.onRoomStarted -= OnWaveStarted;
            StageController.Player.onTakenDamage -= OnPlayerTakeDamage;

            RemoveInvincibility();

            base.Clear();
        }
    }
}