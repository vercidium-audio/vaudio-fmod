using FMOD;
using System;

namespace vaudio_fmod;

public class FMODSystem
{
    private FMOD.System system;
    private Sound sound;

    public FMODSystem()
    {
        Factory.System_Create(out system);
        system.init(512, INITFLAGS.NORMAL, IntPtr.Zero);

        // Enable 3D sound
        system.set3DSettings(1.0f, 1.0f, 1.0f);
    }

    public void LoadSoundData(string filePath)
    {
        // FMOD_3D flag makes the sound positional
        system.createSound(filePath, MODE._3D | MODE.LOOP_NORMAL, out sound);
        sound.set3DMinMaxDistance(1.0f, 1000.0f);
    }

    public FMODSound CreateSound(vaudio.Vector3F position)
    {
        // Start paused so we can set its position and filter before playback
        system.getMasterChannelGroup(out ChannelGroup masterGroup);
        system.playSound(sound, masterGroup, true, out Channel channel);

        // Set initial position
        VECTOR pos = new() { x = position.X, y = position.Y, z = position.Z };
        VECTOR vel = new() { x = 0, y = 0, z = 0 };
        channel.set3DAttributes(ref pos, ref vel);

        // Create a low pass filter
        system.createDSPByType(DSP_TYPE.THREE_EQ, out var dsp);
        channel.addDSP(0, dsp);

        return new FMODSound(channel, dsp);
    }

    public void SetListenerPosition(vaudio.Vector3F position, float pitch, float yaw)
    {
        float cosPitch = MathF.Cos(pitch);
        float sinPitch = MathF.Sin(pitch);
        float cosYaw = MathF.Cos(yaw);
        float sinYaw = MathF.Sin(yaw);

        // Forward vector from pitch and yaw
        VECTOR forward = new()
        {
            x = cosPitch * sinYaw,
            y = sinPitch,
            z = cosPitch * cosYaw
        };

        // Up vector must be perpendicular to forward
        VECTOR up = new()
        {
            x = -sinPitch * sinYaw,
            y = cosPitch,
            z = -sinPitch * cosYaw
        };

        VECTOR pos = new() { x = position.X, y = position.Y, z = position.Z };
        VECTOR vel = new() { x = 0, y = 0, z = 0 };

        system.set3DListenerAttributes(0, ref pos, ref vel, ref forward, ref up);
    }

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
