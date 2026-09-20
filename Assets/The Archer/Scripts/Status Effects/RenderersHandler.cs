using OctoberStudio.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class RenderersHandler : MonoBehaviour
    {
        [SerializeField] protected List<Renderer> renderers;
        protected List<RendererData> rendererDatas = new List<RendererData>();

        [Space]
        [SerializeField] protected RimData hitRimEffectData;
        [SerializeField] protected float hitEffectDuration = 0.15f;
        [SerializeField] protected EasingType hitEffectEasing = EasingType.SineIn;

        protected virtual void Awake()
        {
            for(int i = 0; i < renderers.Count; i++)
            {
                rendererDatas.Add(new RendererData(renderers[i]));
            }
        }

        public virtual void SetStatusEffect(RimData rimData)
        {
            foreach (RendererData data in rendererDatas)
            {
                data.SetEffectData(rimData);
            }
        }

        public virtual void RemoveStatusEffect()
        {
            foreach (RendererData data in rendererDatas)
            {
                data.RemoveEffectsData();
            }
        }

        public virtual void PlayHitEffect()
        {
            foreach(RendererData data in rendererDatas)
            {
                data.DoHitEffect(hitRimEffectData, hitEffectDuration, hitEffectEasing);
            }
        }

        public virtual List<Renderer> GetRenderers()
        {
            return renderers;
        }

        protected virtual void OnDisable()
        {
            foreach (RendererData data in rendererDatas)
            {
                data.Clear();
            }
        }

        public class RendererData
        {
            protected static readonly int _RimColor = Shader.PropertyToID("_RimColor");
            protected static readonly int _RimStrength = Shader.PropertyToID("_RimStrength");
            protected static readonly int _RimGradient = Shader.PropertyToID("_RimGradient");
            protected static readonly int _RimDirectional = Shader.PropertyToID("_RimDirectional");

            public Renderer Renderer { get; protected set; }

            public Material SharedMaterial { get; protected set; }
            public Material EffectsMaterial { get; protected set; }

            public ShaderColorValue RimColorValue { get; protected set; }
            public ShaderFloatValue RimStrengthValue { get; protected set; }
            public ShaderFloatValue RimGradientValue { get; protected set; }
            public ShaderFloatValue RimDirectionalValue { get; protected set; }

            protected Coroutine hitEffectCoroutine;
            protected bool usesEffectData;
            protected bool removingHitEffect;

            protected IEasingCoroutine resetCoroutine;

            public RendererData(Renderer renderer)
            {
                Renderer = renderer;

                SharedMaterial = renderer.sharedMaterial;
                EffectsMaterial = Instantiate(SharedMaterial);

                RimColorValue = new ShaderColorValue(EffectsMaterial, _RimColor);
                RimStrengthValue = new ShaderFloatValue(EffectsMaterial, _RimStrength);
                RimGradientValue = new ShaderFloatValue(EffectsMaterial, _RimGradient);
                RimDirectionalValue = new ShaderFloatValue(EffectsMaterial, _RimDirectional);
            }

            public virtual void SetEffectData(RimData effectData)
            {
                if (Renderer == null) return;

                usesEffectData = true;
                Renderer.material = EffectsMaterial;

                RimColorValue.SetIntermidiateValue(effectData.RimColor);
                RimStrengthValue.SetIntermidiateValue(effectData.RimStrength);
                RimGradientValue.SetIntermidiateValue(effectData.RimGradient);
                RimDirectionalValue.SetIntermidiateValue(effectData.RimDirectional);

                if (removingHitEffect)
                {
                    EasingManager.StopCustomCoroutine(hitEffectCoroutine);
                    hitEffectCoroutine = null;
                    removingHitEffect = false;

                    RimColorValue.DoReset(0.1f);
                    RimStrengthValue.DoReset(0.1f);
                    RimGradientValue.DoReset(0.1f);
                    RimDirectionalValue.DoReset(0.1f);

                    resetCoroutine.StopIfExists();
                }
                else if (hitEffectCoroutine == null)
                {
                    RimColorValue.DoReset(0.1f);
                    RimStrengthValue.DoReset(0.1f);
                    RimGradientValue.DoReset(0.1f);
                    RimDirectionalValue.DoReset(0.1f);

                    resetCoroutine.StopIfExists();
                }
            }

            public virtual void RemoveEffectsData()
            {
                usesEffectData = false;

                RimColorValue.RemoveIntermidiateValue();
                RimStrengthValue.RemoveIntermidiateValue();
                RimGradientValue.RemoveIntermidiateValue();
                RimDirectionalValue.RemoveIntermidiateValue();

                if (removingHitEffect)
                {
                    EasingManager.StopCustomCoroutine(hitEffectCoroutine);
                    hitEffectCoroutine = null;
                    removingHitEffect = false;

                    RimColorValue.DoReset(0.1f);
                    RimStrengthValue.DoReset(0.1f);
                    RimGradientValue.DoReset(0.1f);
                    RimDirectionalValue.DoReset(0.1f);

                    resetCoroutine.StopIfExists();
                    resetCoroutine = EasingManager.DoAfter(0.1f, () =>
                    {
                        if (!usesEffectData)
                        {
                            if (Renderer != null) Renderer.material = SharedMaterial;
                        }
                    });
                } 
                else if (hitEffectCoroutine == null)
                {
                    RimColorValue.DoReset(0.1f);
                    RimStrengthValue.DoReset(0.1f);
                    RimGradientValue.DoReset(0.1f);
                    RimDirectionalValue.DoReset(0.1f);

                    resetCoroutine.StopIfExists();
                    resetCoroutine = EasingManager.DoAfter(0.1f, () =>
                    {
                        if (!usesEffectData)
                        {
                            if(Renderer != null) Renderer.material = SharedMaterial;
                        }
                    });
                } 
            }

            public virtual void DoHitEffect(RimData hitData, float duration, EasingType easingType = EasingType.Linear)
            {
                resetCoroutine.StopIfExists();
                if (hitEffectCoroutine != null) EasingManager.StopCustomCoroutine(hitEffectCoroutine);

                Renderer.material = EffectsMaterial;

                hitEffectCoroutine = EasingManager.StartCustomCoroutine(HitEffectCoroutine(hitData, duration, easingType));
            }

            public virtual IEnumerator HitEffectCoroutine(RimData hitData, float duration, EasingType easingType = EasingType.Linear)
            {
                var firstDuration = duration / 3;

                RimColorValue.DoValue(hitData.RimColor, firstDuration);
                RimStrengthValue.DoValue(hitData.RimStrength, firstDuration);
                RimGradientValue.DoValue(hitData.RimGradient, firstDuration);
                RimDirectionalValue.DoValue(hitData.RimDirectional, firstDuration);

                yield return new WaitForSeconds(firstDuration);
                removingHitEffect = true;

                var secondDuration = duration / 3 * 2;

                RimColorValue.DoReset(secondDuration, easingType);
                RimStrengthValue.DoReset(secondDuration, easingType);
                RimGradientValue.DoReset(secondDuration, easingType);
                RimDirectionalValue.DoReset(secondDuration, easingType);

                yield return new WaitForSeconds(secondDuration);

                if (!usesEffectData)
                {
                    Renderer.material = SharedMaterial;
                }

                removingHitEffect = false;
                hitEffectCoroutine = null;
            }

            public virtual void Reset()
            {
                Renderer.material = SharedMaterial;
            }

            public virtual void Clear()
            {
                resetCoroutine.StopIfExists();
            }
        }
    }
}