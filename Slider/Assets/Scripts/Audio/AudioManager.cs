using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;
    [SerializeField]
    private Sound[] music;
    private static Sound[] _music;

    private static float sfxVolume = 0.5f; // [0..1]
    private static float musicVolume = 0.5f;

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

        foreach (Sound s in _music)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        //menuLowPass = audioListenerObj.GetComponent<AudioLowPassFilter>();
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
    }

    private void Start()
    {

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

    public static void PlayMusic(string name)
    {
        if (_music == null)
            return;
        Sound s = Array.Find(_music, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        if (s.doRandomPitch)
            s.source.pitch = s.pitch * UnityEngine.Random.Range(.95f, 1.05f);

        // stop all other musics
        foreach (Sound m in _music)
        {
            m.source.Stop();
        }

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

    public static void StopMusic(string name)
    {
        if (_music == null)
            return;
        Sound s = Array.Find(_music, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
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
    }

    public static void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        musicVolume = value;

        if (_music == null)
            return;
        foreach (Sound s in _music)
        {
            if (s == null || s.source == null)
                continue;
            s.source.volume = s.volume * value;
        }
    }

    public static void SetPitch(float value)
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

    public static float GetSFXVolume()
    {
        return sfxVolume;
    }

    public static float GetMusicVolume()
    {
        return musicVolume;
    }

    //public static void SetLowPassEnabled(bool value)
    //{
    //    menuLowPass.enabled = value;
    //}
}