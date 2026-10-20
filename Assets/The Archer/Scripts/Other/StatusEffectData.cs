using MrX.Audio;
using UnityEngine;

namespace MrX
{
    [CreateAssetMenu(fileName = "Status Effect Data", menuName = "MrX Studio/Effects/Effect Data")]
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
