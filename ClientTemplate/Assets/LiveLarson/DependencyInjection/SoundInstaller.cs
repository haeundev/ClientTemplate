using LiveLarson.SoundSystem;
using UnityEngine;
using Zenject;

namespace LiveLarson.DependencyInjection
{
    public class SoundInstaller : MonoInstaller<SoundInstaller>
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private SoundSettings soundSettings;

        public override void InstallBindings()
        {
            Container.Bind<ISoundService>().To<SoundService>().AsSingle().WithArguments(bgmSource, sfxSource, soundSettings).NonLazy();
        }
    }
}