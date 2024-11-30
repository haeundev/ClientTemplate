using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LiveLarson.SoundService
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

            ApplySettings();
        }

        public void PlayBGM(string soundName, bool loop = true)
        {
            Addressables.LoadAssetAsync<AudioClip>(soundName).Completed += handle =>
            {
                _bgmSource.clip = handle.Result;
                _bgmSource.loop = loop;
                _bgmSource.Play();
            };
        }

        public void PlaySFX(string soundName)
        {
            Addressables.LoadAssetAsync<AudioClip>(soundName).Completed += handle =>
            {
                _sfxSource.PlayOneShot(handle.Result, _soundSettings.sfxVolume * _soundSettings.masterVolume);
            };
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        private void ApplySettings()
        {
            _bgmSource.volume = _soundSettings.bgmVolume * _soundSettings.masterVolume;
            _sfxSource.volume = _soundSettings.sfxVolume * _soundSettings.masterVolume;
        }
    }
}