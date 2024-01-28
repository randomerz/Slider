using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FMODUnity;
using FMOD.Studio;
using SliderVocalization;
using System.Linq;
using UnityEngine.UIElements;

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

    private static float sfxVolume = 0.5f; // [0..1]
    private static float musicVolume = 0.5f;

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
    [SerializeField, Range(0, 10)]
    private float indoorMuteDB;

    static HashSet<ManagedInstance> PrioritySounds = new ();
    [SerializeField, Range(0, 1), Tooltip("origional dB minus (dB over threshold * factor)")]
    private float duckingDbFactor;
    [SerializeField, Range(-20, 0)]
    private float duckingThreshold;
    [SerializeField, Range(0, 1), Tooltip("ducking factor specifically for dialogue over dialogue")]
    private float dialogueDuckingDbFactor;

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

        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);

        soundDampenInstances = new ();
        PrioritySounds = new ();
    }

    private void Update()
    {
        UpdateCameraPosition();
        UpdateManagedInstances(Time.unscaledDeltaTime);
        UpdateMusicVolume();
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

    public delegate void EventInstanceTick(ref EventInstance item);

    public static ManagedInstance Play(ref SoundWrapper soundWrapper)
    {
        if (!soundWrapper.valid) return null;

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
            managedInstances ??= new List<ManagedInstance>(10);
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
            soundWrapper.PlayAsOneshot();
            return null;
        }
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
        managedInstances ??= new List<ManagedInstance>(10);
        foreach (ManagedInstance instance in managedInstances)
        {
            instance.SetPaused(paused);
        }
    }

    private static void UpdateManagedInstances(float dt)
    {
        managedInstances ??= new List<ManagedInstance>(10);
        managedInstances.RemoveAll(delegate (ManagedInstance instance)
        {
            if (instance.Nullified)
            {
                instance.Stop();
            }
            bool shouldRemove = instance.Nullified || !instance.Valid || instance.Stopped;
            if (shouldRemove)
            {
                PrioritySounds.Remove(instance);
                instance.Stop();
            }
            return shouldRemove;
        });
        float MaxPriorityVolume01 = 0;
        float MaxDialogueVolume01 = 0;
        foreach (ManagedInstance instance in managedInstances)
        {
            if (!paused || !instance.CanPause)
            {
                if (!PrioritySounds.Contains(instance))
                {
                    continue;
                }
                float vol = instance.MixerVolume01;
                MaxPriorityVolume01 = Mathf.Max(MaxPriorityVolume01, vol);
                if (instance.IsDialogue)
                {
                    MaxDialogueVolume01 = Mathf.Max(MaxDialogueVolume01, vol);
                }
            }
        }
        float dbOverThreshold = Mathf.Approximately(0, MaxPriorityVolume01) ?
            0 : Mathf.Max(0, 6 * Mathf.Log(MaxPriorityVolume01, 2) - _instance.duckingThreshold);
        float dialogueDbOverThreshold = Mathf.Approximately(0, MaxDialogueVolume01) ?
            0 : Mathf.Max(0, 6 * Mathf.Log(MaxDialogueVolume01, 2) - _instance.dialogueDuckingDbFactor);
        float priorityDuckingDb = dbOverThreshold * _instance.duckingDbFactor;
        float dialogueDuckingDb = dialogueDbOverThreshold * _instance.dialogueDuckingDbFactor;

        foreach (ManagedInstance instance in managedInstances)
        {
            if (!paused || !instance.CanPause)
            {
                // dialogue-over-dialogue ducking overrides regular ducking
                if (instance.ShouldApplyDialogueDucking)
                {
                    instance.GetAndUpdate(dt, priorityDuckingDb, _instance.indoorMuteDB);
                } else if (PrioritySounds.Contains(instance))
                {
                    instance.GetAndUpdate(dt, 0, _instance.indoorMuteDB);
                } else
                {
                    instance.GetAndUpdate(dt, Mathf.Max(priorityDuckingDb, dialogueDuckingDb), _instance.indoorMuteDB);
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

    public static void PlayMusic(string name, bool stopOtherTracks = true)
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
    }

    private static void UpdateMusicVolume()
    {
        // AT: no music ducking, use dampen instead
        // float vol = Mathf.Clamp(Subtract01SpaceVolumes(musicVolume, ducking) * musicVolumeMultiplier, 0, 1);
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
                    nextSDI.Add(key, (value.amount, value.length, value.t + Time.deltaTime));
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
                        managedInstance.Stop();
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
        private SoundWrapper soundWrapper;
        /// <summary>
        /// For wrappers with useSpatials but no root transform, the listener transform is injected to root. isOverridingTransform is ony set to true in this case.
        /// </summary>
        private readonly bool isOverridingTransform;
        private float progress;
        private Vector3 position;

        public bool Valid => soundWrapper.fmodInstance.isValid();
        public bool Stopped
            => soundWrapper.fmodInstance.getPlaybackState(out PLAYBACK_STATE playback) != FMOD.RESULT.OK || playback == PLAYBACK_STATE.STOPPED;

        public bool Started
            => soundWrapper.fmodInstance.getPlaybackState(out PLAYBACK_STATE playback) == FMOD.RESULT.OK && playback == PLAYBACK_STATE.STARTING;
        public readonly bool IsIndoor;
        public string Name => soundWrapper.sound?.name ?? "(No name)";

        public bool CanPause => soundWrapper.sound.canPause;
        public bool IsDialogue => soundWrapper.dialogueParent;
        public bool ShouldApplyDialogueDucking { 
            get {
                if (!IsDialogue) return false;
                VocalizableParagraph SoloSpeaker = VocalizableParagraph.SoloSpeaker;
                if (SoloSpeaker != null && SoloSpeaker != soundWrapper.dialogueParent)
                {
                    return true;
                }
                return false;
            }
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
                soundWrapper.fmodInstance.setVolume(Mathf.Pow(2, Mathf.Max(0, 6 * Mathf.Log(soundWrapper.volume, 2) - indoorMuteDb)) / 6);
            }
            soundWrapper.fmodInstance.set3DAttributes(position.To3DAttributes());
            soundWrapper.fmodInstance.start();
        }

        public void Stop()
        {
            soundWrapper.fmodInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            soundWrapper.fmodInstance.release();
        }

        public void SetPaused(bool paused)
        {
            if (soundWrapper.sound.canPause) soundWrapper.fmodInstance.setPaused(paused);
        }

        public void Tick(EventInstanceTick tick)
        {
            tick(ref soundWrapper.fmodInstance);
        }

        public void GetAndUpdate(float dt, float priorityDuckDb, float indoorMuteDb)
        {
            float volumeDb = 6 * Mathf.Log(soundWrapper.volume, 2) - priorityDuckDb;
            progress += dt;
            if (progress >= soundWrapper.duration)
            {
                Stop();
                return;
            }
            if (!soundWrapper.useDoppler)
            {
                Vector3 shiftedPosition = soundWrapper.root.position;
                bool indoorStatusDisagree = CalculatePositionIncorporateIndoor(ref shiftedPosition);
                if (indoorStatusDisagree) volumeDb -= indoorMuteDb;
                soundWrapper.fmodInstance.set3DAttributes(shiftedPosition.To3DAttributes());
                soundWrapper.fmodInstance.setVolume(Mathf.Pow(2, volumeDb / 6));
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
                soundWrapper.fmodInstance.setVolume(Mathf.Pow(2, volumeDb / 6));
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
    }
}
