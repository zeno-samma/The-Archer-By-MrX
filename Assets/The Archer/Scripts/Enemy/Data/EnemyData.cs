using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    [System.Serializable]
    public class EnemyData
    {
        [SerializeField] string name;
        public string Name => name;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        [SerializeField] bool isBoss;
        public bool IsBoss => isBoss;

        [SerializeField] protected List<EnemyDropData> drops;
        public List<EnemyDropData> Drops => drops;
    } 
}   