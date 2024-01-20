using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
	public class MMSMPlaylistManager : MMMonoBehaviour
	{
		/// the possible states this playlist can be in
		public enum PlaylistManagerStates
		{
			Idle,
			Playing,
			Paused
		}
		
		[MMInspectorGroup("Settings", true, 18)]
		/// the channel used to target this playlist manager by playlist remote or playlist feedbacks
		[Tooltip("the channel used to target this playlist manager by playlist remote or playlist feedbacks")]
		public int Channel = 0;
		/// the current playlist this manager will play
		[Tooltip("the current playlist this manager will play")]
		public MMSMPlaylist Playlist;
		/// whether this playlist manager should auto play on start or not
		[Tooltip("whether this playlist manager should auto play on start or not")]
		public bool PlayOnStart = false; 
		/// a global volume multiplier to apply when playing a song
		[Tooltip("a global volume multiplier to apply when playing a song")]
		[Range(0f,1f)]
		public float VolumeMultiplier = 1f;
		/// a pitch multiplier to apply to all songs when playing them
		[Tooltip("a pitch multiplier to apply to all songs when playing them")]
		[Range(0f,20f)]
		public float PitchMultiplier = 1f;
		/// if this is true, this playlist manager will persist from scene to scene and will keep playing
		[Tooltip("if this is true, this playlist manager will persist from scene to scene and will keep playing")]
		public bool Persistent = false;
		/// if this is true, this singleton will auto detach if it finds itself parented on awake
		[Tooltip("if this is true, this singleton will auto detach if it finds itself parented on awake")]
		[MMCondition("Persistent", true)]
		public bool AutomaticallyUnparentOnAwake = true;
		/// if this is true, this playlist will automatically pause/resume OnApplicationPause, useful if you've prevented your game from running in the background
		[Tooltip("if this is true, this playlist will automatically pause/resume OnApplicationPause, useful if you've prevented your game from running in the background")]
		public bool AutoHandleApplicationPause = true;

		[MMInspectorGroup("Fade", true, 12)] 
		/// whether or not sounds should fade in when they start playing
		[Tooltip("whether or not sounds should fade in when they start playing")]
		public bool FadeIn;
		/// whether or not sounds should fade out when they stop playing
		[Tooltip("whether or not sounds should fade out when they stop playing")]
		public bool FadeOut;
		/// the duration of the fade, in seconds
		[Tooltip("the duration of the fade, in seconds")]
		public float FadeDuration = 1f;
		/// the tween to use when fading the sound
		[Tooltip("the tween to use when fading the sound")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);

		[MMInspectorGroup("Time", true, 20)] 
		/// whether or not the playlist manager should have its pitch multiplier value driven by the current timescale. If set to true, songs would appear to slow down when time is slowed down, and to speed up when time scale is higher than normal
		[Tooltip("whether or not the playlist manager should have its pitch multiplier value driven by the current timescale. If set to true, songs would appear to slow down when time is slowed down, and to speed up when time scale is higher than normal")]
		public bool BindPitchToTimeScale = false;
		/// the values to remap timescale from (min and max) - when timescale is equal to TimescaleRemapFrom.x, the pitch multiplier will be TimescaleRemapTo.x
		[Tooltip("the values to remap timescale from (min and max) - when timescale is equal to TimescaleRemapFrom.x, the pitch multiplier will be TimescaleRemapTo.x")]
		[MMCondition("BindPitchToTimeScale", true)]
		public Vector2 TimescaleRemapFrom = new Vector2(0f,2f);
		/// the values to remap timescale to (min and max) - when timescale is equal to TimescaleRemapFrom.x, the pitch multiplier will be TimescaleRemapTo.x
		[Tooltip("the values to remap timescale to (min and max) - when timescale is equal to TimescaleRemapFrom.x, the pitch multiplier will be TimescaleRemapTo.x")]
		[MMCondition("BindPitchToTimeScale", true)]
		public Vector2 TimescaleRemapTo = new Vector2(0.8f,1.2f);

		[MMInspectorGroup("Status", true, 14)]
		/// the current state of the playlist, debug display only
		[Tooltip("the current state of the playlist, debug display only")]
		[MMReadOnly]
		public PlaylistManagerStates DebugCurrentManagerState = PlaylistManagerStates.Idle;
		/// the index we're currently playing
		[Tooltip("the index we're currently playing")]
		[MMReadOnly]
		public int CurrentSongIndex = -1;
		/// the name of the song that is currently playing
		[Tooltip("the name of the song that is currently playing")]
		[MMReadOnly]
		public string CurrentSongName;
		/// the current state of this playlist
		[MMReadOnly]
		public MMStateMachine<PlaylistManagerStates> PlaylistManagerState;
		
		/// the time of the currently playing song
		[Tooltip("the time of the currently playing song")]
		[MMReadOnly] 
		public float CurrentTime;
		/// the time (in seconds) left on the song currently playing 
		[Tooltip("the time (in seconds) left on the song currently playing")]
		[MMReadOnly] 
		public float CurrentTimeLeft;
		/// the total duration of the song currently playing
		[Tooltip("the total duration of the song currently playing")]
		[MMReadOnly] 
		public float CurrentClipDuration;
		/// the current normalized progress of the song currently playing
		[Tooltip("the current normalized progress of the song currently playing")]
		[Range(0f, 1f)]
		public float CurrentProgress = 0;

		[MMInspectorGroup("Test Controls", true, 15)]
		/// a play test button
		[MMInspectorButton("Play")]
		public bool PlayButton;
		/// a stop test button
		[MMInspectorButton("Stop")]
		public bool StopButton;
		/// a pause test button
		[MMInspectorButton("Pause")]
		public bool PauseButton;
		/// a next song test button
		[MMInspectorButton("PlayPreviousSong")]
		public bool PreviousButton;
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
		[MMInspectorButton("SetCurrentSongToLoop")]
		public bool SetLoopTargetSongButton;
		/// a next song test button
		[MMInspectorButton("StopCurrentSongFromLooping")]
		public bool StopLoopTargetSongButton;
		/// a playlist you can set to use with the SetTargetPlaylist and PlayTargetPlaylist buttons
		[Tooltip("a playlist you can set to use with the SetTargetPlaylist and PlayTargetPlaylist buttons")]
		public MMSMPlaylist TestPlaylist;
		/// a test button used to set a new playlist 
		[MMInspectorButton("SetTargetPlaylist")]
		public bool SetTargetPlaylistButton;
		/// a test button used to play the target playlist 
		[MMInspectorButton("PlayTargetPlaylist")]
		public bool PlayTargetPlaylistButton;
		/// a test button used to reset the play count
		[MMInspectorButton("ResetPlayCount")]
		public bool ResetPlayCountButton;

		/// a slider used to test volume control
		[Tooltip("a slider used to test volume control")]
		[Range(0f,2f)]
		public float TestVolumeControl = 1f;
		/// a slider used to test speed control
		[Tooltip("a slider used to test speed control")]
		[Range(0f,20f)]
		public float TestPlaybackSpeedControl = 1f;
		
		/// whether or not this playlist manager is currently playing
		public bool IsPlaying => (_currentlyPlayingAudioSource != null && _currentlyPlayingAudioSource.isPlaying);
		
		/// a delegate used to trigger events along the lifecycle of the playlist manager
		public delegate void PlaylistEvent();
		/// an event that gets triggered when a song starts
		public PlaylistEvent OnSongStart;
		/// an event that gets triggered when a song ends
		public PlaylistEvent OnSongEnd;
		/// an event that gets triggered when the playlist gets paused
		public PlaylistEvent OnPause;
		/// an event that gets triggered when the playlist gets stopped
		public PlaylistEvent OnStop;
		/// an event that gets triggered when the playlist gets changed for another one
		public PlaylistEvent OnPlaylistChange;
		/// an event that gets triggered when a playlist ends
		public PlaylistEvent OnPlaylistEnd;

		protected bool _shouldResumeOnApplicationPause = false;
		public static bool HasInstance => _instance != null;
		public static MMSMPlaylistManager Current => _instance;
		protected static MMSMPlaylistManager _instance;
		protected int _queuedSongIndex = -1;
		protected AudioSource _currentlyPlayingAudioSource;
		protected MMSoundManagerPlayOptions _options;
		protected float _lastTestVolumeControl = 1f;
		protected float _lastTestPlaybackSpeedControl = 1f;
		internal bool _listeningToEvents = false;

		#region INITIALIZATION

			/// <summary>
			/// Singleton design pattern
			/// </summary>
			/// <value>The instance.</value>
			public static MMSMPlaylistManager Instance
			{
				get
				{
					if (_instance == null)
					{
						_instance = FindObjectOfType<MMSMPlaylistManager> ();
						if (_instance == null)
						{
							GameObject obj = new GameObject ();
							obj.name = typeof(MMPlaylist).Name + "_AutoCreated";
							_instance = obj.AddComponent<MMSMPlaylistManager> ();
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
				
				if (PlayOnStart)
				{
					PlayFirstSong();
				}
				
				if (!_listeningToEvents)
				{
					StartListening();
				}
			}

			/// <summary>
			/// On init we initialize our state machine and start playing if needed
			/// </summary>
			protected virtual void Initialization()
			{
				InitializeRandomSeed();
				Playlist.Initialization();
				InitializePlaylistManagerState();
			}

			/// <summary>
			/// Initializes the random seed if needed
			/// </summary>
			protected virtual void InitializeRandomSeed()
			{
				if (
					((Playlist.PlayOrder == MMSMPlaylist.PlayOrders.Random) || (Playlist.PlayOrder == MMSMPlaylist.PlayOrders.RandomUnique)) 
					&& Playlist.RandomizeOrderSeed
				)
				{
					Random.InitState(System.Environment.TickCount);
				}
			}

			/// <summary>
			/// Inits the state machine
			/// </summary>
			protected virtual void InitializePlaylistManagerState()
			{
				PlaylistManagerState = new MMStateMachine<PlaylistManagerStates>(this.gameObject, true);
				ChangePlaylistManagerState(PlaylistManagerStates.Idle);
			}

			/// <summary>
			/// a method used to update the state machine
			/// </summary>
			/// <param name="newManagerState"></param>
			protected virtual void ChangePlaylistManagerState(PlaylistManagerStates newManagerState)
			{
				PlaylistManagerState.ChangeState(newManagerState);
				#if UNITY_EDITOR
				DebugCurrentManagerState = newManagerState;
				#endif
			}

		#endregion

		#region LIFECYCLE

			/// <summary>
			/// on update, self disables if needed, 
			/// </summary>
			protected virtual void Update()
			{
				if (PlaylistManagerState.CurrentState == PlaylistManagerStates.Idle)
				{
					this.enabled = false;
					return;
				}
				UpdateTimeAndProgress();
				HandleTimescale();
				HandleEndOfSong();
			}

			/// <summary>
			/// On update, we update our pitch multiplier to match our timescale if necessary
			/// </summary>
			protected virtual void HandleTimescale()
			{
				if (BindPitchToTimeScale)
				{
					float remappedTimescale = MMMaths.Remap(Time.timeScale, TimescaleRemapFrom.x, TimescaleRemapFrom.y,
						TimescaleRemapTo.x, TimescaleRemapTo.y);
					SetPitchMultiplier(remappedTimescale);
				}
			}

			/// <summary>
			/// Updates the various time counters
			/// </summary>
			protected virtual void UpdateTimeAndProgress()
			{
				CurrentTime = _currentlyPlayingAudioSource.time;
				CurrentTimeLeft = _currentlyPlayingAudioSource.clip.length - _currentlyPlayingAudioSource.time;
				CurrentProgress = CurrentTime / CurrentClipDuration;
			}
			
			/// <summary>
			/// Picks and plays the first song
			/// </summary>
			protected virtual void PlayFirstSong()
			{
				Playlist.Initialization();
				CurrentSongIndex = -1;
				HandleNextSong(1);
			}

			/// <summary>
			/// Detects end of song and moves on to the next one
			/// </summary>
			protected virtual void HandleEndOfSong()
			{
				if (PlaylistManagerState.CurrentState != PlaylistManagerStates.Playing)
				{
					return;
				}

				if (_currentlyPlayingAudioSource.isPlaying)
				{
					if (FadeIn && FadeOut && (CurrentTimeLeft < FadeDuration))
					{
						HandleNextSong(1);
					}
					return;
				}

				HandleNextSong(1);
			}

			/// <summary>
			/// Determines the next song to play and triggers the play
			/// </summary>
			/// <param name="direction"></param>
			protected virtual void HandleNextSong(int direction)
			{
				if (IsPlaying)
				{
					OnSongEnd?.Invoke();	
				}

				int newIndex = Playlist.PickNextIndex(direction, CurrentSongIndex, ref _queuedSongIndex);
				
				if (newIndex == -1)
				{
					ChangePlaylistManagerState(PlaylistManagerStates.Idle);
				}
				
				if (newIndex == -2)
				{
					HandleEndOfPlaylist();
					return;
				}
				
				if (newIndex >= 0 && newIndex < Playlist.Songs.Count)
				{
					PlaySongAt(newIndex);
				}
			}

			/// <summary>
			/// Handles the end of playlist, triggers a new one if needed
			/// </summary>
			protected virtual void HandleEndOfPlaylist()
			{
				OnPlaylistEnd?.Invoke();
				
				if (Playlist.NextPlaylist != null)
				{
					ChangePlaylistAndPlay(Playlist.NextPlaylist);
					return;
				}
				ChangePlaylistManagerState(PlaylistManagerStates.Idle);
			}

		#endregion
		
		
		#region CONTROLS
		
			/// <summary>
			/// Use this method to either play the first song of the playlist, or resume after a pause
			/// </summary>
			public virtual void Play()
			{
				switch (PlaylistManagerState.CurrentState)
				{
					case PlaylistManagerStates.Idle:
						PlayFirstSong();
						break;

					case PlaylistManagerStates.Paused:
						MMSoundManager.Instance.ResumeSound(_currentlyPlayingAudioSource);
						ChangePlaylistManagerState(PlaylistManagerStates.Playing);
						break;

					case PlaylistManagerStates.Playing:
						// do nothing
						break;
				}
			}

			/// <summary>
			/// Plays the song at the specified index
			/// </summary>
			/// <param name="index"></param>
			/// <returns></returns>
			public virtual void PlaySongAt(int songIndex)
			{
				this.enabled = true;
			
				// if we don't have a song, we stop
				if (Playlist.Songs.Count == 0)
				{
					return;
				}

				Stop();

				// starts the new song
				_options = Playlist.Songs[songIndex].Options; 
				_options.MmSoundManagerTrack = Playlist.Track;
				_options.Volume *= VolumeMultiplier;
				_options.Pitch *= PitchMultiplier;
				_options.Persistent = Persistent;
			
				_currentlyPlayingAudioSource = MMSoundManagerSoundPlayEvent.Trigger(Playlist.Songs[songIndex].Clip, _options);
				
				OnSongStart?.Invoke();

				if (FadeIn)
				{
					MMSoundManager.Instance.FadeSound(_currentlyPlayingAudioSource, FadeDuration,
						0f, _currentlyPlayingAudioSource.volume, FadeTween, false);	
				}

				// updates our state
				ChangePlaylistManagerState(PlaylistManagerStates.Playing);
			
				CurrentSongIndex = songIndex;
				CurrentClipDuration = _currentlyPlayingAudioSource.clip.length;
				CurrentSongName = Playlist.Songs[songIndex].Name;
				Playlist.Songs[songIndex].PlayCount++;
				Playlist.PlayCount++;
				
				// we trigger an event for other classes to listen to, letting them know a new song has started
				MMPlaylistNewSongStartedEvent.Trigger(Channel);
			}
			
			/// <summary>
			/// Pauses the current song
			/// </summary>
			public virtual void Pause()
			{
				if (PlaylistManagerState.CurrentState != PlaylistManagerStates.Playing)
				{
					return;
				}

				MMSoundManager.Instance.PauseSound(_currentlyPlayingAudioSource);
				ChangePlaylistManagerState(PlaylistManagerStates.Paused);
				
				OnPause?.Invoke();
			}

			/// <summary>
			/// Stops the song currently playing 
			/// </summary>
			public virtual void Stop() 
			{
				// we stop our current song              
				if ((_currentlyPlayingAudioSource == null) || !_currentlyPlayingAudioSource.isPlaying)
				{
					return;	
				}

				if (FadeOut)
				{
					if ( MMSoundManager.Instance.SoundIsFadingOut(_currentlyPlayingAudioSource) )
					{
						return;
					}
				
					MMSoundManager.Instance.FadeSound(_currentlyPlayingAudioSource, FadeDuration,
						_currentlyPlayingAudioSource.volume, 0f, FadeTween, true);
				}
				else
				{
					MMSoundManager.Instance.FreeSound(_currentlyPlayingAudioSource);
				}
				ChangePlaylistManagerState(PlaylistManagerStates.Idle);
				
				OnStop?.Invoke();
			}

			/// <summary>
			/// Stops the current song, lets you specify whether or not to fade it out
			/// </summary>
			public virtual void StopWithFade(bool withFade = true)
			{
				if (PlaylistManagerState.CurrentState == PlaylistManagerStates.Idle)
				{
					return;
				}
				
				if (!withFade)
				{
					MMSoundManager.Instance.FreeSound(_currentlyPlayingAudioSource);
					OnStop?.Invoke();
				}
				else
				{
					Stop();
				}
				
				CurrentSongIndex = -1;
				ChangePlaylistManagerState(PlaylistManagerStates.Idle);
			}

			/// <summary>
			/// Will change the current song's loop status
			/// </summary>
			public virtual void SetCurrentSongLoop(bool loop)
			{
				_currentlyPlayingAudioSource.loop = loop;
			}

			/// <summary>
			/// Plays the next song in the playlist
			/// </summary>
			public virtual void PlayNextSong()
			{
				Stop();
				HandleNextSong(1);
			}

			/// <summary>
			/// Plays the previous song in the playlist
			/// </summary>
			public virtual void PlayPreviousSong()
			{
				Stop();
				HandleNextSong(-1);
			}

			/// <summary>
			/// Queues the song at the specified index to play once the currently playing song finishes
			/// </summary>
			/// <param name="songIndex"></param>
			public virtual void QueueSongAtIndex(int songIndex)
			{
				_queuedSongIndex = songIndex;
			}

			/// <summary>
			/// Changes the playlist for the specified one, doesn't play a song there, it'll play once the song currently playing ends
			/// </summary>
			/// <param name="newPlaylist"></param>
			public virtual void ChangePlaylist(MMSMPlaylist newPlaylist) 
			{
				Playlist = newPlaylist;
				Playlist.Initialization();
				CurrentSongIndex = -1;
				OnPlaylistChange?.Invoke();
			}

			/// <summary>
			/// Changes the playlist for the specified one, and plays its first song 
			/// </summary>
			/// <param name="newPlaylist"></param>
			public virtual void ChangePlaylistAndPlay(MMSMPlaylist newPlaylist)
			{
				ChangePlaylist(newPlaylist);
				PlaySongAt(0);
			}

			/// <summary>
			/// resets all play counts (playlist and songs)
			/// </summary>
			public virtual void ResetPlayCount()
			{
				Playlist.ResetPlayCount();
			}
			
			/// <summary>
			/// Sets a new volume multiplier
			/// </summary>
			/// <param name="newVolumeMultiplier"></param>
			public virtual void SetVolumeMultiplier(float newVolumeMultiplier)
			{
				float newVolume = Mathf.Clamp(newVolumeMultiplier, 0f, 2f);
				MMPlaylistVolumeMultiplierEvent.Trigger(Channel, newVolume, true);
			}

			/// <summary>
			/// Sets a new pitch multiplier
			/// </summary>
			/// <param name="newPitchMultiplier"></param>
			public virtual void SetPitchMultiplier(float newPitchMultiplier)
			{
				float newPitch = Mathf.Clamp(newPitchMultiplier, 0f, 20f);
				MMPlaylistPitchMultiplierEvent.Trigger(Channel, newPitch, true);
			}
		
		#endregion

		#region DEBUG_METHODS

			/// <summary>
			/// a debug method used by the inspector to set the target playlist
			/// </summary>
			protected virtual void SetTargetPlaylist()
			{
				ChangePlaylist(TestPlaylist);
			}
			
			/// <summary>
			/// a debug method used by the inspector to play the target playlist
			/// </summary>
			protected virtual void PlayTargetPlaylist()
			{
				ChangePlaylistAndPlay(TestPlaylist);
			}

			/// <summary>
			/// a debug method used by the inspector to queue the specified song
			/// </summary>
			protected virtual void QueueTargetSong()
			{
				int newIndex = Mathf.Clamp(TargetSongIndex, 0, Playlist.Songs.Count - 1);
				QueueSongAtIndex(newIndex);
			}

			/// <summary>
			/// a debug method used by the inspector to play the specified song
			/// </summary>
			protected virtual void PlayTargetSong()
			{
				int newIndex = Mathf.Clamp(TargetSongIndex, 0, Playlist.Songs.Count - 1);
				PlaySongAt(newIndex);
			}
			
			/// <summary>
			/// a test method used by the inspector debug button to force the current song from looping
			/// </summary>
			protected virtual void SetCurrentSongToLoop()
			{
				SetCurrentSongLoop(true);
			}

			/// <summary>
			/// a test method used by the inspector debug button to prevent the current song from looping
			/// </summary>
			protected virtual void StopCurrentSongFromLooping()
			{
				SetCurrentSongLoop(false);
			}

		#endregion
		
		#region EVENTS
		
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
				PlaySongAt(index);
			}

			protected virtual void OnMMPlaylistVolumeMultiplierEvent(int channel, float newVolumeMultiplier, bool applyVolumeMultiplierInstantly = false)
			{
				if (channel != Channel) { return; }
				VolumeMultiplier = newVolumeMultiplier;
				if (applyVolumeMultiplierInstantly)
				{
					_currentlyPlayingAudioSource.volume = Playlist.Songs[CurrentSongIndex].Options.Volume * VolumeMultiplier; 
				}
			}

			protected virtual void OnMMPlaylistPitchMultiplierEvent(int channel, float newPitchMultiplier, bool applyPitchMultiplierInstantly = false)
			{
				if (channel != Channel) { return; }
				PitchMultiplier = newPitchMultiplier;
				if (applyPitchMultiplierInstantly)
				{
					_currentlyPlayingAudioSource.pitch = Playlist.Songs[CurrentSongIndex].Options.Pitch * PitchMultiplier; 
				}
			}

			protected virtual void OnMMPlaylistChangeEvent(int channel, MMSMPlaylist newPlaylist, bool andPlay)
			{
				if (channel != Channel) { return; }
				if (andPlay)
				{
					ChangePlaylistAndPlay(newPlaylist);
				}
				else
				{
					ChangePlaylist(newPlaylist);
				}
			}
			
			/// <summary>
			/// Starts listening for events
			/// </summary>
			public virtual void StartListening()
			{
				_listeningToEvents = true;
				MMPlaylistPauseEvent.Register(OnPauseEvent);
				MMPlaylistPlayEvent.Register(OnPlayEvent);
				MMPlaylistPlayNextEvent.Register(OnPlayNextEvent);
				MMPlaylistPlayPreviousEvent.Register(OnPlayPreviousEvent);
				MMPlaylistStopEvent.Register(OnStopEvent);
				MMPlaylistPlayIndexEvent.Register(OnPlayIndexEvent);
				MMPlaylistVolumeMultiplierEvent.Register(OnMMPlaylistVolumeMultiplierEvent);
				MMPlaylistPitchMultiplierEvent.Register(OnMMPlaylistPitchMultiplierEvent);
				MMPlaylistChangeEvent.Register(OnMMPlaylistChangeEvent);
			}

			/// <summary>
			/// Stops listening for events
			/// </summary>
			public virtual void StopListening()
			{
				_listeningToEvents = false;
				MMPlaylistPauseEvent.Unregister(OnPauseEvent);
				MMPlaylistPlayEvent.Unregister(OnPlayEvent);
				MMPlaylistPlayNextEvent.Unregister(OnPlayNextEvent);
				MMPlaylistPlayPreviousEvent.Unregister(OnPlayPreviousEvent);
				MMPlaylistStopEvent.Unregister(OnStopEvent);
				MMPlaylistPlayIndexEvent.Unregister(OnPlayIndexEvent);
				MMPlaylistVolumeMultiplierEvent.Unregister(OnMMPlaylistVolumeMultiplierEvent);
				MMPlaylistPitchMultiplierEvent.Unregister(OnMMPlaylistPitchMultiplierEvent);
				MMPlaylistChangeEvent.Unregister(OnMMPlaylistChangeEvent);
			}

			/// <summary>
			/// on destroy we stop listening for events
			/// </summary>
			protected virtual void OnDestroy()
			{
				StopListening();
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
				
				if (pauseStatus && PlaylistManagerState.CurrentState == PlaylistManagerStates.Playing)
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
			
			#if UNITY_EDITOR
			protected virtual void OnValidate()
			{
				if (_lastTestVolumeControl != TestVolumeControl)
				{
					MMPlaylistVolumeMultiplierEvent.Trigger(Channel, TestVolumeControl, true);
				}
				if (_lastTestPlaybackSpeedControl != TestPlaybackSpeedControl)
				{
					MMPlaylistPitchMultiplierEvent.Trigger(Channel, TestPlaybackSpeedControl, true);
				}
			}
			#endif

			#endregion
	}
}
