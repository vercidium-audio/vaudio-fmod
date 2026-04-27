using FMOD;
using System;

namespace vaudio_fmod;

public class FMODExample
{
    private FMOD.System system;
    private Sound sound;
    private Channel channel;

    public void Initialize()
    {
        Factory.System_Create(out system);
        system.init(512, INITFLAGS.NORMAL, IntPtr.Zero);

        // Enable 3D sound
        system.set3DSettings(1.0f, 1.0f, 1.0f);
    }

    public void LoadSound(string filePath)
    {
        // FMOD_3D flag makes the sound positional
        system.createSound(filePath, MODE._3D | MODE.LOOP_OFF, out sound);
        sound.set3DMinMaxDistance(1.0f, 100.0f);
    }

    public void PlayAt(float x, float y, float z)
    {
        // Start paused so we can set position before audible playback
        system.getMasterChannelGroup(out ChannelGroup masterGroup);
        system.playSound(sound, masterGroup, true, out channel);

        VECTOR position = new() { x = x, y = y, z = z };
        VECTOR velocity = new() { x = 0, y = 0, z = 0 };
        channel.set3DAttributes(ref position, ref velocity);

        channel.setPaused(false);
    }

    public void SetListenerPosition(float x, float y, float z, float forwardX, float forwardZ)
    {
        VECTOR listenerPos = new() { x = x, y = y, z = z };
        VECTOR listenerVel = new() { x = 0, y = 0, z = 0 };
        VECTOR forward = new() { x = forwardX, y = 0, z = forwardZ };
        VECTOR up = new() { x = 0, y = 1, z = 0 };

        system.set3DListenerAttributes(0, ref listenerPos, ref listenerVel, ref forward, ref up);
    }

    public void SetFrequencyGain(float gainLF, float gainMF, float gainHF)
    {
        system.createDSPByType(DSP_TYPE.THREE_EQ, out DSP eq);
        eq.setParameterFloat((int)DSP_THREE_EQ.LOWGAIN, gainLF);
        eq.setParameterFloat((int)DSP_THREE_EQ.MIDGAIN, gainLF);
        eq.setParameterFloat((int)DSP_THREE_EQ.HIGHGAIN, gainHF);
        channel.addDSP(0, eq);
    }

    // Call this every frame to update FMOD's internal state
    public void Update()
    {
        system.update();
    }

    public void Dispose()
    {
        sound.release();
        system.close();
        system.release();
    }
}