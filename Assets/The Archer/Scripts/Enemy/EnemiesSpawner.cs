using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemiesSpawner : MonoBehaviour
    {
        [SerializeField] protected EnemiesDatabase enemiesDatabase;
        [SerializeField] protected ChestsDatabase chestsDatabase;
        [SerializeField] protected GameObject spawnPointPrefab;
        [SerializeField] protected GameObject bossSpawnPointPrefab;

        public EnemiesDatabase EnemiesDatabase => enemiesDatabase;
        public ChestsDatabase ChestsDatabase => chestsDatabase;

        protected Dictionary<EnemyType, PoolComponent<EnemyBehavior>> enemiesPools = new Dictionary<EnemyType, PoolComponent<EnemyBehavior>>();
        protected Dictionary<ChestType, PoolComponent<ChestBehavior>> chestsPools = new Dictionary<ChestType, PoolComponent<ChestBehavior>>();

        protected PoolComponent<EnemySpawnPointBehavior> spawnPool;
        protected PoolComponent<EnemySpawnPointBehavior> boosSpawnPool;

        protected virtual void Awake()
        {
            StageController.RegisterEnemiesSpawner(this);

            spawnPool = new PoolComponent<EnemySpawnPointBehavior>(spawnPointPrefab, 10);
            boosSpawnPool = new PoolComponent<EnemySpawnPointBehavior>(bossSpawnPointPrefab, 2);
        }

        public virtual void Init(StageData stageData)
        {
            foreach (var room in stageData.Rooms)
            {
                foreach (var wave in room.Waves)
                {
                    foreach (var spawn in wave.EnemySpawns)
                    {
                        var enemyData = enemiesDatabase.GetEnemy(spawn.EnemyType);

                        if (enemyData != null && !enemiesPools.ContainsKey(spawn.EnemyType))
                        {
                            var pool = new PoolComponent<EnemyBehavior>(enemyData.Prefab, 5);
                            enemiesPools.Add(spawn.EnemyType, pool);
                        }
                    }

                    foreach (var chestSpawn in wave.ChestSpawns)
                    {
                        var chestData = chestsDatabase.GetChestData(chestSpawn.ChestType);
                        if (chestData != null && !chestsPools.ContainsKey(chestSpawn.ChestType))
                        {
                            var pool = new PoolComponent<ChestBehavior>(chestData.ChestPrefab, 5);
                            chestsPools.Add(chestSpawn.ChestType, pool);
                        }
                    }
                }
            }
        }

        public virtual EnemySpawnPointBehavior GetSpawn()
        {
            return spawnPool.GetEntity();
        }

        public virtual EnemySpawnPointBehavior GetBossSpawn()
        {
            return boosSpawnPool.GetEntity();
        }

        public virtual EnemyBehavior GetEnemy(EnemyType enemyType)
        {
            if (enemiesPools.ContainsKey(enemyType))
            {
                var enemy = enemiesPools[enemyType].GetEntity();
                enemy.SetData(enemiesDatabase.GetEnemy(enemyType));

                return enemy;
            }
            else
            {
                var enemyData = enemiesDatabase.GetEnemy(enemyType);

                if (enemyData != null && !enemiesPools.ContainsKey(enemyType))
                {
                    var pool = new PoolComponent<EnemyBehavior>(enemyData.Prefab, 5);
                    enemiesPools.Add(enemyType, pool);

                    var enemy = enemiesPools[enemyType].GetEntity();
                    enemy.SetData(enemiesDatabase.GetEnemy(enemyType));

                    return enemy;
                }
            }

            return null;
        }

        public virtual ChestBehavior GetChest(ChestType chestType)
        {

            if (chestsPools.ContainsKey(chestType))
            {
                var chest = chestsPools[chestType].GetEntity();
                return chest;
            }
            else
            {
                var chestData = chestsDatabase.GetChestData(chestType);

                if (chestData != null && !chestsPools.ContainsKey(chestType))
                {
                    var pool = new PoolComponent<ChestBehavior>(chestData.ChestPrefab, 5);
                    chestsPools.Add(chestType, pool);

                    var chest = chestsPools[chestType].GetEntity();
                    return chest;
                }
            }
            return null;
        }
    }
}