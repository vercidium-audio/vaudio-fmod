using System;
using System.Diagnostics;

namespace vaudio_fmod;

internal class Scene
{
    FMODSystem fmod;
    FMODSound fmodSound;
    vaudio.RaytracingContext context;
    vaudio.Emitter listener;
    vaudio.Emitter speech;
    vaudio.PrismPrimitive prism;

    internal Scene()
    {
        InitialiseVAudio();
        InitialiseFMOD();
    }

    void InitialiseVAudio()
    {
        // Create a Vercidium Audio context
        context = new()
        {
            RenderingEnabled = true,
            WorldSize = new(20),
        };


        // Create a listener that casts occlusion and permeation rays
        listener = new()
        {
            Name = "Listener",
            PermeationRayCount = 32,
            PermeationBounceCount = 3,
            Position = new vaudio.Vector3F(5, 10, 10),
        };

        context.AddEmitter(listener);


        // Create a target emitter that will be discovered by the listener
        speech = new()
        {
            Name = "Speech",
            Position = new vaudio.Vector3F(10),
            OnRaytracedByAnotherEmitter = OnSpeechRaytraced
        };

        context.AddEmitter(speech);
        listener.AddTarget(speech);


        // Add a cloth prism to the simulation
        prism = new()
        {
            size = new(4),
            material = vaudio.MaterialType.Cloth
        };

        context.AddPrimitive(prism);


        // Reduce the transmission of the cloth material, so we can hear the sound when the prism moves on top of the speech Emitter 
        var cloth = context.GetMaterial(vaudio.MaterialType.Cloth);
        cloth.TransmissionLF = 2.5f;
        cloth.TransmissionHF = 5.0f;

        context.MaterialsDirty = true;
    }

    void InitialiseFMOD()
    {
        fmod = new FMODSystem();
        fmod.LoadSoundData("resource/audio/speech.wav");
        fmod.SetListenerPosition(listener.Position.GetPosition(), 0, 0);
    }

    // This callback is invoked when the listener raytraces the speech Emitter
    void OnSpeechRaytraced(vaudio.Emitter other)
    {
        // Create a sound and low-pass filter
        fmodSound = fmod.CreateSound(speech.Position.GetPosition());

        // Update the filter
        UpdateLowPassFilter();

        // Play the sound
        fmodSound.Play();
    }

    Stopwatch prismWatch = Stopwatch.StartNew();

    internal void Update()
    {
        fmod.SetListenerPosition(listener.Position.GetPosition(), 0, MathF.PI / 2);

        // Move the prism onto the speech Emitter to muffle it
        var lerp = (MathF.Sin(prismWatch.ElapsedMilliseconds / 2000.0f + 1.25f) + 1) / 2;

        prism.transform = vaudio.Matrix4F.CreateTranslation(Lerp(10.0f, 15.0f, lerp), 10, 10);

        listener.permeationColor = new vaudio.Color(255, 150, 0, 255);
        listener.trailColor = new vaudio.Color(255, 255, 255, 50);

        context.Update();

        // Update the low pass filter
        if (listener.HasRaytracedTarget(speech))
        {
            UpdateLowPassFilter();
        }

        fmod.Update();
    }

    void UpdateLowPassFilter()
    {
        if (fmodSound == null)
            return;

        var filter = listener.GetTargetFilter(speech);

        // Convert from percentage range (0 to 1) to decibel range (-80 to 10)
        var lfDecibels = PercentToDecibels(filter.gainLF);
        var mfDecibels = PercentToDecibels((filter.gainLF + filter.gainHF) / 2);
        var hfDecibels = PercentToDecibels(filter.gainHF);

        fmodSound.SetFrequencyGain(lfDecibels, mfDecibels, hfDecibels);
    }

    static float Lerp(float current, float target, float lerp) => current + (target - current) * lerp;

    static float PercentToDecibels(float percent)
    {
        if (percent <= 0f)
            return -80f;

        float db = 20f * MathF.Log10(percent);

        return MathF.Max(db, -80f);
    }
}
