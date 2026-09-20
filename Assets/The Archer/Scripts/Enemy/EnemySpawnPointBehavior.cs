using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Enemy
{
    public class EnemySpawnPointBehavior : MonoBehaviour
    {
        [SerializeField] protected Transform visuals;
        [SerializeField] protected Float spawnDelay = 2;

        public EnemySpawnData Data { get; private set; }

        public UnityAction<EnemySpawnPointBehavior, EnemyBehavior> onEnemySpawned;

        public virtual void Init(EnemySpawnData spawnData)
        {
            Data = spawnData;

            transform.position = Data.TransformData.Position;
            transform.rotation = Data.TransformData.Rotation;
            transform.localScale = Data.TransformData.LocalScale;

            visuals.localScale = Vector3.zero;
            visuals.DoLocalScale(Vector3.one, 0.2f).SetEasing(EasingType.SineOut);

            EasingManager.DoAfter(spawnDelay, SpawnEnemy);
        }

        protected virtual void SpawnEnemy()
        {
            var enemy = StageController.EnemiesSpawner.GetEnemy(Data.EnemyType);

            enemy.Spawn(Data);

            onEnemySpawned?.Invoke(this, enemy);
        }

        public virtual void Hide()
        {
            visuals.transform.DoLocalScale(Vector3.zero, 0.2f).SetEasing(EasingType.SineIn).SetOnFinish(() => gameObject.SetActive(false));
        }
    }
}