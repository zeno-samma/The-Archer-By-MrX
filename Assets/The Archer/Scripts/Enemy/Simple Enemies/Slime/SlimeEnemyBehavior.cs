using OctoberStudio.Audio;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class SlimeEnemyBehavior : EnemyBehavior
    {
        [SerializeField] protected bool spawnChildrenOnDeath = false;
        [SerializeField] protected Int childrenCount = 3;
        [SerializeField] protected EnemyType childEnemyType;

        [Space]
        [SerializeField] protected Float idleAfterSpawnDuration = (0.5f, 1f);
        [SerializeField] protected Float idleDuration = (1f, 2f);
        [SerializeField] protected Float movementDuration = (8f, 12f);

        [Space]
        [SerializeField] protected AudioData splitSound;

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return new WaitForSeconds(idleAfterSpawnDuration);

            while (IsAlive)
            {
                yield return MovementCoroutine();

                yield return new WaitForSeconds(idleDuration);
            }
        }

        protected virtual IEnumerator MovementCoroutine()
        {
            var direction = Quaternion.Euler(0, 45 + Random.Range(0, 4) * 90, 0) * Vector3.forward;

            var endTime = Time.time + movementDuration;
            while (Time.time < endTime)
            {
                var farAwayPoint = transform.position + direction * 1000;
                StageController.NavigationManager.IsStraightPathAvailable(transform.position, farAwayPoint, out var hitPoint, out var hitNormal);

                navigationHandler.Move(hitPoint - direction);

                yield return navigationHandler.WaitUntilReachedDestination();

                direction = Vector3.Reflect(direction, hitNormal).normalized;
            }

            navigationHandler.Stop();
        }

        protected override void Defeat()
        {
            if (spawnChildrenOnDeath) 
            { 
                for(int i = 0; i < childrenCount; i++)
                {
                    StageController.Room.SpawnAdditionalEnemy(childEnemyType, transform.position + Random.onUnitSphere.SetY(0).normalized, transform.rotation);
                }

                GameController.AudioManager.PlayAudio(splitSound);
            }

            base.Defeat();
        }
    }
}