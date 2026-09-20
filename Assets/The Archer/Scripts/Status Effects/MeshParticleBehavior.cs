using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class MeshParticleBehavior : MonoBehaviour
    {
        [SerializeField] protected List<ParticleSystem> particlesWithMeshShape;

        public virtual void Init(Renderer renderer)
        {
            if (renderer is MeshRenderer meshRenderer)
            {
                InitMeshRenderer(meshRenderer);
            }
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                InitSkinnedMeshRenderer(skinnedMeshRenderer);
            }
            if (renderer is SpriteRenderer spriteRenderer)
            {
                InitSpriteRenderer(spriteRenderer);
            }
        }

        protected virtual void InitMeshRenderer(MeshRenderer meshRenderer)
        {
            foreach (var particle in particlesWithMeshShape)
            {
                var shape = particle.shape;

                shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                shape.meshRenderer = meshRenderer;
            }
        }

        protected virtual void InitSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            foreach (var particle in particlesWithMeshShape)
            {
                var shape = particle.shape;

                shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shape.skinnedMeshRenderer = skinnedMeshRenderer;
            }
        }

        protected virtual void InitSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            foreach (var particle in particlesWithMeshShape)
            {
                var shape = particle.shape;

                shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
                shape.spriteRenderer = spriteRenderer;
            }
        }
    }
}