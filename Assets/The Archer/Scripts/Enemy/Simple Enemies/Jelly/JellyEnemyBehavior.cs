using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class JellyEnemyBehavior : EnemyBehavior
    {
        [Header("Idle Settings")]
        [SerializeField] protected Float idleAfterSpawnDuration = (0.3f, 0.8f);
        [SerializeField] protected bool lookAtPlayerAfterSpawn = false;

        [Space]
        [SerializeField] protected Float idleAfterAttackDuration = (1, 1.5f);
        [SerializeField] protected bool lookAtPlayerAfterAttack = false;

        [Header("Walk Settings")]
        [SerializeField] protected Float maxWalkDistance = (4, 5);
        [SerializeField] protected Int walkSequenceCount = (1, 2);
        [SerializeField] protected Float delayBetweenWalks = (0.8f, 1.5f);
        [SerializeField] protected Float delayBetweenWalkAndAttack = (0.2f, 0.5f);

        [Header("Attack")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Int attacksSequenceCount = (1, 3);
        [SerializeField] protected Int projectilesCount = 5;
        [SerializeField] protected AudioData shootingSound;

        public float ProjectileDamage => Damage * projectileDamageMultiplier;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return IdleCorountie(idleAfterSpawnDuration, lookAtPlayerAfterSpawn);

            while (IsAlive)
            {
                yield return LocateTarget(0.3f);

                yield return MovementCoroutine();

                yield return IdleCorountie(delayBetweenWalkAndAttack, true);

                yield return AttackCoroutine();

                yield return IdleCorountie(idleAfterAttackDuration, lookAtPlayerAfterAttack);
            }
        }

        protected virtual IEnumerator MovementCoroutine()
        {
            LookAtPlayer = false;

            var walksCount = walkSequenceCount; // Random.Range(walkSequenceCount.Min, walkSequenceCount.Max + 1);

            for (int i = 0; i < walksCount; i++)
            {
                var walkDistance = maxWalkDistance; // Random.Range(maxWalkDistance.Min, maxWalkDistance.Max);
                TryMoveStraightInRadius(walkDistance, 1, 20);

                yield return navigationHandler.WaitUntilReachedDestination();

                if (i != walksCount - 1) yield return IdleCorountie(delayBetweenWalks, false);
            }
        }

        protected virtual IEnumerator IdleCorountie(float duration, bool lookAtPlayer)
        {
            LookAtPlayer = lookAtPlayer;

            yield return new WaitForSeconds(duration);

            LookAtPlayer = false;
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            var attacksCount = attacksSequenceCount; // Random.Range(attacksSequenceCount.Min, attacksSequenceCount.Max + 1);

            for (int i = 0; i < attacksCount; i++)
            {
                animator.SetTrigger(ATTACK_TRIGGER);

                LookAtPlayer = true;
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;
            }
            
            LookAtPlayer = false;
        }

        public override void OnAttackEventFired()
        {
            var rotationToTarget = Quaternion.LookRotation(projectileSpawnPosition.DirectionToXZ(Target.transform.position));
            for (int i = 0; i < projectilesCount; i++)
            {
                var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

                projectile.transform.position = projectileSpawnPosition.position;
                projectile.transform.localScale = Vector3.zero;
                projectile.transform.DoLocalScale(Vector3.one, 0.05f);
                projectile.Target = Target.transform;

                var rotation = Quaternion.Euler(0, 360f / projectilesCount * i, 0);
                projectile.transform.rotation = rotationToTarget * rotation;

                projectile.Launch(ProjectileDamage);
            }

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        public override void OnAttackEndedEventFired()
        {
            base.OnAttackEndedEventFired();

            waitForAttackToEnd.Complete();
        }
    }
}