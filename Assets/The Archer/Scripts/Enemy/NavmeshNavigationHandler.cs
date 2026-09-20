using UnityEngine;
using UnityEngine.AI;

namespace OctoberStudio
{
    public class NavmeshNavigationHandler : NavigationHandler
    {
        [SerializeField] protected NavMeshAgent navmeshAgent;
        [SerializeField] protected Int avoidancePriority = (40, 60);

        protected float speed;
        protected float angularSpeed;

        public override float Speed { get => navmeshAgent.speed; set => navmeshAgent.speed = value; }
        public override Vector3 Destination => navmeshAgent.destination;

        protected virtual void Awake()
        {
            speed = navmeshAgent.speed;
            angularSpeed = navmeshAgent.angularSpeed;

            navmeshAgent.avoidancePriority = avoidancePriority;
        }

        public override bool TryMove(Vector3 position)
        {
            var canMove = StageController.NavigationManager.IsPositionAvailable(position);

            if (canMove)
            {
                if (navmeshAgent.gameObject.activeSelf)
                {
                    navmeshAgent.isStopped = false;
                    navmeshAgent.SetDestination(position);
                }
            }

            return canMove;
        }

        public override void Move(Vector3 position)
        {
            if (navmeshAgent.gameObject.activeSelf && navmeshAgent.enabled)
            {
                navmeshAgent.isStopped = false;
                navmeshAgent.SetDestination(position);
            }
        }

        public override void Stop()
        {
            navmeshAgent.isStopped = true;
        }

        public override void Teleport(Vector3 position)
        {
            navmeshAgent.Warp(position);
            navmeshAgent.isStopped = true;
        }

        public override bool HasReachedDestination()
        {
            return Vector3.Distance(navmeshAgent.transform.position, navmeshAgent.destination) <= navmeshAgent.stoppingDistance;
        }

        public override float GetMovementMultiplier()
        {
            return navmeshAgent.velocity.magnitude / navmeshAgent.speed;
        }

        public override void SetMovementSpeedMultiplier(float multiplier)
        {
            navmeshAgent.speed = speed * multiplier;
            navmeshAgent.angularSpeed = angularSpeed * multiplier;
        }

        public override void SetRotationAllowed(bool allowed)
        {
            navmeshAgent.updateRotation = false;
        }

        public override void Enable()
        {
            navmeshAgent.enabled = true;
        }

        public override void Disable()
        {
            navmeshAgent.enabled = false;
        }
    }
}