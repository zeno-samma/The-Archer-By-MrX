namespace OctoberStudio.Enemy
{
    public class GolemEventsHandler : EnemyEventsHandler
    {
        protected GolemBossBehavior golemBoss;

        protected override void Awake()
        {
            base.Awake();

            golemBoss = enemy as GolemBossBehavior;
        }

        public virtual void OnFlyAway()
        {
            golemBoss.OnFlyAwayEventFired();
        }

        public virtual void OnLand()
        {
            golemBoss.OnLandEventFired();
        }
    }
}