using System.Threading;

namespace vaudio_fmod;

public class Program
{
    public static void Main()
    {
        var fmod = new FMODExample();

        fmod.Initialize();
        fmod.LoadSound("resource/audio/speech.wav");
        fmod.SetListenerPosition(0, 0, 0, 0, 0);
        fmod.PlayAt(5, 0, 0);

        var gain = -40;
        fmod.SetFrequencyGain(0, gain, gain);

        while (true)
        {
            fmod.Update();
            Thread.Sleep(16);
        }
    }
}
