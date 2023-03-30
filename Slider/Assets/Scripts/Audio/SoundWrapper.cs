using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remember to add an Sound extension below with the same method name so it's more versatile
public struct SoundWrapper
{
    public Sound sound;
    public Transform root;
    public FMOD.Studio.EventInstance fmodInstance;
    public bool valid;
    public bool useSpatials;
    public bool useDoppler;

    private SoundWrapper(Sound sound)
    {
        this.sound = sound;
        fmodInstance = default;
        valid = false;
        root = null;
        useSpatials = false;

        useSpatials = false;
        useDoppler = false;

        if (ToFmodInstance())
        {
            valid = true;
            var desc = FMODUnity.RuntimeManager.GetEventDescription(sound.fmodEvent);
            desc.is3D(out useSpatials);
            desc.isDopplerEnabled(out useDoppler);
        }
    }

    public static implicit operator SoundWrapper(Sound sound) => new (sound);

    public SoundWrapper WithSpatials(Transform root)
    {
        if (valid && useSpatials) this.root = root;
        else if (valid && !useSpatials) Debug.LogWarning($"Trying to set spatial information on non-spatial sound { sound.name }");
        return this;
    }

    public SoundWrapper WithVolume(float volume)
    {
        if (valid) fmodInstance.setParameterByName("volume", volume);
        return this;
    }

    public SoundWrapper WithPitch(float pitch)
    {
        if (valid) fmodInstance.setParameterByName("pitch", pitch);
        return this;
    }

    public SoundWrapper WithParameter(string name, float value)
    {
        if (valid) fmodInstance.setParameterByName(name, value);
        return this;
    }

    public FMOD.Studio.EventInstance? AndPlay() => valid ? AudioManager.Play(this) : null;

    private bool ToFmodInstance()
    {
        if (sound == null)
        {
            Debug.LogWarning("Cannot play null sound");
            return false;
        }
        var instOpt = sound.ToFmodInstance();
        if (instOpt == null || !instOpt.HasValue)
        {
            Debug.LogWarning($"Cannot instantiate sound { sound.name }");
            return false;
        }

        fmodInstance = instOpt.Value;
        return true;
    }
}

public static class SoundExtension
{
    public static SoundWrapper WithSpatials(this Sound sound, Transform root) => ((SoundWrapper)sound).WithSpatials(root);
    public static SoundWrapper WithVolume(this Sound sound, float volume) => ((SoundWrapper) sound).WithVolume(volume);
    public static SoundWrapper WithPitch(this Sound sound, float pitch) => ((SoundWrapper)sound).WithPitch(pitch);
    public static SoundWrapper WithParameter(this Sound sound, string name, float value) => ((SoundWrapper)sound).WithParameter(name, value);
    public static FMOD.Studio.EventInstance? AndPlay(this Sound sound) => ((SoundWrapper) sound).AndPlay();

    public static FMOD.Studio.EventInstance? ToFmodInstance(this Sound sound)
    {
        try
        {
            return FMODUnity.RuntimeManager.CreateInstance(sound.fmodEvent);
        }
        catch (FMODUnity.EventNotFoundException e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }
}