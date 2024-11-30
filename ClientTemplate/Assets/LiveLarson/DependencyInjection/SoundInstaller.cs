using LiveLarson.SoundService;
using UnityEngine;
using Zenject;

namespace LiveLarson.DependencyInjection
{
    public class SoundInstaller : MonoInstaller
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private SoundSettings soundSettings;
        
        public override void InstallBindings()
        {
            Container.Bind(typeof(ISoundService)).To(typeof(SoundService.SoundService)).AsSingle().NonLazy();
        }

        private void InitializeServices()
        {
        }
    }
}