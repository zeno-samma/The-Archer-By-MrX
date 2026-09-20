using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class ProjectilesManager : MonoBehaviour, IProjectilesManager
    {
        protected Dictionary<GameObject, PoolComponent<AbstractProjectile>> projectilesPools = new Dictionary<GameObject, PoolComponent<AbstractProjectile>>();

        protected virtual void Awake()
        {
            projectilesPools = new Dictionary<GameObject, PoolComponent<AbstractProjectile>>();

            StageController.RegisterProjectilesManager(this);
        }

        public virtual void RegisterProjectile(GameObject projectilePrefab)
        {
            if (projectilesPools.ContainsKey(projectilePrefab)) return;

            var pool = new PoolComponent<AbstractProjectile>(projectilePrefab.name, projectilePrefab, 5);
            projectilesPools.Add(projectilePrefab, pool);
        }

        public virtual AbstractProjectile GetProjectile(GameObject projectilePrefab)
        {
            if (projectilesPools.ContainsKey(projectilePrefab))
            {
                return projectilesPools[projectilePrefab].GetEntity();
            }

            return null;
        }

        public T GetProjectile<T>(GameObject projectilePrefab) where T : AbstractProjectile
        {
            if (projectilesPools.ContainsKey(projectilePrefab))
            {
                var projectile = projectilesPools[projectilePrefab].GetEntity();

                if (projectile is T t) return t;

                return null;
            }

            return null;
        }
    }
}