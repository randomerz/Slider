using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Cinemachine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;

    [SerializeField]
    private Music[] music;
    private static Music[] _music;

    [SerializeField] // for item pickup cutscene
    private AnimationCurve soundDampenCurve;
    private static AnimationCurve _soundDampenCurve;
    private static Coroutine soundDampenCoroutine;

    private static float sfxVolume = 0.5f; // [0..1]
    private static float musicVolume = 0.5f;
    private static float musicVolumeMultiplier = 1; // for music effects

    private static FMOD.Studio.Bus sfxBus;
    private static FMOD.Studio.Bus musicBus;

    [SerializeField]
    private List<AudioModifier> modifierPool;
    private static Dictionary<AudioModifier.ModifierType, AudioModifier> modifiers;

    // Last-in-first-evaluate queue for each global parameter
    private static Dictionary<string, List<AudioModifier.AudioModifierProperty>> parameterResponsibilityQueue;
    private static Dictionary<string, float> parameterDefaults;

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
            s.emitter = gameObject.AddComponent<StudioEventEmitter>();
            s.emitter.EventReference = s.fmodEvent;
            // s.source = gameObject.AddComponent<AudioSource>();
            // s.source.clip = s.clip;

            // s.source.volume = s.volume;
            // s.source.pitch = s.pitch;
            // s.source.loop = s.loop;
        }

        foreach (Music m in _music)
        {
            m.emitter = gameObject.AddComponent<StudioEventEmitter>();
            m.emitter.EventReference = m.fmodEvent;
        }

        modifiers = new Dictionary<AudioModifier.ModifierType, AudioModifier>(modifierPool.Count);
        foreach (AudioModifier audioModifier in modifierPool)
        {
            if (modifiers.TryAdd(audioModifier.type, audioModifier))
            {
                // successfully added
            } else
            {
                Debug.LogError($"Duplicate modifier when trying to add {audioModifier.name}");
            }
        }
        parameterDefaults = new Dictionary<string, float>();
        parameterResponsibilityQueue = new Dictionary<string, List<AudioModifier.AudioModifierProperty>>();

        sfxBus = RuntimeManager.GetBus("bus:/Master/SFX");
        musicBus = RuntimeManager.GetBus("bus:/Master/Music");
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
    }

    private void Start() {
        StartCoroutine(testvolume());
    }

    private IEnumerator testvolume()
    {
        for (int i = 0; i < 10; i++)
        {
            float val = 2 - (0.2f * i);
            Debug.Log(val);
            PlayWithPitch("TFT Bell", val);
            // Play("TFT Bell");

            yield return new WaitForSeconds(1);
        }
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

        s.emitter.Play();
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

        s.emitter.SetParameter("pitch", pitch);
        s.emitter.Play();
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

        // s.source.volume = s.volume * sfxVolume * volumeMultiplier;
        s.emitter.SetParameter("volume", volumeMultiplier);
        s.emitter.Play();
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

        s.emitter.Stop();
    }

    public static void StopAllSoundAndMusic()
    {
        foreach (Music m in _instance.music)
        {
            m.emitter.Stop();
        }
        foreach (Sound s in _instance.sounds)
        {
            s.emitter.Stop();
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
        // foreach (Sound s in _sounds)
        // {
        //     if (s == null || s.emitter == null)
        //         continue;
        //     s.emitter.volume = s.volume * value;
        // }

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
            SetMusicVolumeMultiplier(Mathf.Lerp(amount, 1, _soundDampenCurve.Evaluate(t / length)));

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

    public static void EnqueueModifier(AudioModifier.ModifierType m)
    {
        if (modifiers.ContainsKey(m))
        {
            EnqueueModifier(modifiers[m]);
        }
        else
        {
            Debug.LogWarning($"Trying to access non-materialized modifier {m}");
        }
    }

    private static void EnqueueModifier(AudioModifier m)
    {
        foreach (var adj in m.adjustments)
        {
            string name = adj.parameter;
            float val = adj.value;

            if (parameterResponsibilityQueue.ContainsKey(name))
                parameterResponsibilityQueue[name].Add(adj);
            else {
                parameterResponsibilityQueue.Add(name, new List<AudioModifier.AudioModifierProperty> { adj });
                if (RuntimeManager.StudioSystem.getParameterByName(name, out float prev) == FMOD.RESULT.OK)
                {
                    parameterDefaults.TryAdd(name, prev);
                }
                else
                {
                    Debug.LogWarning($"Parameter {prev} is modified but does not actually exist");
                }
            }

            SetGlobalParameter(name, val);
        }
    }

    public static void DequeueModifier(AudioModifier.ModifierType m)
    {
        if (modifiers.ContainsKey(m))
        {
            DequeueModifier(modifiers[m]);
        }
        else
        {
            Debug.LogWarning($"Trying to access non-materialized modifier {m}");
        }
    }

    private static void DequeueModifier(AudioModifier m)
    {
        foreach (var adj in m.adjustments)
        {
            string name = adj.parameter;

            if (parameterResponsibilityQueue.ContainsKey(name))
            {
                if (!parameterResponsibilityQueue[name].Remove(adj))
                {
                    Debug.LogWarning($"Modifier {m.name} is not actually in effect (parameter {name} has modifiers attached, but not {m.name})");
                    continue;
                }
                if (parameterResponsibilityQueue[name].Count == 0)
                {
                    // no modifiers left, restore parameter default
                    SetGlobalParameter(name, parameterDefaults[name]);
                }
                else
                {
                    // force evaluate the last effect in this queue
                    SetGlobalParameter(name, parameterResponsibilityQueue[name][^1].value);
                }
            }
            else
            {
                Debug.LogWarning($"Modifier {m.name} is not actually in effect (property {name} does not have any modifiers attached)");
            }
        }
    }

    private static void SetGlobalParameter(string name, float val)
    {
        if (RuntimeManager.StudioSystem.setParameterByName(name, val) == FMOD.RESULT.OK)
        {
            // successfully set parameter
        }
        else
        {
            Debug.LogWarning($"Failed to set global parameter {name} = {val}");
        }
    }
}