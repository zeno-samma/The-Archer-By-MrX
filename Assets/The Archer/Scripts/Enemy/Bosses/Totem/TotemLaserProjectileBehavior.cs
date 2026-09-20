using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class TotemLaserProjectileBehavior : LaserProjectileBehavior
    {
        [SerializeField] protected float growthSpeed = 25;
        [SerializeField] protected bool hideInstantly = true;

        [Space]
        [SerializeField] protected ParticleSystem collisionParticle;

        protected bool isHiding = false;

        protected float maxReachedLength;

        protected IEasingCoroutine positionEasingCoroutine;
        protected IEasingCoroutine scaleEasingCoroutine;

        public override void Launch(float damage)
        {
            base.Launch(damage);

            maxReachedLength = 0;
        }

        protected override void Update()
        {
            if (!isHiding)
            {
                if (Length < maxLength)
                {
                    Length += growthSpeed * Time.deltaTime;
                }

                if(maxReachedLength < maxLength)
                {
                    maxReachedLength += growthSpeed * Time.deltaTime;
                }

                if (Length < maxReachedLength)
                {
                    Length = maxReachedLength;
                }

                var ray = new Ray(transform.position, Quaternion.Euler(physicsCalculationRotationShift) * transform.forward.NormalizeXZ());
                var playerLayerMask = 64;
                var obstacleLayerMask = 4096;

                if (Physics.Raycast(ray, out var hit, Length, playerLayerMask + obstacleLayerMask))
                {
                    Length = hit.distance;
                    if (!collisionParticle.isPlaying)
                    {
                        collisionParticle.Play();
                    }

                    var localHitPosition = hit.point - transform.position;
                    var rotatedLocalHitPosition = Quaternion.Inverse(Quaternion.Euler(physicsCalculationRotationShift)) * localHitPosition;
                    var rotatedHitPosition = rotatedLocalHitPosition + transform.position;


                    collisionParticle.transform.position = rotatedHitPosition - transform.forward.NormalizeXZ() * 0.05f;
                }
                else if (collisionParticle.isPlaying)
                {
                    collisionParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            RecalculateSize();
        }

        public override void Hide()
        {
            InvokeOnProjectileHiddenEvent();
            transform.SetParent(null);

            if(hideInstantly)
            {
                Clear();
            } else
            {
                var time = Length / growthSpeed;

                positionEasingCoroutine = transform.DoPosition(transform.position + transform.forward * Length, time);
                scaleEasingCoroutine = visuals.DoLocalScale(new Vector3(1, 1, 0), time).SetOnFinish(Clear);
            }   
        }

        public override void Clear()
        {
            base.Clear();

            transform.SetParent(null);

            scaleEasingCoroutine.StopIfExists();
            positionEasingCoroutine.StopIfExists();

            gameObject.SetActive(false);
            isHiding = false;

            collisionParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}