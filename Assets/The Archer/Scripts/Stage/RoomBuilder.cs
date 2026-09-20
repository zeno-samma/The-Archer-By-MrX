using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class RoomBuilder
    {
        protected Dictionary<GameObject, PoolObject> poolsDictionary = new Dictionary<GameObject, PoolObject>();

        protected Transform parentTransform;

        public RoomBuilder(RoomBehavior room)
        {
            parentTransform = room.transform;
        }

        public virtual void Init(StageData stageData)
        {
            var prefabsDictionary = new Dictionary<GameObject, int>();

            for (int i = 0; i < stageData.RoomsCount; i++)
            {
                var roomData = stageData.GetRoom(i);

                var roomPrefabsDictionary = new Dictionary<GameObject, int>();

                for (int j = 0; j < roomData.Prefabs.Length; j++)
                {
                    var prefab = roomData.Prefabs[j].Prefab;
                    if (!roomPrefabsDictionary.ContainsKey(prefab))
                    {
                        roomPrefabsDictionary[prefab] = 0;
                    }
                    roomPrefabsDictionary[prefab]++;
                }

                foreach (var prefab in roomPrefabsDictionary)
                {
                    if (!prefabsDictionary.ContainsKey(prefab.Key) || roomPrefabsDictionary[prefab.Key] > prefabsDictionary[prefab.Key])
                    {
                        prefabsDictionary[prefab.Key] = roomPrefabsDictionary[prefab.Key];
                    }
                }
            }

            foreach (var prefab in prefabsDictionary)
            {
                if (!poolsDictionary.ContainsKey(prefab.Key))
                {
                    poolsDictionary[prefab.Key] = new PoolObject(prefab.Key, prefab.Value, parentTransform);
                }
            }
        }

        public GameObject GetPooledObject(GameObject prefab)
        {
            if (poolsDictionary.ContainsKey(prefab))
            {
                return poolsDictionary[prefab].GetEntity();
            }
            else
            {
                Debug.LogError($"Prefab {prefab.name} not found in pools.");
                return null;
            }
        }

        public virtual void ClearRoom()
        {
            foreach (var pool in poolsDictionary.Values)
            {
                pool.DisableAllEntities();
            }
        }

        public virtual void Destroy()
        {
            foreach (var pool in poolsDictionary.Values)
            {
                pool.Destroy();
            }
            poolsDictionary.Clear();
        }
    }
}