using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class LaserProjectileBehavior : AbstractProjectile
    {
        [SerializeField] protected float maxLength;

        [Space]
        [SerializeField] protected Transform visuals;
        [SerializeField] protected BoxCollider trigger;
        [SerializeField] protected Vector3 colliderInitialSize;

        [Space]
        [SerializeField] protected Vector3 physicsCalculationRotationShift;

        public float Length { get; set; }

        public override void Launch(float damage)
        {
            Damage = damage;
            Length = 0;
            RecalculateSize();
        }

        protected virtual void Update()
        {
            var ray = new Ray(transform.position, Quaternion.Euler(physicsCalculationRotationShift) * transform.forward.NormalizeXZ());

            var layerMask = PhysicsLayerMasksHelper.GetMaskForLayer(gameObject.layer);

            if (Physics.Raycast(ray, out var hit, maxLength, layerMask))
            {
                Length = hit.distance;
            }

            RecalculateSize();
        }

        protected virtual void RecalculateSize()
        {
            visuals.localScale = new Vector3(1, 1, Length);
            trigger.size = new Vector3(colliderInitialSize.x, colliderInitialSize.y, colliderInitialSize.z * Length);
            trigger.center = new Vector3(0, 0, Length / 2);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (StageController.Room.AliveEnemiesCount == 0) return;

            var target = other.GetComponent<IProjectileTarget>();

            if (target != null)
            {
                target.TakeDamage(Damage, DamageType.Physical);
            }
        }

        public override void Hide()
        {
            base.Hide();

            gameObject.SetActive(false);
        }

        public override void Clear()
        {
            base.Clear();

            gameObject.SetActive(false);
        }

        // This laser doesn't bounce, so we override this method to do nothing.
        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier)
        {

        }

        // This laser doesn't ricochet, so we override this method to do nothing.
        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier)
        {

        }

        protected override float GetDamageDistanceMultiplier()
        {
            return 1;
        }
    }
}