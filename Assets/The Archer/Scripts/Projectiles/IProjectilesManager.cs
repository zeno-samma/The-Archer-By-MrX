using UnityEngine;

namespace OctoberStudio.Projectile
{
    public interface IProjectilesManager
    {
        AbstractProjectile GetProjectile(GameObject projectilePrefab);
        T GetProjectile<T>(GameObject projectilePrefab) where T : AbstractProjectile;
        void RegisterProjectile(GameObject projectilePrefab);
    }

}