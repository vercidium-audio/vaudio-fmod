using System;

namespace vaudio_fmod;

internal class Scene
{
    FMODExample fmod;
    vaudio.RaytracingContext context;
    vaudio.Emitter listener;
    vaudio.Emitter speech;

    internal Scene()
    {
        InitialiseVAudio();
        InitialiseFMOD();
    }

    internal void InitialiseVAudio()
    {
        context = new()
        {
            RenderingEnabled = true,
            WorldSize = new(20),
        };

        listener = new()
        {
            Name = "Listener",
            OcclusionRayCount = 128,
            OcclusionBounceCount = 8,
            PermeationRayCount = 32,
            PermeationBounceCount = 3,
            Position = new vaudio.Vector3F(5),
        };

        context.AddEmitter(listener);

        speech = new()
        {
            Name = "Speech",
            Position = new vaudio.Vector3F(10),
            OnRaytracedByAnotherEmitter = OnSpeechRaytraced
        };

        context.AddEmitter(speech);
        listener.AddTarget(speech);
    }

    internal void InitialiseFMOD()
    {
        fmod = new FMODExample();

        fmod.Initialize();
        fmod.LoadSound("resource/audio/speech.wav");
    }

    internal void OnSpeechRaytraced(vaudio.Emitter other)
    {
        UpdateLowPassFilter();
        fmod.PlayAt(speech.Position.GetPosition());
    }

    internal void Update()
    {
        fmod.SetListenerPosition(listener.Position.GetPosition(), 0, 0);

        if (listener.HasRaytracedTarget(speech))
        {
            UpdateLowPassFilter();
        }

        context.Update();
        fmod.Update();
    }

    void UpdateLowPassFilter()
    {
        var filter = listener.GetTargetFilter(speech);

        // gainLF and gainHF are both 0.99f
        var lfDecibels = PercentToDecibels(filter.gainLF);
        var mfDecibels = PercentToDecibels((filter.gainLF + filter.gainHF) / 2);
        var hfDecibels = PercentToDecibels(filter.gainHF);

        // BUG - this slowly gets deeper and deeper
        fmod.SetFrequencyGain(lfDecibels, mfDecibels, hfDecibels);
    }

    public static float PercentToDecibels(float percent)
    {
        if (percent <= 0f)
            return -80f;

        float db = 20f * MathF.Log10(percent);

        return MathF.Max(db, -80f);
    }
}
