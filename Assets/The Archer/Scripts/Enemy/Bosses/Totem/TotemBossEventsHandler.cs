namespace OctoberStudio.Enemy
{
    public class TotemBossEventsHandler : EnemyEventsHandler
    {
        protected TotemBossBehavior totem;

        protected override void Awake()
        {
            base.Awake();

            totem = enemy as TotemBossBehavior;
        }

        public virtual void OnSpawnParticle()
        {
            totem.OnSpawnParticleEventFired();
        }

        public virtual void OnMonkeyChargeJump()
        {
            totem.OnMonkeyChargeJumpEventFired();
        }
    }
}