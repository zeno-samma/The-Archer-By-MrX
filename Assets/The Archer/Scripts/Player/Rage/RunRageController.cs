using MrX.Armory;
using UnityEngine;

namespace MrX.Player
{
    public sealed class RunRageController : MonoBehaviour
    {
        private const int FullCharge = 100;

        [Header("Supported Hero")]
        [SerializeField] private HeroData hero;
        [Header("Charge")]
        [SerializeField, Range(1, FullCharge)] private int enemyGain = 10;
        [SerializeField, Range(1, FullCharge)] private int bossGain = 50;
        [Header("Rage")]
        [SerializeField, Min(0.1f)] private float duration = 5f;
        [SerializeField, Min(1f)] private float attackSpeedMultiplier = 3.5f;
        [SerializeField, Min(1f)] private float damageMultiplier = 1.5f;
        [SerializeField, Min(0.02f)] private float shotInterval = 0.15f;
        [SerializeField] private UI.RunRageBar view;

        private PlayerBehavior player;
        private HeroBehavior ranger;
        private StatMultiplier speedBuff;
        private StatMultiplier damageBuff;
        private int charge;
        private float expiresAt;
        private float nextShot;
        private bool active;

        public void Begin(PlayerBehavior runPlayer, HeroData equippedHero)
        {
            End();
            if (equippedHero == null || equippedHero != hero || !(runPlayer.Hero is HeroBehavior supportedHero))
                return;

            player = runPlayer;
            ranger = supportedHero;
            player.SubscribeOnDefeat(OnPlayerDefeated);
            if (view != null) view.ShowCharge(0);
        }

        public void RegisterDefeat(bool isBoss)
        {
            if (player == null || player.HP <= 0f || active) return;

            int gain = isBoss ? bossGain : Mathf.RoundToInt(enemyGain * StageController.StageData.NormalEnemyRageFactor);
            charge = Mathf.Min(FullCharge, charge + gain);
            if (charge < FullCharge)
            {
                if (view != null) view.ShowCharge(charge);
                return;
            }

            active = true;
            expiresAt = Time.time + duration;
            nextShot = Time.time;
            speedBuff = attackSpeedMultiplier;
            damageBuff = damageMultiplier;
            ranger.SetRageShooting(true);
            player.Stats.AttackSpeedStat.AddMultiplier(speedBuff);
            player.Stats.AttackDamageStat.AddMultiplier(damageBuff);
            if (view != null) view.ShowActive(duration, duration);
        }

        private void LateUpdate()
        {
            if (!active) return;

            float remaining = expiresAt - Time.time;
            if (remaining <= 0f)
            {
                ResetCharge();
                return;
            }

            if (view != null) view.ShowActive(remaining, duration);
            if (Time.deltaTime <= 0f || !player.IsPlaying || !player.IsMovingAlowed ||
                player.MovedThisFrame || Time.time < nextShot)
                return;

            if (ranger.TryShootRageVolley())
                nextShot = Time.time + shotInterval;
        }

        private void OnPlayerDefeated(IDefeatable _) => ResetCharge();

        private void ResetCharge()
        {
            if (active && player != null)
            {
                player.Stats.AttackSpeedStat.RemoveMultiplier(speedBuff);
                player.Stats.AttackDamageStat.RemoveMultiplier(damageBuff);
                if (ranger != null) ranger.SetRageShooting(false);
            }

            active = false;
            charge = 0;
            if (view != null) view.ShowCharge(0);
        }

        public void End()
        {
            ResetCharge();
            if (player != null) player.UnsubscribeOnDefeat(OnPlayerDefeated);
            player = null;
            ranger = null;
            if (view != null) view.Hide();
        }

        private void OnDisable() => End();
    }
}
