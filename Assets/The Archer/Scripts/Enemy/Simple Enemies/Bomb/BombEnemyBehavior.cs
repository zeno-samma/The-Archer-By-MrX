using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class BombEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected Float explosionTriggerRadius = 2f;
        [SerializeField] protected Float explosionRadiuis = 3f;
        [SerializeField] protected Float explosionDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected Float walkingDuration = 2f;
        [SerializeField] protected Float idleDuration = 1f;

        [Space] 
        [SerializeField] protected GameObject explosionParticlePrefab;
        [SerializeField] protected AudioData explosionSound;

        public float ExplosionDamage => Damage * explosionDamageMultiplier;

        protected IEasingCoroutine moveCoroutine;
        protected IEasingCoroutine waitCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if(explosionParticlePrefab != null) StageController.ParticlesManager.RegisterParticle(explosionParticlePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return null;

            while (IsAlive)
            {
                if (TryFindTarget())
                {
                    if (Vector3.Distance(transform.position, Target.Position) < explosionTriggerRadius)
                    {
                        //Target is within explosion radius. Exploding.
                        Kill();
                        yield break;
                    } else
                    {
                        // Moving towards target for 'walkingDuration' seconds, every 0.1 second updating target position and checking for explosion radius
                        moveCoroutine = EasingManager.DoRepeatedly(walkingDuration, 0.1f, MoveTowardsTarget);
                        yield return moveCoroutine;

                        navigationHandler.Stop();
                    }

                    // Standing still 'for idleDuration' seconds, evert 0.1 second checking for explosion radius

                    CheckForExplosion();

                    waitCoroutine = EasingManager.DoRepeatedly(idleDuration, 0.1f, CheckForExplosion).SetDelay(0.1f);
                    yield return waitCoroutine;
                }
                else
                {
                    // There is no target detected, just waiting
                    yield return new WaitForSeconds(idleDuration);
                }
            }
        }

        protected virtual void CheckForExplosion()
        {
            if (Vector3.Distance(transform.position, Target.Position) < explosionTriggerRadius)
            {
                waitCoroutine.StopIfExists();

                Kill();
            }
        }

        protected override void MoveTowardsTarget()
        {
            if (Vector3.Distance(transform.position, Target.Position) < explosionTriggerRadius)
            {
                moveCoroutine.Stop();

                Kill();
            } else
            {
                base.MoveTowardsTarget();
            }
        }

        public override void OnAttackEndedEventFired()
        {
            gameObject.SetActive(false);

            if(explosionParticlePrefab != null) 
            {
                var particle = StageController.ParticlesManager.GetParticle(explosionParticlePrefab);
                particle.transform.position = transform.position;
                particle.Play();
            }

            GameController.AudioManager.PlayAudio(explosionSound);

            if (TargetDetector != null)
            {
                TargetDetector.GetDetectedPlayers().ForEach(TryDealingExplosiveDamage);
            } else if(Target != null)
            {
                TryDealingExplosiveDamage(Target);
            }
        }

        protected virtual void TryDealingExplosiveDamage(PlayerBehavior player)
        {
            if(Vector3.Distance(player.Position, Position) < explosionRadiuis)
            {
                player.TakeDamage(ExplosionDamage, DamageType.Physical);
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();

            waitCoroutine.StopIfExists();
            moveCoroutine.StopIfExists();
        }
    }
}