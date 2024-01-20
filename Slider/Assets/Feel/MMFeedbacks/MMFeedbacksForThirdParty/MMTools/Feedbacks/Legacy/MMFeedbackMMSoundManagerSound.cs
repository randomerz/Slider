using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you play a sound via the MMSoundManager. You will need a game object in your scene with a MMSoundManager object on it for this to work.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Sound")]
	[FeedbackHelp("This feedback will let you play a sound via the MMSoundManager. You will need a game object in your scene with a MMSoundManager object on it for this to work.")]
	public class MMFeedbackMMSoundManagerSound : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		/// the duration of this feedback is the duration of the clip being played
		public override float FeedbackDuration { get { return GetDuration(); } }
        
		[Header("Sound")]
		/// the sound clip to play
		[Tooltip("the sound clip to play")]
		public AudioClip Sfx;

		[Header("Random Sound")]
		/// an array to pick a random sfx from
		[Tooltip("an array to pick a random sfx from")]
		public AudioClip[] RandomSfx;

		[Header("Test")]
		[MMFInspectorButton("TestPlaySound")]
		public bool TestButton;
		[MMFInspectorButton("TestStopSound")]
		public bool TestStopButton;
        
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

		[Header("SoundManager Options")]
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
		/// if this is true, this sound won't be recycled if it's not done playing
		[Tooltip("if this is true, this sound won't be recycled if it's not done playing")]
		public bool DoNotAutoRecycleIfNotDonePlaying = false;
        
		[Header("Fade")]
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
        
		[Header("Solo")]
		/// whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing")]
		public bool SoloSingleTrack = false;
		/// whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing")]
		public bool SoloAllTracks = false;
		/// if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing
		[Tooltip("if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing")]
		public bool AutoUnSoloOnEnd = false;

		[Header("Spatial Settings")]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
		[Range(0f,1f)]
		public float SpatialBlend;
        
		[Header("Effects")]
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
        
		[Header("3D Sound Settings")]
        
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
        
		protected AudioClip _randomClip;
		protected AudioSource _editorAudioSource;
		protected MMSoundManagerPlayOptions _options;
		protected AudioSource _playedAudioSource;
        
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
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
			if (Sfx != null)
			{
				PlaySound(Sfx, position, intensityMultiplier);
				return;
			}

			if (RandomSfx.Length > 0)
			{
				_randomClip = RandomSfx[Random.Range(0, RandomSfx.Length)];

				if (_randomClip != null)
				{
					PlaySound(_randomClip, position, intensityMultiplier);
				}
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
			}
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
				if (MMSoundManager.Instance.FindByClip(sfx) != null)
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

			int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
			/*if (!NormalPlayDirection)
			{
			    pitch = -pitch;
			}*/
            
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
			_options.DoNotAutoRecycleIfNotDonePlaying = DoNotAutoRecycleIfNotDonePlaying;

			_playedAudioSource = MMSoundManagerSoundPlayEvent.Trigger(sfx, _options);
		}

		/// <summary>
		/// Returns the duration of the sound, or of the longest of the random sounds
		/// </summary>
		/// <returns></returns>
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

			if (RandomSfx.Length > 0)
			{
				tmpAudioClip = RandomSfx[Random.Range(0, RandomSfx.Length)];
			}

			if (tmpAudioClip == null)
			{
				Debug.LogError(Label + " on " + this.gameObject.name + " can't play in editor mode, you haven't set its Sfx.");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, this.gameObject.scene);
			temporaryAudioHost.transform.position = this.transform.position;
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, 0);
			float length = 1000 * tmpAudioClip.length;
			length = length / Mathf.Abs(pitch);
			await Task.Delay((int)length);
			DestroyImmediate(temporaryAudioHost);
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
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, int timeSamples)
		{
			// we set that audio source clip to the one in paramaters
			audioSource.clip = sfx;
			audioSource.timeSamples = timeSamples;
			// we set the audio source volume to the one in parameters
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			// we set our loop setting
			audioSource.loop = false;
			// we start playing the sound
			audioSource.Play(); 
		}

		#endregion
	}
}