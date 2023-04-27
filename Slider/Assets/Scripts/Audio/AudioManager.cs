using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FMODUnity;
using FMOD.Studio;

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

    [SerializeField]
    private FMODUnity.StudioListener listener;
    [SerializeField]
    private Transform listenerWorldPosition;

    [SerializeField, Range(0, 20), Tooltip("Z-direction distance from top-down listener to game's plane")]
    private float ZLevel;

    private static bool paused;

    [SerializeField]
    private bool IndoorIsolatedFromWorld = false;
    private static bool listenerIsIndoor;

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

    static List<ManagedInstance> managedInstances;

    static CinemachineBrain currentCinemachineBrain;

    void Awake()
    {
        UIManager.OnResume += delegate (object sender, EventArgs e)
        {
            SetPaused(false);
        };

        UIManager.OnPause += delegate (object sender, EventArgs e)
        {
            SetPaused(true);
        };

        if (InitializeSingleton(ifInstanceAlreadySetThenDestroy:gameObject))
        {
            return;
        }
        DontDestroyOnLoad(gameObject);

        _sounds = sounds;
        _music = music;
        _soundDampenCurve = soundDampenCurve;

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

    private void Update()
    {
        UpdateCameraPosition();
        UpdateManagedInstances(Time.deltaTime);
    }

    public static void UpdateCamera(CinemachineBrain brain) => currentCinemachineBrain = brain;
    public void UpdateCameraPosition()
    {
        var cam = currentCinemachineBrain != null ? currentCinemachineBrain.ActiveVirtualCamera : null;
        if (cam == null) return;

        // When camera lerps to target, lock to the target instead of the camera
        var priority = cam.LookAt == null ? cam.Follow : cam.LookAt;
        Vector3 temp = priority == null ? cam.State.FinalPosition : priority.position;
        temp.z = ZLevel;
        listener.transform.position = temp;
        temp.z = 0;
        listenerWorldPosition.position = temp;
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

    public delegate void EventInstanceTick(ref EventInstance item);

    public static ManagedInstance Play(SoundWrapper soundWrapper)
    {
        if (!soundWrapper.valid) return null;
        var inst = soundWrapper.fmodInstance;

        if (soundWrapper.useSpatials)
        {
            managedInstances ??= new List<ManagedInstance>(10);
            var attributes = new ManagedInstance(
                inst,
                soundWrapper.root == null ? _instance.listenerWorldPosition.transform : soundWrapper.root, 
                soundWrapper.useDoppler, 
                soundWrapper.sound.dopplerScale,
                soundWrapper.duration);
            inst.start();
            managedInstances.Add(attributes);
            return attributes;
        }
        inst.start();
        inst.release();
        return null;
    }

    public static SoundWrapper PickSound(string name)
    {
        if (soundsDict.ContainsKey(name))
        {
            Sound s = soundsDict[name];
            if (s != null)
            {
                return s;
            }
            else
            {
                Debug.LogError($"Failed to spawn instance for sound {name}");
                return (SoundWrapper) (null as Sound);
            }

        }
        else
        {
            Debug.LogError("Sound: " + name + " not found!");
            return (SoundWrapper)(null as Sound);
        }
    }

    public static ManagedInstance Play(string name, Transform root) => PickSound(name).WithAttachmentToTransform(root).AndPlay();

    public static ManagedInstance Play(string name) => PickSound(name).AndPlay();

    public static void SetPaused(bool paused)
    {
        AudioManager.paused = paused;
        foreach (ManagedInstance attributes in managedInstances)
        {
            attributes.SetPaused(paused);
        }
    }

    private static void UpdateManagedInstances(float dt)
    {
        managedInstances ??= new List<ManagedInstance>(10);
        if (!paused)
        {
            foreach (ManagedInstance attributes in managedInstances)
            {
                attributes.GetAndUpdate(dt);
            }
        }
        managedInstances.RemoveAll(delegate (ManagedInstance attributes)
        {
            return !attributes.Valid || attributes.Stopped;
        });
    }

    /// <summary>
    /// Plays a sound with pitch
    /// </summary>
    /// <param name="name">String of the sound in Audio Manager</param>
    /// <param name="pitch">Pitch to play between 0.5 and 2.0</param>
    /// <returns>Reference to managed sfx instance in the audio manager, or null if it immediately stopped / does not need to be managed</returns>
    public static ManagedInstance PlayWithPitch(string name, float pitch) => PickSound(name).WithPitch(pitch).AndPlay();

    public static ManagedInstance PlayWithVolume(string name, float volumeMultiplier) => PickSound(name).WithVolume(volumeMultiplier).AndPlay();

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

    // Requires instance to stop, need discussion...
    //public static void StopSound(string name)
    //{
    //    if (_sounds == null)
    //        return;
    //    Sound s = Array.Find(_sounds, sound => sound.name == name);

    //    if (s == null)
    //    {
    //        Debug.LogError("Sound: " + name + " not found!");
    //        return;
    //    }

    //    s.emitter.Stop();
    //}

    public static void StopAllSoundAndMusic()
    {
        musicBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        sfxBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public static void SetGlobalParameter(string name, float val)
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

    public static void SetListenerIsIndoor(bool isInHouse)
    {
        listenerIsIndoor = isInHouse;
        if (isInHouse)
        {
            if (_instance.IndoorIsolatedFromWorld)
            {
                // cut off world sounds
                managedInstances?.RemoveAll(delegate (ManagedInstance managedInstance)
                {
                    if (!managedInstance.IsIndoor)
                    {
                        managedInstance.Stop();
                        return true;
                    }
                    return false;
                });
            }
            EnqueueModifier(AudioModifier.ModifierType.IndoorMusic3Eq);
        }
        else
        {
            if (_instance.IndoorIsolatedFromWorld)
            {
                // cut off indoor sounds
                managedInstances?.RemoveAll(delegate (ManagedInstance managedInstance)
                {
                    if (managedInstance.IsIndoor)
                    {
                        managedInstance.Stop();
                        return true;
                    }
                    return false;
                });
            }
            DequeueModifier(AudioModifier.ModifierType.IndoorMusic3Eq);
        }
    }

    public class ManagedInstance
    {
        private EventInstance inst;
        private readonly Transform transform;
        private float progress;
        private readonly float duration;
        private readonly float dopplerScale;
        private readonly bool useDoppler;
        private Vector3 position;

        public bool Valid => inst.isValid();
        public bool Stopped
            => inst.getPlaybackState(out PLAYBACK_STATE playback) == FMOD.RESULT.OK && playback != PLAYBACK_STATE.STOPPED;
            
        public bool IsIndoor => position.y > -75;

        public ManagedInstance(EventInstance inst, Transform transform, bool useDoppler, float dopplerScale, float duration)
        {
            this.inst = inst;
            this.transform = transform;
            position = transform.position;
            progress = 0;
            this.duration = duration;
            this.useDoppler = useDoppler;
            this.dopplerScale = dopplerScale;
            inst.set3DAttributes(CalculatePositionIncorporateIndoor(position).To3DAttributes());
        }

        public void Stop()
        {
            inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            inst.release();
        }

        public void SetPaused(bool paused)
        {
            inst.setPaused(paused);
        }

        public void Tick(EventInstanceTick tick)
        {
            tick(ref inst);
        }

        public void GetAndUpdate(float dt)
        {
            progress += dt;
            if (progress >= duration)
            {
                Stop();
                return;
            }
            if (!useDoppler)
            {
                Vector3 shiftedPosition = CalculatePositionIncorporateIndoor(transform.position);
                inst.set3DAttributes(shiftedPosition.To3DAttributes());
            }
            else
            {
                Vector3 p = transform.position;
                Vector3 v = dt > float.Epsilon ? (p - position) / (dt) : Vector3.zero;

                position = p;
                inst.set3DAttributes(new FMOD.ATTRIBUTES_3D
                {
                    forward = Vector3.forward.ToFMODVector(),
                    up = Vector3.up.ToFMODVector(),
                    position = CalculatePositionIncorporateIndoor(p).ToFMODVector(),
                    velocity = (v * dopplerScale).ToFMODVector()
                });
            }

            Debug.Log($"{inst} set attributes");
        }

        private Vector3 CalculatePositionIncorporateIndoor(Vector3 original)
        {
            if (IsIndoor == listenerIsIndoor) return original;
            if (listenerIsIndoor)
            {
                // shift down to meet the listener
                return original + SGrid.GetHousingOffset() * Vector3.down;
            } else
            {
                // shift up to meet the listener
                return original + SGrid.GetHousingOffset() * Vector3.up;
            }
        }
    }
}
