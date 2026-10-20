using UnityEngine;

namespace MrX.Projectile
{
    public interface IProjectilesManager
    {
        AbstractProjectile GetProjectile(GameObject projectilePrefab);
        T GetProjectile<T>(GameObject projectilePrefab) where T : AbstractProjectile;
        void RegisterProjectile(GameObject projectilePrefab);
    }

}