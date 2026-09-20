using OctoberStudio.Audio;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Status Effect Data", menuName = "October/Effects/Effect Data")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Audio")]
        [SerializeField] protected AudioData effectAppliedSound;
        [SerializeField] protected AudioData effectTickSound;
        [SerializeField] protected AudioData effectEndedSound;

        public AudioData EffectAppliedSound => effectAppliedSound;
        public AudioData EffectTickSound => effectTickSound;
        public AudioData EffectEndedSound => effectEndedSound;
    }
}
