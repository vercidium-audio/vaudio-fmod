using FMOD;

namespace vaudio_fmod;

public class FMODSound
{
    private Channel channel;
    private DSP eq;

    internal FMODSound(Channel channel, DSP eq)
    {
        this.channel = channel;
        this.eq = eq;
    }

    public void Play()
    {
        channel.setPaused(false);
    }

    public void UpdatePosition(vaudio.Vector3F pos)
    {
        VECTOR position = new() { x = pos.X, y = pos.Y, z = pos.Z };
        VECTOR velocity = new() { x = 0, y = 0, z = 0 };
        channel.set3DAttributes(ref position, ref velocity);
    }

    public void SetFrequencyGain(float gainLF, float gainMF, float gainHF)
    {
        eq.setParameterFloat((int)DSP_THREE_EQ.LOWGAIN, gainLF);
        eq.setParameterFloat((int)DSP_THREE_EQ.MIDGAIN, gainMF);
        eq.setParameterFloat((int)DSP_THREE_EQ.HIGHGAIN, gainHF);
    }
}
