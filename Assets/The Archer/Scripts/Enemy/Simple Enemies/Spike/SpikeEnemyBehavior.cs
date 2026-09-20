using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class SpikeEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected Float idleDuration = 2.5f;
        [SerializeField] protected Float walkDuration = (3, 5);
        [SerializeField] protected Float walkSpeed = (2, 3);
        [SerializeField] protected Float attackSpeed = (7, 9);

        [Space]
        [SerializeField] protected Float maxAttackDistance = (4, 6);
        [SerializeField] protected Float attackDuration = 4f;
        [SerializeField] protected Float magnetism = 0.1f;

        [Space]
        [SerializeField] protected ParticleSystem chargeParticle;

        [Space]
        [SerializeField] protected AudioData chargintSound;

        protected AudioSource chargeSoundSource;

        protected bool isAttacking = false;
        protected bool isTargetClose = false;

        IEasingCoroutine walkCoroutine = null;

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return null;

            while (IsAlive)
            {
                yield return LocateTarget(0.3f);

                yield return IdleCoroutine();

                if (Vector3.Distance(transform.position, Target.Position) <= maxAttackDistance)
                {
                    yield return AttackCoroutine();
                }
                else
                {
                    navigationHandler.Speed = walkSpeed;
                    if (!IsTargetCloseToAttack())
                    {
                        walkCoroutine = EasingManager.DoRepeatedly(walkDuration, 0.1f, WalkTowardsTarget).SetDelay(0.1f);

                        yield return walkCoroutine;

                        navigationHandler.Stop();

                        if (isTargetClose)
                        {
                            yield return AttackCoroutine();
                        }
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StageController.onDefeat += OnPlayerDefeated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StageController.onDefeat -= OnPlayerDefeated;
        }

        protected virtual void OnPlayerDefeated()
        {
            if (chargeSoundSource != null)
            {
                chargeSoundSource.loop = false;
                chargeSoundSource.Stop();
                chargeSoundSource = null;
            }
        }

        protected virtual void WalkTowardsTarget()
        {
            if (IsTargetCloseToAttack())
            {
                walkCoroutine.StopIfExists();
                return;
            }

            MoveTowardsTarget();
        }

        protected virtual bool IsTargetCloseToAttack()
        {
            if (Target == null) TryFindTarget();
            if (Target == null)
            {
                isTargetClose = false;
                return false;
            }

            var distance = Vector3.Distance(transform.position, Target.Position);
            isTargetClose = distance <= maxAttackDistance;
            return isTargetClose;
        }

        protected virtual IEnumerator IdleCoroutine()
        {
            LookAtPlayer = true;

            var endTime = Time.time + idleDuration;
            yield return new WaitUntil(() => IsTargetCloseToAttack() || endTime <= Time.time);

            LookAtPlayer = false;
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            animator.SetTrigger(ATTACK_TRIGGER);

            LookAtPlayer = true;
            isAttacking = true;

            var knockBackCache = useKnockback;
            useKnockback = false;

            if (chargeParticle != null) chargeParticle.Play();

            yield return new WaitForSeconds(attackDuration);

            useKnockback = knockBackCache;
            isAttacking = false;

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            LookAtPlayer = false;

            StartCoroutine(MovementCoroutine());
        }

        protected virtual IEnumerator MovementCoroutine()
        {
            chargeSoundSource = GameController.AudioManager.PlayAudio(chargintSound);
            if (chargeSoundSource != null)
            {
                chargeSoundSource.loop = true;
            }

            navigationHandler.Speed = attackSpeed;

            while (isAttacking)
            {
                yield return null;

                if (Target != null)
                {
                    var direction = transform.position.DirectionTo(Target.Position);

                    var forward = Vector3.Lerp(transform.forward, direction, Mathf.Lerp(Time.deltaTime * magnetism, 1 * magnetism, magnetism));
                    transform.forward = forward;

                    transform.position += forward * Time.deltaTime * navigationHandler.Speed;
                }
                else
                {
                    transform.position += transform.forward * Time.deltaTime * navigationHandler.Speed;
                }
            }

            if (chargeSoundSource != null)
            {
                chargeSoundSource.loop = false;
                chargeSoundSource.Stop();
                chargeSoundSource = null;
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();
            walkCoroutine.StopIfExists();

            if (chargeSoundSource != null)
            {
                chargeSoundSource.loop = false;
                chargeSoundSource.Stop();
                chargeSoundSource = null;
            }
        }
    }
}