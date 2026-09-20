using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EyeLaserWarningBehavior : MonoBehaviour
    {
        private int obstacleLayerMask;

        private void Awake()
        {
            obstacleLayerMask = LayerMask.GetMask("Obstacle");
        }

        public void Show(Vector3 origin, Vector3 direction, float maxDistance)
        {
            direction.Normalize();
            var end = origin + direction * maxDistance;
            if(Physics.Raycast(origin, direction, out var hit, maxDistance, obstacleLayerMask))
            {
                end = hit.point;
            }

            transform.position = (origin + end) / 2f;
            transform.localScale = new Vector3(1, 1, (end - origin).magnitude);
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}