using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EyeEnemyBehavior : EnemyBehavior
    {
        [Header("Idle and Walk Settings")]
        [SerializeField] protected Float idleDuration = (1, 1.5f);
        [SerializeField] protected Float delayBetweenWalkAndAttack = (0.2f, 0.5f);
        [SerializeField] protected Float maxWalkDistance = (4, 5);
        [SerializeField] protected Int walkSequenceCount = (1, 2);
        [SerializeField] protected Float delayBetweenWalks = (0.8f, 1.5f);

        [Header("Attack Warning")]
        [SerializeField] protected Float warningDuration = (0.4f, 0.8f);
        [SerializeField] protected Float maxLaserWarningDistance = 40f;
        [SerializeField] protected EyeLaserWarningBehavior laserWarning;
        [SerializeField] protected Transform lasetOrigin;
        [SerializeField] protected ParticleSystem chargeParticle;

        [Header("Attack")]
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected AudioData shootingSound;

        protected bool isFiringAnimationActive;

        public float ProjectileDamage => Damage * projectileDamageMultiplier;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            while (IsAlive)
            {
                yield return IdleCorountie(idleDuration, false);

                if (TryFindTarget())
                {
                    // Target has been detected
                    // Move a little bit 

                    yield return MovementCoroutine();

                    yield return IdleCorountie(delayBetweenWalkAndAttack, true);

                    yield return AttackCoroutine();

                } else
                {
                    // No Target Detected, should just stay idle

                    yield return null;
                }
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

                if(i != count - 1)
                {
                    LookAtPlayer = true;
                    yield return new WaitForSeconds(delayBetweenWalks);
                    LookAtPlayer = false;
                }
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
            var knockBackCache = useKnockback;
            useKnockback = false;
            laserWarning.gameObject.SetActive(true);
            laserWarning.Show(lasetOrigin.transform.position, transform.rotation * Vector3.forward, maxLaserWarningDistance);

            yield return new WaitForSeconds(warningDuration);
            
            animator.SetTrigger(ATTACK_TRIGGER);

            if(chargeParticle != null)
            {
                chargeParticle.Play();
            }

            waitForAttackToEnd.Reset();
            yield return waitForAttackToEnd;

            useKnockback = knockBackCache;
        }

        public override void OnAttackEventFired()
        {
            laserWarning.gameObject.SetActive(false);

            var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

            projectile.transform.position = lasetOrigin.position;
            projectile.transform.forward = transform.forward;

            projectile.Launch(ProjectileDamage);
            projectile.transform.localScale = Vector3.zero;
            projectile.transform.DoLocalScale(Vector3.one, 0.05f);

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }

        protected override void Defeat()
        {
            base.Defeat();

            if (laserWarning.gameObject.activeSelf)
            {
                laserWarning.gameObject.SetActive(false);
            }
        }
    }
}