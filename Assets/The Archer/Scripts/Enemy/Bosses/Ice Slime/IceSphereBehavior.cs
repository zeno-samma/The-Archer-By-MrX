using OctoberStudio.Audio;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Projectile
{
    public class IceSphereBehavior : SimpleProjectile
    {
        [SerializeField] protected GameObject secondaryProjectilePrefab;
        [SerializeField] protected float secondaryProjectileDamageMultiplier = 0.3f;
        [SerializeField] protected int secondaryProjectilesCount = 6;

        [Space]
        [SerializeField] protected AudioData sphereBurstSound;

        public event UnityAction<AbstractProjectile> onSecondaryProjectileSpawned;

        protected override void Start()
        {
            base.Start();
            StageController.ProjectilesManager.RegisterProjectile(secondaryProjectilePrefab);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                target.TakeDamage(Damage, GetDamageType());
                target.ShowHitEffect(Vector3.zero, false);
            } else
            {
                SpawnSecondaryProjectiles();
            }

            Hide();
        }

        protected override bool CheckLifetime()
        {
            var isLifetimeEnded = base.CheckLifetime();

            if(isLifetimeEnded)
            {
                SpawnSecondaryProjectiles();
            }

            return isLifetimeEnded;
        }

        protected virtual void SpawnSecondaryProjectiles()
        {
            GameController.AudioManager.PlayAudio(sphereBurstSound);

            for (int i = 0; i < secondaryProjectilesCount; i++)
            {
                var angle = 360 / secondaryProjectilesCount * i;

                var direction = Quaternion.Euler(0, angle, 0) * transform.forward.NormalizeXZ();

                var projectile = StageController.ProjectilesManager.GetProjectile(secondaryProjectilePrefab);
                projectile.transform.position = transform.position;
                projectile.transform.forward = direction;

                projectile.Launch(Damage * secondaryProjectileDamageMultiplier);

                onSecondaryProjectileSpawned?.Invoke(projectile);
            }
        }
    }
}