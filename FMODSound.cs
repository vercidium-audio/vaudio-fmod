using FMOD;
using System;

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

    public void UpdateFilter(vaudio.AudioFilter filter)
    {
        // Convert from percentage range (0 to 1) to decibel range (-80 to 10)
        var lfDecibels = PercentToDecibels(filter.gainLF);
        var mfDecibels = PercentToDecibels((filter.gainLF + filter.gainHF) / 2);
        var hfDecibels = PercentToDecibels(filter.gainHF);

        eq.setParameterFloat((int)DSP_THREE_EQ.LOWGAIN, lfDecibels);
        eq.setParameterFloat((int)DSP_THREE_EQ.MIDGAIN, mfDecibels);
        eq.setParameterFloat((int)DSP_THREE_EQ.HIGHGAIN, hfDecibels);
    }

    static float PercentToDecibels(float percent)
    {
        if (percent <= 0f)
            return -80f;

        float db = 20f * MathF.Log10(percent);

        return MathF.Max(db, -80f);
    }
}
