using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Particles
{
    public class ParticlesManager : MonoBehaviour
    {
        protected Dictionary<GameObject, PoolComponent<ParticleSystem>> particlesPools = new Dictionary<GameObject, PoolComponent<ParticleSystem>>();

        protected virtual void Awake()
        {
            particlesPools = new Dictionary<GameObject, PoolComponent<ParticleSystem>>();

            StageController.RegisterParticlesManager(this);
        }

        public virtual void RegisterParticle(GameObject particlePrefab)
        {
            if (particlesPools.ContainsKey(particlePrefab)) return;

            var pool = new PoolComponent<ParticleSystem>(particlePrefab.name, particlePrefab, 5);
            particlesPools.Add(particlePrefab, pool);
        }

        public virtual ParticleSystem GetParticle(GameObject particlePrefab)
        {
            if (particlesPools.ContainsKey(particlePrefab))
            {
                return particlesPools[particlePrefab].GetEntity();
            }

            return null;
        }

        public T GetParticle<T>(GameObject particlePrefab) where T : Component
        {
            if (particlesPools.ContainsKey(particlePrefab))
            {
                var particle = particlesPools[particlePrefab].GetEntity();

                if (particle is T t) return t;

                return null;
            }

            return null;
        }
    }
}