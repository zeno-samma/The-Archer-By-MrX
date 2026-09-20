using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyEventsHandler : MonoBehaviour
    {
        protected EnemyBehavior enemy;

        protected virtual void Awake()
        {
            enemy = GetComponentInParent<EnemyBehavior>();
        }

        public virtual void OnDefeatAnimationEnded()
        {
            enemy.OnDefeatEndedEventFired();
        }

        public virtual void OnAttack()
        {
            enemy.OnAttackEventFired();
        }

        public virtual void OnAttackEnded()
        {
            enemy.OnAttackEndedEventFired();
        }

        public virtual void OnHidden()
        {
            enemy.OnHideEndedEventFired();
        }

        public virtual void OnSpawnEnded()
        {
            enemy.OnSpawnEndedEventFired();
        }
    }
}