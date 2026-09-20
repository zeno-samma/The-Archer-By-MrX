using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class PumpkinEnemyBehavior : EnemyBehavior
    {
        [Header("Idle and Walk Settings")]
        [SerializeField] protected Float idleDuration = (1, 1.5f);
        [SerializeField] protected Float delayBetweenWalkAndAttack = (0.2f, 0.5f);
        [SerializeField] protected Float maxWalkDistance = (4, 5);
        [SerializeField] protected Int walkSequenceCount = (1, 2);
        [SerializeField] protected Float delayBetweenWalks = (0.8f, 1.5f);

        [Header("Attack Settings")]
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Int projectilesCount = (4, 6);
        [SerializeField] protected Int attacksCount = (1, 3);
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;
        [SerializeField] protected AudioData shootingSound;

        protected int projectilesCounter;
        protected int maxProjectilesCount;

        protected float ProjectileDamage => Damage * projectileDamageMultiplier;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            while (IsAlive)
            {
                yield return LocateTarget(0.3f);

                yield return IdleCorountie(idleDuration, false);

                yield return MovementCoroutine();

                yield return IdleCorountie(delayBetweenWalkAndAttack, true);

                yield return AttackCoroutine();
            }
        }

        protected virtual IEnumerator MovementCoroutine()
        {
            LookAtPlayer = false;
            var count = walkSequenceCount;
            for (int i = 0; i < count; i++)
            {
                if (TryMoveStraightInRadius(maxWalkDistance, 1, 20))
                {
                    yield return navigationHandler.WaitUntilReachedDestination();
                }
                else
                {
                    yield return IdleCorountie(idleDuration, false);
                }

                LookAtPlayer = true;
                yield return new WaitForSeconds(delayBetweenWalks);
                LookAtPlayer = false;
            }

        }

        protected virtual IEnumerator IdleCorountie(float duration, bool lookAtPlayer)
        {
            LookAtPlayer = false;

            if (lookAtPlayer)
            {
                yield return new WaitForSeconds(duration * 0.8f);

                LookAtPlayer = true;
                yield return new WaitForSeconds(duration * 0.2f);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            var count = attacksCount; // Random.Range is not used here to avoid unnecessary complexity in the code, as Int already handles randomization

            LookAtPlayer = true;

            for (int i = 0; i < count; i++)
            {
                animator.SetTrigger(ATTACK_TRIGGER);

                waitForAttackToEnd.Reset();

                projectilesCounter = 0;
                maxProjectilesCount = projectilesCount; // Random.Range is not used here to avoid unnecessary complexity in the code, as Int already handles randomization

                yield return waitForAttackToEnd;

                animator.SetTrigger(ATTACK_ENDED_TRIGGER);
            }
        }

        public override void OnAttackEventFired()
        {
            LookAtPlayer = false;

            var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

            projectile.transform.position = projectileSpawnPosition.position;
            projectile.transform.localScale = Vector3.zero;
            projectile.transform.DoLocalScale(Vector3.one, 0.05f);
            projectile.Target = Target.transform;

            projectile.transform.forward = projectileSpawnPosition.forward;

            projectile.Launch(ProjectileDamage);

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        public override void OnAttackEndedEventFired()
        {
            projectilesCounter++;
            if(projectilesCounter >= maxProjectilesCount)
            {
                waitForAttackToEnd.Complete();
            }
        }
    }
}