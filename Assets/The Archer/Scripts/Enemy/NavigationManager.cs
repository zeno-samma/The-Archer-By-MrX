using UnityEngine;

namespace OctoberStudio
{
    public abstract class NavigationManager : MonoBehaviour
    {
        public abstract bool IsPositionAvailable(Vector3 position);
        public abstract bool IsStraightPathAvailable(Vector3 startPosition, Vector3 endPosition, out Vector3 hitPosition, out Vector3 hitNormal);
        public abstract Vector3 GetRandomPosition();
        public abstract void Recalculate();

        protected virtual void Awake()
        {
            StageController.RegisterNavigationManager(this);
        }
    }
}