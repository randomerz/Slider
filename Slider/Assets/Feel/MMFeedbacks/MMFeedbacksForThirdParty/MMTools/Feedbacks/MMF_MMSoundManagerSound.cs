using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{    
	/// <summary>
	/// This feedback will let you play a sound via the MMSoundManager. You will need a game object in your scene with a MMSoundManager object on it for this to work.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Sound")]
	[FeedbackHelp("This feedback will let you play a sound via the MMSoundManager. You will need a game object in your scene with a MMSoundManager object on it for this to work.")]
	public class MMF_MMSoundManagerSound : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override bool EvaluateRequiresSetup()
		{
			bool requiresSetup = false;
			if (Sfx == null)
			{
				requiresSetup = true;
			}
			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				requiresSetup = false;
				foreach (AudioClip clip in RandomSfx)
				{
					if (clip == null)
					{
						requiresSetup = true;
					}
				}    
			}
			if (SoundDataSO != null)
			{
				requiresSetup = false;
			}
			return requiresSetup;
		}
		public override string RequiredTargetText { get { return Sfx != null ? Sfx.name + " - ID:" + ID : "";  } }

		public override string RequiresSetupText { get { return "This feedback requires that you set an Audio clip in its Sfx slot below, or one or more clips in the Random Sfx array."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif

		/// the duration of this feedback is the duration of the clip being played
		public override float FeedbackDuration { get { return GetDuration(); } }
		public override bool HasRandomness => true;
        
		[MMFInspectorGroup("Sound", true, 14, true)]
		/// the sound clip to play
		[Tooltip("the sound clip to play")]
		public AudioClip Sfx;

		[MMFInspectorGroup("Random Sound", true, 34, true)]
        
		/// an array to pick a random sfx from
		[Tooltip("an array to pick a random sfx from")]
		public AudioClip[] RandomSfx;
		/// if this is true, random sfx audio clips will be played in sequential order instead of at random
		[Tooltip("if this is true, random sfx audio clips will be played in sequential order instead of at random")]
		public bool SequentialOrder = false;
		/// if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called
		[Tooltip("if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called")]
		[MMFCondition("SequentialOrder", true)]
		public bool SequentialOrderHoldLast = false;
		/// if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored
		[Tooltip("if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored")]
		[MMFCondition("SequentialOrderHoldLast", true)]
		public float SequentialOrderHoldCooldownDuration = 2f;
		/// if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over
		[Tooltip("if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over")]
		public bool RandomUnique = false;
		
		[MMFInspectorGroup("Scriptable Object", true, 14, true)]
		/// a scriptable object (created via the Create/MoreMountains/Audio/MMF_SoundData menu) to define settings that will override all other settings on this feedback
		[Tooltip("a scriptable object (created via the Create/MoreMountains/Audio/MMF_SoundData menu) to define settings that will override all other settings on this feedback")]
		public MMF_MMSoundManagerSoundData SoundDataSO;

		[MMFInspectorGroup("Sound Properties", true, 24)]
		[Header("Volume")]
		/// the minimum volume to play the sound at
		[Tooltip("the minimum volume to play the sound at")]
		[Range(0f,2f)]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("the maximum volume to play the sound at")]
		[Range(0f,2f)]
		public float MaxVolume = 1f;

		[Header("Pitch")]
		/// the minimum pitch to play the sound at
		[Tooltip("the minimum pitch to play the sound at")]
		[Range(-3f,3f)]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("the maximum pitch to play the sound at")]
		[Range(-3f,3f)]
		public float MaxPitch = 1f;
		
		[MMFInspectorGroup("SoundManager Options", true, 28)]
		/// the track on which to play the sound. Pick the one that matches the nature of your sound
		[Tooltip("the track on which to play the sound. Pick the one that matches the nature of your sound")]
		public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
		/// the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards. 
		[Tooltip("the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards.")]
		public int ID = 0;
		/// the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.
		[Tooltip("the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.")]
		public AudioMixerGroup AudioGroup = null;
		/// if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here 
		[Tooltip("if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here")]
		public AudioSource RecycleAudioSource = null;
		/// whether or not this sound should loop
		[Tooltip("whether or not this sound should loop")]
		public bool Loop = false;
		/// whether or not this sound should continue playing when transitioning to another scene
		[Tooltip("whether or not this sound should continue playing when transitioning to another scene")]
		public bool Persistent = false;
		/// whether or not this sound should play if the same sound clip is already playing
		[Tooltip("whether or not this sound should play if the same sound clip is already playing")]
		public bool DoNotPlayIfClipAlreadyPlaying = false;
		/// if this is true, this sound will stop playing when stopping the feedback
		[Tooltip("if this is true, this sound will stop playing when stopping the feedback")]
		public bool StopSoundOnFeedbackStop = false;
        
		[MMFInspectorGroup("Fade", true, 30)]
		/// whether or not to fade this sound in when playing it
		[Tooltip("whether or not to fade this sound in when playing it")]
		public bool Fade = false;
		/// if fading, the volume at which to start the fade
		[Tooltip("if fading, the volume at which to start the fade")]
		[MMCondition("Fade", true)]
		public float FadeInitialVolume = 0f;
		/// if fading, the duration of the fade, in seconds
		[Tooltip("if fading, the duration of the fade, in seconds")]
		[MMCondition("Fade", true)]
		public float FadeDuration = 1f;
		/// if fading, the tween over which to fade the sound 
		[Tooltip("if fading, the tween over which to fade the sound ")]
		[MMCondition("Fade", true)]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		[MMFInspectorGroup("Solo", true, 32)]
		/// whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing")]
		public bool SoloSingleTrack = false;
		/// whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing")]
		public bool SoloAllTracks = false;
		/// if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing
		[Tooltip("if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing")]
		public bool AutoUnSoloOnEnd = false;

		[MMFInspectorGroup("Spatial Settings", true, 33)]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
		[Range(0f,1f)]
		public float SpatialBlend;
		/// a Transform this sound can 'attach' to and follow it along as it plays
		[Tooltip("a Transform this sound can 'attach' to and follow it along as it plays")]
		public Transform AttachToTransform;
		
		[MMFInspectorGroup("Effects", true, 36)]
		/// Bypass effects (Applied from filter components or global listener filters).
		[Tooltip("Bypass effects (Applied from filter components or global listener filters).")]
		public bool BypassEffects = false;
		/// When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.
		[Tooltip("When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.")]
		public bool BypassListenerEffects = false;
		/// When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.
		[Tooltip("When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.")]
		public bool BypassReverbZones = false;
		/// Sets the priority of the AudioSource.
		[Tooltip("Sets the priority of the AudioSource.")]
		[Range(0, 256)]
		public int Priority = 128;
		/// The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.
		[Tooltip("The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.")]
		[Range(0f,1.1f)]
		public float ReverbZoneMix = 1f;

		[MMFInspectorGroup("Time Options", true, 15)]
		/// a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time) 
		[Tooltip("a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time)")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackTime = new Vector2(0f, 0f);
		/// a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.
		[Tooltip("a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackDuration = new Vector2(0f, 0f);
        
		[MMFInspectorGroup("3D Sound Settings", true, 37)]
		/// Sets the Doppler scale for this AudioSource.
		[Tooltip("Sets the Doppler scale for this AudioSource.")]
		[Range(0f,5f)]
		public float DopplerLevel = 1f;
		/// Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.
		[Tooltip("Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.")]
		[Range(0,360)]
		public int Spread = 0;
		/// Sets/Gets how the AudioSource attenuates over distance.
		[Tooltip("Sets/Gets how the AudioSource attenuates over distance.")]
		public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
		/// Within the Min distance the AudioSource will cease to grow louder in volume.
		[Tooltip("Within the Min distance the AudioSource will cease to grow louder in volume.")]
		public float MinDistance = 1f;
		/// (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
		[Tooltip("(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.")]
		public float MaxDistance = 500f;
		/// whether or not to use a custom curve for custom volume rolloff
		[Tooltip("whether or not to use a custom curve for custom volume rolloff")]
		public bool UseCustomRolloffCurve = false;
		/// the curve to use for custom volume rolloff if UseCustomRolloffCurve is true
		[Tooltip("the curve to use for custom volume rolloff if UseCustomRolloffCurve is true")]
		[MMFCondition("UseCustomRolloffCurve", true)]
		public AnimationCurve CustomRolloffCurve;
		/// whether or not to use a custom curve for spatial blend
		[Tooltip("whether or not to use a custom curve for spatial blend")]
		public bool UseSpatialBlendCurve = false;
		/// the curve to use for custom spatial blend if UseSpatialBlendCurve is true
		[Tooltip("the curve to use for custom spatial blend if UseSpatialBlendCurve is true")]
		[MMFCondition("UseSpatialBlendCurve", true)]
		public AnimationCurve SpatialBlendCurve;
		/// whether or not to use a custom curve for reverb zone mix
		[Tooltip("whether or not to use a custom curve for reverb zone mix")]
		public bool UseReverbZoneMixCurve = false;
		/// the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true
		[Tooltip("the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true")]
		[MMFCondition("UseReverbZoneMixCurve", true)]
		public AnimationCurve ReverbZoneMixCurve;
		/// whether or not to use a custom curve for spread
		[Tooltip("whether or not to use a custom curve for spread")]
		public bool UseSpreadCurve = false;
		/// the curve to use for custom spread if UseSpreadCurve is true
		[Tooltip("the curve to use for custom spread if UseSpreadCurve is true")]
		[MMFCondition("UseSpreadCurve", true)]
		public AnimationCurve SpreadCurve;
        
		[MMFInspectorGroup("Debug", true, 31)]
		/// whether or not to draw sound falloff gizmos when this MMF Player is selected
		[Tooltip("whether or not to draw sound falloff gizmos when this MMF Player is selected")]
		public bool DrawGizmos = false;
		/// an object to use as the center of the gizmos. If left empty, this MMF Player's position will be used.
		[Tooltip("an object to use as the center of the gizmos. If left empty, this MMF Player's position will be used.")]
		[MMFCondition("DrawGizmos", true)]
		public Transform GizmosCenter;
		/// the color to use to draw the min distance sphere of the sound falloff gizmos
		[Tooltip("the color to use to draw the min distance sphere of the sound falloff gizmos")]
		[MMFCondition("DrawGizmos", true)]
		public Color MinDistanceColor = MMColors.CadetBlue;
		/// the color to use to draw the max distance sphere of the sound falloff gizmos
		[Tooltip("the color to use to draw the max distance sphere of the sound falloff gizmos")]
		[MMFCondition("DrawGizmos", true)]
		public Color MaxDistanceColor = MMColors.Orangered;
		/// a test button used to play the sound in inspector
		public MMF_Button TestPlayButton;
		/// a test button used to stop the sound in inspector
		public MMF_Button TestStopButton;
		/// a test button used to stop the sound in inspector
		public MMF_Button ResetSequentialIndexButton;
        
		protected AudioClip _randomClip;
		protected AudioSource _editorAudioSource;
		protected MMSoundManagerPlayOptions _options;
		protected AudioSource _playedAudioSource;
		protected float _randomPlaybackTime;
		protected float _randomPlaybackDuration;
		protected int _currentIndex = 0;
		protected Vector3 _gizmoCenter;
		protected MMShufflebag<int> _randomUniqueShuffleBag;
		
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			HandleSO();
			
			if (RandomUnique)
			{
				_randomUniqueShuffleBag = new MMShufflebag<int>(RandomSfx.Length);
				for (int i = 0; i < RandomSfx.Length; i++)
				{
					_randomUniqueShuffleBag.Add(i,1);
				}
			}
		}

		/// <summary>
		/// Initializes the debug buttons
		/// </summary>
		public override void InitializeCustomAttributes()
		{
			TestPlayButton = new MMF_Button("Debug Play Sound", TestPlaySound);
			TestStopButton = new MMF_Button("Debug Stop Sound", TestStopSound);
			ResetSequentialIndexButton = new MMF_Button("Reset Sequential Index", ResetSequentialIndex);
		}
        
		/// <summary>
		/// Plays either a random sound or the specified sfx
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            

			if (RandomSfx.Length > 0)
			{
				_randomClip = PickRandomClip();

				if (_randomClip != null)
				{
					PlaySound(_randomClip, position, intensityMultiplier);
					return;
				}
			}
			
			if (Sfx != null)
			{
				PlaySound(Sfx, position, intensityMultiplier);
				return;
			}
		}

		/// <summary>
		/// On Stop, we stop our sound if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (StopSoundOnFeedbackStop && (_playedAudioSource != null))
			{
				_playedAudioSource.Stop();
				MMSoundManager.Instance.FreeSound(_playedAudioSource);
			}
		}

		/// <summary>
		/// If a scriptable object is specified, we grab its values
		/// </summary>
		protected virtual void HandleSO()
		{
			if (SoundDataSO == null)
			{
				return;
			}
			
			Sfx = SoundDataSO.Sfx;
			RandomSfx = SoundDataSO.RandomSfx;
			SequentialOrder = SoundDataSO.SequentialOrder;
			SequentialOrderHoldLast = SoundDataSO.SequentialOrderHoldLast;
			SequentialOrderHoldCooldownDuration = SoundDataSO.SequentialOrderHoldCooldownDuration;
			RandomUnique = SoundDataSO.RandomUnique;
	        MinVolume = SoundDataSO.MinVolume;
			MaxVolume = SoundDataSO.MaxVolume;
			MinPitch = SoundDataSO.MinPitch;
			MaxPitch = SoundDataSO.MaxPitch;
			PlaybackTime = SoundDataSO.PlaybackTime;
			PlaybackDuration = SoundDataSO.PlaybackDuration;
			MmSoundManagerTrack = SoundDataSO.MmSoundManagerTrack;
			ID = SoundDataSO.ID;
			AudioGroup = SoundDataSO.AudioGroup;
			RecycleAudioSource = SoundDataSO.RecycleAudioSource;
			Loop = SoundDataSO.Loop;
			Persistent = SoundDataSO.Persistent;
			DoNotPlayIfClipAlreadyPlaying = SoundDataSO.DoNotPlayIfClipAlreadyPlaying;
			StopSoundOnFeedbackStop = SoundDataSO.StopSoundOnFeedbackStop;
			Fade = SoundDataSO.Fade;
			FadeInitialVolume = SoundDataSO.FadeInitialVolume;
			FadeDuration = SoundDataSO.FadeDuration;
			FadeTween = SoundDataSO.FadeTween;
			SoloSingleTrack = SoundDataSO.SoloSingleTrack;
			SoloAllTracks = SoundDataSO.SoloAllTracks;
			AutoUnSoloOnEnd = SoundDataSO.AutoUnSoloOnEnd;
			PanStereo = SoundDataSO.PanStereo;
			SpatialBlend = SoundDataSO.SpatialBlend;
			AttachToTransform = SoundDataSO.AttachToTransform;
			BypassEffects = SoundDataSO.BypassEffects;
			BypassListenerEffects = SoundDataSO.BypassListenerEffects;
			BypassReverbZones = SoundDataSO.BypassReverbZones;
			Priority = SoundDataSO.Priority;
			ReverbZoneMix = SoundDataSO.ReverbZoneMix;
			DopplerLevel = SoundDataSO.DopplerLevel;
			Spread = SoundDataSO.Spread;
			RolloffMode = SoundDataSO.RolloffMode;
			MinDistance = SoundDataSO.MinDistance;
			MaxDistance = SoundDataSO.MaxDistance;
			UseCustomRolloffCurve = SoundDataSO.UseCustomRolloffCurve;
			CustomRolloffCurve = SoundDataSO.CustomRolloffCurve;
			UseSpatialBlendCurve = SoundDataSO.UseSpatialBlendCurve;
			SpatialBlendCurve = SoundDataSO.SpatialBlendCurve;
			UseReverbZoneMixCurve = SoundDataSO.UseReverbZoneMixCurve;
			ReverbZoneMixCurve = SoundDataSO.ReverbZoneMixCurve;
			UseSpreadCurve = SoundDataSO.UseSpreadCurve;
			SpreadCurve = SoundDataSO.SpreadCurve;
		}

		/// <summary>
		/// Randomizes playback time and playback duration
		/// </summary>
		public virtual void RandomizeTimes()
		{
			_randomPlaybackTime = Random.Range(PlaybackTime.x, PlaybackTime.y);
			_randomPlaybackDuration = Random.Range(PlaybackDuration.x, PlaybackDuration.y);
			Owner.ComputeCachedTotalDuration();
		}

		/// <summary>
		/// Triggers a play sound event
		/// </summary>
		/// <param name="sfx"></param>
		/// <param name="position"></param>
		/// <param name="intensity"></param>
		protected virtual void PlaySound(AudioClip sfx, Vector3 position, float intensity)
		{
			if (DoNotPlayIfClipAlreadyPlaying) 
			{
				if ((MMSoundManager.Instance.FindByClip(sfx) != null) && (MMSoundManager.Instance.FindByClip(sfx).isPlaying))
				{
					return;
				}
			}
            
			float volume = Random.Range(MinVolume, MaxVolume);
            
			if (!Timing.ConstantIntensity)
			{
				volume = volume * intensity;
			}
            
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();

			int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
			_options.MmSoundManagerTrack = MmSoundManagerTrack;
			_options.Location = position;
			_options.Loop = Loop;
			_options.Volume = volume;
			_options.ID = ID;
			_options.Fade = Fade;
			_options.FadeInitialVolume = FadeInitialVolume;
			_options.FadeDuration = FadeDuration;
			_options.FadeTween = FadeTween;
			_options.Persistent = Persistent;
			_options.RecycleAudioSource = RecycleAudioSource;
			_options.AudioGroup = AudioGroup;
			_options.Pitch = pitch;
			_options.PlaybackTime = _randomPlaybackTime;
			_options.PlaybackDuration = _randomPlaybackDuration;
			_options.PanStereo = PanStereo;
			_options.SpatialBlend = SpatialBlend;
			_options.SoloSingleTrack = SoloSingleTrack;
			_options.SoloAllTracks = SoloAllTracks;
			_options.AutoUnSoloOnEnd = AutoUnSoloOnEnd;
			_options.BypassEffects = BypassEffects;
			_options.BypassListenerEffects = BypassListenerEffects;
			_options.BypassReverbZones = BypassReverbZones;
			_options.Priority = Priority;
			_options.ReverbZoneMix = ReverbZoneMix;
			_options.DopplerLevel = DopplerLevel;
			_options.Spread = Spread;
			_options.RolloffMode = RolloffMode;
			_options.MinDistance = MinDistance;
			_options.MaxDistance = MaxDistance;
			_options.AttachToTransform = AttachToTransform;
			_options.UseSpreadCurve = UseSpreadCurve;
			_options.SpreadCurve = SpreadCurve;
			_options.UseCustomRolloffCurve = UseCustomRolloffCurve;
			_options.CustomRolloffCurve = CustomRolloffCurve;
			_options.UseSpatialBlendCurve = UseSpatialBlendCurve;
			_options.SpatialBlendCurve = SpatialBlendCurve;
			_options.UseReverbZoneMixCurve = UseReverbZoneMixCurve;
			_options.ReverbZoneMixCurve = ReverbZoneMixCurve;
			
			_options.DoNotAutoRecycleIfNotDonePlaying = (_randomPlaybackDuration > 0) ? true : false;

			_playedAudioSource = MMSoundManagerSoundPlayEvent.Trigger(sfx, _options);

			_lastPlayTimestamp = FeedbackTime;
		}

		/// <summary>
		/// Returns the duration of the sound, or of the longest of the random sounds
		/// </summary>
		/// <returns></returns>
		protected virtual float GetDuration()
		{
			if (SoundDataSO != null)
			{
				return ComputeDuration(SoundDataSO.Sfx, SoundDataSO.RandomSfx);
			}
			else
			{
				return ComputeDuration(Sfx, RandomSfx);
			}
		}

		protected virtual float ComputeDuration(AudioClip sfx, AudioClip[] randomSfx)
		{
			if (sfx != null)
			{
				return (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : sfx.length - _randomPlaybackTime;
			}

			float longest = 0f;
			if ((randomSfx != null) && (randomSfx.Length > 0))
			{
				foreach (AudioClip clip in randomSfx)
				{
					if ((clip != null) && (clip.length > longest))
					{
						longest = clip.length;
					}
				}

				return (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : longest - _randomPlaybackTime;
			}

			return 0f;
		}

		public override void OnDrawGizmosSelectedHandler()
		{
			if (!DrawGizmos)
			{
				return;
			}

			_gizmoCenter = GizmosCenter != null ? GizmosCenter.position : Owner.transform.position;
			Gizmos.color = MinDistanceColor;
			Gizmos.DrawWireSphere(_gizmoCenter, MinDistance);
			Gizmos.color = MaxDistanceColor;
			Gizmos.DrawWireSphere(_gizmoCenter, MaxDistance);
		}

		#region TestMethods

		/// <summary>
		/// A test method that creates an audiosource, plays it, and destroys itself after play
		/// </summary>
		protected virtual async void TestPlaySound()
		{
			AudioClip tmpAudioClip = null;

			if (Sfx != null)
			{
				tmpAudioClip = Sfx;
			}

			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				tmpAudioClip = PickRandomClip();
			}

			if (tmpAudioClip == null)
			{
				Debug.LogError(Label + " on " + Owner.gameObject.name + " can't play in editor mode, you haven't set its Sfx.");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, Owner.gameObject.scene);
			temporaryAudioHost.transform.position = Owner.transform.position;
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, _randomPlaybackTime, _randomPlaybackDuration);
			_lastPlayTimestamp = FeedbackTime;
			float length = (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : tmpAudioClip.length;
			length *= 1000;
			length = length / Mathf.Abs(pitch);
			await Task.Delay((int)length);
			Object.DestroyImmediate(temporaryAudioHost);
		}

		/// <summary>
		/// A test method that stops the test sound
		/// </summary>
		protected virtual void TestStopSound()
		{
			if (_editorAudioSource != null)
			{
				_editorAudioSource.Stop();
			}            
		}

		/// <summary>
		/// Plays the audio source with the specified volume and pitch
		/// </summary>
		/// <param name="audioSource"></param>
		/// <param name="sfx"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, float time, float playbackDuration)
		{
			// we set that audio source clip to the one in paramaters
			audioSource.clip = sfx;
			audioSource.time = time;
			// we set the audio source volume to the one in parameters
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			// we set our loop setting
			audioSource.loop = false;
			// we start playing the sound
			audioSource.Play(); 
		}
        
		/// <summary>
		/// Determines the next index to play when dealing with random clips
		/// </summary>
		/// <returns></returns>
		protected virtual AudioClip PickRandomClip()
		{
			int newIndex = 0;
	        
			if (!SequentialOrder)
			{
				if (RandomUnique)
				{
					newIndex = _randomUniqueShuffleBag.Pick();
				}
				else
				{
					newIndex = Random.Range(0, RandomSfx.Length);	
				}
			}
			else
			{
				newIndex = _currentIndex;
		        
				if (newIndex >= RandomSfx.Length)
				{
					if (SequentialOrderHoldLast)
					{
						newIndex--;
						if ((SequentialOrderHoldCooldownDuration > 0)
						    && (FeedbackTime - _lastPlayTimestamp > SequentialOrderHoldCooldownDuration))
						{
							newIndex = 0;    
						}
					}
					else
					{
						newIndex = 0;
					}
				}
				_currentIndex = newIndex + 1;
			}
			return RandomSfx[newIndex];
		}

		/// <summary>
		/// Forces a reset of the sequential index to 0
		/// </summary>
		public virtual void ResetSequentialIndex()
		{
			_currentIndex = 0;
		}

		/// <summary>
		/// Forces a reset of the sequential index to the value specified in parameters
		/// </summary>
		/// <param name="newIndex"></param>
		public virtual void SetSequentialIndex(int newIndex)
		{
			_currentIndex = newIndex;
		}
		
		/// <summary>
		/// On validate we randomize our times
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			RandomizeTimes();
		}

		#endregion
	}
}