using System.Collections;
using UnityEngine;

namespace OctoberStudio
{
    public abstract class NavigationHandler : MonoBehaviour
    {
        public abstract bool TryMove(Vector3 position);
        public abstract void Move(Vector3 position);
        public abstract float GetMovementMultiplier();
        public abstract void Stop();

        public abstract float Speed { get; set; }
        public abstract Vector3 Destination { get; }

        public abstract bool HasReachedDestination();

        public abstract void Enable();
        public abstract void Disable();

        public abstract void Teleport(Vector3 position);

        public abstract void SetMovementSpeedMultiplier(float multiplier);

        public abstract void SetRotationAllowed(bool allowed);

        public virtual IEnumerator WaitUntilReachedDestination()
        {
            return new WaitUntilReachedDestinationEnumerator(this);
        }

        protected class WaitUntilReachedDestinationEnumerator: IEnumerator
        {
            protected NavigationHandler navigationHandler;

            public object Current => navigationHandler;

            public WaitUntilReachedDestinationEnumerator(NavigationHandler navigationHandler) 
            {
                this.navigationHandler = navigationHandler;   
            }

            public virtual bool MoveNext()
            {
                return !navigationHandler.HasReachedDestination();
            }

            public virtual void Reset()
            {

            }
        }
    }
}