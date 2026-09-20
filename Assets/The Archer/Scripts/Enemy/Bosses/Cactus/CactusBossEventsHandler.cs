namespace OctoberStudio.Enemy
{
    public class CactusBossEventsHandler : EnemyEventsHandler
    {
        protected CactusBossBehavior cactusBoss;

        protected override void Awake()
        {
            base.Awake();
            cactusBoss = enemy as CactusBossBehavior;
        }

        public void OnLeftHandInserted()
        {
            cactusBoss.OnLeftHandInsertedEventFired();
        }

        public void OnRightHandInserted()
        {
            cactusBoss.OnRightHandInsertedEventFired();
        }
    }
}