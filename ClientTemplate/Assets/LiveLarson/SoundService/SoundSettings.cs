using UnityEngine;

namespace LiveLarson.SoundService
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Scriptable Objects/SoundSettings")]
    public class SoundSettings : ScriptableObject
    {
        [Header("Volume Levels")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        /// <summary>
        /// Saves settings to PlayerPrefs.
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads settings from PlayerPrefs.
        /// </summary>
        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
    }
}