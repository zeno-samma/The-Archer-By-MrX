using System.Collections;
using System.Collections.Generic;
using MrX.Enemy;
using UnityEngine;

namespace MrX.Weapon
{
    /// <summary>One contact query per animation event. Uses the same player stats and damage pipeline as projectiles.</summary>
    public sealed class MeleeWeaponBehavior : AbstractWeaponBehavior
    {
        [SerializeField, Range(20, 360)] private float swingAngle = 115;
        [SerializeField, Min(0)] private float extraStrikeDamage = .35f;
        [SerializeField] private Audio.AudioData swingSound;
        [SerializeField] private ParticleSystem swingParticle;
        [SerializeField] private Vector3 detachedPosition = new Vector3(0, .3f, 0);
        [SerializeField] private Vector3 detachedRotation;
        [SerializeField] private Vector3 detachedScale = Vector3.one;
        private readonly Collider[] contacts = new Collider[128];
        private readonly HashSet<EnemyBehavior> hitEnemies = new HashSet<EnemyBehavior>();
        private Coroutine detachedAttack;
        private int enemyMask, obstacleMask;
        public override Vector3 DetachedPosition => detachedPosition;
        public override Vector3 DetachedRotation => detachedRotation;
        public override Vector3 DetachedScale => detachedScale;
        private void Awake() { enemyMask = LayerMask.GetMask("Enemy"); obstacleMask = LayerMask.GetMask("Obstacle"); }
        public override void SetAnimationLength(float length) { }
        public override void OnShootEventFired() { }
        public override void Shoot(Transform target, Vector3 direction)
        {
            if (!StageController.IsLoaded || !StageController.Player.IsAlive || Time.timeScale <= 0) return;
            var player = StageController.Player;
            if (!IsDetached && player.MovedThisFrame) return;
            var origin = IsDetached ? transform.position : player.Position;
            direction.y = 0; if (direction.sqrMagnitude < .001f) direction = player.transform.forward;
            direction.Normalize();
            var stats = player.Stats;
            float reach = Mathf.Max(.5f, stats.ArrowRangeStat.Value);
            int count = Physics.OverlapSphereNonAlloc(origin + Vector3.up * .5f, reach + .5f, contacts, enemyMask, QueryTriggerInteraction.Collide);
            float cosine = Mathf.Cos(swingAngle * .5f * Mathf.Deg2Rad);
            hitEnemies.Clear();
            for (int i = 0; i < count; i++)
            {
                var enemy = contacts[i].GetComponentInParent<EnemyBehavior>(); contacts[i] = null;
                if (enemy == null || !enemy.IsAlive || !enemy.IsColliderEnabled || !hitEnemies.Add(enemy)) continue;
                var offset = enemy.Position - origin; offset.y = 0;
                if (offset.sqrMagnitude > (reach + .3f) * (reach + .3f)) continue;
                var toEnemy = offset.sqrMagnitude > .001f ? offset.normalized : direction;
                float damage = 0;
                if (Vector3.Dot(direction, toEnemy) >= cosine)
                    damage = stats.FrontArrowDamageStat.Value * (1 + Mathf.Max(0, stats.FrontArrowsCountStat.Value - 1) * extraStrikeDamage);
                if (stats.RearArrowsCountStat.Value > 0 && Vector3.Dot(-direction, toEnemy) >= cosine)
                    damage = Mathf.Max(damage, stats.RearArrowDamageStat.Value * stats.RearArrowsCountStat.Value);
                if (stats.DiagonalArrowsCountStat.Value > 0 && (Vector3.Dot(Quaternion.Euler(0,45,0)*direction,toEnemy)>=cosine || Vector3.Dot(Quaternion.Euler(0,-45,0)*direction,toEnemy)>=cosine))
                    damage = Mathf.Max(damage, stats.DiagonalArrowDamageStat.Value * stats.DiagonalArrowsCountStat.Value);
                if (damage <= 0 || (!player.ProjectilesIgnoreObstacles && Physics.Linecast(origin + Vector3.up, enemy.Position + Vector3.up, obstacleMask))) continue;
                bool critical = Random.value * 100 < stats.CritChanceStat.Value;
                if (critical) damage *= stats.CritDamageMultiplierStat.Value;
                foreach (var effect in appliedEffects) if (effect != null) damage *= effect.ArrowDamageMultiplier;
                var damageType = appliedEffects.Count == 0 ? DamageType.Physical : DamageHelper.GetDamageTypeFromStatusEffect(appliedEffects[appliedEffects.Count - 1].Type);
                enemy.TakeDamage(damage, damageType, critical);
                enemy.ShowHitEffect(direction, true);
                if (enemy.IsAlive) foreach (var effect in appliedEffects) if (effect != null) effect.ApplyToTarget(enemy, damage);
            }
            if (swingSound != null) GameController.AudioManager.PlayAudio(swingSound);
            if (swingParticle != null) swingParticle.Play();
        }
        public override void OnDetachedFromHero()
        {
            IsDetached = true;
            if (detachedAttack != null) StopCoroutine(detachedAttack);
            detachedAttack = StartCoroutine(AttackDetached());
        }
        private IEnumerator AttackDetached()
        {
            while (IsDetached)
            {
                var target = StageController.Player.ClosestEnemy;
                if (target != null) Shoot(target.transform, target.Position - transform.position);
                yield return new WaitForSeconds(1f / Mathf.Max(.1f, StageController.Player.Stats.AttackSpeedStat.Value));
            }
        }
        public override void OnAttachedToHero() { IsDetached = false; if (detachedAttack != null) StopCoroutine(detachedAttack); detachedAttack = null; }
        private void OnDisable() { if (detachedAttack != null) StopCoroutine(detachedAttack); detachedAttack = null; hitEnemies.Clear(); }
    }
}
