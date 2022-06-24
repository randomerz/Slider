using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;

    [SerializeField]
    private Music[] music;
    private static Music[] _music;

    private static float sfxVolume = 0.5f; // [0..1]
    private static float musicVolume = 0.5f;

    private static FMOD.Studio.Bus sfxBus;
    private static FMOD.Studio.Bus musicBus;

    //[SerializeField]
    //private GameObject audioListenerObj;
    //private static AudioLowPassFilter menuLowPass;

    public static AudioManager instance;

    private static string currentMusic;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        _sounds = sounds;
        _music = music;

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

    private void Start()
    {

    }

    private static Music GetMusic(string name)
    {
        if (_music == null)
            return null;
        Music m = Array.Find(_music, music => music.name == name);

        if (m == null)
        {
            Debug.LogError("Music: " + name + " not found!");
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


    public static void PlayWithVolume(string name, float volume) //Used in village 8 puzzle
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

        s.source.volume = s.volume * volume;
        s.source.Play();
    }

    public static void Stop(string name)
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

    public static void PlayMusic(string name, bool stopOtherTracks=true)
    {
        Music m = GetMusic(name);
        
        if (m == null)
            return;

        if (stopOtherTracks)
        {
            foreach (Music music in _music)
            {
                music.emitter.Stop(); //FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
        value = Mathf.Clamp(value, 0, 1);
        musicVolume = value;

        if (_music == null)
            return;
        foreach (Music m in _music)
        {
            if (m == null || m.emitter == null)
                continue;
            // m.emitter. = m.volume * value;
        }

        // if (value == 0)
        // {
        //     foreach (Music music in _music)
        //     {
        //         music.emitter.Stop(); //FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //     }
        // }
        
        musicBus.setVolume(value);
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
        //volume = value;

        if (_sounds == null)
            return;
        foreach (Sound s in _sounds)
        {
            if (s == null || s.source == null)
                continue;
            s.source.pitch = s.pitch * value;
        }
    }

    //public static void SetLowPassEnabled(bool value)
    //{
    //    menuLowPass.enabled = value;
    //}
}