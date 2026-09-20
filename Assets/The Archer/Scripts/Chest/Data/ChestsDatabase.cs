using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Chests Database", menuName = "October/Chests Database")]
    public class ChestsDatabase : ScriptableObject
    {
        [SerializeField] public List<ChestData> chests;
        public List<ChestData> Chests => chests;

        public int Count => chests.Count;

        public ChestData GetChestData(ChestType chestType)
        {
            for(int i = 0; i < chests.Count; i++)
            {
                if (chests[i].ChestType == chestType)
                {
                    return chests[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public List<GameObject> GetAllChestsPrefabs()
        {
            var prefabs = new List<GameObject>();
            for (int i = 0; i < chests.Count; i++)
            {
                if (chests[i].ChestPrefab != null && !prefabs.Contains(chests[i].ChestPrefab))
                {
                    prefabs.Add(chests[i].ChestPrefab);
                }
            }
            return prefabs;
        }

        public ChestType GetChestType(GameObject prefab)
        {
            for (int i = 0; i < chests.Count; i++)
            {
                if (chests[i].ChestPrefab == prefab) return chests[i].ChestType;
            }

            return 0;
        }
#endif
    }
}