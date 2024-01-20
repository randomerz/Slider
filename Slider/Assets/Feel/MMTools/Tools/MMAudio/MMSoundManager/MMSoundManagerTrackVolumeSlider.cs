using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// You can add this class to a slider in your UI and it'll let you control a target Track volume
	/// via the MMSoundManager
	/// </summary>
	public class MMSoundManagerTrackVolumeSlider :  MonoBehaviour, 
													MMEventListener<MMSoundManagerEvent>, 
													MMEventListener<MMSoundManagerTrackEvent>,
													MMEventListener<MMSoundManagerTrackFadeEvent>
	{
		/// <summary>
		/// The possible modes this slider can be in
		/// - Read : the slider will move to reflect the volume of the track
		/// - Write : the value of the slider will be applied to the volume of the track
		/// This slider can also listen for events (mute, unmute, fade, volume change) and automatically switch to read mode
		/// if one is caught. This means that most of the time, the slider is in write mode, and switches to read mode only
		/// when needed, to be always accurate
		/// </summary>
		public enum Modes { Read, Write }
		
		[Header("Track Volume Settings")]
		/// The track to change volume on
		[Tooltip("The track to change volume on")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// The volume to apply to the track when the slider is at its minimum
		[Tooltip("The volume to apply to the track when the slider is at its minimum")]
		public float MinVolume = 0f;
		/// The volume to apply to the track when the slider is at its maximum
		[Tooltip("The volume to apply to the track when the slider is at its maximum")]
		public float MaxVolume = 1f;
		
		[Header("Read/Write Mode")]
		/// in read mode, the value of the slider will be applied to the volume of the track. in read mode, the slider will move to reflect the volume of the track
		[Tooltip("in read mode, the value of the slider will be applied to the volume of the track. in read mode, the slider will move to reflect the volume of the track")]
		public Modes Mode = Modes.Write;
		/// if this is true, the slider will automatically switch to read mode for the required duration when a track fade event is caught
		[Tooltip("if this is true, the slider will automatically switch to read mode for the required duration when a track fade event is caught")]
		public bool ChangeModeOnTrackFade = true;
		/// if this is true, the slider will automatically switch to read mode for the required duration when a track mute event is caught
		[Tooltip("if this is true, the slider will automatically switch to read mode for the required duration when a track mute event is caught")]
		public bool ChangeModeOnMute = true;
		/// if this is true, the slider will automatically switch to read mode for the required duration when a track unmute event is caught
		[Tooltip("if this is true, the slider will automatically switch to read mode for the required duration when a track unmute event is caught")]
		public bool ChangeModeOnUnmute = true;
		/// if this is true, the slider will automatically switch to read mode for the required duration when a track volume change event is caught
		[Tooltip("if this is true, the slider will automatically switch to read mode for the required duration when a track volume change event is caught")]
		public bool ChangeModeOnTrackVolumeChange = false;
		/// when switching automatically (and temporarily) to Read Mode, the minimum duration the slider will remain in that mode
		[Tooltip("when switching automatically (and temporarily) to Read Mode, the minimum duration the slider will remain in that mode")]
		public float ModeSwitchBufferTime = 0.1f;

		protected Slider _slider;
		protected Modes _resetToMode;
		protected bool _resetNeeded = false;
		protected float _resetTimestamp;
		
		/// <summary>
		/// On awake we cache our slider
		/// </summary>
		protected virtual void Awake()
		{
			_slider = this.gameObject.GetComponent<Slider>();
		}

		/// <summary>
		/// On late update, we update our slider's value if in read mode, and reset our mode if needed
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (Mode == Modes.Read)
			{
				float trackVolume = MMSoundManager.Instance.GetTrackVolume(Track, false);
				_slider.value = trackVolume; 	
			}

			if (_resetNeeded && (Time.unscaledTime >= _resetTimestamp))
			{
				Mode = _resetToMode;
				_resetNeeded = false;
			}
		}

		/// <summary>
		/// A public method you can use to switch to read mode for a limited time, resetting to write after that
		/// </summary>
		/// <param name="duration"></param>
		public virtual void ChangeModeToRead(float duration)
		{
			_resetToMode = Modes.Write;
			Mode = Modes.Read;
			_resetTimestamp = Time.unscaledTime + duration;
			_resetNeeded = true;
		}

		/// <summary>
		/// Bind your slider to this method
		/// </summary>
		public virtual void UpdateVolume(float newValue)
		{
			if (Mode == Modes.Read)
			{
				return;
			}
			float newVolume = MMMaths.Remap(newValue, 0f, 1f, MinVolume, MaxVolume);
			MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, Track, newVolume);
		}

		/// <summary>
		/// When we get an event letting us know the settings have been loaded, we update our slider to reflect the current track volume
		/// </summary>
		/// <param name="soundManagerEvent"></param>
		public void OnMMEvent(MMSoundManagerEvent soundManagerEvent)
		{
			if (soundManagerEvent.EventType == MMSoundManagerEventTypes.SettingsLoaded)
			{
				UpdateSliderValueWithTrackVolume();
			}
		}

		/// <summary>
		/// Updates the slider value to reflect the current track volume
		/// </summary>
		public virtual void UpdateSliderValueWithTrackVolume()
		{
			_slider.value = MMMaths.Remap(MMSoundManager.Instance.GetTrackVolume(Track, false), 0f, 1f, MinVolume, MaxVolume);
		}

		/// <summary>
		/// if we grab a track event, we switch to read mode if needed
		/// </summary>
		/// <param name="trackEvent"></param>
		public void OnMMEvent(MMSoundManagerTrackEvent trackEvent)
		{
			switch (trackEvent.TrackEventType)
			{
				case MMSoundManagerTrackEventTypes.MuteTrack:
					if (ChangeModeOnMute)
					{
						ChangeModeToRead(ModeSwitchBufferTime);	
					}
					break;
				case MMSoundManagerTrackEventTypes.UnmuteTrack:
					if (ChangeModeOnUnmute)
					{
						ChangeModeToRead(ModeSwitchBufferTime);	
					}
					break;
				case MMSoundManagerTrackEventTypes.SetVolumeTrack:
					if (ChangeModeOnTrackVolumeChange)
					{
						ChangeModeToRead(ModeSwitchBufferTime);	
					}
					break;
			}
		}

		/// <summary>
		/// if we grab a track fade event, we switch to read mode if needed
		/// </summary>
		/// <param name="fadeEvent"></param>
		public void OnMMEvent(MMSoundManagerTrackFadeEvent fadeEvent)
		{
			if (ChangeModeOnTrackFade)
			{
				ChangeModeToRead(fadeEvent.FadeDuration + ModeSwitchBufferTime);	
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMSoundManagerEvent>();
			this.MMEventStartListening<MMSoundManagerTrackEvent>();
			this.MMEventStartListening<MMSoundManagerTrackFadeEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMSoundManagerEvent>();
			this.MMEventStopListening<MMSoundManagerTrackEvent>();
			this.MMEventStopListening<MMSoundManagerTrackFadeEvent>();
		}
	}
}

