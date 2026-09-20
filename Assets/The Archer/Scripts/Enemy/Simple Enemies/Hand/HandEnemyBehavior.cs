using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class HandEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected Float idleDuration = (1.5f, 2f);
        [SerializeField] protected Float walkDuration = (2f, 3f);
        [SerializeField] protected Float attackDistance = 10f;

        protected override IEnumerator BehaviorCoroutine()
        {
            waitForSpawnToEnd.Reset();
            yield return waitForSpawnToEnd;

            while (IsAlive)
            {
                yield return IdleCoroutine();

                TryFindTarget();

                if (Target == null)
                {
                    yield return RandomMoveCoroutine();
                } else
                {
                    yield return AttackCoroutine();
                }
            }
        }

        protected virtual IEnumerator IdleCoroutine()
        {
            yield return new WaitForSeconds(idleDuration);
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            if(!StageController.NavigationManager.IsStraightPathAvailable(transform.position, Target.Position, out var obstaclePosition, out var hitNormal))
            {
                var position = GetRandomPositionAroundPointInRadius(obstaclePosition, 3, true);

                navigationHandler.Move(position);
                yield return new WaitForSeconds(walkDuration);
            }

            if (StageController.NavigationManager.IsStraightPathAvailable(transform.position, Target.Position, out var obstaclePosition1, out var hitNormal1))
            {
                animator.SetTrigger(ATTACK_TRIGGER);

                navigationHandler.SetMovementSpeedMultiplier(3f);

                LookAtPlayer = true;

                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;

                animator.SetTrigger(ATTACK_ENDED_TRIGGER);

                navigationHandler.SetMovementSpeedMultiplier(1f);
            }
        }

        protected virtual IEnumerator RandomMoveCoroutine()
        {
            var position = GetRandomPositionInRadius(5, 2, true);

            navigationHandler.Move(position);

            yield return navigationHandler.WaitUntilReachedDestination();
        }

        public override void OnSpawnEndedEventFired()
        {
            base.OnSpawnEndedEventFired();

            waitForSpawnToEnd.Complete();
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            if (!IsAlive) return;

            LookAtPlayer = false;

            if(Target != null)
            {
                var distance = Vector3.Distance(transform.position, Target.Position);
                var direction = transform.position.DirectionToXZ(Target.Position).normalized;

                if (StageController.NavigationManager.IsStraightPathAvailable(transform.position, Target.Position, out var obstaclePosition, out var hitNormal))
                {
                    navigationHandler.Move(Target.Position + direction);
                }
                else
                {
                    navigationHandler.Move(obstaclePosition);
                }

            } else
            {
                var direction = transform.forward;

                if (StageController.NavigationManager.IsStraightPathAvailable(transform.position, transform.position + direction * attackDistance, out var obstaclePosition, out var hitNormal1))
                {
                    navigationHandler.Move(transform.position + direction * attackDistance);
                }
                else
                {
                    navigationHandler.Move(obstaclePosition);
                }
            }

            StartCoroutine(WaitUntilReachedDestination());
        }

        protected virtual IEnumerator WaitUntilReachedDestination()
        {
            yield return navigationHandler.WaitUntilReachedDestination();

            waitForAttackToEnd.Complete();
        }
    }
}