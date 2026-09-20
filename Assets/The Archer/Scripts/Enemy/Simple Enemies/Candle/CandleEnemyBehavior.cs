using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.StatusEffects;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class CandleEnemyBehavior : EnemyBehavior
    {
        [Header("Idle and Walk Settings")]
        [SerializeField] protected Float idleDuration = (1, 1.5f);
        [SerializeField] protected Float delayBetweenWalkAndAttack = (0.2f, 0.5f);
        [SerializeField] protected Float maxWalkDistance = (4, 5);
        [SerializeField] protected Int walkSequenceCount = (1, 2);
        [SerializeField] protected Float delayBetweenWalks = (0.8f, 1.5f);

        [Header("Attack Settings")]
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Int attacksCount = (1, 3);
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;
        [SerializeField] protected AudioData shootingSound;

        [Header("Projectile Status Effect")]
        [SerializeField] protected bool useEffect = true;
        [SerializeField, Min(0.1f)] protected float effectDuration = 3f;
        [SerializeField, Range(0, 2)] protected float effectDamageMultiplier = 0.1f;
        [SerializeField, Min(0.1f)] protected float effectDamageInterval = 0.5f;

        public float ProjectileDamage => Damage * projectileDamageMultiplier;
        protected IgniteStatusEffect IgniteStatusEffect { get; set; }

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
            if(useEffect) IgniteStatusEffect = StageController.EnemyStatusEffectsManager.RegisterStatusEffect<IgniteStatusEffect>(Data.EnemyType, StatusEffectType.Ignite, effectDuration, effectDamageInterval, effectDamageMultiplier, 0);
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

            for (int i = 0; i < count; i++)
            {
                animator.SetTrigger(ATTACK_TRIGGER);
                LookAtPlayer = true;

                waitForAttackToEnd.Reset();

                yield return waitForAttackToEnd;

                LookAtPlayer = false;
            }
        }

        public override void OnAttackEventFired()
        {
            var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

            projectile.transform.position = projectileSpawnPosition.position;
            projectile.transform.localScale = Vector3.zero;
            projectile.transform.DoLocalScale(Vector3.one, 0.05f);
            projectile.Target = Target.transform;
            if(useEffect) projectile.ApplyStatusEffect(IgniteStatusEffect);

            projectile.transform.rotation = Quaternion.LookRotation((Target.transform.position - projectileSpawnPosition.position).SetY(0).normalized);

            projectile.Launch(ProjectileDamage);

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }
    }
}