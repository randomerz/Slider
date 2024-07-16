using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Cinemachine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using SliderVocalization;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private Sound[] sounds;
    private static Sound[] _sounds;

    [SerializeField]
    private Music[] music;
    private static Music[] _music;

    [SerializeField]
    private Music[] ambience;
    private static Music[] _ambience;

    [SerializeField] // for item pickup cutscene
    private AnimationCurve soundDampenCurve;
    private static AnimationCurve _soundDampenCurve;
    private static Dictionary<object, (float amount, float length, float t)> soundDampenInstances;

    public static bool useVocalizer = true;

    private static float masterVolume = 0.5f; // [0..1]
    private static float sfxVolume = 0.5f;
    private static float ambienceVolume = 0.5f;
    private static float musicVolume = 0.5f;

    private static FMOD.Studio.Bus masterBus;
    private static FMOD.Studio.Bus sfxBus;
    private static FMOD.Studio.Bus ambienceBus;
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
    private static bool musicPaused;
    private static List<FMOD.Studio.EventInstance> pausedMusic;

    [SerializeField]
    private bool IndoorIsolatedFromWorld = false;
    private static bool listenerIsIndoor;
    [SerializeField, Range(0, 10)]
    private float indoorMuteDB;

    static HashSet<ManagedInstance> PrioritySounds = new ();
    [SerializeField, Range(0, 1), Tooltip("origional dB minus (dB over threshold * factor)")]
    private float duckingDbFactor;
    [SerializeField, Range(-20, 0)]
    private float duckingThreshold;
    [SerializeField, Range(0, 1), Tooltip("ducking factor specifically for dialogue over dialogue")]
    private float dialogueDuckingDbFactor;

    [SerializeField, Range(0.1f, 10), Tooltip("higher = more abrupt ducking, slower = smoother ducking")]
    private float duckingSmoothnessInverted;

    [SerializeField, Range(1, 5),
     Tooltip("for more speakers than this amount, speakers who started earlier will undergo ducking")]
    private int maxConcurrentSpeakers;

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

    static List<ManagedInstance> managedInstances = new(25);

    /// <summary>
    /// Singe instance sounds are identified through a key they provide, if a sound instance already exists with that
    /// key, then any new sounds providing the same key will not be played (until that previous sound has stopped)
    /// </summary>
    private static HashSet<string> currentSingleInstanceKeys = new();

    static CinemachineBrain currentCinemachineBrain;

    void Awake()
    {
        /*UIManager.OnPause += delegate (object sender, EventArgs e)
        {
            SetPaused(true);
        };
        UIManager.OnResume += delegate (object sender, EventArgs e)
        {
            SetPaused(false);
        };*/

        PauseManager.PauseStateChanged += (bool newPauseState) =>
        {
            SetPaused(newPauseState);
        };

        FactoryTimeGlitch.TimeGlitchPauseStateChanged += (bool newPauseState) =>
        {
            SetPaused(newPauseState);
            SetMusicAmbiencePaused(newPauseState);
        };

        DebugUIManager.OnOpenDebug += delegate (object sender, EventArgs e)
        {
            SetPaused(true);
        };

        DebugUIManager.OnCloseDebug += delegate (object sender, EventArgs e)
        {
            SetPaused(false);
        };

        if (InitializeSingleton(ifInstanceAlreadySetThenDestroy: gameObject))
        {
            return;
        }
        DontDestroyOnLoad(gameObject);

        _sounds = sounds;
        _music = music;
        _ambience = ambience;
        _soundDampenCurve = soundDampenCurve;

        foreach (Music m in _music)
        {
            m.emitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
            m.emitter.EventReference = m.fmodEvent;
        }

        foreach (Music a in _ambience)
        {
            a.emitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
            a.emitter.EventReference = a.fmodEvent;
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

        masterBus = FMODUnity.RuntimeManager.GetBus("bus:/Master");
        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        ambienceBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Ambience");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);

        soundDampenInstances = new ();
        PrioritySounds = new ();
    }

    private void FixedUpdate()
    {
        UpdateCameraPosition();
        UpdateManagedInstances(Time.fixedUnscaledDeltaTime);
        UpdateMusicVolume(Time.fixedUnscaledDeltaTime);
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
            Debug.LogError("Couldn't find music of name: " + name);
            return null;
        }

        return m;
    }

    private static Music GetAmbience(string name)
    {
        if (_ambience == null)
            return null;
        Music a = Array.Find(_ambience, ambience => ambience.name == name);

        if (a == null)
        {
            Debug.LogError("Couldn't find ambience of name: " + name);
            return null;
        }

        return a;
    }

    public delegate void EventInstanceTick(ref FMOD.Studio.EventInstance item);

    public static ManagedInstance Play(ref SoundWrapper soundWrapper)
    {
        if (!soundWrapper.valid) return null;

        if (soundWrapper.singleInstanceKey != null)
        {
            if (!currentSingleInstanceKeys.Add(soundWrapper.singleInstanceKey))
            {
                return null;
            }
            // Debug.Log($"+{soundWrapper.singleInstanceKey}");
        }

        if (soundWrapper.useSpatials)
        {
            bool isOverridingTransform = soundWrapper.root == null;
            soundWrapper.root = !isOverridingTransform ? soundWrapper.root : _instance.listenerWorldPosition;
            if (_instance.IndoorIsolatedFromWorld && soundWrapper.IsActuallyIndoor() != listenerIsIndoor)
            {
                // AT: this is just to mute the error, idk why FMOD gives a warning log even if the sound is not meant to be played...
                //     releasing the instance doesn't help either
                soundWrapper.fmodInstance.set3DAttributes(_instance.transform.To3DAttributes());
                return null;
            }
            ManagedInstance instance = new (soundWrapper, isOverridingTransform, Mathf.Pow(2, -_instance.indoorMuteDB));
            managedInstances.Add(instance);
            if (soundWrapper.isPriority)
            {
                PrioritySounds.Add(instance);
            }
            instance.SetPaused(paused);
            return instance;
        } else
        {
            soundWrapper.PlayAsOneshot(RemoveSingleInstanceKeyCallback);
            return null;
        }
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT RemoveSingleInstanceKeyCallback(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(_event);
        if (instance.getUserData(out var userData) == RESULT.OK)
        {
            if (userData != IntPtr.Zero)
            {
                GCHandle keyHandle = GCHandle.FromIntPtr(userData);
                string key = (string)keyHandle.Target;
                currentSingleInstanceKeys?.Remove(key);
            }
        }
        
        return RESULT.OK;
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
                return (SoundWrapper)(null as Sound);
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
        foreach (ManagedInstance instance in managedInstances)
        {
            instance.SetPaused(paused);
        }
    }

    private static void UpdateManagedInstances(float dt)
    {
        managedInstances.RemoveAll(delegate (ManagedInstance instance)
        {
            if (!instance.Nullified && instance.Valid && !instance.Stopped) return false;
            
            PrioritySounds.Remove(instance);
            
            // Debug.Log($"rm {instance.Name}");

            if (instance.SingleInstanceKey(out string k))
            {
                // Debug.Log($"-{k}");
                currentSingleInstanceKeys.Remove(k);
            }
            
            instance.HardStop();
            return true;
        });
        float maxPriorityVolume01 = 0;
        float maxDialogueVolume01 = 0;
        foreach (ManagedInstance instance in managedInstances)
        {
            if (!paused || !instance.CanPause)
            {
                if (!PrioritySounds.Contains(instance))
                {
                    continue;
                }
                float vol = instance.MixerVolume01;
                maxPriorityVolume01 = Mathf.Max(maxPriorityVolume01, vol);
                if (instance.IsDialogue)
                {
                    maxDialogueVolume01 = Mathf.Max(maxDialogueVolume01, vol);
                }
            }
        }
        float dbOverThreshold = Mathf.Approximately(0, maxPriorityVolume01) ?
            0 : Mathf.Max(0, 6 * Mathf.Log(maxPriorityVolume01, 2) - _instance.duckingThreshold);
        float dialogueDbOverThreshold = Mathf.Approximately(0, maxDialogueVolume01) ?
            0 : Mathf.Max(0, 6 * Mathf.Log(maxDialogueVolume01, 2) - _instance.dialogueDuckingDbFactor);
        float priorityDuckingDb = dbOverThreshold * _instance.duckingDbFactor;
        float dialogueDuckingDb = dialogueDbOverThreshold * _instance.dialogueDuckingDbFactor;

        foreach (ManagedInstance instance in managedInstances)
        {
            if (!paused || !instance.CanPause)
            {
                // dialogue-over-dialogue ducking overrides regular ducking
                if (instance.IsAmongSoloSpeakers(_instance.maxConcurrentSpeakers))
                {
                    // Debug.Log($"Apply dialogue ducking to { instance.Name }");
                    instance.GetAndUpdate(
                        dt, _instance.duckingSmoothnessInverted, priorityDuckingDb, _instance.indoorMuteDB);
                } else if (PrioritySounds.Contains(instance))
                {
                    // Debug.Log($"Apply no ducking to { instance.Name }");
                    instance.GetAndUpdate(
                        dt,  _instance.duckingSmoothnessInverted, 0, _instance.indoorMuteDB);
                } else
                {
                    // Debug.Log($"Apply max ducking to { instance.Name }");
                    instance.GetAndUpdate(
                        dt,  _instance.duckingSmoothnessInverted, Mathf.Max(priorityDuckingDb, dialogueDuckingDb), _instance.indoorMuteDB);
                }
            }
        }
    }

    /// <summary>
    /// Plays a sound with pitch
    /// </summary>
    /// <param name="name">String of the sound in Audio Manager</param>
    /// <param name="pitch">Pitch to play between 0.5 and 2.0</param>
    /// <returns>Reference to managed sfx instance in the audio manager, or null if it immediately stopped / does not need to be managed</returns>
    public static ManagedInstance PlayWithPitch(string name, float pitch) => PickSound(name).WithPitch(pitch).AndPlay();

    public static ManagedInstance PlayWithVolume(string name, float volumeMultiplier) => PickSound(name).WithVolume(volumeMultiplier).AndPlay();

    public static void PlayMusic(string name, bool stopOtherTracks = true, bool restartTrackIfPlaying = true)
    {
        Music m = GetMusic(name);

        if (m == null)
            return;

        if(m.emitter.IsPlaying() && !restartTrackIfPlaying)
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

    public static void SetMusicAmbiencePaused(bool paused)
    {
        if(paused)
            PauseMusicAndAmbience();
        else
            ResumeMusicAndAmbience();
    }

    public static void PauseMusicAndAmbience()
    {
        musicPaused = true;
        pausedMusic = new();
        foreach (Music music in _music)
        {
            if(music.emitter.IsPlaying())
            {
                var instance = music.emitter.EventInstance;
                instance.setPaused(true);
                pausedMusic.Add(instance);
            }
        }
        foreach (Music a in _ambience)
        {
            if(a.emitter.IsPlaying())
            {
                var instance = a.emitter.EventInstance;
                instance.setPaused(true);
                pausedMusic.Add(instance);
            }
        }
    }

    public static void ResumeMusicAndAmbience()
    {
        musicPaused = false;
        foreach (FMOD.Studio.EventInstance instance in pausedMusic)
        {
            instance.setPaused(false);
        }
        pausedMusic.Clear();
    }
    

    public static void StopMusic(string name)
    {
        Music m = GetMusic(name);

        if (m == null)
            return;

        m.emitter.Stop();
    }

    public static void PlayAmbience(string name)
    {
        Music a = GetAmbience(name);

        if (a == null)
            return;

        a.emitter.Play();
    }

    public static void StopAmbience(string name)
    {
        Music a = GetAmbience(name);

        if (a == null)
            return;

        a.emitter.Stop();
    }

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

    public static void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp(value, 0, 1);
        masterBus.setVolume(masterVolume);
    }

    public static void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        sfxVolume = value;

        if (_sounds == null)
            return;

        sfxBus.setVolume(value);
    }

    public static void SetAmbienceVolume(float value)
    {
        ambienceVolume = Mathf.Clamp(value, 0, 1);
        ambienceBus.setVolume(ambienceVolume);
    }

    public static void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, 0, 1);
    }

    private static void UpdateMusicVolume(float dt)
    {
        float vol = musicVolume;

        // for accurate music volume adjustment, always use multiplier 1 while paused
        if (!paused && soundDampenInstances.Count > 0)
        {
            Dictionary<object, (float amount, float length, float t)> nextSDI = new(soundDampenInstances.Count);
            float minDampened = 1;
            foreach((object key, var value) in soundDampenInstances)
            {
                if (value.t < value.length)
                {
                    float currDampen = Mathf.Lerp(value.amount, 1, _soundDampenCurve.Evaluate(value.t / value.length));
                    nextSDI.Add(key, (value.amount, value.length, value.t + dt));
                    minDampened = Mathf.Min(minDampened, currDampen);
                }
            }
            soundDampenInstances = nextSDI;

            vol *= minDampened;
        }

        musicBus.setVolume(vol);
    }

    public static void DampenMusic(object root, float amount, float length)
    {
        if (soundDampenInstances.ContainsKey(root))
        {
            soundDampenInstances[root] = (amount, length, 0);
        } else
        {
            soundDampenInstances.Add(root, (amount, length, 0));
        }
    }

    public static void StopDampen(object root)
    {
        if (soundDampenInstances.ContainsKey(root))
        {
            soundDampenInstances.Remove(root);
        }
    }
    
    public static void StopDampen<T>()
    {
        foreach(var key in soundDampenInstances.Keys)
        {
            if (key is T)
            {
                soundDampenInstances.Remove(key);
            }
        }
    }

    public static float GetMasterVolume() => masterVolume;
    public static float GetSFXVolume() => sfxVolume;
    public static float GetAmbienceVolume() => ambienceVolume;
    public static float GetMusicVolume() => musicVolume;

    private static void EnqueueModifier(AudioModifier.ModifierType m)
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
                Debug.Log($"[Audio] Modifier {m.name} is not actually in effect (property {name} does not have any modifiers attached)");
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
                        managedInstance.SoftStop();
                        
                        // Debug.Log($"rm {managedInstance.Name}");
                        
                        if (managedInstance.SingleInstanceKey(out string k))
                        {
                            // Debug.Log($"-{k}");
                            currentSingleInstanceKeys.Remove(k);
                        }
                        PrioritySounds.Remove(managedInstance);
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
                        managedInstance.SoftStop();
                        
                        
                        // Debug.Log($"rm {managedInstance.Name}");
                        
                        if (managedInstance.SingleInstanceKey(out string k))
                        {
                            // Debug.Log($"-{k}");
                            currentSingleInstanceKeys.Remove(k);
                        }
                        PrioritySounds.Remove(managedInstance);
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
        private SoundWrapper soundWrapper;
        /// <summary>
        /// For wrappers with useSpatials but no root transform, the listener transform is injected to root. isOverridingTransform is only set to true in this case.
        /// </summary>
        private readonly bool isOverridingTransform;
        private float progress;
        private Vector3 position;

        public bool Valid => soundWrapper.fmodInstance.isValid();

        public bool Stopped
        {
            get
            {
                // AT: it is possible to use callbacks but there's a bunch of malloc bs so this polling is kept for now
                var stopped = soundWrapper.fmodInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playback) != FMOD.RESULT.OK || playback == FMOD.Studio.PLAYBACK_STATE.STOPPED;
                return stopped;
            }
        }

        public bool Started
            => soundWrapper.fmodInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playback) == FMOD.RESULT.OK && playback == FMOD.Studio.PLAYBACK_STATE.STARTING;
        public readonly bool IsIndoor;
        public string Name => soundWrapper.sound?.name ?? "(No name)";

        public bool SingleInstanceKey(out string k)
        {
            k = soundWrapper.singleInstanceKey;
            return k != null;
        }

        public bool CanPause => soundWrapper.sound.canPause;
        public bool IsDialogue => soundWrapper.dialogueParent;
        public bool IsAmongSoloSpeakers(int maxConcurrentSpeakers)
        {
            return IsDialogue && VocalizableParagraph.SoloSpeaker(soundWrapper.dialogueParent, maxConcurrentSpeakers);
        }

        public float MixerVolume01
        {
            get
            {
                soundWrapper.fmodInstance.getVolume(out float volume);
                return volume;
            }
        }

        public bool Nullified => soundWrapper.root == null;

        public ManagedInstance(in SoundWrapper soundWrapper, bool isOverridingTransform, float indoorMuteDb)
        {
            this.soundWrapper = soundWrapper;
            position = soundWrapper.root.position;
            progress = 0;
            IsIndoor = soundWrapper.IsActuallyIndoor();
            this.isOverridingTransform = isOverridingTransform;

            bool indoorStatusDisagree = CalculatePositionIncorporateIndoor(ref position);
            if (indoorStatusDisagree)
            {
                soundWrapper.fmodInstance.setVolume(Mathf.Pow(2, 6 * Mathf.Log(soundWrapper.volume, 2) - indoorMuteDb) / 6);
            }
            else
            {
                soundWrapper.fmodInstance.setVolume(soundWrapper.volume);
            }
            soundWrapper.fmodInstance.set3DAttributes(position.To3DAttributes());
            soundWrapper.fmodInstance.start();
        }
        
        public void SoftStop()
        {
            soundWrapper.fmodInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            // soundWrapper.fmodInstance.setVolume(0);
            soundWrapper.fmodInstance.release();
        }

        public void HardStop()
        {
            soundWrapper.fmodInstance.setVolume(0);
            soundWrapper.fmodInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            soundWrapper.fmodInstance.release();
        }

        public void SetPaused(bool paused)
        {
            if (soundWrapper.sound.canPause) soundWrapper.fmodInstance.setPaused(paused);
            if (paused && IsDialogue)
            {
                HardStop();
            }
        }

        public void Tick(EventInstanceTick tick)
        {
            tick(ref soundWrapper.fmodInstance);
        }

        public void GetAndUpdate(float dt, float duckingSmoothnessInverted, float priorityDuckDb, float indoorMuteDb)
        {
            float volumeDb = 6 * Mathf.Log(soundWrapper.volume, 2) - priorityDuckDb;
            progress += dt;
            if (progress >= soundWrapper.duration)
            {
                HardStop();
                return;
            }
            if (!soundWrapper.useDoppler)
            {
                Vector3 shiftedPosition = soundWrapper.root.position;
                bool indoorStatusDisagree = CalculatePositionIncorporateIndoor(ref shiftedPosition);
                if (indoorStatusDisagree) volumeDb -= indoorMuteDb;
                soundWrapper.fmodInstance.set3DAttributes(shiftedPosition.To3DAttributes());
                volumeDb = Mathf.Pow(2, volumeDb / 6);
                
                // soundWrapper.fmodInstance.setVolume(volumeDb);
                soundWrapper.fmodInstance.getVolume(out float currVolumeDb);
                soundWrapper.fmodInstance.setVolume(Mathf.Lerp(currVolumeDb, volumeDb, dt * duckingSmoothnessInverted));
            }
            else
            {
                Vector3 p = soundWrapper.root.position;
                Vector3 v = dt > float.Epsilon ? (p - position) / (dt) : Vector3.zero;

                position = p;
                bool indoorStatusDisagree = CalculatePositionIncorporateIndoor(ref p);
                soundWrapper.fmodInstance.set3DAttributes(new FMOD.ATTRIBUTES_3D
                {
                    forward = Vector3.forward.ToFMODVector(),
                    up = Vector3.up.ToFMODVector(),
                    position = p.ToFMODVector(),
                    velocity = (v * soundWrapper.sound.dopplerScale).ToFMODVector()
                });
                if (indoorStatusDisagree) volumeDb -= indoorMuteDb;
                volumeDb = Mathf.Pow(2, volumeDb / 6);
                
                // soundWrapper.fmodInstance.setVolume(volumeDb);
                soundWrapper.fmodInstance.getVolume(out float currVolumeDb);
                soundWrapper.fmodInstance.setVolume(Mathf.Lerp(currVolumeDb, volumeDb, dt * duckingSmoothnessInverted));
            }

            if (IsDialogue)
            {
                soundWrapper.fmodInstance.getVolume(out float currVolumeDb);
            }
        }

        /// <returns>Returns whether indoor status disagrees with listener's indoor status</returns>
        private bool CalculatePositionIncorporateIndoor(ref Vector3 original)
        {
            if (IsIndoor == listenerIsIndoor) return false;
            if (isOverridingTransform) return true;
            if (listenerIsIndoor)
            {
                original += SGrid.GetHousingOffset() * Vector3.up;
            } else
            {
                original += SGrid.GetHousingOffset() * Vector3.down;
            }
            return true;
        }
        
        public float GetDurationSeconds()
        {
            soundWrapper.fmodInstance.getDescription(out FMOD.Studio.EventDescription description);
            description.getLength(out int fmodMilisecondLength); // https://www.fmod.com/docs/2.01/api/studio-api-eventdescription.html#studio_eventdescription_getlength
            return fmodMilisecondLength / 1000.0f;
        }
    }
}
