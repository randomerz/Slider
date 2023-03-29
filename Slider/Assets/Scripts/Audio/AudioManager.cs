using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FMODUnity;

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

    private static Dictionary<string, Sound> soundsDict
    {
        get
        {
            if (_instance == null) return null;
            if (_instance._soundsDict == null)
            {
                _instance._soundsDict = new Dictionary<string, Sound>(_instance.sounds.Length);
                foreach (Sound s in _instance.sounds) _instance._soundsDict.Add(s.name, s);
            }
            return _instance._soundsDict;
        }
    }
    private Dictionary<string, Sound> _soundsDict;

    static List<(FMOD.Studio.EventInstance, ManagedAttributes)> managedInstances;

    private static GameObject cachedCMBrainKey;
    private static CinemachineBrain currentCMBrain
    {
        get
        {
            if (Camera.main.gameObject != cachedCMBrainKey || cachedCMBrain == null)
            {
                // cache invalid
                cachedCMBrainKey = Camera.main.gameObject;
                cachedCMBrain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
            }
            return cachedCMBrain;
        }
    }
    private static CinemachineBrain cachedCMBrain;

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
            m.emitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
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

        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
    }

    private void FixedUpdate()
    {
        UpdateManagedInstances();
    }
    
    private void Start() {
        // StartCoroutine(testvolume());
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

    public static void PlayFmodOneshotWithSpatials(FMODUnity.EventReference name, Vector3 worldPosition)
    {
        FMODUnity.RuntimeManager.PlayOneShot(name, GetAudioPosition(worldPosition));
    }

    public static FMOD.Studio.EventInstance? PlayFmodWithSpatials(string name, Transform t)
    {
        if (soundsDict == null) return null;
        if (soundsDict.ContainsKey(name))
        {
            Sound s = soundsDict[name];
            return PlayFmodWithSpatials(s.fmodEvent, t, s.dopplerScale);
        } else
        {
            return null;
        }
    }

    public static FMOD.Studio.EventInstance? PlayFmodWithSpatials(FMODUnity.EventReference name, Transform t,  int dopplerScale = 0)
    {
        var inst = FMODUnity.RuntimeManager.CreateInstance(name);
        if (inst.isValid())
        {
            if (managedInstances == null) managedInstances = new List<(FMOD.Studio.EventInstance, ManagedAttributes)>(10);
            var attributes = new ManagedAttributes(t, dopplerScale);
            inst.set3DAttributes(attributes.GetAndUpdate());
            inst.start();
            managedInstances.Add((inst, attributes));
            
            //if (
            //    inst.getDescription(out FMOD.Studio.EventDescription desc) == FMOD.RESULT.OK
            //    && desc.isDopplerEnabled(out bool doppler) == FMOD.RESULT.OK 
            //    && doppler)
            //{
            //    Debug.Log("Playing doppler enabled event");
            //}
            return inst;
        } else
        {
            return null;
        }
    }

    private static void UpdateManagedInstances()
    {
        if (managedInstances == null) managedInstances = new List<(FMOD.Studio.EventInstance, ManagedAttributes)>(10);
        managedInstances.RemoveAll(delegate ((FMOD.Studio.EventInstance inst, ManagedAttributes attributes) pair)
        {
            if (pair.inst.isValid())
            {
                // instance not already relased
                if (
                    pair.inst.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playback) == FMOD.RESULT.OK 
                    && playback != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    // instance still playing (including the "stopping" state for events that allow fadeout)
                    pair.inst.set3DAttributes(pair.attributes.GetAndUpdate());
                    return false;
                }
                else
                {
                    // instance stopped playing
                    pair.inst.release();
                    return true;
                }
            }
            else
            {
                // instance already released
                return true;
            }
        });
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
            var overrides = SGrid.GetAudioModifierOverrides();
            if (overrides == null)
            {
                EnqueueModifier(modifiers[m]);
            }
            else 
            {
                EnqueueModifier(overrides.GetOverride(modifiers[m]));
            }
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
                if (FMODUnity.RuntimeManager.StudioSystem.getParameterByName(name, out float prev) == FMOD.RESULT.OK)
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
            var overrides = SGrid.GetAudioModifierOverrides();
            if (overrides == null)
            {
                DequeueModifier(modifiers[m]);
            }
            else
            {
                DequeueModifier(overrides.GetOverride(modifiers[m]));
            }
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
        if (FMODUnity.RuntimeManager.StudioSystem.setParameterByName(name, val) == FMOD.RESULT.OK)
        {
            // successfully set parameter
        }
        else
        {
            Debug.LogWarning($"Failed to set global parameter {name} = {val}");
        }
    }

    private static Vector3 GetAudioPosition(Vector3 worldPosition)
    {
        // fmod listener always on main camera object so positions are always evaluated against main camera transform
        // however, cinemachine keeps main camera position stable (by only updating at LateUpdate with +100 execution order)
        // - script position may be different from actual position seen in inspector, if you think main camera "does" move
        // this evaluates the main camera relative to the active vcam and adds the offset
        var cmBrain = currentCMBrain;
        var vCamToListener = cmBrain.transform.position - cmBrain.ActiveVirtualCamera.State.FinalPosition;
        return worldPosition + vCamToListener;
    }

    private class ManagedAttributes
    {
        private readonly Transform transform;
        float time;
        private readonly int dopplerScale;
        private readonly bool useDoppler;
        Vector3 position;

        public ManagedAttributes(Transform transform, int dopplerScale)
        {
            this.transform = transform;
            position = transform.position;
            time = Time.time;
            useDoppler = dopplerScale == 0;
        }

        public FMOD.ATTRIBUTES_3D GetAndUpdate()
        {
            if (!useDoppler) return transform.position.To3DAttributes();

            Vector3 p = transform.position;
            float dt = Time.time - time;
            Vector3 v = dt > float.Epsilon ? (p - position) / (dt) : Vector3.zero;

            position = p;
            time = Time.time;

            Debug.Log(v * dopplerScale);

            return new FMOD.ATTRIBUTES_3D
            {
                forward = transform.forward.ToFMODVector(),
                up = transform.up.ToFMODVector(),
                position = GetAudioPosition(p).ToFMODVector(),
                velocity = (v * dopplerScale).ToFMODVector()
            };
        }
    }
}