using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;

    private static float volume = 1; // [0..1]

    [SerializeField]
    private GameObject audioListenerObj;
    private static AudioLowPassFilter menuLowPass;

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

        foreach (Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        menuLowPass = audioListenerObj.GetComponent<AudioLowPassFilter>();
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

        if (s.doRandomPitch)
            s.source.pitch = s.pitch * UnityEngine.Random.Range(.95f, 1.05f);

        s.source.Play();
    }

    public static void SetVolume(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        volume = value;

        if (_sounds == null)
            return;
        foreach (Sound s in _sounds)
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

    public static float GetVolume()
    {
        return volume;
    }

    public static void SetLowPassEnabled(bool value)
    {
        menuLowPass.enabled = value;
    }
}