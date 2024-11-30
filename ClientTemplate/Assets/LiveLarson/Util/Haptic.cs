using Lofelt.NiceVibrations;

namespace LiveLarson.Util
{
    public static class Haptic
    {
        public static void Vibrate()
        {
            HapticPatterns.PlayEmphasis(0.4f, 0.5f);
        }
    }
}