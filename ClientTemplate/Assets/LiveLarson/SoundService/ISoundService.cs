namespace LiveLarson.SoundService
{
    public interface ISoundService
    {
        void PlayBGM(string soundName, bool loop = true);
        void PlaySFX(string musicName);
        void StopBGM();
    }
}