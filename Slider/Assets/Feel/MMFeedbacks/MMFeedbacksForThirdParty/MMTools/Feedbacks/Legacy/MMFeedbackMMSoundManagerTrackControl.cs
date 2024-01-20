using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control all sounds playing on a specific track (master, UI, music, sfx), and play, pause, mute, unmute, resume, stop, free them all at once. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Track Control")]
	[FeedbackHelp("This feedback will let you control all sounds playing on a specific track (master, UI, music, sfx), and play, pause, mute, unmute, resume, stop, free them all at once. You will need a MMSoundManager in your scene for this to work.")]
	public class MMFeedbackMMSoundManagerTrackControl : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
        
		/// the possible modes you can use to interact with the track. Free will stop all sounds and return them to the pool
		public enum ControlModes { Mute, UnMute, SetVolume, Pause, Play, Stop, Free }
        
		[Header("MMSoundManager Track Control")]
		/// the track to mute/unmute/pause/play/stop/free/etc
		[Tooltip("the track to mute/unmute/pause/play/stop/free/etc")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the selected control mode to interact with the track. Free will stop all sounds and return them to the pool
		[Tooltip("the selected control mode to interact with the track. Free will stop all sounds and return them to the pool")]
		public ControlModes ControlMode = ControlModes.Pause;
		/// if setting the volume, the volume to assign to the track 
		[Tooltip("if setting the volume, the volume to assign to the track")]
		[MMEnumCondition("ControlMode", (int) ControlModes.SetVolume)]
		public float Volume = 0.5f;

		/// <summary>
		/// On play, orders the entire track to follow the specific command, via a MMSoundManager event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (ControlMode)
			{
				case ControlModes.Mute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, Track);
					break;
				case ControlModes.UnMute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, Track);
					break;
				case ControlModes.SetVolume:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, Track, Volume);
					break;
				case ControlModes.Pause:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PauseTrack, Track);
					break;
				case ControlModes.Play:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PlayTrack, Track);
					break;
				case ControlModes.Stop:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.StopTrack, Track);
					break;
				case ControlModes.Free:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.FreeTrack, Track);
					break;
			}
		}
	}
}