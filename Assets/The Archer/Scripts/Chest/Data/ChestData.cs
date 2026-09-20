using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class ChestData 
    {
        [SerializeField] protected ChestType chestType;
        public ChestType ChestType => chestType;

        [SerializeField] protected GameObject chestPrefab;
        public GameObject ChestPrefab => chestPrefab;
    }
}