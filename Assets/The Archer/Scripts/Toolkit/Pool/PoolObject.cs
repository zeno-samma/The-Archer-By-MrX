using UnityEngine;

namespace OctoberStudio.Pool
{
    public class PoolObject : Pool<GameObject>
    {
        public PoolObject(string name, GameObject prefab, int startingSize, Transform parent = null, bool dontDestroyOnLoad = false) : base(name, prefab, startingSize, parent, dontDestroyOnLoad)
        {
            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        public PoolObject(GameObject prefab, int startingSize, Transform parent = null, bool dontDestroyOnLoad = false) : base(prefab.name, prefab, startingSize, parent, dontDestroyOnLoad)
        {
            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        protected override GameObject CreateEntity()
        {
            var entity = Object.Instantiate(prefab, parent);
            if (dontDestroyOnLoad && parent == null)
            {
                Object.DontDestroyOnLoad(entity.gameObject);
            }

            return entity;
        }

        protected override void InitEntity(GameObject entity)
        {
            entity.SetActive(false);
        }

        protected override bool ValidateEntity(GameObject entity)
        {
            return !entity.activeSelf;
        }

        public override GameObject GetEntity()
        {
            var entity = base.GetEntity();

            entity.SetActive(true);

            return entity;
        }

        public T GetEntity<T>() where T : Component
        {
            var entity = base.GetEntity();

            entity.SetActive(true);

            return entity.GetComponent<T>();
        }

        protected override void DisableEntity(GameObject entity)
        {
            entity.SetActive(false);
        }

        protected override void DestroyEntity(GameObject entity)
        {
            Object.Destroy(entity);
        }
    }
}