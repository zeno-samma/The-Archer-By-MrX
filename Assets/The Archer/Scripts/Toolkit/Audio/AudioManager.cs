using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OctoberStudio.Audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        [SerializeField] protected GameObject audioSource;

        [Space]
        [SerializeField] protected AudioMixerSnapshot normalSnapshot;
        [SerializeField] protected AudioMixerSnapshot pausedSnapshot;
        [SerializeField] protected float transitionDuration = 0.1f;

        [Space]
        [SerializeField] protected AudioMixerGroup gameplayGroup;
        [SerializeField] protected AudioMixerGroup uiGroup;
        [SerializeField] protected AudioMixerGroup musicGroup;

        [Space]
        [SerializeField] protected AudioData buttonClick;
        [SerializeField] protected AudioData mainMusic;

        [Space]
        [SerializeField] protected float musicFadeDuration = 0.5f;

        protected AudioSourceData Music { get; set; } = new AudioSourceData();

        protected PoolComponent<AudioSource> audioSourcePool;
        protected List<AudioSourceData> aliveSources = new List<AudioSourceData>();
        protected Dictionary<AudioData, float> lastTimePlayed = new Dictionary<AudioData, float>();
        protected Dictionary<AudioData, int> consecutiveCount = new Dictionary<AudioData, int>();
        protected AudioSave save;

        protected bool isPausedSnapshotActive = false;

        public float SoundVolume
        {
            get => save.SoundVolume;
            set
            {
                save.SoundVolume = value;
                OnSoundVolumeChanged();
            }
        }

        public float MusicVolume
        {
            get => save.MusicVolume;
            set
            {
                save.MusicVolume = value;
                OnMusicVolumeChanged();
            }
        }

        protected virtual void Awake()
        {
            if (!GameController.RegisterAudioManager(this))
            {
                Destroy(gameObject);
                return;
            }

            audioSourcePool = new PoolComponent<AudioSource>("audio source", audioSource, 2, null, true);

            gameObject.AddComponent<AudioListener>();
        }

        protected virtual void Start()
        {
            save = GameController.SaveManager.GetSave<AudioSave>("Audio");
        }

        public virtual void PlayButtonClick()
        {
            if (buttonClick != null)
            {
                PlayAudio(buttonClick);
            }
        }

        public virtual AudioSource PlayAudio(AudioData audioData)
        {
            if (audioData == null) return null;
            if (HasActiveCooldown(audioData)) return null;

            var consecutiveCount = GetConsecutiveCount(audioData);

            var source = audioSourcePool.GetEntity();
            audioData.Apply(source, consecutiveCount);
            source.outputAudioMixerGroup = GetGroup(audioData.AudioGroup);

            var data = new AudioSourceData() { source = source, volume = source.volume, data = audioData };
            aliveSources.Add(data);

            source.volume *= save.SoundVolume;

            source.Play();

            if (audioData.VibrateOnPlay)
            {
                GameController.VibrationManager.LightVibration();
            }

            lastTimePlayed[audioData] = Time.unscaledTime;

            return source;
        }

        public virtual void PlayMainMusic()
        {
            PlayMusic(mainMusic, Music.source != null);
        }

        public virtual void PauseMusic()
        {
            if (Music.source != null) Music.source.Pause();
        }

        public virtual void ResumeMusic()
        {
            if (Music.source != null) Music.source.UnPause();
        }

        public virtual void PlayMusic(AudioData audioData, bool fade = false)
        {
            if (audioData == null) return;
            if (audioData == Music.data) return;

            if (Music.source != null)
            {
                var oldMusic = Music.source;
                if (fade)
                {
                    oldMusic.DoVolume(0, musicFadeDuration).SetOnFinish(() => oldMusic.Stop());
                }
                else
                {
                    oldMusic.Stop();
                }
            }

            Music.source = audioSourcePool.GetEntity();
            audioData.Apply(Music.source, 1);
            Music.data = audioData;

            Music.source.outputAudioMixerGroup = GetGroup(audioData.AudioGroup);

            Music.source.loop = true;
            Music.volume = Music.source.volume;
            Music.volume *= save.MusicVolume;

            if (fade)
            {
                var volume = Music.source.volume;
                Music.source.volume = 0;
                Music.source.DoVolume(volume, musicFadeDuration);
            }

            Music.source.Play();
        }

        public AudioMixerGroup GetGroup(AudioGroup groupType)
        {
            switch (groupType)
            {
                case AudioGroup.UI:
                    return uiGroup;
                case AudioGroup.Gameplay:
                    return gameplayGroup;
                case AudioGroup.Music:
                    return musicGroup;
            }

            return null;
        }

        public bool IsPlayingMainMenuMusic()
        {
            return Music.data == mainMusic;
        }

        public bool IsPlayingMusic(AudioData musicData)
        {
            return Music.data = musicData;
        }

        protected virtual bool HasActiveCooldown(AudioData audioData)
        {
            if (audioData.Cooldown <= 0) return false;
            if (!lastTimePlayed.ContainsKey(audioData)) return false;

            var lastTime = lastTimePlayed[audioData];
            return Time.unscaledTime - lastTime < audioData.Cooldown;
        }

        protected virtual int GetConsecutiveCount(AudioData audioData)
        {
            if (!audioData.IsPitchCurveActive) return 0;

            if (!consecutiveCount.ContainsKey(audioData))
            {
                consecutiveCount.Add(audioData, 0);
            }
            else
            {
                if (!lastTimePlayed.TryGetValue(audioData, out var lastTime) || Time.unscaledTime - lastTime > audioData.PitchResetCooldown)
                {
                    consecutiveCount[audioData] = 0;
                }
            }

            consecutiveCount[audioData] = consecutiveCount[audioData] + 1;
            return consecutiveCount[audioData];
        }

        protected virtual void OnSoundVolumeChanged()
        {
            foreach (var source in aliveSources)
            {
                source.source.volume = source.volume * save.SoundVolume;
            }
        }

        protected virtual void OnMusicVolumeChanged()
        {
            Music.source.volume = Music.volume * save.MusicVolume;
        }

        public virtual AudioData GetAudioData(AudioSource source)
        {
            for (int i = 0; i < aliveSources.Count; i++)
            {
                var sourceData = aliveSources[i];

                if (sourceData.source == source)
                {
                    return sourceData.data;
                }
            }

            return null;
        }

        protected virtual void Update()
        {
            var shouldPause = Time.timeScale <= 0.1f;
            if (shouldPause != isPausedSnapshotActive)
            {
                if (shouldPause)
                {
                    pausedSnapshot.TransitionTo(transitionDuration);
                }
                else
                {
                    normalSnapshot.TransitionTo(transitionDuration);
                }

                isPausedSnapshotActive = shouldPause;
            }

            for (int i = 0; i < aliveSources.Count; i++)
            {
                if (!aliveSources[i].source.isPlaying && !aliveSources[i].source.loop)
                {
                    aliveSources[i].source.gameObject.SetActive(false);
                    aliveSources.RemoveAt(i);
                    i--;
                }
            }
        }

        public class AudioSourceData
        {
            public AudioData data;
            public AudioSource source;
            public float volume;
        }
    }
}