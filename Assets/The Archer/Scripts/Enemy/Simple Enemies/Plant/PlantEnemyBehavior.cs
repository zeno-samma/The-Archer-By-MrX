using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class PlantEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;

        [Space]
        [SerializeField] protected Int attacksSequenceCount = (2, 3);
        [SerializeField] protected Float idleBeforeAttack = (1.5f, 2f);
        [SerializeField] protected Float idleAfterAttack = (1, 1.5f);
        [SerializeField] protected Float ungergroundIdleDuration = (0.5f, 1f);

        [Space]
        [SerializeField] protected GameObject digInParticlePrefab;
        [SerializeField] protected GameObject digOutParticlePrefab;
        [SerializeField] protected ParticleSystem spawnParticle;
        [SerializeField] protected ParticleSystem fireParticle;

        [Header("Sounds")]
        [SerializeField] protected AudioData digInSound;
        [SerializeField] protected AudioData digOutSound;
        [SerializeField] protected AudioData shootingSound;

        protected float ProjectileDamage => Damage * projectileDamageMultiplier;

        protected WaitUntilTrue waitForHidingToStop = new WaitUntilTrue();

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);

            if(digInParticlePrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(digInParticlePrefab);
            }

            if(digOutParticlePrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(digOutParticlePrefab);
            }
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            if(spawnParticle != null) spawnParticle.Play();

            waitForSpawnToEnd.Reset();
            yield return waitForSpawnToEnd;

            while (IsAlive)
            {
                yield return LocateTarget(0.3f);

                LookAtPlayer = true;
                yield return new WaitForSeconds(idleBeforeAttack);

                int attacksCount = attacksSequenceCount; // Random.Range(attacksSequenceCount.Min, attacksSequenceCount.Max + 1);
                for (int i = 0; i < attacksCount; i++)
                {
                    yield return AttackCoroutine();
                }

                yield return new WaitForSeconds(idleAfterAttack);

                yield return DigInCoroutine();

                yield return DigOutCoroutine();
            }
            yield return null;
        }

        protected virtual IEnumerator DigOutCoroutine()
        {
            yield return new WaitForSeconds(ungergroundIdleDuration);

            var position = GetRandomPositionInRadius(10, 1, true);

            transform.position = position;

            animator.SetTrigger(SHOW_TRIGGER);

            healthbar.Show();

            enemyCollider.enabled = true;

            if(digOutParticlePrefab != null)
            {
                var particle = StageController.ParticlesManager.GetParticle(digOutParticlePrefab);
                particle.transform.position = transform.position;
                particle.Play();
            }

            GameController.AudioManager.PlayAudio(digOutSound);
        }

        protected virtual IEnumerator DigInCoroutine()
        {
            LookAtPlayer = false;

            animator.SetTrigger(HIDE_TRIGGER);

            if (digInParticlePrefab != null)
            {
                var particle = StageController.ParticlesManager.GetParticle(digInParticlePrefab);
                particle.transform.position = transform.position;
                particle.Play();
            }

            GameController.AudioManager.PlayAudio(digInSound);

            waitForHidingToStop.Reset();
            yield return waitForHidingToStop;

            enemyCollider.enabled = false;
            healthbar.Hide();
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            animator.SetTrigger(ATTACK_TRIGGER);
            waitForAttackToEnd.Reset();

            yield return waitForAttackToEnd;
        }

        public override void OnAttackEventFired()
        {
            var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

            projectile.transform.position = projectileSpawnPosition.position;
            projectile.transform.localScale = Vector3.zero;
            projectile.transform.DoLocalScale(Vector3.one, 0.05f);
            projectile.Target = Target.transform;
            projectile.transform.rotation = Quaternion.LookRotation((Target.transform.position - projectileSpawnPosition.position).SetY(0).normalized);

            projectile.Launch(ProjectileDamage);

            if(fireParticle != null)
            {
                fireParticle.Play();
            }

            GameController.AudioManager.PlayAudio(shootingSound);
        }

        public override void OnAttackEndedEventFired()
        {
            base.OnAttackEndedEventFired();
            waitForAttackToEnd.Complete();
        }

        public override void OnHideEndedEventFired()
        {
            waitForHidingToStop.Complete();
        }

        public override void OnSpawnEndedEventFired()
        {
            base.OnSpawnEndedEventFired();

            waitForSpawnToEnd.Complete();
        }
    }
}