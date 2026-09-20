using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Audio
{
    [CreateAssetMenu(fileName = "Audio Data", menuName = "October/Audio/Data")]
    public class AudioData : ScriptableObject
    {
        [SerializeField] protected AudioGroup audioGroup;
        [SerializeField] protected AudioDataType audioDataType;
        [SerializeField] protected Float cooldown;
        [SerializeField] protected List<AudioSample> samples;

        [SerializeField] protected bool isPitchCurveActive;
        [SerializeField] protected AnimationCurve pitchCurve;
        [SerializeField] protected float pitchResetCooldown;
        [SerializeField] protected bool vibrateOnPlay;

        public AudioGroup AudioGroup => audioGroup;
        public AudioDataType AudioDataType => audioDataType;
        public float Cooldown => cooldown;
        public List<AudioSample> Samples => samples;

        public bool IsPitchCurveActive => isPitchCurveActive;
        public float PitchResetCooldown => pitchResetCooldown;

        public bool VibrateOnPlay => vibrateOnPlay;

        public virtual void Apply(AudioSource source, int counter)
        {
            if (samples == null || samples.Count == 0)
            {
                Debug.LogWarning($"There are no audio samples in the {name} Audio Data asset");

                return;
            }

            var pitchMultiplier = 1f;
            if (isPitchCurveActive)
            {
                pitchMultiplier = pitchCurve.Evaluate(counter - 1);
            }

            if (audioDataType == AudioDataType.Single)
            {
                samples[0].Apply(source, pitchMultiplier);
            }
            else
            {
                ApplyMulti(source, pitchMultiplier);
            }
        }

        protected virtual void ApplyMulti(AudioSource source, float pitchMultiplier)
        {
            var sumOfChances = 0f;
            for (int i = 0; i < samples.Count; i++)
            {
                sumOfChances += samples[i].Chance;
            }

            var random = Random.Range(0f, sumOfChances);

            var chance = 0f;
            for (int i = 0; i < samples.Count; i++)
            {
                chance += samples[i].Chance;

                if (random <= chance)
                {
                    samples[i].Apply(source, pitchMultiplier);
                    return;
                }
            }

            samples[^1].Apply(source, pitchMultiplier);
        }
    }

    [System.Serializable]
    public class AudioSample
    {
        [SerializeField] protected AudioClip clip;
        [SerializeField] protected Float volume = (1, 1);
        [SerializeField] protected Float pitch = (1, 1);
        [SerializeField, Range(0, 1)] protected float chance = 1f;

        public AudioClip Clip => clip;
        public Float Volume => volume;
        public Float Pitch => pitch;
        public Float Chance => chance;

        public virtual void Apply(AudioSource source, float pitchMultiplier)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch * pitchMultiplier;
        }
    }

    public enum AudioDataType
    {
        Single = 0,
        Multi = 1,
    }

    public enum AudioGroup
    {
        Gameplay = 0,
        UI = 1,
        Music = 2,
    }
}