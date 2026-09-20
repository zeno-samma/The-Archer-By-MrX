using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.StatusEffects
{
    public class StatusEffectsHandler : MonoBehaviour
    {
        [SerializeField] protected RenderersHandler renderersHandler;

        [Space]
        [SerializeField] protected List<EffectData> statusEffectsDatas;
        protected Dictionary<StatusEffectType, EffectData> statusEffectsDictionary;

        protected List<StatusEffectType> appliedStatusEffects = new List<StatusEffectType>();
        protected Dictionary<StatusEffectType, float> statusEffectsCooldownEndTime = new Dictionary<StatusEffectType, float>();

        protected Dictionary<StatusEffectType, List<ParticleSystem>> effectsParticles;

        public int AppliedStatusEffectsCount => appliedStatusEffects.Count;

        protected virtual void Start()
        {
            if (!StageController.IsLoaded)
            {
                Destroy(this);
                return;
            }

            statusEffectsDictionary = new Dictionary<StatusEffectType, EffectData>();

            foreach (var effectData in statusEffectsDatas)
            {
                if (effectData.ParticlePrefab != null) StageController.ParticlesManager.RegisterParticle(effectData.ParticlePrefab);

                statusEffectsDictionary.Add(effectData.Type, effectData);
            }

            effectsParticles = new Dictionary<StatusEffectType, List<ParticleSystem>>();
        }

        protected virtual void OnEnable()
        {
            for (int i = 0; i < statusEffectsDatas.Count; i++)
            {
                var effectData = statusEffectsDatas[i];

                if (effectData.InitialCooldown > 0)
                {
                    statusEffectsCooldownEndTime[effectData.Type] = Time.time + effectData.InitialCooldown;
                }
            }
        }

        public virtual bool HasStatusEffect(StatusEffectType type)
        {
            return appliedStatusEffects.Contains(type);
        }

        public virtual float GetStatusEffectDurationMultiplier(StatusEffectType type)
        {
            if (statusEffectsDictionary.ContainsKey(type))
            {
                return statusEffectsDictionary[type].StatusEffectDurationMultiplier;
            }

            return 1;
        }

        public virtual bool ApplyStatusEffect(StatusEffectType type)
        {
            if (!statusEffectsDictionary.ContainsKey(type)) return false;
            if (appliedStatusEffects.Contains(type)) return false;
            if (statusEffectsCooldownEndTime.ContainsKey(type) && statusEffectsCooldownEndTime[type] > Time.time) return false;

            var effectData = statusEffectsDictionary[type];

            if (effectData.ResistChance > Random.value) return false;

            appliedStatusEffects.Add(type);
            appliedStatusEffects.Sort(StatusEffectsComparator);

            effectData = statusEffectsDictionary[appliedStatusEffects[0]];

            renderersHandler.SetStatusEffect(effectData.RimData);

            var particlesEffectData = statusEffectsDictionary[type];

            if (effectData.ParticlePrefab != null)
            {
                foreach (var renderer in renderersHandler.GetRenderers())
                {
                    var particle = StageController.ParticlesManager.GetParticle(particlesEffectData.ParticlePrefab);

                    particle.transform.SetParent(renderer.transform);
                    particle.transform.ResetLocal();

                    if (particlesEffectData.ChangeParticleShapeToMesh)
                    {
                        var meshParticle = particle.GetComponent<MeshParticleBehavior>();
                        meshParticle.Init(renderer);
                    }

                    if (effectsParticles.ContainsKey(particlesEffectData.Type))
                    {
                        effectsParticles[particlesEffectData.Type].Add(particle);
                    }
                    else
                    {
                        effectsParticles.Add(particlesEffectData.Type, new List<ParticleSystem> { particle });
                    }
                }
            }

            GameController.AudioManager.PlayAudio(effectData.StatusEffectData.EffectAppliedSound);

            return true;
        }

        public virtual void RemoveStatusEffect(StatusEffectType effectType)
        {
            var effectData = statusEffectsDictionary[effectType];
            if (effectData != null)
            {
                statusEffectsCooldownEndTime[effectType] = Time.time + effectData.Cooldown;
            }

            appliedStatusEffects.Remove(effectType);

            if (appliedStatusEffects.Count > 0)
            {
                appliedStatusEffects.Sort(StatusEffectsComparator);

                renderersHandler.SetStatusEffect(statusEffectsDictionary[appliedStatusEffects[0]].RimData);
            }
            else
            {
                renderersHandler.RemoveStatusEffect();
            }

            if (effectsParticles.ContainsKey(effectType))
            {
                var particles = effectsParticles[effectType];

                for (int i = 0; i < particles.Count; i++)
                {
                    var particle = particles[i];
                    if (particle == null) continue;

                    particle.transform.SetParent(null);
                    particle.gameObject.SetActive(false);
                }

                effectsParticles.Remove(effectType);
            }

            GameController.AudioManager.PlayAudio(effectData.StatusEffectData.EffectEndedSound);
        }

        protected virtual int StatusEffectsComparator(StatusEffectType type1, StatusEffectType type2)
        {
            return (int)type1 - (int)type2;
        }

        public virtual void PlayTickSound(StatusEffectType effectType)
        {
            if (statusEffectsDictionary.TryGetValue(effectType, out var effectData))
            {
                GameController.AudioManager.PlayAudio(effectData.StatusEffectData.EffectTickSound);
            }
        }

        [System.Serializable]
        public class EffectData
        {
            [SerializeField] protected StatusEffectType type;
            public StatusEffectType Type => type;

            [Tooltip("Cooldown period after a status effect expires before others can be applied")]
            [SerializeField] protected float cooldown = 1f;
            public float Cooldown => cooldown;

            [Tooltip("Time before the status effect can be applied for the first time")]
            [SerializeField] protected float initialCooldown = 1f;
            public float InitialCooldown => initialCooldown;

            [Tooltip("Chance for the target to resist the status effect")]
            [SerializeField, Range(0, 1)] protected float resistChance;
            public float ResistChance => resistChance;

            [Tooltip("Multiplier that changes the duration of the status effect")]
            [SerializeField] protected float statusEffectDurationMultiplier = 1f;
            public float StatusEffectDurationMultiplier => statusEffectDurationMultiplier;

            [SerializeField] protected GameObject particlePrefab;
            public GameObject ParticlePrefab => particlePrefab;

            [SerializeField] protected bool changeParticleShapeToMesh;
            public bool ChangeParticleShapeToMesh => changeParticleShapeToMesh;

            [SerializeField] protected RimData rimData;
            public RimData RimData => rimData;

            [FormerlySerializedAs("statusEffectData")]
            [SerializeField] protected StatusEffectData statusEffectAudioData;
            public StatusEffectData StatusEffectData => statusEffectAudioData;
        }
    }
}