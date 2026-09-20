using UnityEngine;

namespace OctoberStudio.Enemy.State
{
    public abstract class EnemyStateBehavior : MonoBehaviour
    {
        protected virtual EnemyBehavior Enemy { get; set; }

        protected virtual void Awake()
        {
            Enemy = GetComponent<EnemyBehavior>();
        }

        public abstract void Init();
        public abstract void StartBehavior();
    }
}