using UnityEngine;

namespace OctoberStudio.Audio
{
    public interface IAudioManager
    {
        float SoundVolume { get; set; }
        float MusicVolume { get; set; }

        void PlayButtonClick();
        AudioSource PlayAudio(AudioData data);
        void PlayMainMusic();
        void PauseMusic();
        void ResumeMusic();
        void PlayMusic(AudioData audioData, bool fade = false);
        bool IsPlayingMainMenuMusic();
        bool IsPlayingMusic(AudioData music);
        AudioData GetAudioData(AudioSource source);
    }
}