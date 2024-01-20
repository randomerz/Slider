using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class stores all the info related to items in a playlist
	/// </summary>

	public struct MMPlaylistPlayEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}
	
	public struct MMPlaylistStopEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}
	
	public struct MMPlaylistPauseEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}
	
	public struct MMPlaylistPlayNextEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}
	
	public struct MMPlaylistPlayPreviousEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}

	public struct MMPlaylistPlayIndexEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel, int index);
		static public void Trigger(int channel, int index)
		{
			OnEvent?.Invoke(channel, index);
		}
	}

	public struct MMPlaylistVolumeMultiplierEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel, float newVolumeMultiplier, bool applyVolumeMultiplierInstantly = false);
		static public void Trigger(int channel, float newVolumeMultiplier, bool applyVolumeMultiplierInstantly = false)
		{
			OnEvent?.Invoke(channel, newVolumeMultiplier, applyVolumeMultiplierInstantly);
		}
	}

	public struct MMPlaylistPitchMultiplierEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel, float newPitchMultiplier, bool applyPitchMultiplierInstantly = false);
		static public void Trigger(int channel, float newPitchMultiplier, bool applyPitchMultiplierInstantly = false)
		{
			OnEvent?.Invoke(channel, newPitchMultiplier, applyPitchMultiplierInstantly);
		}
	}
	
	public struct MMPlaylistChangeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel, MMSMPlaylist newPlaylist, bool andPlay);
		static public void Trigger(int channel, MMSMPlaylist newPlaylist, bool andPlay)
		{
			OnEvent?.Invoke(channel, newPlaylist, andPlay);
		}
	}
	
	public struct MMPlaylistNewSongStartedEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel);
		static public void Trigger(int channel)
		{
			OnEvent?.Invoke(channel);
		}
	}

	[System.Serializable]
	public class MMPlaylistSong
	{
		/// the audiosource that contains the audio clip we want to play
		public AudioSource TargetAudioSource;
		/// the min (when it's off) and max (when it's playing) volume for this source
		[MMVector("Min", "Max")]
		public Vector2 Volume = new Vector2(0f, 1f);
		/// a random delay in seconds to apply, between its RMin and RMax
		[MMVector("RMin", "RMax")]
		public Vector2 InitialDelay = Vector2.zero;
		/// a random crossfade duration (in seconds) to apply when transitioning to this song, between its RMin and RMax
		[MMVector("RMin", "RMax")]
		public Vector2 CrossFadeDuration = new Vector2(2f, 2f);
		/// a random pitch to apply to this song, between its RMin and RMax
		[MMVector("RMin", "RMax")]
		public Vector2 Pitch = Vector2.one;
		/// the stereo pan for this song
		[Range(-1f, 1f)]
		public float StereoPan = 0f;
		/// the spatial blend for this song (0 is 2D, 1 is 3D)
		[Range(0f, 1f)]
		public float SpatialBlend = 0f;
		/// whether this song should loop or not
		public bool Loop = false;
		/// whether this song is playing right now or not
		[MMReadOnly]
		public bool Playing = false;
		/// whether this song is fading right now or not
		[MMReadOnly]
		public bool Fading = false;

		[MMHidden]
		public bool _initialized;

		public virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			
			this.Volume = new Vector2(0f, 1f);
			this.InitialDelay = Vector2.zero;
			this.CrossFadeDuration = new Vector2(2f, 2f);
			this.Pitch = Vector2.one;
			this.StereoPan = 0f;
			this.SpatialBlend = 0f;
			this.Loop = false;
			this._initialized = true;
		}
	}

	/// <summary>
	/// Use this class to play audiosources (usually background music but feel free to use that for anything) in sequence, with optional crossfade between songs
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Audio/MMPlaylist")]
	[MMRequiresConstantRepaint]
	public class MMPlaylist : MMMonoBehaviour
	{
		/// the possible states this playlist can be in
		public enum PlaylistStates
		{
			Idle,
			Playing,
			Paused
		}
		
		[MMInspectorGroup("Playlist Songs", true, 18)]
        
		/// the channel on which to broadcast orders for this playlist
		[Tooltip("the channel on which to broadcast orders for this playlist")]
		public int Channel = 0;
		/// the songs that this playlist will play
		[Tooltip("the songs that this playlist will play")]
		public List<MMPlaylistSong> Songs;

		[MMInspectorGroup("Settings", true, 13)]
		
		/// whether this should play in random order or not
		[Tooltip("whether this should play in random order or not")]
		public bool RandomOrder = false;
		/// if this is true, random seed will be randomized by the system clock
		[Tooltip("if this is true, random seed will be randomized by the system clock")]
		[MMCondition("RandomOrder", true)]
		public bool RandomizeOrderSeed = true;
		/// whether this playlist should play and loop as a whole forever or not
		[Tooltip("whether this playlist should play and loop as a whole forever or not")]
		public bool Endless = true;
		/// whether this playlist should auto play on start or not
		[Tooltip("whether this playlist should auto play on start or not")]
		public bool PlayOnStart = true;
		/// a global volume multiplier to apply when playing a song
		[Tooltip("a global volume multiplier to apply when playing a song")]
		public float VolumeMultiplier = 1f;
		/// if this is true, this playlist will automatically pause/resume OnApplicationPause, useful if you've prevented your game from running in the background
		[Tooltip("if this is true, this playlist will automatically pause/resume OnApplicationPause, useful if you've prevented your game from running in the background")]
		public bool AutoHandleApplicationPause = true;
		
		[MMInspectorGroup("Persistence", true, 32)]
		/// if this is true, this playlist will persist from scene to scene
		[Tooltip("if this is true, this playlist will persist from scene to scene")]
		public bool Persistent = false;
		/// if this is true, this singleton will auto detach if it finds itself parented on awake
		[Tooltip("if this is true, this singleton will auto detach if it finds itself parented on awake")]
		[MMCondition("Persistent", true)]
		public bool AutomaticallyUnparentOnAwake = true;

		[MMInspectorGroup("Status", true, 14)]
		
		/// the current state of the playlist, debug display only
		[Tooltip("the current state of the playlist, debug display only")]
		[MMReadOnly]
		public PlaylistStates DebugCurrentState = PlaylistStates.Idle;
		/// the index we're currently playing
		[Tooltip("the index we're currently playing")]
		[MMReadOnly]
		public int CurrentlyPlayingIndex = -1;
		/// the name of the song that is currently playing
		[Tooltip("the name of the song that is currently playing")]
		[MMReadOnly]
		public string CurrentSongName;
		/// the current state of this playlist
		[MMReadOnly]
		public MMStateMachine<MMPlaylist.PlaylistStates> PlaylistState;

		[MMInspectorGroup("Tests", true, 15)]
		
		/// a play test button
		[MMInspectorButton("Play")]
		public bool PlayButton;
		/// a pause test button
		[MMInspectorButton("Pause")]
		public bool PauseButton;
		/// a stop test button
		[MMInspectorButton("Stop")]
		public bool StopButton;
		/// a next song test button
		[MMInspectorButton("PlayNextSong")]
		public bool NextButton;
		/// the index of the song to play when pressing the PlayTargetSong button
		[Tooltip("the index of the song to play when pressing the PlayTargetSong button")]
		public int TargetSongIndex = 0;
		/// a next song test button
		[MMInspectorButton("PlayTargetSong")]
		public bool TargetSongButton;
		/// a next song test button
		[MMInspectorButton("QueueTargetSong")]
		public bool QueueTargetSongButton;
		/// a next song test button
		[MMInspectorButton("SetLoopTargetSong")]
		public bool SetLoopTargetSongButton;
		/// a next song test button
		[MMInspectorButton("StopLoopTargetSong")]
		public bool StopLoopTargetSongButton;
        
		protected int _songsPlayedSoFar = 0;
		protected int _songsPlayedThisCycle = 0;
		protected Coroutine _coroutine;
		protected bool _shouldResumeOnApplicationPause = false;
		public static bool HasInstance => _instance != null;
		public static MMPlaylist Current => _instance;
		protected static MMPlaylist _instance;
		protected bool _enabled;
		protected int _queuedSong = -1;
		
		/// <summary>
		/// Singleton design pattern
		/// </summary>
		/// <value>The instance.</value>
		public static MMPlaylist Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<MMPlaylist> ();
					if (_instance == null)
					{
						GameObject obj = new GameObject ();
						obj.name = typeof(MMPlaylist).Name + "_AutoCreated";
						_instance = obj.AddComponent<MMPlaylist> ();
					}
				}
				return _instance;
			}
		}
		
		/// <summary>
		/// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
		/// </summary>
		protected virtual void Awake ()
		{
			InitializeSingleton();
		}

		/// <summary>
		/// Initializes the singleton.
		/// </summary>
		protected virtual void InitializeSingleton()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (!Persistent)
			{
				return;
			}

			if (AutomaticallyUnparentOnAwake)
			{
				this.transform.SetParent(null);
			}

			if (_instance == null)
			{
				//If I am the first instance, make me the Singleton
				_instance = this;
				DontDestroyOnLoad (transform.gameObject);
				_enabled = true;
			}
			else
			{
				//If a Singleton already exists and you find
				//another reference in scene, destroy it!
				if(this != _instance)
				{
					Destroy(this.gameObject);
				}
			}
		}
        
		/// <summary>
		/// On Start we initialize our playlist
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we initialize our state machine and start playing if needed
		/// </summary>
		protected virtual void Initialization()
		{
			if (RandomOrder && RandomizeOrderSeed)
			{
				Random.InitState(System.Environment.TickCount);
			}
			_songsPlayedSoFar = 0;
			PlaylistState = new MMStateMachine<MMPlaylist.PlaylistStates>(this.gameObject, true);
			ChangePlaylistState(PlaylistStates.Idle);
			if (Songs.Count == 0)
			{
				return;
			}
			if (PlayOnStart)
			{
				PlayFirstSong();
			}
		}

		protected virtual void ChangePlaylistState(PlaylistStates newState)
		{
			PlaylistState.ChangeState(newState);
			DebugCurrentState = newState;
		}

		/// <summary>
		/// Picks and plays the first song
		/// </summary>
		protected virtual void PlayFirstSong()
		{
			_songsPlayedThisCycle = 0;
			CurrentlyPlayingIndex = -1;
			int newIndex = PickNextIndex();
			_coroutine = StartCoroutine(PlaySong(newIndex));
		}

		/// <summary>
		/// Plays a new song in the playlist, and stops / fades the previous one
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected virtual IEnumerator PlaySong(int index)
		{
			// if we don't have a song, we stop
			if (Songs.Count == 0)
			{
				yield break;
			}

			// if we've played all our songs, we stop
			if (!Endless && (_songsPlayedThisCycle > Songs.Count))
			{
				yield break;
			}

			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
            
			// we stop our current song                        
			if ((PlaylistState.CurrentState == PlaylistStates.Playing) 
			    && (index >= 0 && index < Songs.Count)
			    && !Songs[index].TargetAudioSource.isPlaying)
			{
				StartCoroutine(Fade(CurrentlyPlayingIndex,
					Random.Range(Songs[index].CrossFadeDuration.x, Songs[index].CrossFadeDuration.y),
					Songs[CurrentlyPlayingIndex].Volume.y * VolumeMultiplier,
					Songs[CurrentlyPlayingIndex].Volume.x * VolumeMultiplier,
					true));
			}

			// we stop all other coroutines
			if ((CurrentlyPlayingIndex >= 0) && (Songs.Count > CurrentlyPlayingIndex))
			{
				foreach (MMPlaylistSong song in Songs)
				{
					if (song != Songs[CurrentlyPlayingIndex])
					{
						song.Fading = false;
					}
				}
			}     
            
			if (index < 0 || index >= Songs.Count)
			{
				yield break;
			}

			// initial delay
			yield return MMCoroutine.WaitFor(Random.Range(Songs[index].InitialDelay.x, Songs[index].InitialDelay.y));

			if (Songs[index].TargetAudioSource == null)
			{
				Debug.LogError(this.name + " : the playlist song you're trying to play is null");
				yield break;
			}

			Songs[index].TargetAudioSource.pitch = Random.Range(Songs[index].Pitch.x, Songs[index].Pitch.y);
			Songs[index].TargetAudioSource.panStereo = Songs[index].StereoPan;
			Songs[index].TargetAudioSource.spatialBlend = Songs[index].SpatialBlend;
			Songs[index].TargetAudioSource.loop = Songs[index].Loop;
            
			// fades the new song's volume
			StartCoroutine(Fade(index,
				Random.Range(Songs[index].CrossFadeDuration.x, Songs[index].CrossFadeDuration.y),
				Songs[index].Volume.x * VolumeMultiplier,
				Songs[index].Volume.y * VolumeMultiplier,
				false));	

			// starts the new song
			Songs[index].TargetAudioSource.Play();

			// updates our state
			CurrentSongName = Songs[index].TargetAudioSource.clip.name;
			ChangePlaylistState(PlaylistStates.Playing);
			Songs[index].Playing = true;
			CurrentlyPlayingIndex = index;
			_songsPlayedSoFar++;
			_songsPlayedThisCycle++;

			while (Songs[index].TargetAudioSource.isPlaying || (PlaylistState.CurrentState == PlaylistStates.Paused) || _shouldResumeOnApplicationPause)
			{
				yield return null;
			}

			if (PlaylistState.CurrentState != PlaylistStates.Playing)
			{
				yield break;
			}
            
			if (_songsPlayedSoFar < Songs.Count)
			{
				_coroutine = StartCoroutine(PlaySong(PickNextIndex()));
			}
			else
			{
				if (Endless)
				{
					_coroutine = StartCoroutine(PlaySong(PickNextIndex()));
				}
				else
				{
					ChangePlaylistState(PlaylistStates.Idle);
				}
			}
		}

		/// <summary>
		/// Fades an audiosource in or out, optionnally stopping it at the end
		/// </summary>
		/// <param name="source"></param>
		/// <param name="duration"></param>
		/// <param name="initialVolume"></param>
		/// <param name="endVolume"></param>
		/// <param name="stopAtTheEnd"></param>
		/// <returns></returns>
		protected virtual IEnumerator Fade(int index, float duration, float initialVolume, float endVolume, bool stopAtTheEnd)
		{
			if (index < 0 || index >= Songs.Count)
			{
				yield break;
			}

			float startTimestamp = Time.time;
			float progress = 0f;
			Songs[index].Fading = true;

			while ((Time.time - startTimestamp < duration) && (Songs[index].Fading))
			{
				progress = MMMaths.Remap(Time.time - startTimestamp, 0f, duration, 0f, 1f);
				Songs[index].TargetAudioSource.volume = Mathf.Lerp(initialVolume, endVolume, progress);
				yield return null;
			}

			Songs[index].TargetAudioSource.volume = endVolume;

			if (stopAtTheEnd)
			{
				Songs[index].TargetAudioSource.Stop();
				Songs[index].Playing = false;
				Songs[index].Fading = false;
			}
		}

		/// <summary>
		/// Picks the next song to play
		/// </summary>
		/// <returns></returns>
		protected virtual int PickNextIndex()
		{
			if (Songs.Count == 0)
			{
				return -1;
			}

			if (_queuedSong != -1)
			{
				int newRequestedIndex = _queuedSong;
				_queuedSong = -1;
				return newRequestedIndex;
			}

			int newIndex = CurrentlyPlayingIndex;
			if (RandomOrder)
			{
				while (newIndex == CurrentlyPlayingIndex)
				{
					newIndex = Random.Range(0, Songs.Count);
				}                
			}
			else
			{
				newIndex = (CurrentlyPlayingIndex + 1) % Songs.Count;
			}

			return newIndex;
		}
		/// <summary>
		/// Picks the previous song to play
		/// </summary>
		/// <returns></returns>
		protected virtual int PickPreviousIndex()
		{
			if (Songs.Count == 0)
			{
				return -1;
			}

			int newIndex = CurrentlyPlayingIndex;
			if (RandomOrder)
			{
				while (newIndex == CurrentlyPlayingIndex)
				{
					newIndex = Random.Range(0, Songs.Count);
				}                
			}
			else
			{
				newIndex = (CurrentlyPlayingIndex - 1);
				if (newIndex < 0)
				{
					newIndex = Songs.Count - 1;
				}
			}

			return newIndex;
		}

		/// <summary>
		/// Plays either the first song or resumes playing a paused one
		/// </summary>
		public virtual void Play()
		{
			switch (PlaylistState.CurrentState)
			{
				case PlaylistStates.Idle:
					PlayFirstSong();
					break;

				case PlaylistStates.Paused:
					Songs[CurrentlyPlayingIndex].TargetAudioSource.UnPause();
					ChangePlaylistState(PlaylistStates.Playing);
					break;

				case PlaylistStates.Playing:
					// do nothing
					break;
			}
		}

		public virtual void PlayAtIndex(int songIndex)
		{
			_coroutine = StartCoroutine(PlaySong(songIndex));
		}

		public virtual void QueueSongAtIndex(int songIndex)
		{
			_queuedSong = songIndex;
		}
        
		/// <summary>
		/// Pauses the current song
		/// </summary>
		public virtual void Pause()
		{
			if (PlaylistState.CurrentState != PlaylistStates.Playing)
			{
				return;
			}

			Songs[CurrentlyPlayingIndex].TargetAudioSource.Pause();
			ChangePlaylistState(PlaylistStates.Paused);
		}

		/// <summary>
		/// Stops the playlist
		/// </summary>
		public virtual void Stop()
		{
			if (PlaylistState.CurrentState != PlaylistStates.Playing)
			{
				return;
			} 
	        
			Songs[CurrentlyPlayingIndex].TargetAudioSource.Stop();
			Songs[CurrentlyPlayingIndex].Playing = false;
			Songs[CurrentlyPlayingIndex].Fading = false;
			CurrentlyPlayingIndex = -1;
			ChangePlaylistState(PlaylistStates.Idle);
		}

		/// <summary>
		/// Will change the current track loop status
		/// </summary>
		public virtual void SetLoop(bool loop)
		{
			Songs[CurrentlyPlayingIndex].TargetAudioSource.loop = loop;
		}

		/// <summary>
		/// Plays the next song in the playlist
		/// </summary>
		public virtual void PlayNextSong()
		{
			int newIndex = PickNextIndex();
			_coroutine = StartCoroutine(PlaySong(newIndex));
		}

		/// <summary>
		/// Plays the previous song in the playlist
		/// </summary>
		public virtual void PlayPreviousSong()
		{
			int newIndex = PickPreviousIndex();
			_coroutine = StartCoroutine(PlaySong(newIndex));
		}

		protected virtual void PlayTargetSong()
		{
			int newIndex = Mathf.Clamp(TargetSongIndex, 0, Songs.Count - 1);
			PlayAtIndex(newIndex);
		}

		protected virtual void QueueTargetSong()
		{
			int newIndex = Mathf.Clamp(TargetSongIndex, 0, Songs.Count - 1);
			QueueSongAtIndex(newIndex);
		}

		protected virtual void SetLoopTargetSong()
		{
			SetLoop(true);
		}

		protected virtual void StopLoopTargetSong()
		{
			SetLoop(false);
		}

		protected virtual void OnPlayEvent(int channel)
		{
			if (channel != Channel) { return; }
			Play();
		}

		protected virtual void OnPauseEvent(int channel)
		{
			if (channel != Channel) { return; }
			Pause();
		}

		protected virtual void OnStopEvent(int channel)
		{
			if (channel != Channel) { return; }
			Stop();
		}

		protected virtual void OnPlayNextEvent(int channel)
		{
			if (channel != Channel) { return; }
			PlayNextSong();
		}

		protected virtual void OnPlayPreviousEvent(int channel)
		{
			if (channel != Channel) { return; }
			PlayPreviousSong();
		}

		protected virtual void OnPlayIndexEvent(int channel, int index)
		{
			if (channel != Channel) { return; }
			_coroutine = StartCoroutine(PlaySong(index));
		}

		protected virtual void OnMMPlaylistVolumeMultiplierEvent(int channel, float newVolumeMultiplier, bool applyVolumeMultiplierInstantly = false)
		{
			if (channel != Channel) { return; }
			VolumeMultiplier = newVolumeMultiplier;
			if (applyVolumeMultiplierInstantly)
			{
				Songs[CurrentlyPlayingIndex].TargetAudioSource.volume = Songs[CurrentlyPlayingIndex].Volume.y * VolumeMultiplier; 
			}
		}

		/// <summary>
		/// On enable, starts listening for playlist events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMPlaylistPauseEvent.Register(OnPauseEvent);
			MMPlaylistPlayEvent.Register(OnPlayEvent);
			MMPlaylistPlayNextEvent.Register(OnPlayNextEvent);
			MMPlaylistPlayPreviousEvent.Register(OnPlayPreviousEvent);
			MMPlaylistStopEvent.Register(OnStopEvent);
			MMPlaylistPlayIndexEvent.Register(OnPlayIndexEvent);
			MMPlaylistVolumeMultiplierEvent.Register(OnMMPlaylistVolumeMultiplierEvent);
		}

		/// <summary>
		/// On disable, stops listening for playlist events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMPlaylistPauseEvent.Unregister(OnPauseEvent);
			MMPlaylistPlayEvent.Unregister(OnPlayEvent);
			MMPlaylistPlayNextEvent.Unregister(OnPlayNextEvent);
			MMPlaylistPlayPreviousEvent.Unregister(OnPlayPreviousEvent);
			MMPlaylistStopEvent.Unregister(OnStopEvent);
			MMPlaylistPlayIndexEvent.Unregister(OnPlayIndexEvent);
			MMPlaylistVolumeMultiplierEvent.Unregister(OnMMPlaylistVolumeMultiplierEvent);
		}
        
		protected bool _firstDeserialization = true;
		protected int _listCount = 0;

		/// <summary>
		/// On Validate, we check if our array has changed and if yes we initialize our new elements
		/// </summary>
		protected virtual void OnValidate()
		{
			if (_firstDeserialization)
			{
				if (Songs == null)
				{
					_listCount = 0;
					_firstDeserialization = false;
				}
				else
				{
					_listCount = Songs.Count;
					_firstDeserialization = false;
				}                
			}
			else
			{
				if (Songs.Count != _listCount)
				{
					if (Songs.Count > _listCount)
					{
						foreach(MMPlaylistSong song in Songs)
						{
							song.Initialization();
						}                            
					}
					_listCount = Songs.Count;
				}
			}
		}

		/// <summary>
		/// On ApplicationPause, we pause the playlist and resume it afterwards
		/// </summary>
		/// <param name="pauseStatus"></param>
		protected virtual void OnApplicationPause(bool pauseStatus)
		{
			if (!AutoHandleApplicationPause)
			{
				return;
			}
			
			if (pauseStatus && PlaylistState.CurrentState == PlaylistStates.Playing)
			{
				Pause();
				_shouldResumeOnApplicationPause = true;
			}

			if (!pauseStatus && _shouldResumeOnApplicationPause)
			{
				_shouldResumeOnApplicationPause = false;
				Play();
			}
		}
	}
}