using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class TotemTornadoBehavior : SimpleProjectile
    {
        [Space]
        [SerializeField] protected float waveLength = 3f;
        [SerializeField] protected float waveSize = 1f;
        [SerializeField] protected Collider tornadoCollider;

        protected float prevTime;

        public override void Launch(float damage)
        {
            base.Launch(damage);

            prevTime = 0;

            tornadoCollider.enabled = true;
            transform.localScale = Vector3.one;
        }

        protected override void Update()
        {
            base.Update();

            var waveLengthTime = waveLength / speed;

            var time = Time.time - launchTime;
            time /= waveLengthTime;

            var prevShift = Mathf.Sin(prevTime);
            var shift = Mathf.Sin(time);
            var difference = shift - prevShift;

            transform.position -= transform.right * difference * waveSize;

            prevTime = time;
        }

        public override void Hide()
        {
            if (onDeathParticlePrefab != null)
            {
                var deathParticle = StageController.ParticlesManager.GetParticle(onDeathParticlePrefab);
                deathParticle.transform.position = transform.position;
                deathParticle.Play();
            }

            ResetOverrides();

            for (int i = 0; i < trailsToDisableOnHit.Count; i++)
            {
                trailsToDisableOnHit[i].Clear();
                trailsToDisableOnHit[i].enabled = false;
            }

            transform.DoLocalScale(Vector3.zero, 0.2f).SetEasing(EasingType.SineIn).SetOnFinish(() => gameObject.SetActive(false));
        }
    }
}