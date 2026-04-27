using FMOD;
using System;

namespace vaudio_fmod;

public class FMODSystem
{
    private FMOD.System system;
    private Sound sound;
    private DSP reverbDSP;

    public FMODSystem()
    {
        Factory.System_Create(out system);
        system.init(512, INITFLAGS.NORMAL, IntPtr.Zero);
        system.set3DSettings(1.0f, 1.0f, 1.0f);
        system.createDSPByType(DSP_TYPE.SFXREVERB, out reverbDSP);

        system.getMasterChannelGroup(out ChannelGroup masterGroup);
        masterGroup.addDSP(0, reverbDSP);
    }

    public void UpdateReverb(vaudio.EAXReverbResults eax)
    {
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.DECAYTIME, eax.DecayTime * 1000); // ms
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.EARLYDELAY, eax.ReflectionsDelay * 1000); // ms
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.LATEDELAY, eax.LateReverbDelay * 1000); // ms
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.HFREFERENCE, eax.HFReference); // Hz
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.HFDECAYRATIO, Math.Clamp(eax.DecayHFRatio * 100f, 10f, 100f));
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.DIFFUSION, eax.Diffusion * 100f); // 0-1 → %
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.DENSITY, eax.Density * 100f); // 0-1 → %

        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.LOWSHELFFREQUENCY, eax.LFReference);
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.LOWSHELFGAIN, 20f * MathF.Log10(MathF.Max(eax.GainLF, 1e-6f)));
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.HIGHCUT, eax.HFReference * eax.AirAbsorptionGainHF);

        float totalGain = eax.ReflectionsGain + eax.LateReverbGain;
        float earlyLateMix = totalGain > 0f ? eax.ReflectionsGain / totalGain * 100f : 50f;
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.EARLYLATEMIX, Math.Clamp(earlyLateMix, 0f, 100f));

        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.WETLEVEL, 20f * MathF.Log10(MathF.Max((eax.GainLF + eax.GainHF) / 2, 1e-6f)));
        reverbDSP.setParameterFloat((int)DSP_SFXREVERB.DRYLEVEL, 0f);
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
        reverbDSP.release();
        system.close();
        system.release();
    }
}
