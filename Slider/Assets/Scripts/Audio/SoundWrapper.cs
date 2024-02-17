using SliderVocalization;
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
    public float duration;
    public float volume;
    public bool isPriority;
    public VocalizableParagraph dialogueParent;

    public enum IndoorStatus
    {
        AlwaysIndoor,
        AlwaysOutdoor,
        UseEmitterLocation
    }
    public IndoorStatus indoorStatus;

    private SoundWrapper(Sound sound)
    {
        this.sound = sound;
        fmodInstance = default;
        valid = false;
        root = null;
        useSpatials = false;
        useDoppler = false;

        duration = float.MaxValue;
        indoorStatus = IndoorStatus.UseEmitterLocation;

        volume = 1;

        isPriority = false;

        dialogueParent = null;

        if (ToFmodInstance())
        {
            valid = true;
            var desc = FMODUnity.RuntimeManager.GetEventDescription(sound.fmodEvent);
            desc.is3D(out useSpatials);
            desc.isDopplerEnabled(out useDoppler);
        }
    }

    public bool IsActuallyIndoor()
    {
        switch (indoorStatus)
        {
            case IndoorStatus.AlwaysIndoor: return true;
            case IndoorStatus.AlwaysOutdoor: return false;
            case IndoorStatus.UseEmitterLocation: return (root.position.y <= -75);
        }
        return false;
    }

    public static implicit operator SoundWrapper(Sound sound) => new (sound);

    public SoundWrapper WithAttachmentToTransform(Transform root)
    {
        if (valid && useSpatials) this.root = root;
        else if (valid && !useSpatials) Debug.LogWarning($"Trying to set spatial information on non-spatial sound { sound.name }");
        return this;
    }

    public SoundWrapper WithVolume(float volume)
    {
        this.volume = volume;
        return this;
    }

    public SoundWrapper WithPitch(float pitch)
    {
        if (valid) fmodInstance.setParameterByName("Pitch", pitch);
        return this;
    }

    public SoundWrapper WithParameter(string name, float value)
    {
        if (valid) fmodInstance.setParameterByName(name, value);
        return this;
    }

    public SoundWrapper WithFixedDuration(float value)
    {
        if (value >= 0f) duration = value;
        else Debug.LogWarning($"Setting sfx {sound?.name ?? "(no name)"} duration to negative");
        return this;
    }

    public SoundWrapper WithIndoorStatus(IndoorStatus status)
    {
        indoorStatus = status;
        return this;
    }
    public SoundWrapper WithPriorityOverDucking(bool priorityOverDucking)
    {
        isPriority = priorityOverDucking;
        return this;
    }

    public AudioManager.ManagedInstance AndPlay() => valid ? AudioManager.Play(ref this) : null;

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

    public void PlayAsOneshot()
    {
        if (!valid)
        {
            Debug.LogWarning($"Trying to play invalid { sound?.name } as oneshot");
        }
        fmodInstance.start();
        fmodInstance.setVolume(volume);
        fmodInstance.release();
    }
}

public static class SoundExtension
{
    public static SoundWrapper WithAttachmentToTransform(this Sound sound, Transform root) => ((SoundWrapper)sound).WithAttachmentToTransform(root);
    public static SoundWrapper WithVolume(this Sound sound, float volume) => ((SoundWrapper) sound).WithVolume(volume);
    public static SoundWrapper WithPitch(this Sound sound, float pitch) => ((SoundWrapper)sound).WithPitch(pitch);
    public static SoundWrapper WithParameter(this Sound sound, string name, float value) => ((SoundWrapper)sound).WithParameter(name, value);
    public static SoundWrapper WithFixedDuration(this Sound sound, float value) => ((SoundWrapper)sound).WithFixedDuration(value);
    public static SoundWrapper WithIndoorStatus(this Sound sound, SoundWrapper.IndoorStatus indoorStatus) => ((SoundWrapper) sound).WithIndoorStatus(indoorStatus);
    public static SoundWrapper WithPriorityOverDucking(this Sound sound, bool priorityOverDucking) => ((SoundWrapper)sound).WithPriorityOverDucking(priorityOverDucking);
    public static AudioManager.ManagedInstance AndPlay(this Sound sound) => ((SoundWrapper) sound).AndPlay();

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

    public static FMOD.Studio.EventDescription? ToFmodEventDescription(this Sound sound)
    {
        try
        {
            return FMODUnity.RuntimeManager.GetEventDescription(sound.fmodEvent);
        }
        catch (FMODUnity.EventNotFoundException e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }
}