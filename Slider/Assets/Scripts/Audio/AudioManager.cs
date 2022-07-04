using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;

    [SerializeField]
    private Music[] music;
    private static Music[] _music;

    [SerializeField]
    private AnimationCurve soundDampenCurve;
    private static AnimationCurve _soundDampenCurve;
    private static Coroutine soundDampenCoroutine;

    private static float sfxVolume = 0.5f; // [0..1]
    private static float musicVolume = 0.5f;
    private static float musicVolumeMultiplier = 1; // for music effects

    private static FMOD.Studio.Bus sfxBus;
    private static FMOD.Studio.Bus musicBus;

    public static AudioManager instance;

    void Awake()
    {
        if (InitializeSingleton(ifInstanceAlreadySetThenDestroy:gameObject))
        {
            return;
        }
        DontDestroyOnLoad(gameObject);

        _sounds = sounds;
        _music = music;
        _soundDampenCurve = soundDampenCurve;

        foreach (Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        foreach (Music m in _music)
        {
            m.emitter = gameObject.AddComponent<StudioEventEmitter>();
            m.emitter.EventReference = m.fmodEvent;
        }

        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
    }

    private static Music GetMusic(string name)
    {
        if (_music == null)
            return null;
        Music m = Array.Find(_music, music => music.name == name);

        if (m == null)
        {
            return null;
        }

        return m;
    }

    public static void Play(string name)
    {
        if (_sounds == null)
            return;
        Sound s = Array.Find(_sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        if (s.doRandomPitch)
            s.source.pitch = s.pitch * UnityEngine.Random.Range(.95f, 1.05f);
        else
            s.source.pitch = s.pitch;

        s.source.Play();
    }

    public static void PlayWithPitch(string name, float pitch) //Used In Ocean Scene
    {
        if (_sounds == null)
            return;
        Sound s = Array.Find(_sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        if (s.doRandomPitch)
            s.source.pitch = s.pitch * UnityEngine.Random.Range(.95f, 1.05f) * pitch;
        else
            s.source.pitch = s.pitch * pitch;

        s.source.Play();
    }


    public static void PlayWithVolume(string name, float volumeMultiplier)
    {
        if (_sounds == null)
            return;
        Sound s = Array.Find(_sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        if (s.doRandomPitch)
            s.source.pitch = s.pitch * UnityEngine.Random.Range(.95f, 1.05f);
        else
            s.source.pitch = s.pitch;

        s.source.volume = s.volume * sfxVolume * volumeMultiplier;
        s.source.Play();
    }

    public static void PlayMusic(string name, bool stopOtherTracks=true)
    {
        Music m = GetMusic(name);
        
        if (m == null)
            return;

        if (stopOtherTracks)
        {
            foreach (Music music in _music)
            {
                music.emitter.Stop();
            }
        }

        m.emitter.Play();
    }

    public static void StopMusic(string name)
    {
        Music m = GetMusic(name);
        
        if (m == null)
            return;

        m.emitter.Stop();
    }

    public static void StopSound(string name)
    {
        if (_sounds == null)
            return;
        Sound s = Array.Find(_sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
    }

    public static void StopAllSoundAndMusic()
    {
        foreach (Music m in _instance.music)
        {
            m.emitter.Stop();
        }
        foreach (Sound s in _instance.sounds)
        {
            s.source.Stop();
        }
    }

    public static void SetMusicParameter(string name, string parameterName, float value)
    {
        // for global parameters
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameterName, value);

        Music m = GetMusic(name);
        
        if (m == null)
            return;

        // for track-specific parameters
        m.emitter.SetParameter(parameterName, value);
    }


    public static void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        sfxVolume = value;

        if (_sounds == null)
            return;
        foreach (Sound s in _sounds)
        {
            if (s == null || s.source == null)
                continue;
            s.source.volume = s.volume * value;
        }

        sfxBus.setVolume(value);
    }

    public static void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, 0, 1);
        UpdateMusicVolume();
    }

    public static void SetMusicVolumeMultiplier(float value)
    {
        musicVolumeMultiplier = value;
        UpdateMusicVolume();
    }

    private static void UpdateMusicVolume()
    {
        float vol = Mathf.Clamp(musicVolume * musicVolumeMultiplier, 0, 1);

        if (_music == null)
            return;

        musicBus.setVolume(vol);
    }

    public static void DampenMusic(float amount, float length)
    {
        StopDampen();
        soundDampenCoroutine = _instance.StartCoroutine(_DampenMusic(amount, length));
    }

    public static void StopDampen()
    {
        if (soundDampenCoroutine != null)
        {
            _instance.StopCoroutine(soundDampenCoroutine);
            soundDampenCoroutine = null;
        }
    }

    private static IEnumerator _DampenMusic(float amount, float length)
    {
        float t = 0;

        while (t < length)
        {
            SetMusicVolumeMultiplier(Mathf.Lerp(1, amount, _soundDampenCurve.Evaluate(t / length)));

            yield return null;
            t += Time.deltaTime;
        }

        SetMusicVolumeMultiplier(1);
        soundDampenCoroutine = null;
    }

    public static float GetSFXVolume()
    {
        return sfxVolume;
    }

    public static float GetMusicVolume()
    {
        return musicVolume;
    }

    public static void SetSFXPitch(float value)
    {
        value = Mathf.Clamp(value, 0.3f, 3f);

        if (_sounds == null)
            return;
        foreach (Sound s in _sounds)
        {
            if (s == null || s.source == null)
                continue;
            s.source.pitch = s.pitch * value;
        }
    }
}