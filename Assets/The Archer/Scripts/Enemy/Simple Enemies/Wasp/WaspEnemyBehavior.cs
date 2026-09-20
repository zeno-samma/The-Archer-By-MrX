using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Enemy
{
    public class WaspEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected Float idleDuration = 2.5f;
        [SerializeField] protected Float walkDuration = (3, 5);
        [SerializeField] protected Float walkSpeed = (2, 3);
        [SerializeField] protected Float attackSpeed = (7, 9);

        [Space]
        [SerializeField] protected Float maxAttackDistance = (4, 6);
        [SerializeField] protected Float attackDuration = 4f;

        [Space]
        [SerializeField] protected ParticleSystem chargeParticle;
        [SerializeField] protected ParticleSystem chargeIndicatorParticle;

        [Space]
        [SerializeField] protected WaspChargeLineBehavior chargeLine;

        [Space]
        [SerializeField] protected AudioData warningSound;
        [FormerlySerializedAs("chargintSound")]
        [SerializeField] protected AudioData chargingSound;

        protected AudioSource chargeSoundSource;

        protected bool isAttacking = false;
        protected bool isTargetClose = false;

        protected IEasingCoroutine walkCoroutine = null;

        protected WaitUntilTrue waitForChargeToEnd = new WaitUntilTrue();

        protected Transform chacheChargeLineParent;
        protected Vector3 cacheChargeLineLocalPosition;
        protected bool knockBackCache;

        protected override void Awake()
        {
            base.Awake();

            chacheChargeLineParent = chargeLine.transform.parent;
            cacheChargeLineLocalPosition = chargeLine.transform.localPosition;
            knockBackCache = useKnockback;
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return null;
            isAttacking = false;

            while (IsAlive)
            {
                yield return IdleCoroutine();

                if (TryFindTarget() && Vector3.Distance(transform.position, Target.Position) <= maxAttackDistance)
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

            if (isTargetClose)
            {
                isTargetClose = StageController.NavigationManager.IsStraightPathAvailable(transform.position, Target.Position, out var obstaclePosition, out var hitNormal);
            }
            return isTargetClose;
        }

        protected virtual IEnumerator IdleCoroutine()
        {
            LookAtPlayer = true;

            yield return new WaitForSeconds(idleDuration);

            LookAtPlayer = false;
        }


        protected virtual IEnumerator AttackCoroutine()
        {
            chargeLine.transform.SetParent(null);
            chargeLine.transform.forward = chargeLine.transform.DirectionToXZ(Target.Position);

            var chargeDistance = attackDuration * attackSpeed;
            chargeLine.Show(chargeDistance, 0.2f);

            useKnockback = false;

            LookAtPlayer = false;

            if (chargeIndicatorParticle != null) chargeIndicatorParticle.Play();

            animator.SetTrigger(ATTACK_TRIGGER);

            GameController.AudioManager.PlayAudio(warningSound);

            waitForChargeToEnd.Reset();
            while (waitForChargeToEnd)
            {
                transform.forward = Vector3.Lerp(transform.forward, chargeLine.transform.forward, Time.deltaTime * 5f);
                yield return null;
            }

            chargeLine.Hide();
            chargeLine.transform.SetParent(chacheChargeLineParent);
            chargeLine.transform.localPosition = cacheChargeLineLocalPosition;

            yield return new WaitForSeconds(attackDuration);

            useKnockback = knockBackCache;
            isAttacking = false;

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            if (isAttacking || !IsAlive) return;

            isAttacking = true;

            LookAtPlayer = false;

            waitForChargeToEnd.Complete();

            StartCoroutine(MovementCoroutine());
        }

        protected virtual IEnumerator MovementCoroutine()
        {
            if (chargeParticle != null) chargeParticle.Play();

            chargeSoundSource = GameController.AudioManager.PlayAudio(chargingSound);

            navigationHandler.Speed = attackSpeed;

            while (isAttacking)
            {
                yield return null;

                transform.position += transform.forward * Time.deltaTime * navigationHandler.Speed;
            }

            if (chargeParticle != null) chargeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();

            walkCoroutine.StopIfExists();

            if (chargeLine.transform.parent == null)
            {
                chargeLine.transform.SetParent(chacheChargeLineParent);
                chargeLine.transform.localPosition = cacheChargeLineLocalPosition;
            }
            isAttacking = false;
            chargeLine.gameObject.SetActive(false);
        }

    }
}