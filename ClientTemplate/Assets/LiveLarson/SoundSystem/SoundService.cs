using UnityEngine;

namespace LiveLarson.SoundSystem
{
    public class SoundService : ISoundService
    {
        private readonly AudioSource _bgmSource;
        private readonly AudioSource _sfxSource;
        private readonly SoundSettings _soundSettings;

        public SoundService(AudioSource bgmSource, AudioSource sfxSource, SoundSettings soundSettings)
        {
            _bgmSource = bgmSource;
            _sfxSource = sfxSource;
            _soundSettings = soundSettings;
        }

        public void PlayBGM(string soundName, bool loop = true)
        {
            // Implementation for playing background music
            Debug.Log($"Playing BGM: {soundName}");
        }

        public void PlaySFX(string soundName)
        {
            // Implementation for playing sound effects
            Debug.Log($"Playing SFX: {soundName}");
        }

        public void StopBGM()
        {
            // Implementation for stopping background music
            Debug.Log("Stopping BGM");
        }
    }
}