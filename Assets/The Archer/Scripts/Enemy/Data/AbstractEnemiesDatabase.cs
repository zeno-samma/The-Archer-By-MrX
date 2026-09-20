using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public abstract class AbstractEnemiesDatabase : ScriptableObject
    {
        public abstract int EnemiesCount { get; }

        public abstract EnemyData GetEnemy(int index);
        public abstract EnemyData GetEnemy(EnemyType type);
        public abstract List<string> GetEnemyNames();
        public abstract int GetEnemyIndex(EnemyType type);
        public abstract EnemyType GetEnemyType(string name);
    }
}