using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Projectile;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class CouldronEnemyBehavior : EnemyBehavior
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
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float puddleDamageMultiplier = (0.4f, 0.5f);
        [SerializeField] protected Int attacksSequenceCount = (1, 3);
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected GameObject puddlePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;
        [SerializeField] protected AudioData shootingSound;
        [SerializeField] protected AudioData projectileLandingSound;

        protected WaitUntilTrue waitForFiringAnimationToStop = new WaitUntilTrue();

        public float ProjectileDamage => Damage * projectileDamageMultiplier;
        public float PuddleDamage => Damage * puddleDamageMultiplier;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(puddlePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            transform.forward = transform.position.DirectionToXZ(StageController.Player.Position);

            yield return IdleCorountie(idleAfterSpawnDuration, lookAtPlayerAfterSpawn);

            while (IsAlive)
            {
                if (TryFindTarget())
                {
                    // Target has been detected
                    // Move a little bit 

                    var walksCount = walkSequenceCount; // Random.Range(walkSequenceCount.Min, walkSequenceCount.Max + 1);
                    for (int i = 0; i < walksCount; i++)
                    {
                        if (TryMoveStraightInRadius(maxWalkDistance, 1, 20))
                        {
                            yield return navigationHandler.WaitUntilReachedDestination();
                        }

                        if(i != walksCount - 1) yield return IdleCorountie(delayBetweenWalks, false);
                    }

                    yield return IdleCorountie(delayBetweenWalkAndAttack, true);

                    var attacksCount = attacksSequenceCount; // Random.Range(attacksSequenceCount.Min, attacksSequenceCount.Max + 1);
                    for (int i = 0; i < attacksCount; i++)
                    {
                        yield return AttackCoroutine();
                    }

                    yield return IdleCorountie(idleAfterAttackDuration, lookAtPlayerAfterAttack);
                }
                else
                {
                    // No Target Detected, should just stay idle

                    yield return null;
                }

                
            }
            yield return null;
        }

        protected virtual IEnumerator IdleCorountie(Float duration, bool lookAtPlayer = false)
        {
            LookAtPlayer = lookAtPlayer;
            yield return new WaitForSeconds(duration);
            LookAtPlayer = false;
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            LookAtPlayer = true;
            var startedWaitingTime = Time.time;
            yield return new WaitUntil(() => IsLookingAtPlayer(5f) || startedWaitingTime + 0.5f <= Time.time);

            animator.SetTrigger(ATTACK_TRIGGER);

            waitForFiringAnimationToStop.Reset();
            yield return waitForFiringAnimationToStop;
        }

        protected virtual bool IsLookingAtPlayer(float angleDelta)
        {
            var directionToPlayer = transform.position.DirectionToXZ(Target.Position);
            var forwardDirection = transform.forward.NormalizeXZ();

            var angle = Vector3.Angle(directionToPlayer, forwardDirection);

            return angle < angleDelta;
        }

        public override void OnAttackEventFired()
        {
            var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

            projectile.transform.position = projectileSpawnPosition.position;
            projectile.transform.localScale = Vector3.zero;
            projectile.transform.DoLocalScale(Vector3.one, 0.05f);
            projectile.Target = Target.transform;

            projectile.onProjectileHidden += OnProjectileHidden;

            projectile.Launch(ProjectileDamage);

            GameController.AudioManager.PlayAudio(shootingSound);

            LookAtPlayer = false;
        }

        public override void OnAttackEndedEventFired()
        {
            waitForFiringAnimationToStop.Complete();
        }

        protected virtual void OnProjectileHidden(AbstractProjectile projectile)
        {
            projectile.onProjectileHidden -= OnProjectileHidden;

            var puddle = StageController.ProjectilesManager.GetProjectile(puddlePrefab);

            puddle.transform.position = projectile.transform.position.SetY(0.1f);            
            puddle.Launch(PuddleDamage);

            GameController.AudioManager.PlayAudio(projectileLandingSound);
        }
    }
}