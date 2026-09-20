using OctoberStudio.Audio;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class QueenWaspBossBehavior : EnemyBehavior
    {
        [SerializeField]
        protected List<QueenWaspAttackType> attacksSequence = new List<QueenWaspAttackType>
        {
            QueenWaspAttackType.Mines,
            QueenWaspAttackType.Projectiles,
            QueenWaspAttackType.Charge,
        };

        [SerializeField] protected Float idleDuration = (1, 1.5f);

        [Header("Mines Attack")]
        [SerializeField] protected GameObject honeyMinePrefab;

        [Space]
        [SerializeField] protected Float mineDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected Int consecutiveMinesAttacks = 3;
        [SerializeField] protected Float delayBetweenMinesAttack = 0.5f;

        [Space]
        [SerializeField] protected Int minesCount = 3;
        [SerializeField] protected Float minesRadius = 3f;
        [SerializeField] protected Float delayBetweenMines = 0.1f;

        [Space]
        [SerializeField] protected AudioData minesAttackSound;

        [Header("Projectiles Attack")]
        [SerializeField] protected GameObject honeyProjectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;

        [Space]
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected Int projectileLinesCount = 3;
        [SerializeField] protected Float projectileLinesSpread = 25f;

        [Space]
        [SerializeField] protected Int projectilesCountInLine = 5;

        [Space]
        [SerializeField] protected AudioData shootingSound;

        [Header("Charge Attack")]
        [SerializeField] protected WaspChargeLineBehavior chargeLine;
        [SerializeField] protected ParticleSystem chargeParticle;
        [SerializeField] protected ParticleSystem chargeIndicatorParticle;

        [Space]
        [SerializeField] protected Int chargeAttacksCount = 3;
        [SerializeField] protected Float chargeWarningDelay = 1.5f;
        [SerializeField] protected Float chargeDistance = 15;
        [SerializeField] protected Float chargeSpeed = 20f;

        [Space]
        [SerializeField] protected AudioData chargeSound;

        protected int projectilesAttackCounter = 0;

        public float ProjectileDamage => Damage * projectileDamageMultiplier;
        public float MineDamage => Damage * mineDamageMultiplier;

        protected PoolComponent<HoneyMineBehavior> honeyMinesPool;

        protected List<AbstractProjectile> launchedProjectiles = new List<AbstractProjectile>();
        protected List<HoneyMineBehavior> spawnedMines = new List<HoneyMineBehavior>();

        protected override void Awake()
        {
            base.Awake();

            honeyMinesPool = new PoolComponent<HoneyMineBehavior>(honeyMinePrefab, 10);
        }

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(honeyProjectilePrefab);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            waitForSpawnToEnd.Reset();
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            waitForSpawnToEnd.Reset();

            enemyCollider.enabled = false;

            yield return waitForSpawnToEnd;

            enemyCollider.enabled = true;

            if (TryFindTarget()) LookAtPlayer = true;

            int i = 0;
            while (IsAlive)
            {
                yield return IdleCoroutine();

                yield return MoveTowardsTargetCoroutine();

                yield return IdleCoroutine();

                switch (attacksSequence[i % attacksSequence.Count])
                {
                    case QueenWaspAttackType.Charge:
                        yield return ChargeAttack();
                        break;
                    case QueenWaspAttackType.Mines:
                        yield return MinesAttack();
                        break;
                    case QueenWaspAttackType.Projectiles:
                        yield return ProjectilesAttack();
                        break;
                }

                i++;
            }
        }

        protected virtual IEnumerator IdleCoroutine()
        {
            LookAtPlayer = true;
            yield return new WaitForSeconds(idleDuration.Value);
            LookAtPlayer = false;
        }

        protected virtual IEnumerator MoveTowardsTargetCoroutine()
        {
            if (Target != null)
            {
                navigationHandler.Move(Target.Position);
            }

            yield return new WaitForSeconds(1f);

            navigationHandler.Stop();
        }

        protected virtual IEnumerator ChargeAttack()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)QueenWaspAttackType.Charge);

            if (TryFindTarget())
            {
                for (int i = 0; i < chargeAttacksCount; i++)
                {
                    LookAtPlayer = true;

                    chargeLine.Show(chargeDistance, 0.3f);

                    yield return new WaitForSeconds(chargeWarningDelay);

                    if (chargeIndicatorParticle != null) chargeIndicatorParticle.Play();

                    animator.SetTrigger(ATTACK_TRIGGER);

                    waitForAttackToEnd.Reset();
                    yield return waitForAttackToEnd;

                    chargeLine.Hide();
                    LookAtPlayer = false;

                    if (chargeParticle != null) chargeParticle.Play();
                    GameController.AudioManager.PlayAudio(chargeSound);

                    var startPosition = transform.position;
                    var endPostion = chargeLine.EndPosition;
                    var duration = (endPostion - startPosition).magnitude / chargeSpeed;
                    float time = 0f;

                    while (time <= duration)
                    {
                        if(animator.speed > 0.1f)
                        {
                            time += Time.deltaTime;

                            float t = time / duration;
                            transform.position = Vector3.Lerp(startPosition, endPostion, t);
                        }

                        yield return null;
                    }

                    animator.SetTrigger(ATTACK_ENDED_TRIGGER);

                    if (chargeParticle != null) chargeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                    yield return null;

                }
            }

            LookAtPlayer = false;
        }

        protected virtual IEnumerator MinesAttack()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)QueenWaspAttackType.Mines);

            if (TryFindTarget())
            {
                LookAtPlayer = true;
                for (int i = 0; i < consecutiveMinesAttacks; i++)
                {
                    animator.SetTrigger(ATTACK_TRIGGER);

                    GameController.AudioManager.PlayAudio(minesAttackSound);

                    waitForAttackToEnd.Reset();

                    yield return waitForAttackToEnd;

                    yield return new WaitForSeconds(delayBetweenMinesAttack);
                }

                LookAtPlayer = false;
            }
        }

        protected virtual IEnumerator ProjectilesAttack()
        {
            if (TryFindTarget())
            {
                LookAtPlayer = true;

                animator.SetInteger(ATTACK_TYPE_INT, (int)QueenWaspAttackType.Projectiles);
                animator.SetTrigger(ATTACK_TRIGGER);

                projectilesAttackCounter = 0;

                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;

                LookAtPlayer = false;
            }
        }

        public override void OnSpawnEndedEventFired()
        {
            waitForSpawnToEnd.Complete();
        }

        public override void OnAttackEventFired()
        {
            var attackType = (QueenWaspAttackType) animator.GetInteger(ATTACK_TYPE_INT);

            switch (attackType)
            {
                case QueenWaspAttackType.Mines:
                    StartCoroutine(SpawnMines());
                    break;

                case QueenWaspAttackType.Projectiles:
                    SpawnProjectiles();
                    break;

                case QueenWaspAttackType.Charge:
                    waitForAttackToEnd.Complete();
                    break;
            }
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }

        protected virtual IEnumerator SpawnMines()
        {
            for(int i = 0; i < minesCount; i++)
            {
                var mine = honeyMinesPool.GetEntity();
                Vector3 minePosition;

                if(Target != null)
                {
                    minePosition = GetRandomPositionAroundPointInRadius(Target.Position, minesRadius, true);
                } else
                {
                    minePosition = GetRandomPositionInRadius(minesRadius, 0, true);
                }

                mine.Damage = MineDamage;
                mine.transform.position = minePosition;

                yield return new WaitForSeconds(delayBetweenMines);
                if(animator.speed < 0.1f)
                {
                    yield return new WaitUntil(() => animator.speed > 0.1f);
                }
            }
        }

        protected virtual void SpawnProjectiles()
        {
            var forwardRotation = Quaternion.LookRotation(transform.forward.SetY(0).normalized, transform.up);

            //LookAtPlayer = false;

            float angleStep;
            float startAngle;
            if(projectileLinesCount == 1)
            {
                angleStep = 0;
                startAngle = 0;
            } else
            {
                angleStep = projectileLinesSpread / (projectileLinesCount - 1);
                startAngle = -projectileLinesSpread / 2f;
            }

            projectilesAttackCounter++;

            for(int i = 0; i < projectileLinesCount; i++)
            {
                var spreadRotation = Quaternion.Euler(0, startAngle + angleStep * i, 0);

                var projectile = StageController.ProjectilesManager.GetProjectile(honeyProjectilePrefab);

                projectile.transform.rotation = forwardRotation * spreadRotation;
                projectile.transform.position = projectileSpawnPosition.position;

                projectile.onProjectileHidden += OnProjectileHidden;
                launchedProjectiles.Add(projectile);

                projectile.Launch(ProjectileDamage);
            }

            if (projectilesAttackCounter == projectilesCountInLine)
            {
                projectilesAttackCounter = 0;
                animator.SetTrigger(ATTACK_ENDED_TRIGGER);

                waitForAttackToEnd.Complete();
            }

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        protected virtual void OnProjectileHidden(AbstractProjectile projectile)
        {
            launchedProjectiles.Remove(projectile);

            projectile.onProjectileHidden -= OnProjectileHidden;
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();
        }

        public enum QueenWaspAttackType
        {
            Charge = 0,
            Mines = 1,
            Projectiles = 2,
        }
    }
}