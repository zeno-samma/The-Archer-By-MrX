using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class HoneyMineEventsHandler : MonoBehaviour
    {
        HoneyMineBehavior mine;

        protected virtual void Awake()
        {
            mine = GetComponentInParent<HoneyMineBehavior>();
        }

        public virtual void OnSpawnEnded()
        {
            mine.OnSpawnEndedEventFired();
        }

        public virtual void OnChargeEnded()
        {
            mine.OnChargeEndedEventFired();
        }
    }
}