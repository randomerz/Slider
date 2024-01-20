using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	[ExecuteAlways]
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Sound")]
	[FeedbackHelp("This feedback lets you play the specified AudioClip, either via event (you'll need something in your scene to catch a MMSfxEvent, for example a MMSoundManager), or cached (AudioSource gets created on init, and is then ready to be played), or on demand (instantiated on Play). For all these methods you can define a random volume between min/max boundaries (just set the same value in both fields if you don't want randomness), random pitch, and an optional AudioMixerGroup.")]
	public class MMF_Sound : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override bool HasCustomInspectors { get { return true; } }
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
			return requiresSetup;
		}
		public override string RequiredTargetText { get { return Sfx != null ? Sfx.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that you set an Audio clip in its Sfx slot below, or one or more clips in the Random Sfx array."; } }
		#endif
		public override bool HasRandomness => true;

		/// <summary>
		/// The possible methods to play the sound with. 
		/// Event : sends a MMSfxEvent, you'll need a class to catch this event and play the sound
		/// Cached : creates and stores an audiosource to play the sound with, parented to the owner
		/// OnDemand : creates an audiosource and destroys it everytime you want to play the sound
		/// </summary>
		public enum PlayMethods { Event, Cached, OnDemand, Pool }

		[MMFInspectorGroup("Sound", true, 14, true)]
		/// the sound clip to play
		[Tooltip("the sound clip to play")]
		public AudioClip Sfx;

		/// an array to pick a random sfx from
		[Tooltip("an array to pick a random sfx from")]
		public AudioClip[] RandomSfx;

		/// a test button used to play the sound in inspector
		public MMF_Button TestPlayButton;
		/// a test button used to stop the sound in inspector
		public MMF_Button TestStopButton;

		[MMFInspectorGroup("Play Method", true, 27)]
		/// the play method to use when playing the sound (event, cached or on demand)
		[Tooltip("the play method to use when playing the sound (event, cached or on demand)")]
		public PlayMethods PlayMethod = PlayMethods.Event;
		/// the size of the pool when in Pool mode
		[Tooltip("the size of the pool when in Pool mode")]
		[MMFEnumCondition("PlayMethod", (int)PlayMethods.Pool)]
		public int PoolSize = 10;
		/// in event mode, whether to use legacy events (MMSfxEvent) or the current events (MMSoundManagerSoundPlayEvent)
		[Tooltip("in event mode, whether to use legacy events (MMSfxEvent) or the current events (MMSoundManagerSoundPlayEvent)")]
		[MMFEnumCondition("PlayMethod", (int)PlayMethods.Event)]
		public bool UseLegacyEventsMode = false;
		/// if this is true, calling Stop on this feedback will also stop the sound from playing further
		[Tooltip("if this is true, calling Stop on this feedback will also stop the sound from playing further")]
		public bool StopSoundOnFeedbackStop = true;
		
		[MMFInspectorGroup("Sound Properties", true, 28)]
        
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

		[Header("Mixer")]
		/// the audiomixer to play the sound with (optional)
		[Tooltip("the audiomixer to play the sound with (optional)")]
		public AudioMixerGroup SfxAudioMixerGroup;
		/// the audiosource priority
		[Tooltip("the audiosource priority, to be specified if needed between 0 (highest) and 256")] 
		public int Priority = 128;

		[MMFInspectorGroup("Spatial Settings", true, 33, false, true)]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
		[Range(0f,1f)]
		public float SpatialBlend;
		
		[MMFInspectorGroup("3D Sound Settings", true, 37, false, true)]
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


		/// the duration of this feedback is the duration of the clip being played
		public override float FeedbackDuration { get { return GetDuration(); } }

		protected AudioClip _randomClip;
		protected AudioSource _cachedAudioSource;
		protected AudioSource[] _pool;
		protected AudioSource _tempAudioSource;
		protected float _duration;
		protected AudioSource _editorAudioSource;
		protected AudioSource _audioSource;

		public override void InitializeCustomAttributes()
		{
			TestPlayButton = new MMF_Button("Debug Play Sound", TestPlaySound);
			TestStopButton = new MMF_Button("Debug Stop Sound", TestStopSound);
		}

		/// <summary>
		/// Custom init to cache the audiosource if required
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (PlayMethod == PlayMethods.Cached)
			{
				_cachedAudioSource = CreateAudioSource(owner.gameObject, "CachedFeedbackAudioSource");
			}
			if (PlayMethod == PlayMethods.Pool)
			{
				// create a pool
				_pool = new AudioSource[PoolSize];
				for (int i = 0; i < PoolSize; i++)
				{
					_pool[i] = CreateAudioSource(owner.gameObject, "PooledAudioSource"+i);
				}
			}
		}

		protected virtual AudioSource CreateAudioSource(GameObject owner, string audioSourceName)
		{
			// we create a temporary game object to host our audio source
			GameObject temporaryAudioHost = new GameObject(audioSourceName);
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, Owner.gameObject.scene);
			// we set the temp audio's position
			temporaryAudioHost.transform.position = owner.transform.position;
			temporaryAudioHost.transform.SetParent(owner.transform);
			// we add an audio source to that host
			_tempAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			_tempAudioSource.playOnAwake = false;
			return _tempAudioSource; 
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
            
			if (Sfx != null)
			{
				_duration = Sfx.length;
				PlaySound(Sfx, position, intensityMultiplier);
				return;
			}

			if (RandomSfx.Length > 0)
			{
				_randomClip = RandomSfx[Random.Range(0, RandomSfx.Length)];

				if (_randomClip != null)
				{
					_duration = _randomClip.length;
					PlaySound(_randomClip, position, intensityMultiplier);
				}
                
			}
		}

		protected virtual float GetDuration()
		{
			if (Sfx != null)
			{
				return Sfx.length;
			}

			float longest = 0f;
			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				foreach (AudioClip clip in RandomSfx)
				{
					if ((clip != null) && (clip.length > longest))
					{
						longest = clip.length;
					}
				}

				return longest;
			}

			return 0f;
		}

		/// <summary>
		/// Plays a sound differently based on the selected play method
		/// </summary>
		/// <param name="sfx"></param>
		/// <param name="position"></param>
		protected virtual void PlaySound(AudioClip sfx, Vector3 position, float intensity)
		{
			float volume = Random.Range(MinVolume, MaxVolume);
            
			if (!Timing.ConstantIntensity)
			{
				volume = volume * intensity;
			}
            
			float pitch = Random.Range(MinPitch, MaxPitch);

			int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
			if (!NormalPlayDirection)
			{
				pitch = -pitch;
			}

			switch (PlayMethod)
			{
				case PlayMethods.Event:
					if (UseLegacyEventsMode)
					{
						MMSfxEvent.Trigger(sfx, SfxAudioMixerGroup, volume, pitch, Priority);
					}
					else
					{
						MMSoundManagerPlayOptions options = new MMSoundManagerPlayOptions();
						options = MMSoundManagerPlayOptions.Default;
						options.Location = Owner.transform.position;
						options.AudioGroup = SfxAudioMixerGroup;
						options.DoNotAutoRecycleIfNotDonePlaying = false;
						options.Volume = volume;
						options.Pitch = pitch;
						if (Priority >= 0)
						{
							options.Priority = Mathf.Min(Priority, 256);
						}
						options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
						options.Loop = false;
						MMSoundManagerSoundPlayEvent.Trigger(sfx, options);	
					}
					break;
				case PlayMethods.Cached:
					// we set that audio source clip to the one in paramaters
					PlayAudioSource(_cachedAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
					break;
				case PlayMethods.OnDemand:
					// we create a temporary game object to host our audio source
					GameObject temporaryAudioHost = new GameObject("TempAudio");
					SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, Owner.gameObject.scene);
					// we set the temp audio's position
					temporaryAudioHost.transform.position = position;
					// we add an audio source to that host
					AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
					PlayAudioSource(audioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
					// we destroy the host after the clip has played
					Owner.ProxyDestroy(temporaryAudioHost, sfx.length);
					break;
				case PlayMethods.Pool:
					_tempAudioSource = GetAudioSourceFromPool();
					if (_tempAudioSource != null)
					{
						PlayAudioSource(_tempAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
					}
					break;
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
            
			if (StopSoundOnFeedbackStop && (_audioSource != null))
			{
				_audioSource.Stop();
			}
		}

		/// <summary>
		/// Plays the audio source with the specified volume and pitch
		/// </summary>
		/// <param name="audioSource"></param>
		/// <param name="sfx"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, int timeSamples, AudioMixerGroup audioMixerGroup = null, int priority = 128)
		{
			_audioSource = audioSource;
			// we set that audio source clip to the one in paramaters
			audioSource.clip = sfx;
			audioSource.timeSamples = timeSamples;
			// we set the audio source volume to the one in parameters
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.priority = priority;
			// we set spatial settings
			audioSource.panStereo = PanStereo;
			audioSource.spatialBlend = SpatialBlend;
			audioSource.dopplerLevel = DopplerLevel;
			audioSource.spread = Spread;
			audioSource.rolloffMode = RolloffMode;
			audioSource.minDistance = MinDistance;
			audioSource.maxDistance = MaxDistance;
			if (UseSpreadCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.Spread, SpreadCurve); }
			if (UseCustomRolloffCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, CustomRolloffCurve); }
			if (UseSpatialBlendCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, SpatialBlendCurve); }
			if (UseReverbZoneMixCurve) { audioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, ReverbZoneMixCurve); }
			// we set our loop setting
			audioSource.loop = false;
			if (audioMixerGroup != null)
			{
				audioSource.outputAudioMixerGroup = audioMixerGroup;
			}
			// we start playing the sound
			audioSource.Play(); 
		}

		/// <summary>
		/// Gets an audio source from the pool if possible
		/// </summary>
		/// <returns></returns>
		protected virtual AudioSource GetAudioSourceFromPool()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				if (!_pool[i].isPlaying)
				{
					return _pool[i];
				}
			}
			return null;
		}

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

			if (RandomSfx.Length > 0)
			{
				tmpAudioClip = RandomSfx[Random.Range(0, RandomSfx.Length)];
			}

			if (tmpAudioClip == null)
			{
				Debug.LogError(Label + " on " + Owner.gameObject.name + " can't play in editor mode, you haven't set its Sfx.");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, Owner.gameObject.scene);
			temporaryAudioHost.transform.position = Owner.transform.position;
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, 0);
			float length = 1000 * tmpAudioClip.length;
			await Task.Delay((int)length);
			Owner.ProxyDestroyImmediate(temporaryAudioHost);
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
	}
}