using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    [CreateAssetMenu(menuName = "October/Enemies/Enemies Database", fileName = "Enemies Database")]
    public class EnemiesDatabase : AbstractEnemiesDatabase
    {
        [SerializeField] EnemyData[] enemies;

        public override int EnemiesCount => enemies.Length;

        public override EnemyData GetEnemy(int index)
        {
            if(enemies.Length > index) return enemies[index];
            return null;
        }

        public override EnemyData GetEnemy(EnemyType type)
        {
            for(int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType == type) return enemies[i];
            }

            return null;
        }

        public override List<string> GetEnemyNames()
        {
            var names = new List<string>();

            for(int i = 0; i < enemies.Length; i++)
            {
                names.Add(enemies[i].Name);
            }

            return names;
        }

        public override int GetEnemyIndex(EnemyType type)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType == type) return i;
            }

            return 0;
        }

        public override EnemyType GetEnemyType(string name)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].Name == name) return enemies[i].EnemyType;
            }

            return 0;
        }

#if UNITY_EDITOR
        public List<GameObject> GetAllEnemiesPrefabs()
        {
            var prefabs = new List<GameObject>();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].Prefab != null && !prefabs.Contains(enemies[i].Prefab))
                {
                    prefabs.Add(enemies[i].Prefab);
                }
            }
            return prefabs;
        }

        public EnemyType GetEnemyType(GameObject prefab)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].Prefab == prefab) return enemies[i].EnemyType;
            }

            return 0;
        }
#endif
    }
}