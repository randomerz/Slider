using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple yet powerful sound manager, that will let you play sounds with an event based approach and performance in mind.
	/// 
	/// Features :
	/// 
	/// - Play/stop/pause/resume/free sounds
	/// - Full control : loop, volume, pitch, pan, spatial blend, bypasses, priority, reverb, doppler level, spread, rolloff mode, distance
	/// - 2D & 3D spatial support
	/// - Built-in pooling, automatically recycle a set of audio sources for maximum performance
	/// - Built in audio mixer and groups, with ready-made tracks (Master, Music, SFX, UI), and options to play on more groups if needed
	/// - Stop/pause/resume/free entire tracks
	/// - Stop/pause/resume/free all sounds at once
	/// - Mute / set volume entire tracks
	/// - Save and load settings, with auto save / auto load mechanics built-in
	/// - Fade in/out sounds
	/// - Fade in/out tracks
	/// - Solo mode : play a sound with one or all tracks muted, then unmute them automatically afterwards
	/// - PlayOptions struct
	/// - Option to have sounds persist across scene loads and from scene to scene
	/// - Inspector controls for tracks (volume, mute, unmute, play, pause, stop, resume, free, number of sounds)
	/// - MMSfxEvents
	/// - MMSoundManagerEvents : mute track, control track, save, load, reset, stop persistent sounds 
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Audio/MMSoundManager")]
	public class MMSoundManager : MMPersistentSingleton<MMSoundManager>, 
		MMEventListener<MMSoundManagerTrackEvent>, 
		MMEventListener<MMSoundManagerEvent>,
		MMEventListener<MMSoundManagerSoundControlEvent>,
		MMEventListener<MMSoundManagerSoundFadeEvent>,
		MMEventListener<MMSoundManagerAllSoundsControlEvent>,
		MMEventListener<MMSoundManagerTrackFadeEvent>
	{
		/// <summary>
		/// Statics initialization to support enter play modes
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}
		
		/// the possible ways to manage a track
		public enum MMSoundManagerTracks { Sfx, Music, UI, Master, Other}
        
		[Header("Settings")]
		/// the current sound settings 
		[Tooltip("the current sound settings ")]
		public MMSoundManagerSettingsSO settingsSo;

		[Header("Pool")]
		/// the size of the AudioSource pool, a reserve of ready-to-use sources that will get recycled. Should be approximately equal to the maximum amount of sounds that you expect to be playing at once 
		[Tooltip("the size of the AudioSource pool, a reserve of ready-to-use sources that will get recycled. Should be approximately equal to the maximum amount of sounds that you expect to be playing at once")]
		public int AudioSourcePoolSize = 10;
		/// whether or not the pool can expand (create new audiosources on demand). In a perfect world you'd want to avoid this, and have a sufficiently big pool, to avoid costly runtime creations.
		[Tooltip("whether or not the pool can expand (create new audiosources on demand). In a perfect world you'd want to avoid this, and have a sufficiently big pool, to avoid costly runtime creations.")]
		public bool PoolCanExpand = true;
        
		protected MMSoundManagerAudioPool _pool;
		protected GameObject _tempAudioSourceGameObject;
		protected MMSoundManagerSound _sound;
		protected List<MMSoundManagerSound> _sounds; 
		protected AudioSource _tempAudioSource;
		protected Dictionary<AudioSource, Coroutine> _fadeInSoundCoroutines;
		protected Dictionary<AudioSource, Coroutine> _fadeOutSoundCoroutines;
		protected Dictionary<MMSoundManagerTracks, Coroutine> _fadeTrackCoroutines;

		#region Initialization

		/// <summary>
		/// On Awake we initialize our manager
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			InitializeSoundManager();
		}
        
		/// <summary>
		/// On Start we load and apply our saved settings if needed.
		/// This is done on Start and not Awake because of a bug in Unity's AudioMixer API
		/// </summary>
		protected virtual void Start()
		{
			if ((settingsSo != null) && (settingsSo.Settings.AutoLoad))
			{
				settingsSo.LoadSoundSettings();    
			}
		}

		/// <summary>
		/// Initializes the pool, fills it, registers to the scene loaded event
		/// </summary>
		protected virtual void InitializeSoundManager()
		{
			if (_pool == null)
			{
				_pool = new MMSoundManagerAudioPool();    
			}
			_sounds = new List<MMSoundManagerSound>();
			_pool.FillAudioSourcePool(AudioSourcePoolSize, this.transform);
			_fadeInSoundCoroutines = new Dictionary<AudioSource, Coroutine>();
			_fadeOutSoundCoroutines = new Dictionary<AudioSource, Coroutine>();
			_fadeTrackCoroutines = new Dictionary<MMSoundManagerTracks, Coroutine>();
		}
        
		#endregion
        
		#region PlaySound

		/// <summary>
		/// Plays a sound, separate options object signature
		/// </summary>
		/// <param name="audioClip"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public virtual AudioSource PlaySound(AudioClip audioClip, MMSoundManagerPlayOptions options)
		{
			return PlaySound(audioClip, options.MmSoundManagerTrack, options.Location,
				options.Loop, options.Volume, options.ID,
				options.Fade, options.FadeInitialVolume, options.FadeDuration, options.FadeTween,
				options.Persistent,
				options.RecycleAudioSource, options.AudioGroup,
				options.Pitch, options.PanStereo, options.SpatialBlend,
				options.SoloSingleTrack, options.SoloAllTracks, options.AutoUnSoloOnEnd,
				options.BypassEffects, options.BypassListenerEffects, options.BypassReverbZones, options.Priority,
				options.ReverbZoneMix,
				options.DopplerLevel, options.Spread, options.RolloffMode, options.MinDistance, options.MaxDistance, 
				options.DoNotAutoRecycleIfNotDonePlaying, options.PlaybackTime, options.PlaybackDuration, options.AttachToTransform,
				options.UseSpreadCurve, options.SpreadCurve, options.UseCustomRolloffCurve, options.CustomRolloffCurve,
				options.UseSpatialBlendCurve, options.SpatialBlendCurve, options.UseReverbZoneMixCurve, options.ReverbZoneMixCurve
			);
		}

		/// <summary>
		/// Plays a sound, signature with all options
		/// </summary>
		/// <param name="audioClip"></param>
		/// <param name="mmSoundManagerTrack"></param>
		/// <param name="location"></param>
		/// <param name="loop"></param>
		/// <param name="volume"></param>
		/// <param name="ID"></param>
		/// <param name="fade"></param>
		/// <param name="fadeInitialVolume"></param>
		/// <param name="fadeDuration"></param>
		/// <param name="fadeTween"></param>
		/// <param name="persistent"></param>
		/// <param name="recycleAudioSource"></param>
		/// <param name="audioGroup"></param>
		/// <param name="pitch"></param>
		/// <param name="panStereo"></param>
		/// <param name="spatialBlend"></param>
		/// <param name="soloSingleTrack"></param>
		/// <param name="soloAllTracks"></param>
		/// <param name="autoUnSoloOnEnd"></param>
		/// <param name="bypassEffects"></param>
		/// <param name="bypassListenerEffects"></param>
		/// <param name="bypassReverbZones"></param>
		/// <param name="priority"></param>
		/// <param name="reverbZoneMix"></param>
		/// <param name="dopplerLevel"></param>
		/// <param name="spread"></param>
		/// <param name="rolloffMode"></param>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		public virtual AudioSource PlaySound(AudioClip audioClip, MMSoundManagerTracks mmSoundManagerTrack, Vector3 location, 
			bool loop = false, float volume = 1.0f, int ID = 0,
			bool fade = false, float fadeInitialVolume = 0f, float fadeDuration = 1f, MMTweenType fadeTween = null,
			bool persistent = false,
			AudioSource recycleAudioSource = null, AudioMixerGroup audioGroup = null,
			float pitch = 1f, float panStereo = 0f, float spatialBlend = 0.0f,  
			bool soloSingleTrack = false, bool soloAllTracks = false, bool autoUnSoloOnEnd = false,  
			bool bypassEffects = false, bool bypassListenerEffects = false, bool bypassReverbZones = false, int priority = 128, float reverbZoneMix = 1f,
			float dopplerLevel = 1f, int spread = 0, AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic, float minDistance = 1f, float maxDistance = 500f,
			bool doNotAutoRecycleIfNotDonePlaying = false, float playbackTime = 0f, float playbackDuration = 0f, Transform attachToTransform = null,
			bool useSpreadCurve = false, AnimationCurve spreadCurve = null, bool useCustomRolloffCurve = false, AnimationCurve customRolloffCurve = null,
			bool useSpatialBlendCurve = false, AnimationCurve spatialBlendCurve = null, bool useReverbZoneMixCurve = false, AnimationCurve reverbZoneMixCurve = null
		)
		{
			if (this == null) { return null; }
			if (!audioClip) { return null; }
            
			// audio source setup ---------------------------------------------------------------------------------
            
			// we reuse an audiosource if one is passed in parameters
			AudioSource audioSource = recycleAudioSource;   
            
			if (!audioSource)
			{
				// we pick an idle audio source from the pool if possible
				audioSource = _pool.GetAvailableAudioSource(PoolCanExpand, this.transform);
				if ((audioSource) && (!loop))
				{
					recycleAudioSource = audioSource;
					// we destroy the host after the clip has played (if it is not tagged for reusability.
					StartCoroutine(_pool.AutoDisableAudioSource(audioClip.length / Mathf.Abs(pitch), audioSource, audioClip, doNotAutoRecycleIfNotDonePlaying, playbackTime, playbackDuration));
				}
			}

			// we create an audio source if needed
			if (!audioSource)
			{
				_tempAudioSourceGameObject = new GameObject("MMAudio_"+audioClip.name);
				SceneManager.MoveGameObjectToScene(_tempAudioSourceGameObject, this.gameObject.scene);
				audioSource = _tempAudioSourceGameObject.AddComponent<AudioSource>();
			}
            
			// audio source settings ---------------------------------------------------------------------------------
            
			audioSource.transform.position = location;
			audioSource.clip = audioClip;
			audioSource.pitch = pitch;
			audioSource.spatialBlend = spatialBlend;
			audioSource.panStereo = panStereo;
			audioSource.loop = loop;
			audioSource.bypassEffects = bypassEffects;
			audioSource.bypassListenerEffects = bypassListenerEffects;
			audioSource.bypassReverbZones = bypassReverbZones;
			audioSource.priority = priority;
			audioSource.reverbZoneMix = reverbZoneMix;
			audioSource.dopplerLevel = dopplerLevel;
			audioSource.spread = spread;
			audioSource.rolloffMode = rolloffMode;
			audioSource.minDistance = minDistance;
			audioSource.maxDistance = maxDistance;
			audioSource.time = playbackTime; 
			
			// curves
			if (useSpreadCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.Spread, spreadCurve); }
			if (useCustomRolloffCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloffCurve); }
			if (useSpatialBlendCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatialBlendCurve); }
			if (useReverbZoneMixCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, reverbZoneMixCurve); }
			
			// attaching to target
			if (attachToTransform != null)
			{
				MMFollowTarget followTarget = audioSource.gameObject.MMGetComponentNoAlloc<MMFollowTarget>();
				if (followTarget == null)
				{
					followTarget = audioSource.gameObject.AddComponent<MMFollowTarget>();
				}
				followTarget.Target = attachToTransform;
				followTarget.InterpolatePosition = false;
				followTarget.InterpolateRotation = false;
				followTarget.InterpolateScale = false;
				followTarget.FollowRotation = false;
				followTarget.FollowScale = false;
				followTarget.enabled = true;
			}
            
			// track and volume ---------------------------------------------------------------------------------
            
			if (settingsSo != null)
			{
				audioSource.outputAudioMixerGroup = settingsSo.MasterAudioMixerGroup;
				switch (mmSoundManagerTrack)
				{
					case MMSoundManagerTracks.Master:
						audioSource.outputAudioMixerGroup = settingsSo.MasterAudioMixerGroup;
						break;
					case MMSoundManagerTracks.Music:
						audioSource.outputAudioMixerGroup = settingsSo.MusicAudioMixerGroup;
						break;
					case MMSoundManagerTracks.Sfx:
						audioSource.outputAudioMixerGroup = settingsSo.SfxAudioMixerGroup;
						break;
					case MMSoundManagerTracks.UI:
						audioSource.outputAudioMixerGroup = settingsSo.UIAudioMixerGroup;
						break;
				}
			}
			if (audioGroup) { audioSource.outputAudioMixerGroup = audioGroup; }
			audioSource.volume = volume;  
            
			// we start playing the sound
			audioSource.Play();
            
			// we destroy the host after the clip has played if it was a one time AS.
			if (!loop && !recycleAudioSource)
			{
				float destroyDelay = (playbackDuration > 0) ? playbackDuration : audioClip.length - playbackTime;
				Destroy(_tempAudioSourceGameObject, destroyDelay);
			}
            
			// we fade the sound in if needed
			if (fade)
			{
				FadeSound(audioSource, fadeDuration, fadeInitialVolume, volume, fadeTween);
			}
            
			// we handle soloing
			if (soloSingleTrack)
			{
				MuteSoundsOnTrack(mmSoundManagerTrack, true, 0f);
				audioSource.mute = false;
				if (autoUnSoloOnEnd)
				{
					MuteSoundsOnTrack(mmSoundManagerTrack, false, audioClip.length);
				}
			}
			else if (soloAllTracks)
			{
				MuteAllSounds();
				audioSource.mute = false;
				if (autoUnSoloOnEnd)
				{
					StartCoroutine(MuteAllSoundsCoroutine(audioClip.length - playbackTime, false));
				}
			}
            
			// we prepare for storage
			_sound.ID = ID;
			_sound.Track = mmSoundManagerTrack;
			_sound.Source = audioSource;
			_sound.Persistent = persistent;
			_sound.PlaybackTime = playbackTime;
			_sound.PlaybackDuration = playbackDuration;

			// we check if that audiosource is already being tracked in _sounds
			bool alreadyIn = false;
			for (int i = 0; i < _sounds.Count; i++)
			{
				if (_sounds[i].Source == audioSource)
				{
					_sounds[i] = _sound;
					alreadyIn = true;
				}
			}

			if (!alreadyIn)
			{
				_sounds.Add(_sound);    
			}

			// we return the audiosource reference
			return audioSource;
		}
        
		#endregion

		#region SoundControls

		/// <summary>
		/// Pauses the specified audiosource
		/// </summary>
		/// <param name="source"></param>
		public virtual void PauseSound(AudioSource source)
		{
			source.Pause();
		}

		/// <summary>
		/// resumes play on the specified audio source
		/// </summary>
		/// <param name="source"></param>
		public virtual void ResumeSound(AudioSource source)
		{
			source.Play();
		}
        
		/// <summary>
		/// Stops the specified audio source
		/// </summary>
		/// <param name="source"></param>
		public virtual void StopSound(AudioSource source)
		{
			source.Stop();
		}
        
		/// <summary>
		/// Frees a specific sound, stopping it and returning it to the pool
		/// </summary>
		/// <param name="source"></param>
		public virtual void FreeSound(AudioSource source)
		{
			source.Stop();
			if (!_pool.FreeSound(source))
			{
				Destroy(source.gameObject);    
			}
		}

		#endregion
        
		#region TrackControls
        
		/// <summary>
		/// Mutes an entire track
		/// </summary>
		/// <param name="track"></param>
		public virtual void MuteTrack(MMSoundManagerTracks track)
		{
			ControlTrack(track, ControlTrackModes.Mute, 0f);
		}

		/// <summary>
		/// Unmutes an entire track
		/// </summary>
		/// <param name="track"></param>
		public virtual void UnmuteTrack(MMSoundManagerTracks track)
		{
			ControlTrack(track, ControlTrackModes.Unmute, 0f);
		}

		/// <summary>
		/// Sets the volume of an entire track
		/// </summary>
		/// <param name="track"></param>
		/// <param name="volume"></param>
		public virtual void SetTrackVolume(MMSoundManagerTracks track, float volume)
		{
			ControlTrack(track, ControlTrackModes.SetVolume, volume);
		}

		/// <summary>
		/// Returns the current volume of a track
		/// </summary>
		/// <param name="track"></param>
		/// <param name="volume"></param>
		public virtual float GetTrackVolume(MMSoundManagerTracks track, bool mutedVolume)
		{
			switch (track)
			{
				case MMSoundManagerTracks.Master:
					if (mutedVolume)
					{
						return settingsSo.Settings.MutedMasterVolume;
					}
					else
					{
						return settingsSo.Settings.MasterVolume;
					}
				case MMSoundManagerTracks.Music:
					if (mutedVolume)
					{
						return settingsSo.Settings.MutedMusicVolume;
					}
					else
					{
						return settingsSo.Settings.MusicVolume;
					}
				case MMSoundManagerTracks.Sfx:
					if (mutedVolume)
					{
						return settingsSo.Settings.MutedSfxVolume;
					}
					else
					{
						return settingsSo.Settings.SfxVolume;
					}
				case MMSoundManagerTracks.UI:
					if (mutedVolume)
					{
						return settingsSo.Settings.MutedUIVolume;
					}
					else
					{
						return settingsSo.Settings.UIVolume;
					}
			}

			return 1f;
		}
        
		/// <summary>
		/// Pauses all sounds on a track
		/// </summary>
		/// <param name="track"></param>
		public virtual void PauseTrack(MMSoundManagerTracks track)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Track == track)
				{
					sound.Source.Pause();
				}
			}    
		}

		/// <summary>
		/// Plays or resumes all sounds on a track
		/// </summary>
		/// <param name="track"></param>
		public virtual void PlayTrack(MMSoundManagerTracks track)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Track == track)
				{
					sound.Source.Play();
				}
			}    
		}

		/// <summary>
		/// Stops all sounds on a track
		/// </summary>
		/// <param name="track"></param>
		public virtual void StopTrack(MMSoundManagerTracks track)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Track == track)
				{
					sound.Source.Stop();
				}
			}
		}

		/// <summary>
		/// Returns true if sounds are currently playing on that track
		/// </summary>
		/// <param name="track"></param>
		public virtual bool HasSoundsPlaying(MMSoundManagerTracks track)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if ((sound.Track == track) && (sound.Source.isPlaying))
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Returns a list of MMSoundManagerSounds for the specified track
		/// </summary>
		/// <param name="track">the track on which to grab the playing sounds</param>
		/// <returns></returns>
		public virtual List<MMSoundManagerSound> GetSoundsPlaying(MMSoundManagerTracks track)
		{
			List<MMSoundManagerSound> soundsPlaying = new List<MMSoundManagerSound>();
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if ((sound.Track == track) && (sound.Source.isPlaying))
				{
					soundsPlaying.Add(sound);
				}
			}
			return soundsPlaying;
		}
        
		/// <summary>
		/// Stops all sounds on a track, and returns them to the pool
		/// </summary>
		/// <param name="track"></param>
		public virtual void FreeTrack(MMSoundManagerTracks track)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Track == track)
				{
					sound.Source.Stop();
					sound.Source.gameObject.SetActive(false);
				}
			}
		}
        
		/// <summary>
		/// Mutes the music track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void MuteMusic() { MuteTrack(MMSoundManagerTracks.Music); }
        
		/// <summary>
		/// Unmutes the music track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void UnmuteMusic() { UnmuteTrack(MMSoundManagerTracks.Music); }
        
		/// <summary>
		/// Mutes the sfx track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void MuteSfx() { MuteTrack(MMSoundManagerTracks.Sfx); }
        
        
		/// <summary>
		/// Unmutes the sfx track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void UnmuteSfx() { UnmuteTrack(MMSoundManagerTracks.Sfx); }
        
		/// <summary>
		/// Mutes the UI track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void MuteUI() { MuteTrack(MMSoundManagerTracks.UI); }
        
		/// <summary>
		/// Unmutes the UI track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void UnmuteUI() { UnmuteTrack(MMSoundManagerTracks.UI); }
        
		/// <summary>
		/// Mutes the master track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void MuteMaster() { MuteTrack(MMSoundManagerTracks.Master); }
        
		/// <summary>
		/// Unmutes the master track, QoL method ready to bind to a UnityEvent
		/// </summary>
		public virtual void UnmuteMaster() { UnmuteTrack(MMSoundManagerTracks.Master); }
        
        
		/// <summary>
		/// Sets the volume of the Music track to the specified value, QoL method, ready to bind to a UnityEvent
		/// </summary>
		public virtual void SetVolumeMusic(float newVolume) { SetTrackVolume(MMSoundManagerTracks.Music, newVolume);}
		/// <summary>
		/// Sets the volume of the SFX track to the specified value, QoL method, ready to bind to a UnityEvent
		/// </summary>
		public virtual void SetVolumeSfx(float newVolume) { SetTrackVolume(MMSoundManagerTracks.Sfx, newVolume);}
		/// <summary>
		/// Sets the volume of the UI track to the specified value, QoL method, ready to bind to a UnityEvent
		/// </summary>
		public virtual void SetVolumeUI(float newVolume) { SetTrackVolume(MMSoundManagerTracks.UI, newVolume);}
		/// <summary>
		/// Sets the volume of the Master track to the specified value, QoL method, ready to bind to a UnityEvent
		/// </summary>
		public virtual void SetVolumeMaster(float newVolume) { SetTrackVolume(MMSoundManagerTracks.Master, newVolume);}

		/// <summary>
		/// Returns true if the specified track is muted, false otherwise
		/// </summary>
		/// <param name="track"></param>
		/// <returns></returns>
		public virtual bool IsMuted(MMSoundManagerTracks track)
		{
			switch (track)
			{
				case MMSoundManagerTracks.Master:
					return !settingsSo.Settings.MasterOn; 
				case MMSoundManagerTracks.Music:
					return !settingsSo.Settings.MusicOn;
				case MMSoundManagerTracks.Sfx:
					return !settingsSo.Settings.SfxOn;
				case MMSoundManagerTracks.UI:
					return !settingsSo.Settings.UIOn;
			}
			return false;
		}
        
		/// <summary>
		/// A method that will let you mute/unmute a track, or set it to a specified volume
		/// </summary>
		public enum ControlTrackModes { Mute, Unmute, SetVolume }
		protected virtual void ControlTrack(MMSoundManagerTracks track, ControlTrackModes trackMode, float volume = 0.5f)
		{
			string target = "";
			float savedVolume = 0f; 
            
			switch (track)
			{
				case MMSoundManagerTracks.Master:
					target = settingsSo.Settings.MasterVolumeParameter;
					if (trackMode == ControlTrackModes.Mute) { settingsSo.TargetAudioMixer.GetFloat(target, out settingsSo.Settings.MutedMasterVolume); settingsSo.Settings.MasterOn = false; }
					else if (trackMode == ControlTrackModes.Unmute) { savedVolume = settingsSo.Settings.MutedMasterVolume; settingsSo.Settings.MasterOn = true; }
					break;
				case MMSoundManagerTracks.Music:
					target = settingsSo.Settings.MusicVolumeParameter;
					if (trackMode == ControlTrackModes.Mute) { settingsSo.TargetAudioMixer.GetFloat(target, out settingsSo.Settings.MutedMusicVolume);  settingsSo.Settings.MusicOn = false; }
					else if (trackMode == ControlTrackModes.Unmute) { savedVolume = settingsSo.Settings.MutedMusicVolume;  settingsSo.Settings.MusicOn = true; }
					break;
				case MMSoundManagerTracks.Sfx:
					target = settingsSo.Settings.SfxVolumeParameter;
					if (trackMode == ControlTrackModes.Mute) { settingsSo.TargetAudioMixer.GetFloat(target, out settingsSo.Settings.MutedSfxVolume);  settingsSo.Settings.SfxOn = false; }
					else if (trackMode == ControlTrackModes.Unmute) { savedVolume = settingsSo.Settings.MutedSfxVolume;  settingsSo.Settings.SfxOn = true; }
					break;
				case MMSoundManagerTracks.UI:
					target = settingsSo.Settings.UIVolumeParameter;
					if (trackMode == ControlTrackModes.Mute) { settingsSo.TargetAudioMixer.GetFloat(target, out settingsSo.Settings.MutedUIVolume);  settingsSo.Settings.UIOn = false; }
					else if (trackMode == ControlTrackModes.Unmute) { savedVolume = settingsSo.Settings.MutedUIVolume;  settingsSo.Settings.UIOn = true; }
					break;
			}

			switch (trackMode)
			{
				case ControlTrackModes.Mute:
					settingsSo.SetTrackVolume(track, 0f);
					break;
				case ControlTrackModes.Unmute:
					settingsSo.SetTrackVolume(track, settingsSo.MixerVolumeToNormalized(savedVolume));
					break;
				case ControlTrackModes.SetVolume:
					settingsSo.SetTrackVolume(track, volume);
					break;
			}

			settingsSo.GetTrackVolumes();

			if (settingsSo.Settings.AutoSave)
			{
				settingsSo.SaveSoundSettings();
			}
		}
        
		#endregion

		#region Fades
        
		/// <summary>
		/// Fades an entire track over the specified duration towards the desired finalVolume
		/// </summary>
		/// <param name="track"></param>
		/// <param name="duration"></param>
		/// <param name="initialVolume"></param>
		/// <param name="finalVolume"></param>
		/// <param name="tweenType"></param>
		public virtual void FadeTrack(MMSoundManagerTracks track, float duration, float initialVolume = 0f, float finalVolume = 1f, MMTweenType tweenType = null)
		{
			Coroutine coroutine = StartCoroutine(FadeTrackCoroutine(track, duration, initialVolume, finalVolume, tweenType));
			_fadeTrackCoroutines[track] = coroutine;
		}
        
		/// <summary>
		/// Fades a target sound towards a final volume over time
		/// </summary>
		/// <param name="source"></param>
		/// <param name="duration"></param>
		/// <param name="initialVolume"></param>
		/// <param name="finalVolume"></param>
		/// <param name="tweenType"></param>
		public virtual void FadeSound(AudioSource source, float duration, float initialVolume, float finalVolume, MMTweenType tweenType, bool freeAfterFade = false)
		{
			Coroutine coroutine = StartCoroutine(FadeCoroutine(source, duration, initialVolume, finalVolume, tweenType, freeAfterFade));
			if (initialVolume < finalVolume)
			{
				_fadeInSoundCoroutines[source] = coroutine;	
			}
			else
			{
				_fadeOutSoundCoroutines[source] = coroutine;
			}
		}

		/// <summary>
		/// Returns true if the specified source is already fading, false otherwise
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public virtual bool SoundIsFadingIn(AudioSource source)
		{
			if (_fadeInSoundCoroutines.TryGetValue(source, out Coroutine co))
			{
				return (_fadeInSoundCoroutines[source] != null);	
			}

			return false;
		}

		/// <summary>
		/// Returns true if the specified source is already fading, false otherwise
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public virtual bool SoundIsFadingOut(AudioSource source)
		{
			if (_fadeOutSoundCoroutines.TryGetValue(source, out Coroutine co))
			{
				return (_fadeOutSoundCoroutines[source] != null);	
			}

			return false;
		}

		/// <summary>
		/// Stops any fade currently happening on the specified track
		/// </summary>
		/// <param name="track"></param>
		public virtual void StopFadeTrack(MMSoundManagerTracks track)
		{
			Coroutine outCoroutine;
			if (_fadeTrackCoroutines.TryGetValue(track, out outCoroutine))
			{
				StopCoroutine(outCoroutine);
				_fadeTrackCoroutines.Remove(track);
			}
		}

		/// <summary>
		/// Stops any fade currently happening on the specified sound
		/// </summary>
		/// <param name="source"></param>
		public virtual void StopFadeSound(AudioSource source)
		{
			Coroutine outCoroutine;
			if ((source != null) && (_fadeInSoundCoroutines.TryGetValue(source, out outCoroutine)))
			{
				StopCoroutine(outCoroutine);
				_fadeInSoundCoroutines.Remove(source);
			}
			if ((source != null) && (_fadeOutSoundCoroutines.TryGetValue(source, out outCoroutine)))
			{
				StopCoroutine(outCoroutine);
				_fadeOutSoundCoroutines.Remove(source);
			}
		}

		/// <summary>
		/// Fades an entire track over time
		/// </summary>
		/// <param name="track"></param>
		/// <param name="duration"></param>
		/// <param name="initialVolume"></param>
		/// <param name="finalVolume"></param>
		/// <param name="tweenType"></param>
		/// <returns></returns>
		protected virtual IEnumerator FadeTrackCoroutine(MMSoundManagerTracks track, float duration, float initialVolume, float finalVolume, MMTweenType tweenType)
		{
			float startedAt = Time.unscaledTime;
			if (tweenType == null)
			{
				tweenType = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
			}
			while (Time.unscaledTime - startedAt <= duration)
			{
				float elapsedTime = Time.unscaledTime - startedAt;
				float newVolume = MMTween.Tween(elapsedTime, 0f, duration, initialVolume, finalVolume, tweenType);
				settingsSo.SetTrackVolume(track, newVolume);
				yield return null;
			}
			settingsSo.SetTrackVolume(track, finalVolume);
		}

		/// <summary>
		/// Fades an audiosource's volume over time
		/// </summary>
		/// <param name="source"></param>
		/// <param name="duration"></param>
		/// <param name="initialVolume"></param>
		/// <param name="finalVolume"></param>
		/// <param name="tweenType"></param>
		/// <returns></returns>
		protected virtual IEnumerator FadeCoroutine(AudioSource source, float duration, float initialVolume, float finalVolume, MMTweenType tweenType, bool freeAfterFade = false)
		{
			float startedAt = Time.unscaledTime;
			if (tweenType == null)
			{
				tweenType = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
			}
			while (Time.unscaledTime - startedAt <= duration)
			{
				float elapsedTime = Time.unscaledTime - startedAt;
				float newVolume = MMTween.Tween(elapsedTime, 0f, duration, initialVolume, finalVolume, tweenType);
				source.volume = newVolume;
				yield return null;
			}
			source.volume = finalVolume;
			
			if (freeAfterFade)
			{
				FreeSound(source);
			}

			if (initialVolume < finalVolume)
			{
				_fadeInSoundCoroutines[source] = null;	
			}
			else
			{
				_fadeOutSoundCoroutines[source] = null;
			}
		}
        
		#endregion

		#region Solo

		/// <summary>
		/// Mutes all sounds playing on a specific track
		/// </summary>
		/// <param name="track"></param>
		/// <param name="mute"></param>
		/// <param name="delay"></param>
		public virtual void MuteSoundsOnTrack(MMSoundManagerTracks track, bool mute, float delay = 0f)
		{
			StartCoroutine(MuteSoundsOnTrackCoroutine(track, mute, delay));
		}
        
		/// <summary>
		/// Mutes all sounds playing on the MMSoundManager
		/// </summary>
		/// <param name="mute"></param>
		public virtual void MuteAllSounds(bool mute = true)
		{
			StartCoroutine(MuteAllSoundsCoroutine(0f, mute));
		}

		/// <summary>
		/// Mutes all sounds on the specified track after an optional delay
		/// </summary>
		/// <param name="track"></param>
		/// <param name="mute"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		protected virtual IEnumerator MuteSoundsOnTrackCoroutine(MMSoundManagerTracks track, bool mute, float delay)
		{
			if (delay > 0)
			{
				yield return MMCoroutine.WaitForUnscaled(delay);    
			}
            
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Track == track)
				{
					sound.Source.mute = mute;
				}
			}
		}

		/// <summary>
		/// Mutes all sounds after an optional delay
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="mute"></param>
		/// <returns></returns>
		protected  virtual IEnumerator MuteAllSoundsCoroutine(float delay, bool mute = true)
		{
			if (delay > 0)
			{
				yield return MMCoroutine.WaitForUnscaled(delay);    
			}
			foreach (MMSoundManagerSound sound in _sounds)
			{
				sound.Source.mute = mute;
			}   
		}

		#endregion

		#region Find

		/// <summary>
		/// Returns an audio source played with the specified ID, if one is found
		/// </summary>
		/// <param name="ID"></param>
		/// <returns></returns>
		public virtual AudioSource FindByID(int ID)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.ID == ID)
				{
					return sound.Source;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns an audio source played with the specified ID, if one is found
		/// </summary>
		/// <param name="ID"></param>
		/// <returns></returns>
		public virtual AudioSource FindByClip(AudioClip clip)
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Source.clip == clip)
				{
					return sound.Source;
				}
			}

			return null;
		}

		#endregion

		#region AllSoundsControls

		/// <summary>
		/// Pauses all sounds playing on the MMSoundManager
		/// </summary>
		public virtual void PauseAllSounds()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				sound.Source.Pause();
			}    
		}

		/// <summary>
		/// Plays all sounds playing on the MMSoundManager
		/// </summary>
		public virtual void PlayAllSounds()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				sound.Source.Play();
			}    
		}

		/// <summary>
		/// Stops all sounds playing on the MMSoundManager
		/// </summary>
		public virtual void StopAllSounds()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				sound.Source.Stop();
			}
		}

		/// <summary>
		/// Stops all sounds and returns them to the pool
		/// </summary>
		public virtual void FreeAllSounds()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if (sound.Source != null)
				{
					FreeSound(sound.Source);    
				}
			}
		}
        
		/// <summary>
		/// Stops all sounds except the persistent ones, and returns them to the pool
		/// </summary>
		public virtual void FreeAllSoundsButPersistent()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if ((!sound.Persistent) && (sound.Source != null))
				{
					FreeSound(sound.Source);
				}
			}
		}

		/// <summary>
		/// Stops all looping sounds and returns them to the pool
		/// </summary>
		public virtual void FreeAllLoopingSounds()
		{
			foreach (MMSoundManagerSound sound in _sounds)
			{
				if ((sound.Source.loop) && (sound.Source != null))
				{
					FreeSound(sound.Source);
				}
			}
		}

		#endregion

		#region Events
        
		/// <summary>
		/// Registered on enable, triggers every time a new scene is loaded
		/// At which point we free all sounds except the persistent ones
		/// </summary>
		protected virtual void OnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
		{
			FreeAllSoundsButPersistent();
		}

		public virtual void OnMMEvent(MMSoundManagerTrackEvent soundManagerTrackEvent)
		{
			switch (soundManagerTrackEvent.TrackEventType)
			{
				case MMSoundManagerTrackEventTypes.MuteTrack:
					MuteTrack(soundManagerTrackEvent.Track);
					break;
				case MMSoundManagerTrackEventTypes.UnmuteTrack:
					UnmuteTrack(soundManagerTrackEvent.Track);
					break;
				case MMSoundManagerTrackEventTypes.SetVolumeTrack:
					SetTrackVolume(soundManagerTrackEvent.Track, soundManagerTrackEvent.Volume);
					break;
				case MMSoundManagerTrackEventTypes.PlayTrack:
					PlayTrack(soundManagerTrackEvent.Track);
					break;
				case MMSoundManagerTrackEventTypes.PauseTrack:
					PauseTrack(soundManagerTrackEvent.Track);
					break;
				case MMSoundManagerTrackEventTypes.StopTrack:
					StopTrack(soundManagerTrackEvent.Track);
					break;
				case MMSoundManagerTrackEventTypes.FreeTrack:
					FreeTrack(soundManagerTrackEvent.Track);
					break;
			}
		}
        
		public virtual void OnMMEvent(MMSoundManagerEvent soundManagerEvent)
		{
			switch (soundManagerEvent.EventType)
			{
				case MMSoundManagerEventTypes.SaveSettings:
					SaveSettings();
					break;
				case MMSoundManagerEventTypes.LoadSettings:
					settingsSo.LoadSoundSettings();
					break;
				case MMSoundManagerEventTypes.ResetSettings:
					settingsSo.ResetSoundSettings();
					break;
			}
		}

		/// <summary>
		/// Save sound settings to file
		/// </summary>
		public virtual void SaveSettings()
		{
			settingsSo.SaveSoundSettings();
		}

		/// <summary>
		/// Loads sound settings from file
		/// </summary>
		public virtual void LoadSettings()
		{
			settingsSo.LoadSoundSettings();
		}

		/// <summary>
		/// Deletes any saved sound settings
		/// </summary>
		public virtual void ResetSettings()
		{
			settingsSo.ResetSoundSettings();
		}
        
		public virtual void OnMMEvent(MMSoundManagerSoundControlEvent soundControlEvent)
		{
			if (soundControlEvent.TargetSource == null)
			{
				_tempAudioSource = FindByID(soundControlEvent.SoundID);    
			}
			else
			{
				_tempAudioSource = soundControlEvent.TargetSource;
			}

			if (_tempAudioSource != null)
			{
				switch (soundControlEvent.MMSoundManagerSoundControlEventType)
				{
					case MMSoundManagerSoundControlEventTypes.Pause:
						PauseSound(_tempAudioSource);
						break;
					case MMSoundManagerSoundControlEventTypes.Resume:
						ResumeSound(_tempAudioSource);
						break;
					case MMSoundManagerSoundControlEventTypes.Stop:
						StopSound(_tempAudioSource);
						break;
					case MMSoundManagerSoundControlEventTypes.Free:
						FreeSound(_tempAudioSource);
						break;
				}
			}
		}
        
		public virtual void OnMMEvent(MMSoundManagerTrackFadeEvent trackFadeEvent)
		{
			switch (trackFadeEvent.Mode)
			{
				case MMSoundManagerTrackFadeEvent.Modes.PlayFade:
					FadeTrack(trackFadeEvent.Track, trackFadeEvent.FadeDuration, settingsSo.GetTrackVolume(trackFadeEvent.Track), trackFadeEvent.FinalVolume, trackFadeEvent.FadeTween);
					break;
				case MMSoundManagerTrackFadeEvent.Modes.StopFade:
					StopFadeTrack(trackFadeEvent.Track);
					break;
			}
		}
        
		public virtual void OnMMEvent(MMSoundManagerSoundFadeEvent soundFadeEvent)
		{
			_tempAudioSource = FindByID(soundFadeEvent.SoundID);
			switch (soundFadeEvent.Mode)
			{
				case MMSoundManagerSoundFadeEvent.Modes.PlayFade:
					if (_tempAudioSource != null)
					{
						FadeSound(_tempAudioSource, soundFadeEvent.FadeDuration, _tempAudioSource.volume, soundFadeEvent.FinalVolume,
							soundFadeEvent.FadeTween);
					}
					break;
				case MMSoundManagerSoundFadeEvent.Modes.StopFade:
					StopFadeSound(_tempAudioSource);
					break;
			}
		}
        
		public virtual void OnMMEvent(MMSoundManagerAllSoundsControlEvent allSoundsControlEvent)
		{
			switch (allSoundsControlEvent.EventType)
			{
				case MMSoundManagerAllSoundsControlEventTypes.Pause:
					PauseAllSounds();
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Play:
					PlayAllSounds();
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Stop:
					StopAllSounds();
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Free:
					FreeAllSounds();
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent:
					FreeAllSoundsButPersistent();
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping:
					FreeAllLoopingSounds();
					break;
			}
		}

		public virtual void OnMMSfxEvent(AudioClip clipToPlay, AudioMixerGroup audioGroup = null, float volume = 1f, float pitch = 1f, int priority = 128)
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.Location = this.transform.position;
			options.AudioGroup = audioGroup;
			options.Volume = volume;
			options.Pitch = pitch;
			if (priority >= 0)
			{
				options.Priority = Mathf.Min(priority, 256);
			}
			options.MmSoundManagerTrack = MMSoundManagerTracks.Sfx;
			options.Loop = false;
            
			PlaySound(clipToPlay, options);
		}

		public virtual AudioSource OnMMSoundManagerSoundPlayEvent(AudioClip clip, MMSoundManagerPlayOptions options)
		{
			return PlaySound(clip, options);
		}
        
		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMSfxEvent.Register(OnMMSfxEvent);
			MMSoundManagerSoundPlayEvent.Register(OnMMSoundManagerSoundPlayEvent);
			this.MMEventStartListening<MMSoundManagerEvent>();
			this.MMEventStartListening<MMSoundManagerTrackEvent>();
			this.MMEventStartListening<MMSoundManagerSoundControlEvent>();
			this.MMEventStartListening<MMSoundManagerTrackFadeEvent>();
			this.MMEventStartListening<MMSoundManagerSoundFadeEvent>();
			this.MMEventStartListening<MMSoundManagerAllSoundsControlEvent>();
            
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_enabled)
			{
				MMSfxEvent.Unregister(OnMMSfxEvent);
				MMSoundManagerSoundPlayEvent.Unregister(OnMMSoundManagerSoundPlayEvent);
				this.MMEventStopListening<MMSoundManagerEvent>();
				this.MMEventStopListening<MMSoundManagerTrackEvent>();
				this.MMEventStopListening<MMSoundManagerSoundControlEvent>();
				this.MMEventStopListening<MMSoundManagerTrackFadeEvent>();
				this.MMEventStopListening<MMSoundManagerSoundFadeEvent>();
				this.MMEventStopListening<MMSoundManagerAllSoundsControlEvent>();
            
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}
        
		#endregion
	}    
}