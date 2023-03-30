using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remember to add an Sound extension below with the same method name so it's more versatile
public class SoundWrapper
{
    public Sound sound;
    public Dictionary<string, float> parameters;
    public Transform root = null;
    public FMOD.Studio.EventInstance? fmodInstance = null;
    bool toFMODInstanceCalled = false;

    private SoundWrapper(Sound sound)
    {
        this.sound = sound;
        parameters = new Dictionary<string, float>();
    }

    public static implicit operator SoundWrapper(Sound sound) => new (sound);

    public SoundWrapper WithSpatials(Transform root)
    {
        this.root = root;
        return this;
    }

    public SoundWrapper WithVolume(float volume)
    {
        parameters.Add("volume", volume);
        return this;
    }

    public SoundWrapper WithPitch(float pitch)
    {
        parameters.Add("pitch", pitch);
        return this;
    }

    public SoundWrapper WithParameter(string name, float value)
    {
        parameters.Add(name, value);
        return this;
    }

    public FMOD.Studio.EventInstance? AndPlay() => toFMODInstanceCalled ? AudioManager.Play(this) : AudioManager.Play(ToFmodInstance());

    public SoundWrapper ToFmodInstance()
    {
        toFMODInstanceCalled = true;
        if (sound == null)
        {
            Debug.LogWarning("Cannot play null sound");
            return this;
        }
        var instOpt = sound.ToFmodInstance();
        if (instOpt == null || !instOpt.HasValue)
        {
            Debug.LogWarning($"Cannot instantiate sound { sound.name }");
            return this;
        }

        fmodInstance = instOpt.Value;
        return this;
    }
}

public static class SoundExtension
{
    public static SoundWrapper WithSpatials(this Sound sound, Transform root) => ((SoundWrapper)sound).WithSpatials(root);
    public static SoundWrapper WithVolume(this Sound sound, float volume) => ((SoundWrapper) sound).WithVolume(volume);
    public static SoundWrapper WithPitch(this Sound sound, float pitch) => ((SoundWrapper)sound).WithPitch(pitch);
    public static SoundWrapper WithParameter(this Sound sound, string name, float value) => ((SoundWrapper)sound).WithParameter(name, value);
    public static FMOD.Studio.EventInstance? AndPlay(this Sound sound) => ((SoundWrapper) sound).ToFmodInstance().AndPlay();

    public static FMOD.Studio.EventInstance? ToFmodInstance(this Sound sound)
    {
        var inst = FMODUnity.RuntimeManager.CreateInstance(sound.fmodEvent);
        if (inst.isValid())
        {
            return inst;
        }
        else
        {
            return null;
        }
    }
}